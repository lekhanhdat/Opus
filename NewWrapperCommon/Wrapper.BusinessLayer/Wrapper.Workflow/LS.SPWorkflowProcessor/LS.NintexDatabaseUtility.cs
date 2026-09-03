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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.SPWorkflowProcessor
{
    internal class NintexDatabaseUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static NintexDatabaseInvoker nintexDBInvoker = new NintexDatabaseInvoker();

        public static string GetConfigDBConnectionString()
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetConfigDBConnectionString");
            try
            {
                string configDBConnectionString = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateFarm().Local.Properties["NW2007ConfigurationDatabase"] as string;
                if (!string.IsNullOrEmpty(configDBConnectionString))
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ConfigDBConnString, configDBConnectionString);
                    return configDBConnectionString;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_GetConfigDBException, e.Message);
                logger.Warn("An exception occurred while get nintex workflow configuration database. exception:{0}", e.ToString());
                return null;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetConfigDBConnectionString");
            }
        }

        public static void DeleteNintexPublishedWorkflowRecord(Guid workflowId, Guid siteId, Guid webapplicationId)
        {
            int result = -2;
            try
            {
                
                var connStr = GetConfigDBConnectionString();
                if (!string.IsNullOrEmpty(connStr))
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string commandText = "IF OBJECT_ID (N'Workflows', N'U') IS NOT NULL  delete from Workflows where  SiteId=@SiteId and WorkflowId=@workflowId and WebApplicationId=@WebApplicationId";
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = commandText;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@SiteId", siteId);
                        cmd.Parameters.AddWithValue("@workflowId", workflowId);
                        cmd.Parameters.AddWithValue("@webapplicationId", webapplicationId);
                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("DeleteNintexPublishedWorkflowRecord faile.Error:{0}", e);
            }
            logger.Info("DeleteNintexPublishedWorkflowRecord result:{0}", result);
        }

        public static string GetContentDBConnectionString(Guid siteId, string configDBConnectionString = null)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetContentDBConnectionString");
            int siteCount = 0;
            int id = -1;
            SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder();
            try
            {
                if (SPWorkflowProcessorRuntime.NintexConfigDBConnection == null)
                {
                    configDBConnectionString = string.IsNullOrEmpty(configDBConnectionString) ? GetConfigDBConnectionString() : configDBConnectionString;
                    if (string.IsNullOrEmpty(configDBConnectionString))
                    {
                        return null;
                    }
                    SPWorkflowProcessorRuntime.NintexConfigDBConnection = new SqlConnection(configDBConnectionString ?? GetConfigDBConnectionString());//(SqlConnection connection = nintexDBInvoker.OpenConfigDataBase())//                
                    SPWorkflowProcessorRuntime.NintexConfigDBConnection.Open();
                }
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = SPWorkflowProcessorRuntime.NintexConfigDBConnection;
                    command.CommandText = "GetStorageDatabase";
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter parameter = new SqlParameter("@SiteID", SqlDbType.UniqueIdentifier);
                    parameter.Value = siteId;
                    command.Parameters.Add(parameter);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sqlConnectionString.DataSource = (reader["ServerName"] == null ? string.Empty : reader["ServerName"].ToString());
                            sqlConnectionString.InitialCatalog = (reader["DatabaseName"] == null ? string.Empty : reader["DatabaseName"].ToString());
                            sqlConnectionString.IntegratedSecurity = (bool)reader["UseIntegrated"];
                            sqlConnectionString.UserID = (reader["Username"] == null ? string.Empty : reader["Username"].ToString());
                            sqlConnectionString.Password = (reader["Password"] == null ? string.Empty : reader["Password"].ToString());
                            siteCount = (int)reader["SiteCount"];
                            id = (int)reader["DatabaseID"];
                        }
                    }
                }
                return sqlConnectionString.ConnectionString;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_GetContentDBException, e.Message);
                logger.Warn("An exception occurred while get nintex workflow content database. {0}", e.ToString());
                return null;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetContentDBConnectionString");
            }
        }

        private static object GetContentDatabase(Guid siteId)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetContentDatabase");
            int siteCount = 0;
            int id = -1;
            SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder();
            try
            {
                using (SqlConnection connection = new SqlConnection(GetConfigDBConnectionString()))//(SqlConnection connection = nintexDBInvoker.OpenConfigDataBase())//
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "GetStorageDatabase";
                        command.CommandType = CommandType.StoredProcedure;
                        SqlParameter parameter = new SqlParameter("@SiteID", SqlDbType.UniqueIdentifier);
                        parameter.Value = siteId;
                        command.Parameters.Add(parameter);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sqlConnectionString.DataSource = (reader["ServerName"] == null ? string.Empty : reader["ServerName"].ToString());
                                sqlConnectionString.InitialCatalog = (reader["DatabaseName"] == null ? string.Empty : reader["DatabaseName"].ToString());
                                sqlConnectionString.IntegratedSecurity = (bool)reader["UseIntegrated"];
                                sqlConnectionString.UserID = (reader["Username"] == null ? string.Empty : reader["Username"].ToString());
                                sqlConnectionString.Password = (reader["Password"] == null ? string.Empty : reader["Password"].ToString());
                                siteCount = (int)reader["SiteCount"];
                                id = (int)reader["DatabaseID"];
                            }
                        }
                    }
                    connection.Close();
                }
                //return (ContentDatabase)LSInvoker.CreateNewInstance(typeof(ContentDatabase),
                //    new Type[] { typeof(SqlConnectionStringBuilder), typeof(int), typeof(int) },
                //    new object[] { sqlConnectionString, siteCount, id });
                return sqlConnectionString.ConnectionString;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_GetContentDBException, e.Message);
                logger.Warn("An exception occurred while get nintex workflow content database. exception:{0}", e.ToString());
                return null;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetContentDatabase");
            }
        }

        internal static void ExecuteUpdateCommand(SqlCommand cmd, string commandText, string conditionParams, string excludeParams, Hashtable data)
        {
            StringBuilder keyEx = new StringBuilder();
            StringBuilder cmdParam = new StringBuilder();
            bool isFirstParam = true;
            conditionParams = conditionParams.ToLower(CultureInfo.InvariantCulture);
            excludeParams = excludeParams.ToLower(CultureInfo.InvariantCulture);

            cmd.Parameters.Clear();
            foreach (DictionaryEntry de in data)
            {
                string key = (string)de.Key;
                string key2 = key.Substring(1);
                keyEx.Remove(0, keyEx.Length);
                keyEx.Append(",");
                keyEx.Append(key2.ToLower(CultureInfo.InvariantCulture));
                keyEx.Append(",");
                if (excludeParams.IndexOf(keyEx.ToString(), StringComparison.Ordinal) >= 0)
                    continue;

                if (data[key] == null)
                    cmd.Parameters.AddWithValue("@" + key2, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@" + key2, data[key]);

                if (conditionParams.IndexOf(keyEx.ToString(), StringComparison.Ordinal) >= 0)
                    continue;

                if (!isFirstParam)
                    cmdParam.Append(",");
                else
                    isFirstParam = false;
                cmdParam.Append(key2);
                cmdParam.Append("=");
                cmdParam.Append("@");
                cmdParam.Append(key2);
            }

            if (cmdParam.Length > 0)
            {
                cmd.CommandText = string.Format(commandText, cmdParam.ToString());
                cmd.ExecuteNonQuery();
            }
        }

        internal static void ExecuteActivityActionsInsertCommand(SqlCommand cmd, Hashtable data)
        {
            List<string> fieldNames = new List<string>();
            cmd.CommandText = "SELECT Name FROM syscolumns WHERE ID=OBJECT_ID('ActivityActivation')";
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    fieldNames.Add(sdr.GetString(0));
                }
            }
            StringBuilder cmdParam = new StringBuilder();
            StringBuilder cmdKeys = new StringBuilder();
            StringBuilder cmdValues = new StringBuilder();
            cmdParam.Append("INSERT INTO ");
            cmdParam.Append("ActivityActivation");
            cmdParam.Append("(");

            bool isFirstParam = true;
            foreach (string field in fieldNames)
            {
                string nameEx = "#" + field;
                if ((!data.ContainsKey(nameEx) || data[nameEx] == null))
                {
                    continue;
                }
                else
                {
                    if (isFirstParam)
                    {
                        isFirstParam = false;
                    }
                    else
                    {
                        cmdKeys.Append(",");
                        cmdValues.Append(",");
                    }
                    cmdKeys.Append(field);
                    cmdValues.Append("@");
                    cmdValues.Append(field);
                    cmd.Parameters.AddWithValue("@" + field, data[nameEx]);
                }
            }

            cmdParam.Append(cmdKeys);
            cmdParam.Append(") VALUES (");
            cmdParam.Append(cmdValues);
            cmdParam.Append(")");

            cmd.CommandText = cmdParam.ToString();
            cmd.ExecuteNonQuery();
        }

        internal static void ExecuteInsertCommand(SqlCommand cmd, string tableName, Hashtable data)
        {
            bool isHasIDENTITY_INSERT = true;
            try
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "Select OBJECTPROPERTY(OBJECT_ID('" + tableName + "'),'TableHasIdentity')";
                if ((int)cmd.ExecuteScalar() == 0)
                {
                    isHasIDENTITY_INSERT = false;
                }
                else
                {
                    isHasIDENTITY_INSERT = true;
                }
                List<string> fieldNames = new List<string>();
                if (isHasIDENTITY_INSERT)
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SET IDENTITY_INSERT " + tableName + " ON";
                    cmd.ExecuteNonQuery();
                }
                cmd.CommandText = "SELECT Name FROM syscolumns WHERE ID=OBJECT_ID('" + tableName + "')";
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        fieldNames.Add(sdr.GetString(0));
                    }
                }


                StringBuilder cmdParam = new StringBuilder();
                StringBuilder cmdKeys = new StringBuilder();
                StringBuilder cmdValues = new StringBuilder();
                cmdParam.Append("INSERT INTO ");
                cmdParam.Append(tableName);
                cmdParam.Append("(");

                bool isFirstParam = true;
                foreach (string field in fieldNames)
                {
                    string nameEx = "#" + field;
                    if (!data.ContainsKey(nameEx) || data[nameEx] == null)
                    {
                        if (isFirstParam)
                        {
                            isFirstParam = false;
                        }
                        else
                        {
                            cmdKeys.Append(",");
                            cmdValues.Append(",");
                        }
                        cmdKeys.Append(field);
                        cmdValues.Append("NULL");
                    }
                    else
                    {
                        if (isFirstParam)
                        {
                            isFirstParam = false;
                        }
                        else
                        {
                            cmdKeys.Append(",");
                            cmdValues.Append(",");
                        }
                        cmdKeys.Append(field);
                        cmdValues.Append("@");
                        cmdValues.Append(field);
                        cmd.Parameters.AddWithValue("@" + field, data[nameEx]);
                    }
                }

                cmdParam.Append(cmdKeys);
                cmdParam.Append(") VALUES (");
                cmdParam.Append(cmdValues);
                cmdParam.Append(")");

                cmd.CommandText = cmdParam.ToString();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (isHasIDENTITY_INSERT)
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SET IDENTITY_INSERT " + tableName + " OFF";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void SetPropsFromDataReader(SqlDataReader sdr, int startIndex, Hashtable properties)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetPropsFromDataReader");
            try
            {
                int fieldCount = sdr.FieldCount;
                int i;
                for (i = startIndex; i < fieldCount; ++i)
                {
                    if (sdr.IsDBNull(i))
                        continue;
                    StringBuilder b1 = new StringBuilder();
                    b1.Append("#");
                    b1.Append(sdr.GetName(i));
                    properties.AddEx(b1.ToString(), sdr.GetValue(i));
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_GetPropertiesFromReaderException, e.Message);
                logger.Warn("An exception occurred while get properties from reader. exception:{0}", e.ToString());
                throw new SPWFProcessorException(SPWFProcessorErrorCode.SetPropsFromDataReaderError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetPropsFromDataReader");
            }
        }
    }

    internal class NintexDatabaseInvoker
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string NintexAssemblyName = @"Nintex.Workflow.dll";
        private static Assembly nintexAssembly = null;
        private static Type configurationDatabase = null;

        public NintexDatabaseInvoker()
        {
            try
            {
                nintexAssembly = Assembly.LoadFile(AveEnv.AgentRootFolder + NintexAssemblyName);
                configurationDatabase = nintexAssembly.GetType("Nintex.Workflow.Administration.ConfigurationDatabase", true);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.INFO, WrapperWorkflowResource.NintexDBLoadAssemblyError, ex.Message);
            }
        }
        private object GetConfigurationDatabase()
        {
            return LSInvoker.CallStaticMethod(configurationDatabase, "GetConfigurationDatabase");
        }

        internal SqlConnectionStringBuilder GetConnectionString()
        {
            return GetConnectionString(GetConfigurationDatabase());
        }
        internal SqlConnectionStringBuilder GetConnectionString(object configDB)
        {
            if (configDB != null)
            {
                return LSInvoker.CallMethod(configDB, "SQLConnectionString") as SqlConnectionStringBuilder;
            }
            return null;
        }

        internal SqlConnection OpenConfigDataBase()
        {
            SqlConnection conn = LSInvoker.CallStaticMethod(configurationDatabase, "OpenConfigDataBase") as SqlConnection;
            return conn;
        }
        internal Type GetContentdatabaseType()
        {
            return nintexAssembly.GetType("Nintex.Workflow.Administration.ContentDatabase");
        }
    }

    internal class NintexMessageTemplateHelper : IDisposable
    {
        private SqlConnection mConn;
        private static bool mFarmLevelIsRestored = false;
        private Dictionary<Guid, List<string>> mWebMessageTemplates = new Dictionary<Guid, List<string>>();
        private Dictionary<Guid, List<string>> mSiteMessageTemplates = new Dictionary<Guid, List<string>>();
        private List<string> mFarmLevelMessageTemplates = new List<string>();
        internal NintexMessageTemplateHelper(Guid siteId)
        {
            mConn = new SqlConnection();
            if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWConfigDBConnectionStringOfBackup"))
            {
                mConn.ConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWConfigDBConnectionStringOfBackup"];
            }
            else
            {
                mConn.ConnectionString = NintexDatabaseUtility.GetConfigDBConnectionString();
            }
            mConn.Open();
        }

        internal void BackupMessageTemplates(Guid webId, List<Hashtable> data, Guid siteId)
        {
            BackupFarmMessageTemplates(data);
            BackupSiteMessageTemplates(data, siteId);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.CommandText = "SELECT * FROM MessageTemplates WITH(NOLOCK) WHERE WebID=@WebID";
            cmd.Parameters.AddWithValue("@WebID", webId);
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    Hashtable ht = new Hashtable();
                    NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                    data.Add(ht);
                }
            }
        }

        private void BackupSiteMessageTemplates(List<Hashtable> data, Guid siteId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.Parameters.AddWithValue("@SiteId", siteId);
            cmd.CommandText = "SELECT * FROM MessageTemplates WITH(NOLOCK) WHERE SiteID = @SiteId AND WebID IS NULL";
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    Hashtable ht = new Hashtable();
                    NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                    data.Add(ht);
                }
            }
        }

        private void BackupFarmMessageTemplates(List<Hashtable> data)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.CommandText = "SELECT * FROM MessageTemplates WITH(NOLOCK) WHERE SiteID IS NULL AND WebID IS NULL";
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    Hashtable ht = new Hashtable();
                    NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                    data.Add(ht);
                }
            }
        }



        internal void RestoreMessageTemplates(Guid siteId, Guid webId, List<Hashtable> data)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            foreach (Hashtable ht in data)
            {
                if (!ht.Contains("#SiteID"))
                {
                    RestoreFarmMessageTemplates(ht);
                }
                else if (!ht.Contains("#WebID"))
                {
                    RestoreSiteMessageTemplates(ht, siteId);
                }
                else
                {
                    string header = (string)ht["#Header"];
                    if (!mWebMessageTemplates.ContainsKey(webId) || (mWebMessageTemplates.ContainsKey(webId) && mWebMessageTemplates[webId].Count == 0))
                    {
                        //cmd.CommandText = "SELECT COUNT(*) FROM MessageTemplates WHERE TemplateID=@TemplateID AND WebID=@WebID AND Header=@Header";
                        cmd.CommandText = "SELECT Header FROM MessageTemplates WITH(NOLOCK) WHERE WebID=@WebID";
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@WebID", webId);

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                if (mWebMessageTemplates.ContainsKey(webId))
                                {
                                    mWebMessageTemplates[webId].Add((string)sdr.GetValue(0));
                                }
                                else
                                {
                                    mWebMessageTemplates.Add(webId, new List<string>());
                                    mWebMessageTemplates[webId].Add((string)sdr.GetValue(0));
                                }

                            }
                        }
                    }

                    if (mWebMessageTemplates.ContainsKey(webId) && mWebMessageTemplates[webId].Contains(header))
                    {
                        if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                        {
                            NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE MessageTemplates SET {0} WHERE TemplateID=@TemplateID AND WebID=@WebID", ",TemplateID,WebID,", ",TemplateID,SiteID,WebID,", ht);
                        }
                    }
                    else
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT TOP(1) TemplateID FROM MessageTemplates WITH(NOLOCK) ORDER BY TemplateID DESC";
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.Read())
                            {
                                ht["#TemplateID"] = sdr.GetInt32(0) + 1;
                            }
                        }
                        ht["#SiteID"] = siteId;
                        ht["#WebID"] = webId;
                        NintexDatabaseUtility.ExecuteInsertCommand(cmd, "MessageTemplates", ht);
                    }

                }
            }
            mFarmLevelIsRestored = true;
        }

        private void RestoreSiteMessageTemplates(Hashtable ht, Guid siteId)
        {
            if (!mFarmLevelIsRestored)
            {
                string header = (string)ht["#Header"];
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.CommandText = "SELECT Header FROM MessageTemplates WITH(NOLOCK) WHERE SiteID = @SiteId AND WebID IS NULL";
                if (mSiteMessageTemplates.Count == 0)
                {
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            if (mSiteMessageTemplates.ContainsKey(siteId))
                            {
                                mSiteMessageTemplates[siteId].Add((string)sdr.GetValue(0));
                            }
                            else
                            {
                                mSiteMessageTemplates.Add(siteId, new List<string>());
                                mSiteMessageTemplates[siteId].Add((string)sdr.GetValue(0));
                            }
                        }
                    }
                }
                if (mSiteMessageTemplates.ContainsKey(siteId) && mSiteMessageTemplates[siteId].Contains(header))
                {
                    if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                    {
                        NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE MessageTemplates SET {0} WHERE TemplateID=@TemplateID AND SiteID=@SiteID AND WebID IS NULL", ",TemplateID,SiteID,", ",TemplateID,SiteID,WebID,", ht);
                    }
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) TemplateID FROM MessageTemplates WITH(NOLOCK) ORDER BY TemplateID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            ht["#TemplateID"] = sdr.GetInt32(0) + 1;
                        }
                    }
                    ht["#SiteID"] = siteId;
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "MessageTemplates", ht);
                }
            }
        }

        private void RestoreFarmMessageTemplates(Hashtable ht)
        {
            if (!mFarmLevelIsRestored)
            {
                string header = (string)ht["#Header"];
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                cmd.CommandText = "SELECT Header FROM MessageTemplates WITH(NOLOCK) WHERE SiteID IS NUll AND WebID IS NULL";
                cmd.Parameters.Clear();

                if (mFarmLevelMessageTemplates.Count == 0)
                {
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            string tempValue = (string)sdr.GetValue(0);
                            if (!mFarmLevelMessageTemplates.Contains(tempValue))
                            {
                                mFarmLevelMessageTemplates.Add(tempValue);
                            }
                        }
                    }
                }

                if (mFarmLevelMessageTemplates.Contains(header))
                {
                    if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                    {
                        NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE MessageTemplates SET {0} WHERE TemplateID=@TemplateID AND SiteID IS NUll AND WebID IS NULL", ",TemplateID,", ",TemplateID,SiteID,WebID,", ht);
                    }
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) TemplateID FROM MessageTemplates WITH(NOLOCK) ORDER BY TemplateID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            ht["#TemplateID"] = sdr.GetInt32(0) + 1;
                        }
                    }
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "MessageTemplates", ht);
                }

            }
        }
        public void Dispose()
        {
            mConn.Dispose();
        }
    }

    internal class NintexWorkflowConstantHelper : IDisposable
    {
        private SqlConnection mConn;
        private static bool mFarmLevelIsRestored = false;
        internal NintexWorkflowConstantHelper(Guid siteId)
        {
            //if (SPWorkflowProcessorRuntime.NintexConfigDBConnection == null)
            //{
            //    SPWorkflowProcessorRuntime.NintexConfigDBConnection = new SqlConnection(NintexDatabaseUtility.GetConfigDBConnectionString());
            //    SPWorkflowProcessorRuntime.NintexConfigDBConnection.Open();
            //}
            //mConn = SPWorkflowProcessorRuntime.NintexConfigDBConnection;
            mConn = new SqlConnection();
            if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWConfigDBConnectionStringOfBackup"))
            {
                mConn.ConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWConfigDBConnectionStringOfBackup"];
            }
            else
            {
                mConn.ConnectionString = NintexDatabaseUtility.GetConfigDBConnectionString();
            }
            mConn.Open();
        }

        private void BackupSiteConstants(List<Hashtable> data, Guid siteId)
        {
            int datacount = data.Count;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.Parameters.AddWithValue("@SiteId", siteId);
            cmd.CommandText = "SELECT * FROM WorkflowConstants WITH(NOLOCK) WHERE SiteId = @SiteId AND WebId IS NULL";
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    Hashtable ht = new Hashtable();
                    NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                    data.Add(ht);
                }
            }
            BackUpConstantsPremission(data, cmd, datacount);
        }

        private void BackupFarmConstants(List<Hashtable> data)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.CommandText = "SELECT * FROM WorkflowConstants WITH(NOLOCK) WHERE SiteId IS NULL AND WebId IS NULL";
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    Hashtable ht = new Hashtable();
                    NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                    data.Add(ht);
                }
            }
            BackUpConstantsPremission(data, cmd, 0);
        }

        internal void BackupConstants(Guid webId, List<Hashtable> data, Guid siteId)
        {
            BackupFarmConstants(data);
            BackupSiteConstants(data, siteId);
            int datacount = data.Count;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.CommandText = "SELECT * FROM WorkflowConstants WITH(NOLOCK) WHERE WebId=@WebId";
            cmd.Parameters.AddWithValue("@WebId", webId);
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    Hashtable ht = new Hashtable();
                    NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                    data.Add(ht);
                }
            }
            BackUpConstantsPremission(data, cmd, datacount);
        }

        private void BackUpConstantsPremission(List<Hashtable> data, SqlCommand cmd, int datacount)
        {
            for (int i = datacount; i < data.Count; i++)
            {
                List<Hashtable> sdrPermissionCollection = new List<Hashtable>();
                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT * FROM WorkflowConstantSecurity WITH(NOLOCK) where WorkflowConstantID = @InstanceID";
                cmd.Parameters.AddWithValue("@InstanceID", data[i]["#ID"]);
                using (SqlDataReader sdrPermission = cmd.ExecuteReader())
                {
                    while (sdrPermission.Read())
                    {
                        Hashtable htPermission = new Hashtable();
                        NintexDatabaseUtility.SetPropsFromDataReader(sdrPermission, 0, htPermission);
                        sdrPermissionCollection.Add(htPermission);
                    }
                    data[i].AddEx("#Permission", sdrPermissionCollection);
                }
            }
        }

        private void RestoreSiteConstants(Hashtable ht, Guid siteId)
        {
            if (!mFarmLevelIsRestored)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                //int Id = (int)ht["#ID"];
                string title = (string)ht["#Title"];
                //cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WHERE ID=@ID AND Title=@Title AND SiteId IS NUll AND WebId IS NULL";
                cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WITH(NOLOCK) WHERE Title=@Title AND SiteId = @SiteId AND WebId IS NULL";
                cmd.Parameters.AddWithValue("@SiteId", siteId);
                //cmd.Parameters.AddWithValue("@ID", Id);
                cmd.Parameters.AddWithValue("@Title", title);
                int count = (int)cmd.ExecuteScalar();
                ht["#SiteId"] = siteId;
                if (count > 0)
                {
                    if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                    {
                        NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE WorkflowConstants SET {0} WHERE ID=@ID AND SiteId = '" + siteId + "' AND WebId IS NULL", ",ID,", ",ID,SiteId,WebId,", ht);
                    }
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) ID FROM WorkflowConstants WITH(NOLOCK) ORDER BY ID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            ht["#ID"] = sdr.GetInt32(0) + 1;
                        }
                    }
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "WorkflowConstants", ht);
                    RestoreConstantPermission(cmd, "WorkflowConstantSecurity", ht);
                }

            }
        }

        private void RestoreFarmConstants(Hashtable ht)
        {
            if (!mFarmLevelIsRestored)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                //int Id = (int)ht["#ID"];
                string title = (string)ht["#Title"];
                //cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WHERE ID=@ID AND Title=@Title AND SiteId IS NUll AND WebId IS NULL";
                cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WITH(NOLOCK) WHERE Title=@Title AND SiteId IS NULL AND WebId IS NULL";
                cmd.Parameters.Clear();
                //cmd.Parameters.AddWithValue("@ID", Id);
                cmd.Parameters.AddWithValue("@Title", title);
                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                    {
                        NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE WorkflowConstants SET {0} WHERE ID=@ID AND SiteId IS NULL AND WebId IS NULL", ",ID,", ",ID,SiteId,WebId,", ht);
                    }
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) ID FROM WorkflowConstants WITH(NOLOCK) ORDER BY ID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            ht["#ID"] = sdr.GetInt32(0) + 1;
                        }
                    }
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "WorkflowConstants", ht);
                    RestoreConstantPermission(cmd, "WorkflowConstantSecurity", ht);
                }

            }
        }

        internal void RestoreConstants(Guid siteId, Guid webId, List<Hashtable> data)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;

            foreach (Hashtable ht in data)
            {
                if (!ht.Contains("#SiteId"))
                {
                    RestoreFarmConstants(ht);
                }
                else if (!ht.Contains("#WebId"))
                {
                    RestoreSiteConstants(ht, siteId);
                }
                else
                {
                    //int Id = (int)ht["#ID"];
                    string title = (string)ht["#Title"];
                    //cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WHERE ID=@ID AND WebId=@WebId AND Title=@Title";
                    cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WITH(NOLOCK) WHERE WebId=@WebId AND Title=@Title";
                    cmd.Parameters.Clear();
                    //cmd.Parameters.AddWithValue("@ID", Id);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {
                        if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                        {
                            NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE WorkflowConstants SET {0} WHERE ID=@ID AND WebId=@WebId", ",ID,WebId,", ",ID,SiteId,WebId,", ht);
                        }
                    }
                    else
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT TOP(1) ID FROM WorkflowConstants WITH(NOLOCK) ORDER BY ID DESC";
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.Read())
                            {
                                ht["#ID"] = sdr.GetInt32(0) + 1;
                            }
                        }
                        ht["#SiteId"] = siteId;
                        ht["#WebId"] = webId;
                        NintexDatabaseUtility.ExecuteInsertCommand(cmd, "WorkflowConstants", ht);
                        RestoreConstantPermission(cmd, "WorkflowConstantSecurity", ht);
                    }

                }
            }
            mFarmLevelIsRestored = true;
        }

        private void RestoreConstantPermission(SqlCommand cmd, string tableName, Hashtable data)
        {
            List<Hashtable> permissionHt = (List<Hashtable>)data["#Permission"];
            foreach (Hashtable ht in permissionHt)
            {
                ht["#WorkflowConstantID"] = data["#ID"];
                NintexDatabaseUtility.ExecuteInsertCommand(cmd, "WorkflowConstantSecurity", ht);
            }
        }

        public void Dispose()
        {
            mConn.Dispose();
        }
    }

    /// <summary>
    /// review 后未解决问题：
    /// 1. backup 一个export,restore 很多case metadata 对应（涉及外围）
    /// 2. sql 语句考虑放到queryservice里面
    /// 3. 备份数据用hasttable存储不好，考虑写个info 类来存储
    /// 4. sp wonflow instanceunit  公共处理replace Id和url方法
    /// 5. export template 方法名字不合理(还原是接口，涉及外围改动)
    /// </summary>
    internal class NintexWorkflowUserDefinedActionHelper : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SqlConnection mConn;       
        private Dictionary<int, int> restoredUserDefinedActions = new Dictionary<int, int>();
        private Guid oldSiteId = Guid.Empty;

        private const string commandText = @"SELECT udas.Id AS Id,StaticId,Name,Description,Category,IconUrl,ToolboxIconUrl,WarningIconUrl,ConfigurationPageUrl,SiteId,WebId,
 udavs.Id AS UdavsId,UdaId,InOutParameters,Xoml,Rules,ModifiedBy,Modified, UIMajorVersion,UIMinorVersion,Version,Published,Comments
 FROM UserDefinedActions AS udas  WITH(NOLOCK)
 LEFT JOIN UserDefinedActionVersions AS udavs  WITH(NOLOCK)
 ON udas.Id=udavs.UdaId
 WHERE
 ORDER BY udas.Id ASC, version ASC"; 

        internal NintexWorkflowUserDefinedActionHelper(Guid siteId)
        {
            mConn = new SqlConnection();
            if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWConfigDBConnectionStringOfBackup"))
            {
                mConn.ConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWConfigDBConnectionStringOfBackup"];
            }
            else
            {
                mConn.ConnectionString = NintexDatabaseUtility.GetConfigDBConnectionString();
            }
            mConn.Open();
        }

        internal void BackupUserDefinedActions(Guid siteId,Guid webId, List<Hashtable> data)
        {
            try
            {
                logger.Info("Start Backup the user defined actions. Site Id:{0},Web Id:{1}.",siteId,webId);
                //BackupFarmUserDefinedActions(data);
                //BackupSiteUserDefinedActions(siteId, data);
                var containsFarmLevel = true;
                var containsSiteLevel = true;
                var whereText = string.Empty;
                if (containsFarmLevel)
                {
                    whereText += @"(SiteID IS NULL AND WebID IS NULL) OR ";
                }
                if (containsSiteLevel)
                {
                    whereText += @"(SiteId=@SiteID AND WebId IS NULL) OR ";
                }
                whereText =@"WHERE "+whereText+ @"(SiteID=@SiteID AND WebID=@WebID)";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                //cmd.CommandText = commandText.Replace("WHERE", @"WHERE SiteID=@SiteID AND WebID=@WebID");
                cmd.CommandText = commandText.Replace("WHERE", whereText);
                cmd.Parameters.AddWithValue("@SiteID", siteId);
                cmd.Parameters.AddWithValue("@WebID", webId);
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        Hashtable ht = new Hashtable();
                        NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                        data.Add(ht);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Backup user defined actions failed. Error message:{0}. ", e);
            }
        }

        #region Unused

        private void BackupSiteUserDefinedActions(Guid siteId,List<Hashtable> data)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                cmd.CommandText = commandText.Replace("WHERE", @"WHERE SiteId=@SiteID AND WebId IS NULL");
                cmd.Parameters.AddWithValue("@SiteID", siteId);
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        Hashtable ht = new Hashtable();
                        NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                        data.Add(ht);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Backup site user defined actions failed. Error message:{0}. ", e);
            }
        }

        private void BackupFarmUserDefinedActions(List<Hashtable> data)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                cmd.CommandText = commandText.Replace("WHERE", @"WHERE SiteID IS NULL AND WebID IS NULL");
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        Hashtable ht = new Hashtable();
                        NintexDatabaseUtility.SetPropsFromDataReader(sdr, 0, ht);
                        data.Add(ht);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Backup farm user defined actions failed. Error message:{0}. ", e);
            }
        }
        
        #endregion

        internal void CacheUserDefinedActions(Guid siteId, Guid webId, List<Hashtable> data)
        {
            RestoreUserDefinedActions(siteId, webId, data, true);
        }

        internal void RestoreUserDefinedActions(Guid siteId, Guid webId, List<Hashtable> data)
        {
            RestoreUserDefinedActions(siteId, webId, data, false);
        }

        /// <summary>
        /// 现在cache和restore的逻辑耦合在一起，稍后需要单提出来
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="data"></param>
        /// <param name="cachedOnly"></param>
        private void RestoreUserDefinedActions(Guid siteId, Guid webId, List<Hashtable> data, bool cachedOnly)
        {
            UserDefiniedActionIdMapping mapping = SPWorkflowProcessorRuntime.UDAMappingManager.TryGetUDAIDMapping(siteId, webId);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            //var replaceWhere=@"WHERE Id=@Id AND StaticId=@StaticId AND Name=@Name AND SiteId IS NULL AND WebId IS NULL";
            var replaceWhere = @"WHERE StaticId=@StaticId AND Name=@Name AND SiteId IS NULL AND WebId IS NULL";
            foreach (Hashtable ht in data)
            {
                int id = (int)ht["#Id"];
                var udaId = id;
                string name = (string)ht["#Name"];
                //当需要在目的端创建user defined action时，需要new 一个staticId
                //因为UDA在delete时时根据staticId来删除的，如果不替换，那么删除这个UDA时会连同原端的一同删除
                Guid staticId = (Guid)ht["#StaticId"];
                if (!cachedOnly && !SPWorkflowProcessorRuntime.NeedRestoreUserDefiniedActionId.Contains(staticId))
                {
                    logger.Debug("Skip restore user defined action, static id: {0}, id: {1}", staticId.ToString(), id.ToString());
                    continue;
                }
                UserDefinedActionInfo uda=new UserDefinedActionInfo();
                uda.StaticId=staticId;
                uda.Name = name;

                if (SPWorkflowProcessorRuntime.RestoredUDAMapping.ContainsKey(udaId))
                {
                    continue;
                }
                //static id+name
                if (!ht.Contains("#SiteId"))
                {
                    replaceWhere = @"WHERE StaticId=@StaticId AND Name=@Name AND SiteId IS NULL AND WebId IS NULL"; ;
                    uda.SiteId=Guid.Empty;
                    uda.WebId=Guid.Empty;
                }
                else if (!ht.Contains("#WebId"))
                {
                    replaceWhere = @"WHERE StaticId=@StaticId AND Name=@Name AND SiteId=@SiteId AND WebId IS NULL";
                    ht["#SiteId"] = siteId;
                    uda.SiteId=siteId;
                    uda.WebId=Guid.Empty;
                }
                else
                {
                    replaceWhere = @"WHERE StaticId=@StaticId AND Name=@Name AND SiteId=@SiteId AND WebId=@WebId";
                    ht["#SiteId"] = siteId;
                    ht["#WebId"] = webId;
                     uda.SiteId=siteId;
                    uda.WebId=webId;
                }

                var udaHT = new Hashtable();
                var udaVersionHT = new Hashtable();
                var udaActivityActivationHT = new Hashtable();
                try
                {
                    SplitHashtable(ht, udaHT, udaVersionHT);

                    //还原user defined action
                    if (!restoredUserDefinedActions.ContainsKey(id))
                    {
                        bool isExist=false;
                        if (cachedOnly)
                        {
                            isExist= HandleUserDefinedActionConflictResolution(uda, cmd, replaceWhere);
                        }
                        else
                        {
                            //能从mapping中找到就用mapping的value，如果找不到，就用原来的static id
                            Guid mappedStaticId;
                            if (mapping.TryGetValue(staticId,out mappedStaticId))
                            {
                                //需要更新ht中staticId用于update
                                ht["#StaticId"] = mappedStaticId;
                            }
                            else
                            {
                                mappedStaticId = staticId;
                            }

                            cmd.CommandText = "SELECT Id FROM UserDefinedActions WITH(NOLOCK) WHERE".Replace("WHERE", replaceWhere);
                            cmd.Parameters.Clear();

                            //cmd.Parameters.AddWithValue("@Id", Id);
                            cmd.Parameters.AddWithValue("@Name", name);
                            cmd.Parameters.AddWithValue("@StaticId", mappedStaticId);
                            cmd.Parameters.AddWithValue("@SiteId", siteId);
                            cmd.Parameters.AddWithValue("@WebId", webId);
                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                                while (sdr.Read())
                                {
                                    udaId = (int)sdr[0];
                                    isExist = true;
                                    //找到就是update，找不到就是insert
                                    break;
                                }
                            }
                        }
                        if (cachedOnly)
                        {
                            if (staticId!=uda.StaticId)
                            {
                                SPWorkflowProcessorRuntime.UDAMappingManager.Add(siteId, webId, staticId, uda.StaticId);
                            }
                            continue;
                        }
                        else if (isExist)
                        {
                            logger.Debug("User defined action exist. StaticId: {0}, new staticId: {1}, id: {2}", ht.ContainsKey("#StaticId") ? ht["#StaticId"] : "Null", udaHT.ContainsKey("#StaticId") ? udaHT["#StaticId"].ToString() : "Null", udaHT.ContainsKey("#Id") ? udaHT["#Id"] : "Null");
                            if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                            {
                                NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE UserDefinedActions SET {0} WHERE".Replace("WHERE", replaceWhere), ",Name,SiteId,WebId,", ",Id,", udaHT);
                            }
                            if (udaHT.ContainsKey("#Id") && !udaHT.Contains("#SiteId"))
                            {
                                if (!ActivityDataExist(cmd, udaHT["#Id"].ToString()))
                                {
                                    logger.Debug("Restore farm user defined action display info, id: {0}", udaHT["#Id"].ToString());
                                    udaActivityActivationHT["#ActivityID"] = udaHT["#Id"];
                                    NintexDatabaseUtility.ExecuteActivityActionsInsertCommand(cmd, udaActivityActivationHT);
                                }
                            }
                        }
                        else
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "SELECT TOP(1) ID FROM UserDefinedActions WITH(NOLOCK) ORDER BY ID DESC";
                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                                if (sdr.Read())
                                {
                                    udaId = sdr.GetInt32(0) + 1;
                                    udaHT["#Id"] = udaId;
                                }
                            }
                            if (mapping != null)
                            {
                                Guid mappedStaticId;
                                if (mapping.TryGetValue(staticId, out mappedStaticId))
                                {
                                    //需要更新ht中staticId用于update
                                    udaHT["#StaticId"] = mappedStaticId;
                                }
                            }
                            logger.Debug("Create new user defined action. Old staticId: {0}, new staticId: {1}, id: {2}", staticId.ToString(), udaHT.ContainsKey("#StaticId") ? udaHT["#StaticId"].ToString() : "Null", udaHT.ContainsKey("#Id") ? udaHT["#Id"] : "Null");
                            NintexDatabaseUtility.ExecuteInsertCommand(cmd, "UserDefinedActions", udaHT);
                            if (udaHT.ContainsKey("#Id") && !udaHT.Contains("#SiteId"))
                            {
                                if (!ActivityDataExist(cmd, udaHT["#Id"].ToString()))
                                {
                                    logger.Debug("Restore farm user defined action display info, id: {0}", udaHT["#Id"].ToString());
                                    udaActivityActivationHT["#ActivityID"] = udaHT["#Id"];
                                    NintexDatabaseUtility.ExecuteActivityActionsInsertCommand(cmd, udaActivityActivationHT);
                                }
                            }
                        }
                        restoredUserDefinedActions.Add(id, udaId);
                    }
                    //还原user defined action version
                    RestoreUserDefinedActionVersion(cmd, restoredUserDefinedActions[id], udaVersionHT, siteId, webId);
                }
                catch (Exception e)
                {
                    logger.Warn("Restore the user defined Action failed. ID:{0},Name:{1}, Error Message:{2}.", id, name, e);
                }
            }
            foreach (var map in restoredUserDefinedActions)
            {
                SPWorkflowProcessorRuntime.RestoredUDAMapping[map.Key] = map.Value;
            }
        }

        private bool ActivityDataExist(SqlCommand cmd, string id)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = string.Format("select * from ActivityActivation where ActivityID={0}", id);
            var scalar = cmd.ExecuteScalar();
            if (scalar != null && (int)cmd.ExecuteScalar() > 0)
            {
                return true;
            }
            return false;
        }

        private bool HandleUserDefinedActionConflictResolution(UserDefinedActionInfo udaInfo, SqlCommand cmd, string replaceWhere)
        {
            #region  find by static id and name
            int udaId = -1;
            Guid returnStaticId = udaInfo.StaticId;
            cmd.CommandText = "SELECT Id FROM UserDefinedActions WITH(NOLOCK) WHERE".Replace("WHERE", replaceWhere);
            cmd.Parameters.Clear();

            //cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@Name", udaInfo.Name);
            cmd.Parameters.AddWithValue("@StaticId", udaInfo.StaticId);
            cmd.Parameters.AddWithValue("@SiteId", udaInfo.SiteId);
            cmd.Parameters.AddWithValue("@WebId", udaInfo.WebId);
            bool isExist = false;
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    udaId = (int)sdr[0];
                    isExist = true;
                    break;
                }
            }
            #endregion

            if (!isExist)
            {
                #region find by name only
                if (udaInfo.SiteId == Guid.Empty)
                {
                    replaceWhere = @"WHERE Name=@Name AND SiteId IS NULL AND WebId IS NULL"; ;
                }
                else if (udaInfo.WebId == Guid.Empty)
                {
                    replaceWhere = @"WHERE Name=@Name AND SiteId=@SiteId AND WebId IS NULL";
                }
                else
                {
                    replaceWhere = @"WHERE Name=@Name AND SiteId=@SiteId AND WebId=@WebId";
                }

                cmd.CommandText = "SELECT Id,StaticId FROM UserDefinedActions WITH(NOLOCK) WHERE".Replace("WHERE", replaceWhere);
                cmd.Parameters.Clear();

                //cmd.Parameters.AddWithValue("@Id", Id);
                cmd.Parameters.AddWithValue("@Name", udaInfo.Name);
                cmd.Parameters.AddWithValue("@SiteId", udaInfo.SiteId);
                cmd.Parameters.AddWithValue("@WebId", udaInfo.WebId);

                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        udaId = (int)sdr[0];
                        returnStaticId = (Guid)sdr[1];
                        isExist = true;
                        break;
                    }
                }

                if (!isExist)
                {//没有find到符合条件的，需要new一个id
                    returnStaticId = Guid.NewGuid();
                }

                #endregion
            }
            else
            {
                //找到static id name都符合的了，不需要mapping都可以
            }
            udaInfo.StaticId = returnStaticId;
            return isExist;
        }

        internal class UserDefinedActionInfo
        {
            public string Name { get; set; }
            public int UdaId { get; set; }
            public Guid StaticId { get; set; }
            public Guid SiteId { get; set; }
            public Guid WebId { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Uda")]
        private void RestoreUserDefinedActionVersion(SqlCommand cmd, int udaId, Hashtable udaVersionHT, Guid siteId, Guid webId)
        {
            
            
            udaVersionHT["#UdaId"] = udaId;
            var version = (int)udaVersionHT["#Version"];
            try
            {

                cmd.CommandText = "SELECT COUNT(*) FROM UserDefinedActionVersions WITH(NOLOCK) WHERE UdaId=@UdaId AND Version=@Version";
                cmd.Parameters.Clear();
                //cmd.Parameters.AddWithValue("@ID", Id);
                cmd.Parameters.AddWithValue("@UdaId", udaId);
                cmd.Parameters.AddWithValue("@Version", version);
                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                    {
                        HandleUDAXmlInfo(udaVersionHT, siteId, webId);
                        NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE UserDefinedActionVersions SET {0} WHERE UdaId=@ID AND Version=@Version", ",UdaId,Version,", ",ID,", udaVersionHT);
                    }
                }
                else
                {
                    HandleUDAXmlInfo(udaVersionHT, siteId, webId);
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) ID FROM UserDefinedActionVersions WITH(NOLOCK) ORDER BY ID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            udaVersionHT["#Id"] = sdr.GetInt32(0) + 1;
                        }
                    }
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "UserDefinedActionVersions", udaVersionHT);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Restore the user defined action version failed. Uda id:{0},Version:{1}, Error message:{2}.", udaId, version, e);
            }
        }

        private void HandleUDAXmlInfo(Hashtable udaVersionHT,Guid siteId, Guid webId)
        {
            if (udaVersionHT.ContainsKey("#Xoml"))
            {
                //替换nintex workflow user defined actions中的id的逻辑在nintex数据和workflow template中都要用到，
                //所以先挪到替换SPWorkflowSubFileUnit类中，以后考虑放到一个单独的类中
                udaVersionHT["#Xoml"] =NintexWorkflowUtility.ReplaceIdsInUserDefinedAction(udaVersionHT["#Xoml"].ToString(), siteId, webId, oldSiteId);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Udavs"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Uda")]
        private void SplitHashtable(Hashtable ht, Hashtable udaHT, Hashtable udaVersionHT)
        {
            List<string> columnNames = new List<string>() { "#UdavsId", "#UdaId", "#InOutParameters", "#Xoml", "#Rules", "#ModifiedBy", "#Modified", "#UIMajorVersion", "#UIMinorVersion", "#Version", "#Published", "#Comments" };
            foreach (DictionaryEntry htEx in ht)
            {
                var key = htEx.Key;
                if (columnNames.Any(name => name.Equals(htEx.Key.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    if ("#UdavsId".Equals(htEx.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        key = "#Id";
                    }
                    udaVersionHT.Add(key, htEx.Value);
                }
                else
                {
                    udaHT.Add(key, htEx.Value);
                }
            }
        }
      
        public void Dispose()
        {
            mConn.Dispose();
            restoredUserDefinedActions.Clear();
            restoredUserDefinedActions = null;
        }
    }

}
