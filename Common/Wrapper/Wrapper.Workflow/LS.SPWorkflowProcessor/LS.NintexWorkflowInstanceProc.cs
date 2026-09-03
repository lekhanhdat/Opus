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
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Common;
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;
namespace LS.SPWorkflowProcessor
{
    public class NintexWorkflowInstanceProc : ICustomWorkflowInstanceProc
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static string assemblyString = "Nintex.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=913f6bae0ca5ae12";
        private static Assembly assembly;

        public static bool IsNintexDllInstalled
        {
            get
            {
                try
                {
                    if (assembly != null)
                        return true;
                    if (SPWorkflowProcessorRuntime.AllProcessorParams != null && SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWAssemblyName"))
                    {
                        assemblyString = SPWorkflowProcessorRuntime.AllProcessorParams["NWAssemblyName"];
                    }
                    assembly = Assembly.Load(assemblyString);
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_IsInstalled, "true");
                    return true;
                }
                catch(Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.LoadAssemblyError, e.ToString());
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_IsInstalled, "false");
                    return false;
                }
            }
        }

        public string NintexDBConnectionString
        {
            get;
            set;
        }


        #region Backup Region
        public void BackupCustomWorkflowData(SPWorkflowSubItemUnit parentUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "NWDBConnectionStringOfBackup");
            if (IsNintexDllInstalled)
            {
                if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWDBConnectionStringOfBackup"))
                {
                    this.NintexDBConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionStringOfBackup"];
                }
                else
                {
                    if (parentUnit.ItemType == WorkflowSubItemType.Task)
                        this.NintexDBConnectionString = NintexDatabaseUtility.GetContentDBConnectionString((Guid)parentUnit.Properties["~0_tp_SiteId"]);
                    else if (parentUnit.ItemType == WorkflowSubItemType.Instance)
                        this.NintexDBConnectionString = NintexDatabaseUtility.GetContentDBConnectionString((Guid)parentUnit.Properties["#SiteId"]);
                }

                if (string.IsNullOrEmpty(this.NintexDBConnectionString))
                {
                    return;
                }
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ContentDBConnString, this.NintexDBConnectionString);

                switch (parentUnit.ItemType)
                {
                    case WorkflowSubItemType.Instance:
                        //Fortify fix: Unreleased Resource: Database
                        using (SqlConnection nintexConn = new SqlConnection())
                        {
                            nintexConn.ConnectionString = this.NintexDBConnectionString;
                            nintexConn.Open();
                            BackupNintexWorkflowInstance(nintexConn, parentUnit);
                        }
                        break;
                    default:
                        break;
                }
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupCustomWorkflowData");
            }
        }

        private void BackupNintexWorkflowInstance(SqlConnection nintexConn, SPWorkflowSubItemUnit parentInstanceUnit)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = nintexConn;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@WorkflowInstanceID", parentInstanceUnit.Properties["#Id"]);
                cmd.CommandText = "SELECT * FROM WorkflowInstance WHERE WorkflowInstanceID=@WorkflowInstanceID";


                using (SqlDataAdapter nwAdapter = new SqlDataAdapter())
                {
                    nwAdapter.SelectCommand = cmd;
                    using (DataTable nwMemoryTable = new DataTable())
                    {
                        nwAdapter.Fill(nwMemoryTable);
                        foreach (DataRow dr in nwMemoryTable.Rows)
                        {
                            SPWorkflowSubItemUnit tempUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Custom, parentInstanceUnit);
                            tempUnit.UnitId = "Nintex.WorkflowInstance";
                            tempUnit.SetPropsFromDataRow(dr, nwMemoryTable.Columns);
                            BackupNintexWorkflowProgress(nintexConn, tempUnit);
                            parentInstanceUnit.ChildUnits.Add(tempUnit);
                        }
                        nwMemoryTable.Clear();
                    }
                }
            }
        }

        private void BackupNintexWorkflowProgress(SqlConnection nintexConn, SPWorkflowSubItemUnit parentNWInstanceUnit)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = nintexConn;

            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@InstanceID", parentNWInstanceUnit.Properties["#InstanceID"]);
            cmd.CommandText = "SELECT * FROM WorkflowProgress WHERE InstanceID=@InstanceID";


            using (SqlDataAdapter nwAdapter = new SqlDataAdapter())
            {
                nwAdapter.SelectCommand = cmd;
                using (DataTable nwMemoryTable = new DataTable())
                {
                    nwAdapter.Fill(nwMemoryTable);
                    foreach (DataRow dr in nwMemoryTable.Rows)
                    {
                        SPWorkflowSubItemUnit tempUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Custom, parentNWInstanceUnit);
                        tempUnit.UnitId = "Nintex.WorkflowProgress";
                        tempUnit.SetPropsFromDataRow(dr, nwMemoryTable.Columns);
                        BackupNintexHumanWorkflow(nintexConn, tempUnit);
                        parentNWInstanceUnit.ChildUnits.Add(tempUnit);
                    }
                    nwMemoryTable.Clear();
                }
            }
        }

        private void BackupNintexHumanWorkflow(SqlConnection nintexConn, SPWorkflowSubItemUnit parentProgressUnit)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = nintexConn;

            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@WorkflowProgressID", (long)parentProgressUnit.Properties["#WorkflowProgressID"]);
            cmd.CommandText = "SELECT * FROM HumanWorkflow WHERE WorkflowProgressID=@WorkflowProgressID";

            using (SqlDataAdapter nwAdapter = new SqlDataAdapter())
            {
                nwAdapter.SelectCommand = cmd;
                using (DataTable nwMemoryTable = new DataTable())
                {
                    nwAdapter.Fill(nwMemoryTable);
                    foreach (DataRow dr in nwMemoryTable.Rows)
                    {
                        SPWorkflowSubItemUnit tempUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Custom, parentProgressUnit);
                        tempUnit.UnitId = "Nintex.HumanWorkflow";
                        tempUnit.SetPropsFromDataRow(dr, nwMemoryTable.Columns);
                        BackupNintexHumanWorkflowApprovers(nintexConn, tempUnit);
                        parentProgressUnit.ChildUnits.Add(tempUnit);
                    }
                    nwMemoryTable.Clear();
                }
            }
        }

        private void BackupNintexHumanWorkflowApprovers(SqlConnection nintexConn, SPWorkflowSubItemUnit parentNWHumanWorkflow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = nintexConn;

            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@HumanWorkflowID", parentNWHumanWorkflow.Properties["#HumanWorkflowID"]);
            cmd.CommandText = "SELECT * FROM HumanWorkflowApprovers WHERE HumanWorkflowID=@HumanWorkflowID";


            using (SqlDataAdapter nwAdapter = new SqlDataAdapter())
            {
                nwAdapter.SelectCommand = cmd;
                using (DataTable nwMemoryTable = new DataTable())
                {
                    nwAdapter.Fill(nwMemoryTable);
                    foreach (DataRow dr in nwMemoryTable.Rows)
                    {
                        SPWorkflowSubItemUnit tempUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Custom, parentNWHumanWorkflow);
                        tempUnit.UnitId = "Nintex.HumanWorkflowApprovers";
                        tempUnit.SetPropsFromDataRow(dr, nwMemoryTable.Columns);
                        parentNWHumanWorkflow.ChildUnits.Add(tempUnit);
                    }
                    nwMemoryTable.Clear();
                }
            }
        }
        #endregion



        #region Restore Region

        public void RestoreCustomWorkflowData(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
        {
            //SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreCustomWorkflowData");
            //if (IsNintexDllInstalled)
            //{
            //    if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWDBConnectionStringOfRestore"))
            //    {
            //        this.NintexDBConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionStringOfRestore"];
            //    }
            //    else
            //    {
            //        this.NintexDBConnectionString = NintexDatabaseUtility.GetContentDBConnectionString(parentUnit.FixupParameters.mSiteIdDic.GetValue(0));
            //    }
            //    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ContentDBConnString, this.NintexDBConnectionString);

            //    SqlConnection conn = null;
            //    try
            //    {
            //        conn = new SqlConnection();
            //        conn.ConnectionString = this.NintexDBConnectionString;
            //        conn.Open();

            //        if (parentItem.ItemType == WorkflowSubItemType.Instance)
            //        {
            //            foreach (SPWorkflowSubItemUnit instanceChild in parentItem.ChildUnits)
            //            {
            //                if (instanceChild.ItemType == WorkflowSubItemType.Custom && instanceChild.UnitId.Equals("Nintex.WorkflowInstance", StringComparison.OrdinalIgnoreCase))
            //                {
            //                    //Nintex Workflow在显示action时,是根据Workflow Instance表中XOML和Rules字段中的数值来选择xoml,rules文件的version
            //                    if (!string.IsNullOrEmpty(parentUnit.ParentAssociationUnit.XomlVersionLabel) && instanceChild.Properties.Contains("#XOML"))
            //                    {
            //                        instanceChild.Properties["#XOML"] = parentUnit.ParentAssociationUnit.XomlVersionLabel.Substring(0, parentUnit.ParentAssociationUnit.XomlVersionLabel.IndexOf('.'));
            //                    }
            //                    if (!string.IsNullOrEmpty(parentUnit.ParentAssociationUnit.RulesVersionLabel) && instanceChild.Properties.Contains("#Rules"))
            //                    {
            //                        instanceChild.Properties["#Rules"] = parentUnit.ParentAssociationUnit.RulesVersionLabel.Substring(0, parentUnit.ParentAssociationUnit.RulesVersionLabel.IndexOf('.'));
            //                    }
            //                    //
            //                    HandleWorkflowInstance(instanceChild, conn, parentUnit.FixupParameters);
            //                }
            //            }
            //        }
            //        else if (parentItem.ItemType == WorkflowSubItemType.Task)
            //        {
            //            if (parentUnit.ParentAssociationUnit.mTaskListUnit.FieldProcessor.AveFieldCollection[Nintex_HumanWorkflowIDColStaticName] == null)
            //                return;

            //            AveSPField aveField = parentUnit.ParentAssociationUnit.mTaskListUnit.FieldProcessor.AveFieldCollection.GetAveFieldByInternalName(Nintex_HumanWorkflowIDColStaticName);
            //            if (!parentItem.Properties.Contains("#" + aveField.SerializableData.mDstColName))
            //                return;
            //            long humanWorkflowId = (long)(int)parentItem.Properties["#" + aveField.SerializableData.mDstColName];
            //            long newHumanWorkflowId = 0;

            //            #region Find New Human Workflow ID
            //            bool found = false;
            //            foreach (SPWorkflowSubItemUnit insChild in parentItem.ParentUnit.ChildUnits)
            //            {
            //                if (insChild.ItemType == WorkflowSubItemType.Custom && insChild.UnitId.Equals("Nintex.WorkflowInstance", StringComparison.OrdinalIgnoreCase))
            //                {
            //                    foreach (SPWorkflowSubItemUnit progressChild in insChild.ChildUnits)
            //                    {
            //                        if (progressChild.ItemType == WorkflowSubItemType.Custom && progressChild.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase))
            //                        {
            //                            foreach (SPWorkflowSubItemUnit humansChild in progressChild.ChildUnits)
            //                            {
            //                                if (humansChild.ItemType == WorkflowSubItemType.Custom && humansChild.UnitId.Equals("Nintex.HumanWorkflow", StringComparison.OrdinalIgnoreCase))
            //                                {
            //                                    if (humansChild.Properties.Contains("~HumanWorkflowID"))
            //                                    {
            //                                        long temp = (long)humansChild.Properties["~HumanWorkflowID"];//old humanworkflowid
            //                                        if (temp == humanWorkflowId)
            //                                        {
            //                                            newHumanWorkflowId = (long)humansChild.Properties["#HumanWorkflowID"];//new humanworkflowid
            //                                            found = true;
            //                                            foreach (SPWorkflowSubItemUnit approversChild in humansChild.ChildUnits)
            //                                            {
            //                                                if (approversChild.ItemType == WorkflowSubItemType.Custom && progressChild.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase))
            //                                                {
            //                                                    int tempTaskId = (int)approversChild.Properties["#SPTaskID"];
            //                                                    if (parentItem.Properties.ContainsKey("#tp_ID") && (int)parentItem.Properties["#tp_ID"] == tempTaskId) 
            //                                                    {
            //                                                        HandleHumanWorkflowApprovers(approversChild, conn, parentUnit.FixupParameters);
            //                                                    }
            //                                                }
            //                                            }
            //                                            break;
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                            if (found)
            //                                break;
            //                        }
            //                    }
            //                    if (found)
            //                        break;
            //                }
            //                if (found)
            //                    break;
            //            }
            //            #endregion

            //            if (newHumanWorkflowId > 0)
            //            {
            //                #region Add Replace Dictionary

            //                if (parentUnit.HasInstanceData)
            //                {
            //                    if (parentUnit.ParentAssociationUnit.mNonSerializedCustomData != null)
            //                    {
            //                        Dictionary<int, NintexActivityMemberInfo> humanWorkflowIdFields = (Dictionary<int, NintexActivityMemberInfo>)parentUnit.ParentAssociationUnit.mNonSerializedCustomData;
            //                        foreach (KeyValuePair<int, NintexActivityMemberInfo> pair in humanWorkflowIdFields)
            //                        {
            //                            if (!pair.Value.Flag)
            //                            {
            //                                foreach (string parameter in pair.Value.Parameters)
            //                                {
            //                                    int index = parameter.IndexOf('.');
            //                                    if (index < 0)
            //                                        continue;
            //                                    string profix = parameter.Substring(0, index);
            //                                    LS.BinarySerialization.Replacer.LSMemberDataInfo info = null;
            //                                    switch (profix)
            //                                    {
            //                                        case LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember:
            //                                            info = new LS.BinarySerialization.Replacer.LSMemberDataInfo(humanWorkflowId, newHumanWorkflowId, LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, parameter.Substring(index + 1));
            //                                            break;
            //                                        case LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfSetVariable:
            //                                            info = new LS.BinarySerialization.Replacer.LSMemberDataInfo(humanWorkflowId, newHumanWorkflowId, LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfSetVariable, "Variable");
            //                                            break;
            //                                        default:
            //                                            break;
            //                                    }
            //                                    if (info != null)
            //                                        parentUnit.FixupParameters.mCustomDic1.AddEx(parameter, info);
            //                                }
            //                                pair.Value.Flag = true;
            //                                break;
            //                            }
            //                        }
            //                    }

            //                    for (int i = 0; i < 1000; i++)
            //                    {
            //                        string taskItemIDKey = LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember + "." + i.ToString() + ".TaskListItemId";
            //                        if (parentUnit.FixupParameters.mCustomDic1.ContainsKey(taskItemIDKey))
            //                            continue;
            //                        parentUnit.FixupParameters.mCustomDic1.AddEx(taskItemIDKey, new LS.BinarySerialization.Replacer.LSMemberDataInfo(parentUnit.FixupParameters.mLastTaskItemIdDic.GetKey(0), parentUnit.FixupParameters.mLastTaskItemIdDic.GetValue(0), LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, "TaskListItemId"));
            //                        break;
            //                    }
            //                    //parentUnit.FixupParameters.mCustomDic1.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember + "TaskListItemId", new LS.BinarySerialization.Replacer.LSMemberDataInfo(parentUnit.FixupParameters.mLastTaskItemIdDic.GetKey(0), parentUnit.FixupParameters.mLastTaskItemIdDic.GetValue(0)));
            //                    LS.BinarySerialization.Replacer.LSMemberDataInfo dependInfo = new LS.BinarySerialization.Replacer.LSMemberDataInfo(humanWorkflowId, newHumanWorkflowId, LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty, "HumanWorkflowId");
            //                    parentUnit.FixupParameters.mCustomDic1.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty + ".HumanWorkflowId", dependInfo);
            //                }

            //                if (aveField == null || !parentItem.Properties.ContainsKey("#" + aveField.SerializableData.mDstColName) || (int)parentItem.Properties["#" + aveField.SerializableData.mDstColName] != humanWorkflowId)
            //                    return;
            //                parentItem.Properties["#" + aveField.SerializableData.mDstColName] = newHumanWorkflowId;
            //                #endregion
            //            }
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_NativeRestoreException, e.Message);
            //    }
            //    finally
            //    {
            //        if (conn != null)
            //            conn.Close();
            //        SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreCustomWorkflowData");
            //    }
            //}
        }

        /*private bool HandleWorkflowInstance(SPWorkflowSubItemUnit currentUnit, SqlConnection conn, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleWorkflowInstance");
            Hashtable data = currentUnit.Properties;
            long instanceIndex = 0;
            long oldInstanceIndex = (long)data["#InstanceID"];
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@SiteId", fixupParams.mSiteIdDic.GetValue(0));
                cmd.Parameters.AddWithValue("@WebId", fixupParams.mWebIdDic.GetValue(0));
                cmd.Parameters.AddWithValue("@ListId", fixupParams.mListIdDic.GetValue(0));
                cmd.Parameters.AddWithValue("@ItemId", fixupParams.mItemIdDic.GetValue(0));
                cmd.Parameters.AddWithValue("@WorkflowInstanceId", fixupParams.mInstanceIdDic.GetValue(0));
                cmd.CommandText = "SELECT InstanceID FROM WorkflowInstance WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND ItemId=@ItemId AND WorkflowInstanceId=@WorkflowInstanceId";
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    if (sdr.Read())
                    {
                        instanceIndex = sdr.GetInt64(0);
                    }
                }


                data["#InstanceID"] = instanceIndex;
                data["#SiteID"] = fixupParams.mSiteIdDic.GetValue(0);
                data["#ListID"] = fixupParams.mListIdDic.GetValue(0);
                data["#ItemID"] = fixupParams.mItemIdDic.GetValue(0);
                data["#WebID"] = fixupParams.mWebIdDic.GetValue(0);
                data["#WorkflowInstanceID"] = fixupParams.mInstanceIdDic.GetValue(0);
                data["#WorkflowID"] = fixupParams.mParentAssociationBaseIdDic.GetValue(0);
                data["#TaskListID"] = fixupParams.mTaskListIdDic.GetValue(0);
                data["#HistoryListID"] = fixupParams.mHistoryListIdDic.GetValue(0);
                data["#WebApplicationID"] = fixupParams.mWebApplicationIdDic.GetValue(0);
                if (SPWorkflowProcessorRuntime.RestoreHistoryOnly) 
                {
                    if (data.Contains("#State") && (int)data["#State"] == 2) 
                    {
                        data["#State"] = 8;
                    }
                }
                if (instanceIndex > 0)
                {
                    NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE WorkflowInstance SET {0} WHERE InstanceID=@InstanceID", ",InstanceID,", ",SiteId,WebId,ListId,ItemId,WorkflowInstanceId,TableInternalName,", data);
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) InstanceID FROM WorkflowInstance ORDER BY InstanceID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            instanceIndex = sdr.GetInt64(0);
                        }
                    }
                    instanceIndex = instanceIndex + 1;
                    data["#InstanceID"] = instanceIndex;
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "WorkflowInstance", data);
                }
                data.AddEx("~InstanceID", oldInstanceIndex);
                foreach (SPWorkflowSubItemUnit child in currentUnit.ChildUnits)
                {
                    if (child.ItemType == WorkflowSubItemType.Custom && child.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase))
                    {
                        child.Properties["#InstanceID"] = currentUnit.Properties["#InstanceID"];
                        HandleWorkflowProgress(child, conn, fixupParams);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, "WorkflowInstance", e.Message);
                return false;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleWorkflowInstance");
            }
        }*/

        /*private bool HandleWorkflowProgress(SPWorkflowSubItemUnit currentUnit, SqlConnection conn, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleWorkflowProgress");
            Hashtable data = currentUnit.Properties;
            long progressIndex = 0;
            long oldProgressIndex = (long)data["#WorkflowProgressID"];
            long instanceIndex = (long)data["#InstanceID"];
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@InstanceIndex", instanceIndex);
                cmd.Parameters.AddWithValue("@ActivityComplete", data["#ActivityComplete"]);
                cmd.Parameters.AddWithValue("@SequenceID", data["#SequenceID"]);
                cmd.CommandText = "SELECT TOP(1) WorkflowProgressID FROM WorkflowProgress WHERE InstanceId=@InstanceIndex AND ActivityComplete=@ActivityComplete AND SequenceID=@SequenceID ORDER BY WorkflowProgressID DESC";
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    if (sdr.Read())
                    {
                        progressIndex = sdr.GetInt64(0);
                    }
                }


                data["#WorkflowProgressID"] = progressIndex;
                data["#InstanceID"] = instanceIndex;
                if (progressIndex > 0)
                {

                    NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE WorkflowProgress SET {0} WHERE WorkflowProgressID=@WorkflowProgressID", ",WorkflowProgressID,", ",InstanceID,TableInternalName,", data);
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) WorkflowProgressID FROM WorkflowProgress ORDER BY WorkflowProgressID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            progressIndex = sdr.GetInt64(0);
                        }
                    }
                    progressIndex = progressIndex + 1;
                    data["#WorkflowProgressID"] = progressIndex;
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "WorkflowProgress", data);
                }

                data.AddEx("~WorkflowProgressID", oldProgressIndex);
                foreach (SPWorkflowSubItemUnit child in currentUnit.ChildUnits)
                {
                    if (child.ItemType == WorkflowSubItemType.Custom && child.UnitId.Equals("Nintex.HumanWorkflow", StringComparison.OrdinalIgnoreCase))
                    {
                        child.Properties["#WorkflowProgressID"] = currentUnit.Properties["#WorkflowProgressID"];
                        HandleHumanWorkflow(child, conn, fixupParams);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, "WorkflowProgress", e.Message);
                return false;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleWorkflowProgress");
            }
        }*/

        /*private bool HandleHumanWorkflow(SPWorkflowSubItemUnit currentUnit, SqlConnection conn, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleHumanWorkflow");
            Hashtable data = currentUnit.Properties;
            long humanWorkflowIndex = 0;
            long oldHumanWorkflowIndex = (long)data["#HumanWorkflowID"];
            long progressIndex = (long)data["#WorkflowProgressID"];
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@ProgressIndex", progressIndex);
                cmd.CommandText = "SELECT HumanWorkflowID FROM HumanWorkflow WHERE WorkflowProgressID=@ProgressIndex ORDER BY HumanWorkflowID DESC";
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    if (sdr.Read())
                    {
                        humanWorkflowIndex = sdr.GetInt64(0);
                    }
                }


                data["#HumanWorkflowID"] = humanWorkflowIndex;
                data["#WorkflowProgressID"] = progressIndex;
                if (humanWorkflowIndex > 0)
                {
                    NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE HumanWorkflow SET {0} WHERE HumanWorkflowID=@HumanWorkflowID", ",HumanWorkflowID,", ",WorkflowProgressID,TableInternalName,", data);
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) HumanWorkflowID FROM HumanWorkflow ORDER BY HumanWorkflowID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            humanWorkflowIndex = sdr.GetInt64(0);
                        }
                    }
                    humanWorkflowIndex = humanWorkflowIndex + 1;
                    data["#HumanWorkflowID"] = humanWorkflowIndex;
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "HumanWorkflow", data);
                }
                data.AddEx("~HumanWorkflowID", oldHumanWorkflowIndex);
                foreach (SPWorkflowSubItemUnit child in currentUnit.ChildUnits)
                {
                    if (child.ItemType == WorkflowSubItemType.Custom && child.UnitId.Equals("Nintex.HumanWorkflowApprovers", StringComparison.OrdinalIgnoreCase))
                    {
                        child.Properties["#HumanWorkflowID"] = currentUnit.Properties["#HumanWorkflowID"];
                        HandleHumanWorkflowApprovers(child, conn, fixupParams);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, "HumanWorkflow", e.Message);
                return false;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleHumanWorkflow");
            }
        }*/

        /*private long HandleHumanWorkflowApprovers(SPWorkflowSubItemUnit currentUnit, SqlConnection conn, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleHumanWorkflowApprovers");
            Hashtable data = currentUnit.Properties;
            long approverIndex = 0;
            int sourceSPTaskId = 0;
            int targetSPTaskId = 0;
            long humanWorkflowIndex = (long)data["#HumanWorkflowID"];
            try
            {
                if (data.ContainsKey("#SPTaskID"))
                {
                    sourceSPTaskId = (int)data["#SPTaskID"];
                    if (fixupParams.mLastTaskItemIdDic != null)
                    {
                        if (!fixupParams.mLastTaskItemIdDic.ContainsKey(sourceSPTaskId))
                            return 0;
                        else
                            targetSPTaskId = fixupParams.mLastTaskItemIdDic[sourceSPTaskId];
                    }
                }
                if (targetSPTaskId == 0)
                    return 0;

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@HumanWorkflowID", humanWorkflowIndex);
                cmd.Parameters.AddWithValue("@SPTaskID", targetSPTaskId);
                cmd.CommandText = "SELECT ApproverID FROM HumanWorkflowApprovers WHERE HumanWorkflowID=@HumanWorkflowID AND SPTaskID=@SPTaskID ORDER BY ApproverID DESC";
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    if (sdr.Read())
                    {
                        approverIndex = sdr.GetInt64(0);
                    }
                }

                data["#SPTaskID"] = targetSPTaskId;
                data["#ApproverID"] = approverIndex;
                data["#HumanWorkflowID"] = humanWorkflowIndex;

                if (data.ContainsKey("#Username"))
                {
                    string approverLogin = (string)data["#Username"];
                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(approverLogin);
                    if (user != null)
                        data["#Username"] = user.LoginName;
                }
                if (data.ContainsKey("#Username"))
                {
                    string approverLogin = (string)data["#Username"];
                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(approverLogin);
                    if (user != null)
                        data["#Username"] = user.LoginName;
                }
                if (approverIndex > 0)
                {
                    NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE HumanWorkflowApprovers SET {0} WHERE ApproverID=@ApproverID", ",ApproverID,", ",HumanWorkflowID,TableInternalName,", data);
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) ApproverID FROM HumanWorkflowApprovers ORDER BY ApproverID DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            approverIndex = sdr.GetInt64(0);
                        }
                    }
                    approverIndex = approverIndex + 1;
                    data["#ApproverID"] = approverIndex;
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "HumanWorkflowApprovers", data);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, "HumanWorkflowApprovers", e.Message);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleHumanWorkflowApprovers");
            }
            return approverIndex;
        }*/

        public void ResetData(SPWFInstanceUnit parentUnit)
        {
            if (parentUnit.HasInstanceData)
            {
                if (parentUnit.ParentAssociationUnit.mNonSerializedCustomData != null)
                {
                    Dictionary<int, NintexActivityMemberInfo> humanWorkflowIdFields = (Dictionary<int, NintexActivityMemberInfo>)parentUnit.ParentAssociationUnit.mNonSerializedCustomData;
                    foreach (KeyValuePair<int, NintexActivityMemberInfo> pair in humanWorkflowIdFields)
                    {
                        pair.Value.Flag = false;
                    }
                }
            }
        }

        public void OnSPInstanceDeleted(Guid siteId, List<Guid> instanceId)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "OnSPInstanceDeleted");
            if (IsNintexDllInstalled)
            {
                if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWDBConnectionString"))
                {
                    this.NintexDBConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionString"];
                }
                else
                {
                    this.NintexDBConnectionString = NintexDatabaseUtility.GetContentDBConnectionString(siteId);
                }
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ContentDBConnString, this.NintexDBConnectionString);

                SqlConnection conn = null;
                try
                {
                    List<long> nwInstanceId = new List<long>();
                    List<long> nwProgressId = new List<long>();
                    List<long> nwHumanWorkflowId = new List<long>();

                    string deleteCmdStr = string.Empty;

                    conn = new SqlConnection();
                    conn.ConnectionString = this.NintexDBConnectionString;
                    conn.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    deleteCmdStr = "DELETE FROM WorkflowInstance WHERE WorkflowInstanceID=@WorkflowInstanceID";
                    cmd.CommandText = "SELECT InstanceID FROM WorkflowInstance WHERE WorkflowInstanceID=@WorkflowInstanceID";
                    cmd.Parameters.AddWithValue("@WorkflowInstanceID", Guid.Empty);
                    foreach (Guid wfInstanceId in instanceId)
                    {
                        cmd.Parameters["@WorkflowInstanceID"].Value = wfInstanceId;
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                nwInstanceId.Add(sdr.GetInt64(0));
                            }
                        }
                        cmd.CommandText = deleteCmdStr;
                        cmd.ExecuteNonQuery();
                    }

                    cmd.Parameters.Clear();
                    deleteCmdStr = "DELETE FROM WorkflowProgress WHERE InstanceID=@InstanceID";
                    cmd.CommandText = "SELECT WorkflowProgressID FROM WorkflowProgress WHERE InstanceID=@InstanceID";
                    cmd.Parameters.AddWithValue("@InstanceID", 0L);
                    foreach (long id in nwInstanceId)
                    {
                        cmd.Parameters["@InstanceID"].Value = id;
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                nwProgressId.Add(sdr.GetInt64(0));
                            }
                        }
                        cmd.CommandText = deleteCmdStr;
                        cmd.ExecuteNonQuery();
                    }


                    cmd.Parameters.Clear();
                    deleteCmdStr = "DELETE FROM HumanWorkflow WHERE WorkflowProgressID=@WorkflowProgressID";
                    cmd.CommandText = "SELECT HumanWorkflowID FROM HumanWorkflow WHERE WorkflowProgressID=@WorkflowProgressID";
                    cmd.Parameters.AddWithValue("@WorkflowProgressID", 0L);
                    foreach (long id in nwProgressId)
                    {
                        cmd.Parameters["@WorkflowProgressID"].Value = id;
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                nwHumanWorkflowId.Add(sdr.GetInt64(0));
                            }
                        }
                        cmd.CommandText = deleteCmdStr;
                        cmd.ExecuteNonQuery();
                    }


                    cmd.Parameters.Clear();
                    cmd.CommandText = "DELETE FROM HumanWorkflowApprovers WHERE HumanWorkflowID=@HumanWorkflowID";
                    cmd.Parameters.AddWithValue("@HumanWorkflowID", 0L);
                    foreach (long id in nwHumanWorkflowId)
                    {
                        cmd.Parameters["@HumanWorkflowID"].Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_NativeRestoreException, e.Message);
                }
                finally
                {
                    if (conn != null)
                        conn.Close();
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "OnSPInstanceDeleted");
                }
            }
        }
        #endregion
    }
}

