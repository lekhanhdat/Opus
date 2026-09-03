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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using AutoInstallation.Contract.DataBase;
using AutoInstallationCommon.Utility.Handler;

namespace AutoInstallationCommon.Utility
{
    public class DatabaseUtility
    {
        #region -- authentication type --

        private readonly Authentication authenticationType = Authentication.WindowsAuthentication;

        #endregion

        private SqlConnection connection = new SqlConnection();

        public DatabaseUtility(
            Authentication type,
            string server,
            string name,
            string username,
            string password,
            string inputpassphrase)
        {
            authenticationType = type;
            DBServer = server;
            DBUsername = username;
            DBPassword = password;
            DBName = name;
        }

        public DatabaseUtility(
            Authentication type,
            string server,
            string name,
            string username,
            string password)
        {
            authenticationType = type;
            DBServer = server;
            DBUsername = username;
            DBPassword = password;
            DBName = name;
        }

        #region -- db server --

        public string DBServer { get; } = string.Empty;

        #endregion

        #region -- db username --

        public string DBUsername { get; } = string.Empty;

        #endregion

        #region -- db password --

        public string DBPassword { get; } = string.Empty;

        #endregion

        #region -- db name --

        public string DBName { get; } = string.Empty;

        #endregion


        /// <summary>
        ///     Init master db connection string
        /// </summary>
        /// <returns></returns>
        public string InitializeMasterDBConnectionString()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = DBServer;
            connectionStringBuilder.InitialCatalog = "master";
            connectionStringBuilder.Pooling = false;
            if (authenticationType == Authentication.WindowsAuthentication)
            {
                connectionStringBuilder.IntegratedSecurity = true;
            }
            else if (authenticationType == Authentication.SQLAuthentication)
            {
                connectionStringBuilder.IntegratedSecurity = false;
                connectionStringBuilder.UserID = DBUsername;
                connectionStringBuilder.Password = DBPassword;
            }

            return connectionStringBuilder.ConnectionString;
        }

        /// <summary>
        ///     open the sql connection
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public bool OpenConnection(string connectionString)
        {
            if (authenticationType == Authentication.WindowsAuthentication)
                return OpenLocalConnectionWA(connectionString);
            return OpenConnectionSA(connectionString);
            return true; //Later add
        }

        private bool OpenConnectionSA(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            return true;
        }

        /// <summary>
        ///     check if the database is exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsDBExist()
        {
            var command = connection.CreateCommand();
            command.CommandText = "select name from sysdatabases where name=@DBName";
            command.Parameters.AddWithValue("@DBName", DBName);
            try
            {
                var reader = command.ExecuteReader();
                var result = reader.HasRows;
                reader.Close();
                return result;
            }
            finally
            {
                command.Dispose();
            }
        }

        public bool DeleteDB()
        {
            var command = connection.CreateCommand();
            var cmdText = "drop database [" + DBName + "];\r\n";
            var deletebackup = "EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'" + DBName + "';\r\n";
            var singlemode = "ALTER DATABASE [" + DBName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\r\n";
            command.CommandText = deletebackup + singlemode + cmdText;
            command.CommandType = CommandType.Text;
            //command.Parameters.AddWithValue("@DBName", dbName);
            try
            {
                var result = command.ExecuteNonQuery();
                return true;
            }
            finally
            {
                command.Dispose();
            }
        }

        /// <summary>
        ///     open the sql connection
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public bool OpenConnection(string connectionString, bool isUnattendedInstall)
        {
            if (authenticationType == Authentication.WindowsAuthentication)
                return true; //OpenConnectionWA(connectionString);
            return OpenConnectionSA(connectionString);
        }

        /// <summary>
        ///     open the sql connection with WA
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private bool OpenConnectionWA(string connectionString)
        {
            var items = DBUsername.Split('\\');
            var domain = items[0];
            var username = items[1];
            connection = new SqlConnection(connectionString);
            var instanceName = VerifyIsLocalSql.GetSqlHost(DBServer);
            if (VerifyIsLocalSql.IsLocalSqlInstance(instanceName))
            {
                var ave = new AveImpersonator(domain, username, DBPassword, false, true);
                try
                {
                    ave.Impersonate();
                    connection.Open();
                    return true;
                }
                finally
                {
                    ave.Undo();
                }
            }
            else
            {
                var ave = new AveImpersonator(domain, username, DBPassword, true, true);
                try
                {
                    ave.Impersonate();
                    connection.Open();
                    return true;
                }
                finally
                {
                    ave.Undo();
                }
            }
        }


