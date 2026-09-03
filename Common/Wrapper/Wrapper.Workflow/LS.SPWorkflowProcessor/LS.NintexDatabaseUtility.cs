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
using Nintex.Workflow.Administration;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;
namespace LS.SPWorkflowProcessor
{
    internal class NintexDatabaseUtility
    {
        private static NintexDatabaseInvoker nintexDBInvoker = new NintexDatabaseInvoker();

        public static SqlConnectionStringBuilder GetConfigDBConnectionString()
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetConfigDBConnectionString");
            try
            {
                SqlConnectionStringBuilder strBuilder = nintexDBInvoker.GetConnectionString();//ConfigurationDatabase.GetConfigurationDatabase().SQLConnectionString;
                if (!string.IsNullOrEmpty(strBuilder.ConnectionString))
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ConfigDBConnString, strBuilder.ConnectionString);
                    return strBuilder;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_GetConfigDBException, e.Message);
                return null;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetConfigDBConnectionString");
            }
        }

        public static string GetContentDBConnectionString(Guid siteId)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetContentDBConnectionString");
            int siteCount = 0;
            int id = -1;
            SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder();
            try
            {
                using (SqlConnection connection = ConfigurationDatabase.OpenConfigDataBase())//(SqlConnection connection = nintexDBInvoker.OpenConfigDataBase())//
                {
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
                }
                return sqlConnectionString.ConnectionString;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_GetContentDBException, e.Message);
                return null;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetContentDBConnectionString");
            }
        }

      

        /*internal static void ExecuteUpdateCommand(SqlCommand cmd, string commandText, string conditionParams, string excludeParams, Hashtable data)
        {
            StringBuilder keyEx = new StringBuilder();
            StringBuilder cmdParam = new StringBuilder();
            bool isFirstParam = true;
            conditionParams = conditionParams.ToLower();
            excludeParams = excludeParams.ToLower();

            cmd.Parameters.Clear();
            foreach (DictionaryEntry de in data)
            {
                string key = (string)de.Key;
                string key2 = key.Substring(1);
                keyEx.Remove(0, keyEx.Length);
                keyEx.Append(",");
                keyEx.Append(key2.ToLower());
                keyEx.Append(",");
                if (excludeParams.IndexOf(keyEx.ToString()) >= 0)
                    continue;

                if (data[key] == null)
                    cmd.Parameters.AddWithValue("@" + key2, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@" + key2, data[key]);

                if (conditionParams.IndexOf(keyEx.ToString()) >= 0)
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
        }*/

        /*internal static void ExecuteInsertCommand(SqlCommand cmd, string tableName, Hashtable data)
        {
            try
            {


                List<string> fieldNames = new List<string>();
                *//* Fortify Issue Type: SQL Injection
                 * Sink Location：LS.SPWorkflowProcessor.ExecuteInsertCommand 290
                 * Ignore Reason：这里直接拼接了tableName传入数据库，经检查这里传入的tableName均为hardcode，暂无注入风险
                 *//*
                cmd.Parameters.Clear();
                cmd.CommandText = "SET IDENTITY_INSERT " + tableName + " ON";
                cmd.ExecuteNonQuery();
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
                cmd.Parameters.Clear();
                cmd.CommandText = "SET IDENTITY_INSERT " + tableName + " OFF";
                cmd.ExecuteNonQuery();
            }
        }*/

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
                logger.Log(AveLogLevel.INFO, WrapperWorkflowResource.NintexDBLoadAssemblyError, ex);
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
        internal NintexMessageTemplateHelper()
        {
            mConn = new SqlConnection();
            mConn.ConnectionString = NintexDatabaseUtility.GetConfigDBConnectionString().ConnectionString;
            mConn.Open();
        }

        internal void BackupMessageTemplates(Guid webId, List<Hashtable> data)
        {
            BackupFarmMessageTemplates(data);

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.CommandText = "SELECT * FROM MessageTemplates WHERE WebID=@WebID";
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

        private void BackupFarmMessageTemplates(List<Hashtable> data)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.CommandText = "SELECT * FROM MessageTemplates WHERE SiteID IS NULL AND WebID IS NULL";
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



        /*internal void RestoreMessageTemplates(Guid siteId, Guid webId, List<Hashtable> data)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            foreach (Hashtable ht in data)
            {
                if (!ht.Contains("#WebID"))
                {
                    RestoreFarmMessageTemplates(ht);
                }
                else
                {
                    string header = (string)ht["#Header"];
                    if (!mWebMessageTemplates.ContainsKey(webId) || (mWebMessageTemplates.ContainsKey(webId) && mWebMessageTemplates[webId].Count == 0))
                    {
                        //cmd.CommandText = "SELECT COUNT(*) FROM MessageTemplates WHERE TemplateID=@TemplateID AND WebID=@WebID AND Header=@Header";
                        cmd.CommandText = "SELECT Header FROM MessageTemplates WHERE WebID=@WebID";
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
                        cmd.CommandText = "SELECT TOP(1) TemplateID FROM MessageTemplates ORDER BY TemplateID DESC";
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
        }*/

        /*private void RestoreFarmMessageTemplates(Hashtable ht)
        {
            if (!mFarmLevelIsRestored)
            {
                string header = (string)ht["#Header"];
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                cmd.CommandText = "SELECT Header FROM MessageTemplates WHERE SiteID IS NUll AND WebID IS NULL";
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
                    cmd.CommandText = "SELECT TOP(1) TemplateID FROM MessageTemplates ORDER BY TemplateID DESC";
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
        }*/

        public void Dispose()
        {
            if(mConn != null)
            {
                mConn.Dispose();
            }
        }
    }

    internal class NintexWorkflowConstantHelper: IDisposable
    {
        private SqlConnection mConn;
        internal NintexWorkflowConstantHelper()
        {
            mConn = new SqlConnection();
            mConn.ConnectionString = NintexDatabaseUtility.GetConfigDBConnectionString().ConnectionString;
            mConn.Open();
        }

        private void BackupFarmConstants(List<Hashtable> data)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.CommandText = "SELECT * FROM WorkflowConstants WHERE SiteId IS NULL AND WebId IS NULL";
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

        internal void BackupConstants(Guid webId, List<Hashtable> data)
        {
            BackupFarmConstants(data);

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;
            cmd.CommandText = "SELECT * FROM WorkflowConstants WHERE WebId=@WebId";
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
        }

        /*private void RestoreFarmConstants(Hashtable ht)
        {
            if (!mFarmLevelIsRestored)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = mConn;
                //int Id = (int)ht["#ID"];
                string title = (string)ht["#Title"];
                //cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WHERE ID=@ID AND Title=@Title AND SiteId IS NUll AND WebId IS NULL";
                cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WHERE Title=@Title AND SiteId IS NUll AND WebId IS NULL";
                cmd.Parameters.Clear();
                //cmd.Parameters.AddWithValue("@ID", Id);
                cmd.Parameters.AddWithValue("@Title", title);
                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    if (SPWorkflowProcessorRuntime.OverwriteNWMessageTemplates)
                    {
                        NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE WorkflowConstants SET {0} WHERE ID=@ID AND SiteId IS NUll AND WebId IS NULL", ",ID,", ",ID,SiteId,WebId,", ht);
                    }
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) ID FROM WorkflowConstants ORDER BY ID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            ht["#ID"] = sdr.GetInt32(0) + 1;
                        }
                    }
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "WorkflowConstants", ht);
                }

            }
        }*/

        /*internal void RestoreConstants(Guid siteId, Guid webId, List<Hashtable> data)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = mConn;

            foreach (Hashtable ht in data)
            {
                if (!ht.Contains("#WebId"))
                {
                    RestoreFarmConstants(ht);
                }
                else
                {
                    //int Id = (int)ht["#ID"];
                    string title = (string)ht["#Title"];
                    //cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WHERE ID=@ID AND WebId=@WebId AND Title=@Title";
                    cmd.CommandText = "SELECT COUNT(*) FROM WorkflowConstants WHERE WebId=@WebId AND Title=@Title";
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
                        cmd.CommandText = "SELECT TOP(1) ID FROM WorkflowConstants ORDER BY ID DESC";
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
                    }

                }
            }
            mFarmLevelIsRestored = true;
        }*/

        public void Dispose()
        {
            if(mConn != null)
            {
                mConn.Dispose();
            }
        }
    }
}
