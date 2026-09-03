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
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AvePoint.GCommon.ComplianceDBWrapper.Common;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon;

namespace AvePoint.GCommon.ComplianceDBWrapper.Utility
{
    /// <summary>
    /// 继承自DBConnection,以后都用这个喔.
    /// </summary>
    public sealed class ConnectionInfo
    {

        #region - Private Params -

        private static AveLogger mLog = new AveLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _userID;

        public string UserID
        {
            get { return _userID; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {

                    string[] userInfo = value.Split("\\".ToCharArray());
                    if (userInfo.Length > 1)
                    {
                        Domain = userInfo[0];
                        _userID = userInfo[1];
                    }
                    else
                    {
                        _userID = value;
                    }
                }
            }
        }

        public string Domain { get; set; }

        public string Password { get; set; }

        public string InitialCatalog { get; set; }

        public SQLConnectionType SQLConnectionType { get; set; }

        public string DataSource { get; set; }

        public string ConnectionString { get; set; }

        public EDConnectioType Type { get; set; }

        public string FailoverPartner { get; set; }

        public bool IsLocalMachine { get; set; }

        #endregion

        public enum EDConnectioType
        {
            Normal,
            ConnectionString
        }

        private string _tables = "'CPLED_HeldData','CPLED_HoldItem','CPLED_Attachment','CPLED_HoldRelations','CPLED_SyncPoint','CPLED_PropertyMapping','CPLED_StorageInfo'";

        #region - 创建table logic -

        public CreateResult CreateTable()
        {
            CreateResult result = CreateResult.Success;
            AveImpersonator impersonator = null;
            SqlConnection conn = null;
            try
            {
                string connStr = GetConnString(true);
                if(SQLConnectionType == SQLConnectionType.WindowsAuthentication)
                {
                    impersonator = new AveImpersonator(this.Domain, this.UserID, this.Password,!IsLocalMachine, true);
                    impersonator.Impersonate();
                }
                conn = new SqlConnection(connStr);
                conn.Open();
                bool exist = ExistDatabaseName(conn); //检查Database Name 是否存在.
                if(exist)
                {
                    TablesState state = TablesState.Exist; //默认假装表已经存在.
                    try
                    {
                        UseDatabase(conn);
                        state = ExistTables(conn); //获得table在此database的状态.
                    }
                    catch(Exception ex)
                    {
                        mLog.Error("Get database status failed. {0}",ex);
                        conn.Close();
                        conn.Dispose();
                        if(impersonator != null)
                        {
                            impersonator.Dispose();
                        }
                        return CreateResult.UserNotHavePermission;
                    }
                    switch (state)
                    {
                        case TablesState.Exist:
                            if (!ValidataHeldDataTable(conn) || !ValidataHoldItemTable(conn) || !ValidataDataMappingTable(conn) ||
                               !ValidataAttachmentTable(conn) || !ValidataSyncPointTable(conn) || !ValidataPropertyMappingTable(conn) ||
                                !ValidataStorageInfoTable(conn))
                            {
                                result = CreateResult.TableExistButValidateFailed;
                            }
                            break;
                        case TablesState.NotFullyExist:
                            result = CreateResult.TableNotFullyExist;
                            break;
                        case TablesState.NotExist:
                            CreateTables(conn);
                            break;
                    }
                }
                else
                {
                    try
                    {
                        CreateDatabase(conn);
                    }
                    catch (Exception ex)
                    {
                        mLog.Error("Create database failed. {0}", ex);
                        if (impersonator != null)
                        {
                            impersonator.Dispose();
                        }
                        conn.Dispose();
                        conn.Close();
                        return CreateResult.UserNotHavePermission;
                    }
                    CreateTables(conn);
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Message: {0}", e);
                result = CreateResult.TableCreateFailed;
            }
            finally
            {
                if (impersonator != null)
                {
                    impersonator.Dispose();
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn.Close();
                }
            }

//            if(result == CreateResult.Success && !string.IsNullOrEmpty(FailoverPartner))
//            {
//                SqlConnection mirrorConnection = null;
//                AveImpersonator mirrorImpersonator = null;
//                try
//                {
//                    string mirrorConnectionString = GetConnString();
//                    if (SQLConnectionType == SQLConnectionType.WindowsAuthentication)
//                    {
//                        mirrorImpersonator = new AveImpersonator(this.Domain, this.UserID, this.Password, true);
//                        mirrorImpersonator.Impersonate();
//                    }
//                    mirrorConnection = new SqlConnection(mirrorConnectionString);
//                    mirrorConnection.Open();
//                }
//                catch (Exception e)
//                {
//                    mLog.Warn("Message : {0}.", e);
//                    result = CreateResult.MirrorServerFailed;
//                }
//                finally
//                {
//                    if (mirrorImpersonator != null)
//                    {
//                        mirrorImpersonator.Dispose();
//                    }
//                    if (mirrorConnection != null)
//                    {
//                        mirrorConnection.Dispose();
//                        mirrorConnection.Close();
//                    }
//                }
//            }
            return result;
        }

        #endregion

        #region - Get Database FileName -

        private string GetDatabaseFileName(string dbName, SqlConnection conn)
        {
            string dbPath = GetDataPath(conn);
            return Path.Combine(dbPath, dbName);
        }

        #endregion

        #region - Get Database Log File Name -

        private string GetDatabaseLogFileName(string dbName, SqlConnection conn)
        {
            string logPath = GetLogPath(conn);
            return Path.Combine(logPath, dbName) + "_log";
        }

        #endregion

        #region - Get Log Path -
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private string GetLogPath(SqlConnection conn)
        {
            string path = GetDefaultLogPath(conn);
            if (String.IsNullOrEmpty(path))
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandText = @"select* from master.dbo.sysdatabases where name='master'";
                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        path = reader["filename"].ToString();
                        if (path.Contains("master.mdf"))
                        {
                            path = path.Replace("master.mdf", "");
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    command.Dispose();
                }
            }
            return path;
        }