        public bool IsLocalSystemDBUser()
        {
            var domain = Environment.UserDomainName;
            var username = Environment.UserName;
            var localSystemUser = domain + "\\" + username;
            if (localSystemUser.Equals(DBUsername, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        /// <summary>
        ///     以localsystem身份登录数据库
        /// </summary>
        /// <param name="connectionString">登录数据库的拼接字符串</param>
        /// <returns>false:登录数据库失败 true:登录数据库成功</returns>
        public bool OpenLocalConnectionWA(string connectionString)
        {
            //if (CheckLocalSystemDBUsername())
            //{
            try
            {
                connection = new SqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed) connection.Open();
                return true;
            }
            catch (Exception E)
            {
                return false;
            }

            //}
            //else
            //{
            //    return false;
            //}
        }

        public bool CheckLocalSystemDBUsername()
        {
            var items = DBUsername.Split('\\');
            var domain = items[0];
            var user = items[1];
            var result = new Impersonator(user, domain, DBPassword).LogonUser();
            return result;
        }

        /// <summary>
        ///     get database server version
        /// </summary>
        /// <returns></returns>
        public string GetDatabaseVersion()
        {
            var versionString = string.Empty;
            var command = connection.CreateCommand();
            command.CommandText = "select @@version as version";
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read()) versionString = reader["version"].ToString();
                reader.Close();
                return versionString;
            }
            finally
            {
                command.Dispose();
            }
        }

        /// <summary>
        ///     create the database
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public bool CreateDatabase()
        {
            var _dbName = ReplaceIllegalCharacters(DBName);
            var dbFileName = GetDatabaseFileName(_dbName);
            var dbLogName = GetDatabaseLogFileName(_dbName);
            var name = DBName;
            name = FormatDBName(name);
            var command = connection.CreateCommand();
            var commandString =
                string.Format(
                    @"CREATE DATABASE [{0}] ON  PRIMARY ( NAME = N'{1}', FILENAME = N'{2}.mdf') LOG ON ( NAME = N'{3}_log', FILENAME = N'{4}.LDF') alter database [{5}] collate Latin1_General_CI_AS_KS_WS alter database [{6}] set recovery simple,auto_close off",
                    name,
                    _dbName,
                    dbFileName,
                    _dbName,
                    dbLogName,
                    name,
                    name);
            command.CommandText = commandString;
            command.CommandTimeout = 0;
            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            finally
            {
                command.Dispose();
            }
        }

        public string FormatDBName(string dbName)
        {
            if (dbName.Contains("]")) return dbName.Replace("]", "]]");
            return dbName;
        }

        private string GetDatabaseLogFileName(string dbName)
        {
            var logPath = GetLogPath();
            return Path.Combine(logPath, dbName) + "_log";
        }