        #endregion

        #region - Get Default LogPath -
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private string GetDefaultLogPath(SqlConnection conn)
        {
            string path = string.Empty;
            SqlCommand command = conn.CreateCommand();
            command.CommandText = @"declare @DefaultLog nvarchar(512) exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultLog', @DefaultLog OUTPUT select @DefaultLog as LogPath";
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    path = reader["LogPath"].ToString();
                }
                reader.Close();
                return path;
            }
            finally
            {
                command.Dispose();
            }
        }

        #endregion

        #region - Create Table Method -
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private void CreateTables(SqlConnection conn)
        {
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand()) {CommandText = string.Format(tableCreateSql, InitialCatalog)};
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region - Validata Method -

        #region - 验证 Held Data Table -

        public bool ValidataHeldDataTable(SqlConnection conn)
        {
            bool flag = false;
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand())
                                   {
                                       CommandText =
                                           "SELECT name,xtype,length FROM syscolumns WHERE id in (SELECT id FROM sysobjects WHERE name='CPLED_HeldData')"
                                   };
            EDSqlDataReader reader = cmd.ExecuteReader();
            int columnCnt = 24;
            while(reader.Read)
            {
                bool validateSuccess = ValidateHeldDataColumn(reader.GetString(0), reader.GetByte(1), reader.GetSmallInt(2));
                if(!validateSuccess)
                {
                    break;
                }
                columnCnt--;
            }
            reader.Close();
            if(columnCnt == 0)
            {
                flag = true;
            }
            return flag;
        }

        public bool ValidateHeldDataColumn(string colName, long xtype, long len)
        {
            switch (colName)
            {
                case "ID":
                case "SPGuid":
                case "WebAppID":
                case "SiteID":
                case "WebID":
                case "ListID":
                    return xtype == 36 && len == 16;
                case "Name":
                case "CreateBy":
                    return xtype == 231 && len == 512;
                case "UniqueID":
                case "Version":
                case "DeviceID":
                case "FarmID":
                case "SubJobID":
                case "PathMD5":
                    return xtype == 231 && len == 256;
                case "DataSource":
                case "DataType":
                case "MarkState":
                case "IsCurrent":
                    return xtype == 56 && len == 4;
                case "Size":
                    return xtype == 127 && len == 8;
                case "ModifiedTime":
                    return xtype == 61 && len == 8;
                case "DisplayURL" :
                case "FileURL":
                case "MetaDataURL" :
                case "SiteURL":
                    return xtype == 231 && len == -1;
            }
            return false;
        }

        #endregion

        #region - 验证 Hold Item Table -

        public bool ValidataHoldItemTable(SqlConnection conn)
        {
            bool flag = false;
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand())
            {
                CommandText =
                    "SELECT name,xtype,length FROM syscolumns WHERE id in (SELECT id FROM sysobjects WHERE name='CPLED_HoldItem')"
            };
            EDSqlDataReader reader = cmd.ExecuteReader();
            int columnCnt = 15;
            while (reader.Read)
            {
                bool validateSuccess = ValidateHoldItemColumn(reader.GetString(0), reader.GetByte(1), reader.GetSmallInt(2));
                if (!validateSuccess)
                {
                    break;
                }
                columnCnt--;
            }
            reader.Close();
            if (columnCnt == 0)
            {
                flag = true;
            }
            return flag;
        }

        public bool ValidateHoldItemColumn(string colName, long xtype, long len)
        {
            switch (colName)
            {
                case "ID":
                case "WebAppID":
                case "SiteID":
                case "WebID":
                case "ListID":
                case "SPGuid":
                    return xtype == 36 && len == 16;
                case "Name":
                    return xtype == 231 && len == 512;
                case "Description":
                case "FullPath":
                case "ManagedBy":
                    return xtype == 231 && len == -1;
                case "ModifiedTime":
                    return xtype == 61 && len == 8;
                case "UniqueID":
                case "ParentID":
                case "FarmID":
                    return xtype == 231 && len == 256;
                case "Type":
                    return xtype == 56 && len == 4;
            }

            return false;
        }

        #endregion

        #region - 验证 Data Mapping Table -

        public bool ValidataDataMappingTable(SqlConnection conn)
        {
            bool flag = false;
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand())
            {
                CommandText = "SELECT name,xtype,length FROM syscolumns WHERE id in (SELECT id FROM sysobjects WHERE name='CPLED_HoldRelations')"
            };
            EDSqlDataReader reader = cmd.ExecuteReader();
            int columnCnt = 3;
            while (reader.Read)
            {
                bool validateSuccess = ValidateDataMappingColumn(reader.GetString(0), reader.GetByte(1), reader.GetSmallInt(2));
                if (!validateSuccess)
                {
                    break;
                }
                columnCnt--;
            }
            reader.Close();
            if (columnCnt == 0)
            {
                flag = true;
            }
            return flag;
        }

        public bool ValidateDataMappingColumn(string colName, long xtype, long len)
        {
            switch (colName)
            {
                case "ID":
                    return xtype == 36 && len == 16;
                case "DataUniqueID":
                case "ItemUniqueID":
                    return xtype == 231 && len == 256;
            }
            return false;
        }

        #endregion

        #region - 验证 Attachment Table -

        public bool ValidataAttachmentTable(SqlConnection conn)
        {
            bool flag = false;
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand())
            {
                CommandText =
                    "SELECT name,xtype,length FROM syscolumns WHERE id in (SELECT id FROM sysobjects WHERE name='CPLED_Attachment')"
            };
            EDSqlDataReader reader = cmd.ExecuteReader();
            int columnCnt = 4;
            while (reader.Read)
            {
                bool validateSuccess = ValidateAttachmentColumn(reader.GetString(0), reader.GetByte(1), reader.GetSmallInt(2));
                if (!validateSuccess)
                {
                    break;
                }
                columnCnt--;
            }
            reader.Close();
            if (columnCnt == 0)
            {
                flag = true;
            }
            return flag;
        }

        public bool ValidateAttachmentColumn(string colName, long xtype, long len)
        {
            switch (colName)
            {
                case "ID":
                    return xtype == 36 && len == 16;
                case "ItemUniqueID":
                case "DeviceID":
                    return xtype == 231 && len == 256;
                case "Name":
                    return xtype == 231 && len == 512;
            }
            return false;
        }

        #endregion

        #region - 验证 SyncPoint Table -

        public bool ValidataSyncPointTable(SqlConnection conn)
        {
            bool flag = false;
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand())
            {
                CommandText =
                    "SELECT name,xtype,length FROM syscolumns WHERE id in (SELECT id FROM sysobjects WHERE name='CPLED_SyncPoint')"
            };
            EDSqlDataReader reader = cmd.ExecuteReader();
            int columnCnt = 4;
            while (reader.Read)
            {
                bool validateSuccess = ValidateSyncPointColumn(reader.GetString(0), reader.GetByte(1), reader.GetSmallInt(2));
                if (!validateSuccess)
                {
                    break;
                }
                columnCnt--;
            }
            reader.Close();
            if (columnCnt == 0)
            {
                flag = true;
            }
            return flag;
        }

        public bool ValidateSyncPointColumn(string colName, long xtype, long len)
        {
            switch (colName)
            {
                case "FarmID":
                    return xtype == 231 && len == 256;
                case "WebAppID":
                    return xtype == 36 && len == 16;
                case "WebAppURL":
                    return xtype == 231 && len == -1;
                case "TimePoint":
                    return xtype == 61 && len == 8;
            }
            return false;
        }

        #endregion

        #region - 验证 PropertyMapping Table -

        public bool ValidataPropertyMappingTable(SqlConnection conn)
        {
            bool flag = false;
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand())
            {
                CommandText =
                    "SELECT name,xtype,length FROM syscolumns WHERE id in (SELECT id FROM sysobjects WHERE name='CPLED_PropertyMapping')"
            };
            EDSqlDataReader reader = cmd.ExecuteReader();
            int columnCnt = 9;
            while (reader.Read)
            {
                bool validateSuccess = ValidatePropertyMappingColumn(reader.GetString(0), reader.GetByte(1), reader.GetSmallInt(2));
                if (!validateSuccess)
                {
                    break;
                }
                columnCnt--;
            }
            reader.Close();
            if (columnCnt == 0)
            {
                flag = true;
            }
            return flag;
        }

        public bool ValidatePropertyMappingColumn(string colName, long xtype, long len)
        {
            switch (colName)
            {
                case "UniqueID":
                case "FarmID":
                    return xtype == 231 && len == 256;
                case "SSAID":
                    return xtype == 36 && len == 16;
                case "CurrentCrawlProperty":
                case "CurrentManagedProperty":
                case "VersionCrawlProperty":
                case "VersionManagedProperty":
                    return xtype == 231 && len == -1;
                case "FieldName":
                    return xtype == 231 && len == 512;
                case "FieldType":
                    return xtype == 231 && len == 200;
            }
            return false;
        }

        #endregion

        #region - 验证 StorageInfo Table -

        public bool ValidataStorageInfoTable(SqlConnection conn)
        {
            bool flag = false;
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand())
            {
                CommandText =
                    "SELECT name,xtype,length FROM syscolumns WHERE id in (SELECT id FROM sysobjects WHERE name='CPLED_StorageInfo')"
            };
            EDSqlDataReader reader = cmd.ExecuteReader();
            int columnCnt = 11;
            while (reader.Read)
            {
                bool validateSuccess = ValidateStorageInfoColumn(reader.GetString(0), reader.GetByte(1), reader.GetSmallInt(2));
                if (!validateSuccess)
                {
                    break;
                }
                columnCnt--;
            }
            reader.Close();
            if (columnCnt == 0)
            {
                flag = true;
            }
            return flag;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HightName is a protocol.")]
        public bool ValidateStorageInfoColumn(string colName, long xtype, long len)
        {
            switch (colName)
            {
                
                case "DataID":
                    return xtype == 231 && len == 256;
                case "ID":
                    return xtype == 36 && len == 16;
                case "DataType":
                case "Type":
                case "DataVersion":
                    return xtype == 56 && len == 4;
                case "Offset":
                case "Length":
                    return xtype == 127 && len == 8;
                case "HightName":
                case "LowName":
                    return xtype == 231 && len == 512;
                case "ExtraInfo":
                case "ClipID":
                    return xtype == 231 && len == -1;
            }
            return false;
        }

        #endregion

        #endregion

        #region - 根据ConnInfo信息,对DB进行连接测试 -

        public bool Test()
        {       
            AveImpersonator impersonator = null;
            SqlConnection conn = null;
            try
            {   
                string connStr = GetConnString(true);
                if(SQLConnectionType == SQLConnectionType.WindowsAuthentication)
                {
                    impersonator = new AveImpersonator(Domain, UserID, Password,!IsLocalMachine, true);
                    impersonator.Impersonate();
                }
                conn = new SqlConnection(connStr);
                conn.Open();
            }
            catch (Exception e)
            {
                mLog.Warn("Message : {0}", e.StackTrace);
                return false;
            }
            finally
            {
                if (impersonator != null)
                {
                    impersonator.Dispose();
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn.Close();
                }
            }
            bool dbTest = true;
            if (!string.IsNullOrEmpty(this.FailoverPartner))
            {
                var cloneObj = this.Clone();
                cloneObj.FailoverPartner = string.Empty;
                cloneObj.DataSource = this.FailoverPartner;
                if (this.Type == EDConnectioType.ConnectionString)
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(this.ConnectionString);
                    builder.FailoverPartner = string.Empty;
                    cloneObj.ConnectionString = builder.ToString();
                }
                dbTest = cloneObj.Test();
            }
            return dbTest;
        }

        #endregion

        #region - Clone -

        public ConnectionInfo Clone()
        {
            ConnectionInfo cloneObj = new ConnectionInfo
                                          {
                                              ConnectionString = ConnectionString,
                                              DataSource = DataSource,
                                              Domain = Domain,
                                              FailoverPartner = FailoverPartner,
                                              InitialCatalog = InitialCatalog,
                                              Password = Password,
                                              SQLConnectionType = SQLConnectionType,
                                              Type = Type,
                                              UserID = UserID
                                          };
            return cloneObj;
        }

        #endregion

        #region - 获得Conn String连接 -

        public string GetConnString(bool isTest = false)
        {
            string connStr = string.Empty;
            if(Type == EDConnectioType.Normal)
            {
                connStr = NormalStringBuilder(isTest);
            }
            else if (Type == EDConnectioType.ConnectionString)
            {
                SqlConnectionStringBuilder tempBuilder = new SqlConnectionStringBuilder(ConnectionString);
                InitialCatalog = tempBuilder.InitialCatalog;
                FailoverPartner = tempBuilder.FailoverPartner;
                if(isTest)
                {
                    tempBuilder.InitialCatalog = string.Empty;
                    tempBuilder.FailoverPartner = string.Empty;
                }
                tempBuilder.Pooling = false;
                connStr = tempBuilder.ToString();
            }
            return connStr;
        }

        private string NormalStringBuilder(bool isTest = false)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
            if (SQLConnectionType == SQLConnectionType.SqlAuthentication)
            {
                connectionStringBuilder = new SqlConnectionStringBuilder
                {
                    UserID = UserID,
                    Password = Password,
                    DataSource = DataSource,
                    Pooling = false
                };
            }
            else if (SQLConnectionType == SQLConnectionType.WindowsAuthentication)
            {
                connectionStringBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = DataSource,
                    IntegratedSecurity = true,
                    Pooling = false
                };

            }
            if(!string.IsNullOrEmpty(FailoverPartner) && !isTest)
            {
                connectionStringBuilder.FailoverPartner = FailoverPartner;
            }
            if (!string.IsNullOrEmpty(DataSource) && !isTest)
            {
                connectionStringBuilder.InitialCatalog = InitialCatalog;
            }
            return connectionStringBuilder.ToString();
        }

        #endregion

        #region - Private Methods -

        #region - 验证DatabaseName是否存在 -

        private bool ExistDatabaseName(SqlConnection conn)
        {
            #region - execute sql -

            const string executeSql = @"SELECT COUNT(*) FROM sysdatabases With(noLock) WHERE name=@Name";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@Name",InitialCatalog);
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        #endregion

        #region - 使用data base -

        private void UseDatabase(SqlConnection conn)
        {
            EDSqlCommand cmd = new EDSqlCommand(conn.CreateCommand()) { CommandText = string.Format("USE [{0}]", InitialCatalog) };
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region - Format Database Name -

        private string FormatDBName(string dbName)
        {
            if (dbName.Contains("]"))
            {
                return dbName.Replace("]", "]]");
            }
            return dbName;
        }

        #endregion

        #region - replace Illegal Characters -

        private string ReplaceIllegalCharacters(string illegalString)
        {
            string legalString = illegalString;
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

        #endregion

        #region - Get Default DataPath -

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HightName is a protocol.")]
        private string GetDefaultDataPath(SqlConnection conn)
        {
            string path = string.Empty;
            SqlCommand command = conn.CreateCommand();
            command.CommandText = @"declare @DefaultData nvarchar(512) exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData', @DefaultData OUTPUT select @DefaultData as DataPath";
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    path = reader["DataPath"].ToString();
                }
                reader.Close();
                return path;
            }
            finally
            {
                command.Dispose();
            }
        }

        #endregion

        #region - Get DataPath -
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private string GetDataPath(SqlConnection conn)
        {
            string path = GetDefaultDataPath(conn);
            if (String.IsNullOrEmpty(path))
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandText = @"select* from master.dbo.sysdatabases where name='master'";
                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        path = reader["filename"].ToString();
                        if (path.Contains("master.mdf"))
                        {
                            path = path.Replace("master.mdf", "");
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    command.Dispose();
                }
            }
            return path;
        }

        #endregion

        #region - 创建Database -

        /// <summary>
        /// 创建database
        /// </summary>
        /// <param name="conn"></param>
        private void CreateDatabase(SqlConnection conn)
        {
            string dbName = ReplaceIllegalCharacters(InitialCatalog);
            string dbFileName = GetDatabaseFileName(InitialCatalog, conn);
            string dbLogName = GetDatabaseLogFileName(InitialCatalog, conn);
            string name = InitialCatalog;
            name = FormatDBName(name);
            SqlCommand command = conn.CreateCommand();
            string commandString =
                String.Format(
                    @"CREATE DATABASE [{0}] ON  PRIMARY ( NAME = N'{1}', FILENAME = N'{2}.mdf') LOG ON ( NAME = N'{3}_log', FILENAME = N'{4}.LDF') alter database [{5}] collate Latin1_General_CI_AS_KS_WS alter database [{6}] set recovery simple,auto_close off",
                    name,
                    dbName,
                    dbFileName,
                    dbName,
                    dbLogName,
                    name,
                    name);
            command.CommandText = commandString;
            command.CommandTimeout = 0;
            try
            {
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Dispose();
            }
        }

        #endregion

        private TablesState ExistTables(SqlConnection conn)
        {
            TablesState state = TablesState.NotFullyExist;
            SqlCommand sqlCmd = conn.CreateCommand();
            sqlCmd.CommandText = string.Format("Select Count(name) from sysobjects where xtype = 'U' and name in ({0})",_tables);
            int existCnt = (int)sqlCmd.ExecuteScalar();
            if (existCnt == 7)
            {
                state = TablesState.Exist;
            }
            else if(existCnt == 0)
            {
                state = TablesState.NotExist;
            }
            return state;
        }

        #endregion

        #region - Private Param - 

        /// <summary>
        /// Tables存在于CPL DB中的状态.
        /// 1. Exist 全部存在.
        /// 2. Not Fully Exist 不完全存在.
        /// 3. Not Exist 完全不存在.
        /// </summary>
        private enum TablesState
        {
            Exist = 0,
            NotFullyExist = 1,
            NotExist = 2
        }

        #endregion

        #region - Sqls -

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HightName is a protocol.")]
        private const string tableCreateSql = @"USE [{0}];

                                                CREATE TABLE [CPLED_HeldData]
                                                (
	                                                [ID] UNIQUEIDENTIFIER NOT NULL,
	                                                [Name] NVARCHAR(256) NOT NULL,
	                                                [UniqueID] NVARCHAR(128) NOT NULL, 
	                                                [DataSource] INT NOT NULL DEFAULT 0,
	                                                [DataType] INT NOT NULL DEFAULT 0,
	                                                [Size] BIGINT DEFAULT 0,
	                                                [CreateBy] NVARCHAR(256),
	                                                [MarkState] INT DEFAULT 0,
	                                                [IsCurrent] INT DEFAULT 0,
	                                                [Version] NVARCHAR(128),
	                                                [ModifiedTime] DATETIME,
	                                                [DisplayURL] NVARCHAR(MAX) NOT NULL,
	                                                [FileURL] NVARCHAR(MAX),
	                                                [MetaDataURL] NVARCHAR(MAX),
	                                                [DeviceID] NVARCHAR(128) NOT NULL,
	                                                [FarmID] NVARCHAR(128) NOT NULL,
	                                                [SPGuid] UNIQUEIDENTIFIER,
	                                                [WebAppID] UNIQUEIDENTIFIER,
	                                                [SiteID] UNIQUEIDENTIFIER,
	                                                [WebID] UNIQUEIDENTIFIER,
	                                                [ListID] UNIQUEIDENTIFIER,
                                                    [SubJobID] NVARCHAR(128),
                                                    [SiteURL] NVARCHAR(MAX),
                                                    [PathMD5] NVARCHAR(128)  
                                                );

                                                CREATE TABLE [CPLED_HoldItem]
                                                (
	                                                [ID] UNIQUEIDENTIFIER NOT NULL,
	                                                [Name] NVARCHAR(256) NOT NULL,
	                                                [Description] NVARCHAR(MAX),
	                                                [ModifiedTime] DateTime NOT NULL,
	                                                [ManagedBy] NVARCHAR(MAX),
	                                                [FullPath] NVARCHAR(MAX),
	                                                [UniqueID] NVARCHAR(128),
	                                                [Type] INT NOT NULL DEFAULT 0,
	                                                [ParentID] NVARCHAR(128),
	                                                [FarmID] NVARCHAR(128) NOT NULL,
	                                                [WebAppID] UNIQUEIDENTIFIER,
	                                                [SiteID] UNIQUEIDENTIFIER,
	                                                [WebID] UNIQUEIDENTIFIER,
	                                                [ListID] UNIQUEIDENTIFIER,
                                                    [SPGuid] UNIQUEIDENTIFIER 
                                                );

                                                CREATE TABLE [CPLED_Attachment]
                                                (
	                                                [ID] UNIQUEIDENTIFIER NOT NULL,
	                                                [ItemUniqueID] NVARCHAR(128) NOT NULL,
	                                                [Name] NVARCHAR(256) NOT NULL,
	                                                [DeviceID] NVARCHAR(128) NOT NULL
                                                );

                                                CREATE TABLE [CPLED_HoldRelations]
                                                (
	                                                [ID] UNIQUEIDENTIFIER NOT NULL,
	                                                [DataUniqueID] NVARCHAR(128) NOT NULL,
	                                                [ItemUniqueID] NVARCHAR(128) NOT NULL
                                                );

                                                CREATE TABLE [CPLED_SyncPoint]
                                                (
	                                                [FarmID] NVARCHAR(128) NOT NULL,
	                                                [WebAppID] UNIQUEIDENTIFIER NOT NULL,
	                                                [WebAppURL] NVARCHAR(max) NULL,
	                                                [TimePoint] DateTime NOT NULL
                                                );
                                                
                                                CREATE TABLE [CPLED_PropertyMapping]
                                                (
	                                                [UniqueID] NVARCHAR(128) NOT NULL,
	                                                [FarmID] NVARCHAR(128) NOT NULL,
	                                                [SSAID] UNIQUEIDENTIFIER NOT NULL,
	                                                [CurrentCrawlProperty] NVARCHAR(MAX),
	                                                [CurrentManagedProperty] NVARCHAR(MAX),
	                                                [VersionCrawlProperty] NVARCHAR(MAX),
	                                                [VersionManagedProperty] NVARCHAR(MAX),
	                                                [FieldName] NVARCHAR(256) NOT NULL,
	                                                [FieldType] NVARCHAR(100) NOT NULL,
                                                );
                                                
                                                CREATE TABLE [CPLED_StorageInfo]
                                                (
                                                    [ID] UNIQUEIDENTIFIER NOT NULL,
                                                    [DataID] NVARCHAR(128) NOT NULL,
                                                    [DataType] INT NOT NULL DEFAULT 0,
                                                    [Type] INT NOT NULL DEFAULT 0, 
                                                    [Offset] BIGINT NOT NULL,
                                                    [HightName] NVARCHAR(256),
                                                    [LowName] NVARCHAR(256),
                                                    [Length] BIGINT DEFAULT 0,
                                                    [DataVersion] INT DEFAULT 0,
                                                    [ExtraInfo] NVARCHAR(MAX),
                                                    [ClipID] NVARCHAR(MAX)  
                                                );
                
                                                ALTER TABLE [CPLED_HeldData]
                                                ADD CONSTRAINT [PK_CPLED_HeldData] PRIMARY KEY CLUSTERED ([ID] ASC);

                                                ALTER TABLE [CPLED_HoldItem]
                                                ADD CONSTRAINT [PK_CPLED_HoldItem] PRIMARY KEY CLUSTERED ([ID] ASC);

                                                ALTER TABLE [CPLED_HoldRelations]
                                                ADD CONSTRAINT [PK_CPLED_HoldRelations] PRIMARY KEY CLUSTERED ([ID] ASC); ";

        #endregion
    }

    #region - create result -

    public enum CreateResult
    {
        Success = 0,
        TableExistButValidateFailed = 1,
        TableNotFullyExist = 2,
        TableCreateFailed = 3,
        UserNotHavePermission = 4,
        MirrorServerFailed = 5
    }
    
    #endregion

    public enum SQLConnectionType
    {
        SqlAuthentication = 0,
        WindowsAuthentication = 1
    }
}