        private string GetLogPath()
        {
            var path = GetDefaultLogPath();
            if (string.IsNullOrEmpty(path))
            {
                var command = connection.CreateCommand();
                command.CommandText = @"select* from master.dbo.sysdatabases where name='master'";
                try
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        path = reader["filename"].ToString();
                        if (path.Contains("master.mdf")) path = path.Replace("master.mdf", "");
                    }

                    reader.Close();
                }
                finally
                {
                    command.Dispose();
                }
            }
            else
            {
                path = path + "\\"; //ADO-62672
            }

            return path;
        }

        private string GetDefaultLogPath()
        {
            var path = string.Empty;
            var command = connection.CreateCommand();
            command.CommandText =
                @"declare @DefaultLog nvarchar(512) exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultLog', @DefaultLog OUTPUT select @DefaultLog as LogPath";
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read()) path = reader["LogPath"].ToString();
                reader.Close();
                return path;
            }
            finally
            {
                command.Dispose();
            }
        }

        private string ReplaceIllegalCharacters(string illegalString)
        {
            var legalString = illegalString;
            legalString = legalString.Replace("'", "''");
            legalString = legalString.Replace("*", "_");
            legalString = legalString.Replace("|", "_");
            legalString = legalString.Replace("\\", "_");
            legalString = legalString.Replace(":", "_");
            legalString = legalString.Replace("\"", "_");
            legalString = legalString.Replace("<", "_");
            legalString = legalString.Replace(">", "_");
            legalString = legalString.Replace("?", "_");
            legalString = legalString.Replace("/", "_");
            return legalString;
        }

        private string GetDatabaseFileName(string dbName)
        {
            var dbPath = GetDataPath();
            return Path.Combine(dbPath, dbName);
        }

        private string GetDataPath()
        {
            var path = GetDefaultDataPath();
            if (string.IsNullOrEmpty(path))
            {
                var command = connection.CreateCommand();
                command.CommandText = @"select* from master.dbo.sysdatabases where name='master'";
                try
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        path = reader["filename"].ToString();
                        if (path.Contains("master.mdf")) path = path.Replace("master.mdf", "");
                    }

                    reader.Close();
                }
                finally
                {
                    command.Dispose();
                }
            }
            else
            {
                path = path + "\\"; //ADO-62672
            }

            return path;
        }

        private string GetDefaultDataPath()
        {
            var path = string.Empty;
            var command = connection.CreateCommand();
            command.CommandText =
                @"declare @DefaultData nvarchar(512) exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData', @DefaultData OUTPUT select @DefaultData as DataPath";
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read()) path = reader["DataPath"].ToString();
                reader.Close();
                return path;
            }
            finally
            {
                command.Dispose();
            }
        }

        public bool CloseConnection()
        {
            if (connection != null) connection.Dispose();
            return true;
        }

        /// <summary>
        ///     Init connect to specified db connection string
        /// </summary>
        /// <returns></returns>
        public string InitializeSpecifiedDBConnectionString()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = DBServer;
            connectionStringBuilder.InitialCatalog = DBName;
            connectionStringBuilder.Pooling = false;

            if (authenticationType == Authentication.WindowsAuthentication)
            {
                connectionStringBuilder.IntegratedSecurity = true;
            }
            else if (authenticationType == Authentication.SQLAuthentication)
            {
                connectionStringBuilder.IntegratedSecurity = false;
                connectionStringBuilder.UserID = DBUsername;
                connectionStringBuilder.Password = DBPassword;
            }

            return connectionStringBuilder.ConnectionString;
        }

        /// <summary>
        ///     check if the database have table
        /// </summary>
        /// <returns></returns>
        public bool IsHaveTable()
        {
            var command = connection.CreateCommand();
            var commandString = "select name from sysobjects where xtype='U' order by name";
            command.CommandText = commandString;
            try
            {
                var reader = command.ExecuteReader();
                var result = reader.Read();
                reader.Close();
                return result;
            }
            finally
            {
                command.Dispose();
            }
        }

        /// <summary>
        ///     get the collation
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public string GetCollation()
        {
            var result = string.Empty;
            SetAutoCloseOff();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT DATABASEPROPERTYEX ( @key , 'Collation' )";
            command.Parameters.AddWithValue("@key", DBName);
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read()) result = reader[0].ToString();
                reader.Close();
                return result;
            }
            finally
            {
                command.Dispose();
            }
        }

        /// <summary>
        ///     set auto close off
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private bool SetAutoCloseOff()
        {
            if (IsAutoClose())
            {
                var _dbName = DBName;
                _dbName = FormatDBName(_dbName);
                var command = connection.CreateCommand();
                command.CommandText = string.Format("alter database [{0}] set auto_close off", _dbName);
                try
                {
                    command.ExecuteNonQuery();
                }
                finally
                {
                    command.Dispose();
                }
            }

            return true;
        }

        /// <summary>
        ///     get datbase type list
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public List<string> GetDBTypeList(string table, string column)
        {
            var result = new List<string>();

            var command = connection.CreateCommand();
            var commmandString = "select * from {0}";
            command.CommandText = string.Format(commmandString, table);
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read()) result.Add(reader[column].ToString());
                reader.Close();
                return result;
            }
            finally
            {
                command.Dispose();
            }
        }

        /// <summary>
        ///     check if the database is auto close
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private bool IsAutoClose()
        {
            var result = string.Empty;
            var command = connection.CreateCommand();
            command.CommandText = "SELECT DATABASEPROPERTYEX ( @key , 'IsAutoClose' )";
            command.Parameters.AddWithValue("@key", DBName);
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read()) result = reader[0].ToString();
                reader.Close();
                return "1".Equals(result);
            }
            finally
            {
                command.Dispose();
            }
        }
    }
}