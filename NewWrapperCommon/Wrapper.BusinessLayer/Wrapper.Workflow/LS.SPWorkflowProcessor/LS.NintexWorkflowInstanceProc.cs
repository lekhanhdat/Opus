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
using AvePoint.Wrapper.Resource.ServerAPI2010;
namespace LS.SPWorkflowProcessor
{
    public class NintexWorkflowInstanceProc : ICustomWorkflowInstanceProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //private static string assemblyString = "Nintex.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=913f6bae0ca5ae12";
        //private static Assembly assembly;

        //public static bool IsNintexDllInstalled
        //{
        //    get
        //    {
        //        try
        //        {
        //            if (assembly != null)
        //                return true;
        //            if (SPWorkflowProcessorRuntime.AllProcessorParams != null && SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWAssemblyName"))
        //            {
        //                assemblyString = SPWorkflowProcessorRuntime.AllProcessorParams["NWAssemblyName"];
        //            }
        //            assembly = Assembly.Load(assemblyString);
        //            SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_IsInstalled, "true");
        //            return true;
        //        }
        //        catch(Exception e)
        //        {
        //            log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.LoadAssemblyError, e.Message);
        //            SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_IsInstalled, "false");
        //            return false;
        //        }
        //    }
        //}

        private const string Nintex_HumanWorkflowIDColStaticName = "HumanWorkflowID";
        public string NintexDBConnectionString
        {
            get;
            set;
        }


        #region Backup Region
        public void BackupCustomWorkflowData(SPWorkflowSubItemUnit parentUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "NWDBConnectionStringOfBackup");

            switch (parentUnit.ItemType)
            {
                case WorkflowSubItemType.Instance:
                    if (EnsureNintexContentDBConnection(null, parentUnit, true))
                    {
                        BackupNintexWorkflowInstance(parentUnit);
                    }
                    break;
                case WorkflowSubItemType.Schedule:
                    if (EnsureNintexConfigDBConnection())
                    {
                        BackupNintexWorkflowSchedule(parentUnit);
                    }
                    break;
                default:
                    break;
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupCustomWorkflowData");
        }

        private void BackupNintexWorkflowSchedule(SPWorkflowSubItemUnit parentNWInstanceUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupNintexWorkflowSchedule"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexConfigDBConnection;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@WorkflowID", parentNWInstanceUnit.Properties["#WorkflowID"]);
                cmd.Parameters.AddWithValue("@SiteID", parentNWInstanceUnit.Properties["#SiteID"]);
                cmd.Parameters.AddWithValue("@WebID", parentNWInstanceUnit.Properties["#WebID"]);
                cmd.Parameters.AddWithValue("@ListID", parentNWInstanceUnit.Properties["#ListID"]);
                cmd.Parameters.AddWithValue("@ItemID", parentNWInstanceUnit.Properties["#ItemID"]);
                cmd.CommandText = "SELECT * FROM WorkflowSchedule WITH(NOLOCK) WHERE WorkflowID=@WorkflowID AND SiteId=@SiteID AND WebId=@WebID AND ListId=@ListID AND ItemId=@ItemID";

                using (SqlDataAdapter nwAdapter = new SqlDataAdapter())
                {
                    nwAdapter.SelectCommand = cmd;
                    using (DataTable nwMemoryTable = new DataTable())
                    {
                        nwAdapter.Fill(nwMemoryTable);
                        foreach (DataRow dr in nwMemoryTable.Rows)
                        {
                            SPWorkflowSubItemUnit tempUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Schedule, parentNWInstanceUnit);
                            tempUnit.UnitId = "Nintex.WorkflowSchedule";
                            tempUnit.SetPropsFromDataRow(dr, nwMemoryTable.Columns);
                            parentNWInstanceUnit.ChildUnits.Add(tempUnit);
                        }
                        nwMemoryTable.Clear();
                    }
                }
            }
        }

        private void BackupNintexWorkflowInstance(SPWorkflowSubItemUnit parentInstanceUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupNintexWorkflowInstance"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@WorkflowInstanceID", parentInstanceUnit.Properties["#Id"]);
                cmd.CommandText = "SELECT * FROM WorkflowInstance WITH(NOLOCK) WHERE WorkflowInstanceID=@WorkflowInstanceID";


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
                            BackupNintexWorkflowProgress(tempUnit);
                            parentInstanceUnit.ChildUnits.Add(tempUnit);
                        }
                        nwMemoryTable.Clear();
                    }
                }
            }
        }

        private void BackupNintexWorkflowProgress(SPWorkflowSubItemUnit parentNWInstanceUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupNintexWorkflowInstance.BackupNintexWorkflowProgress"))
            {
                if (SPWorkflowProcessorRuntime.NintexWorkflowMaxBackupInstanceProgressCount == 0)
                {
                    return;
                }

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@InstanceID", parentNWInstanceUnit.Properties["#InstanceID"]);
                cmd.CommandText = string.Format("SELECT TOP {0} * FROM WorkflowProgress WITH(NOLOCK) WHERE InstanceID=@InstanceID order by WorkflowProgressID", SPWorkflowProcessorRuntime.NintexWorkflowMaxBackupInstanceProgressCount);


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
                            BackupHumanWorkflowCustomOutcomeName(tempUnit);
                            BackupNintexHumanWorkflow(tempUnit);
                            parentNWInstanceUnit.ChildUnits.Add(tempUnit);
                        }
                        nwMemoryTable.Clear();
                    }
                }
            }
        }

        private void BackupHumanWorkflowCustomOutcomeName(SPWorkflowSubItemUnit tempUnit)
        {
            try
            {
                if (tempUnit.Properties.ContainsKey("#CustomOutcome"))
                {
                    string outcoumeId = tempUnit.Properties["#CustomOutcome"].ToString();
                    string databaseName = SPWorkflowProcessorRuntime.NintexContentDBConnection.Database;
                    Dictionary<string, string> idNames;
                    if (!SPWorkflowProcessorRuntime.CustomOutcomeIDNames.ContainsKey(databaseName))
                    {
                        idNames = GetCustomOutcomeIdNames(true);
                        SPWorkflowProcessorRuntime.CustomOutcomeIDNames[databaseName] = idNames;
                    }
                    else
                    {
                        idNames = SPWorkflowProcessorRuntime.CustomOutcomeIDNames[databaseName];
                    }
                    if (idNames.ContainsKey(outcoumeId))
                    {
                        tempUnit.Properties.Add("#CustomOutcomeName", idNames[outcoumeId]);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("BackupHumanWorkflowCustomOutcomeName error.error is:{0}", ex);
            }
        }
        private Dictionary<string, string> GetCustomOutcomeIdNames(bool idAsKey)
        {
            Dictionary<string, string> idNames = new Dictionary<string, string>();
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT * FROM ConfiguredOutcomes WITH(NOLOCK)";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (idAsKey)
                            {
                                idNames.Add(reader[0].ToString(), reader[1].ToString());
                            }
                            else
                            {
                                idNames.Add(reader[1].ToString(), reader[0].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetCustomOutcomeIdNames error.Error is:{0}", ex);
            }
            return idNames;
        }

        private void BackupNintexHumanWorkflow(SPWorkflowSubItemUnit parentProgressUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupNintexWorkflowInstance.BackupNintexHumanWorkflow"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@WorkflowProgressID", (long)parentProgressUnit.Properties["#WorkflowProgressID"]);
                cmd.CommandText = "SELECT * FROM HumanWorkflow WITH(NOLOCK) WHERE WorkflowProgressID=@WorkflowProgressID";

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
                            BackupNintexHumanWorkflowApprovers(tempUnit);
                            parentProgressUnit.ChildUnits.Add(tempUnit);
                        }
                        nwMemoryTable.Clear();
                    }
                }
            }
        }

        private void BackupNintexHumanWorkflowApprovers(SPWorkflowSubItemUnit parentNWHumanWorkflow)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupNintexWorkflowInstance.BackupNintexHumanWorkflowApprovers"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@HumanWorkflowID", parentNWHumanWorkflow.Properties["#HumanWorkflowID"]);
                cmd.CommandText = "SELECT * FROM HumanWorkflowApprovers WITH(NOLOCK) WHERE HumanWorkflowID=@HumanWorkflowID";


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
                            BackupHumanWorkflowCustomOutcomeName(tempUnit);
                            parentNWHumanWorkflow.ChildUnits.Add(tempUnit);
                        }
                        nwMemoryTable.Clear();
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentUnit"></param>
        /// <param name="parentItem"></param>
        /// <param name="isBackup">isBackup=true means in backup function, isBackup=false means in restore function</param>
        private bool EnsureNintexContentDBConnection(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem, bool isBackup)
        {
            if (!SPWorkflowProcessorRuntime.HasSetNintexContentDBConnection && SPWorkflowProcessorRuntime.NintexContentDBConnection == null)
            {
                if (!SPWorkflowProcessorRuntime.IsNintexDllInstalled)
                {
                    return false;
                }
                if (isBackup)
                {
                    logger.Info("Begin to get Nintex Content DB.");
                    if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWContentDBConnectionStringOfBackup"))
                    {
                        this.NintexDBConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWContentDBConnectionStringOfBackup"];
                    }
                    else
                    {
                        Guid siteId = Guid.Empty;
                        if (parentItem.ItemType == WorkflowSubItemType.Task)
                        {
                            siteId = (Guid)parentItem.Properties["~0_tp_SiteId"];
                        }
                        else if (parentItem.ItemType == WorkflowSubItemType.Instance)
                        {
                            siteId = (Guid)parentItem.Properties["#SiteId"];
                        }
                        else
                        {
                            logger.Warn("cannot get siteId in EnsureNintexContentDBConnection.");
                        }
                        this.NintexDBConnectionString = NintexDatabaseUtility.GetContentDBConnectionString(siteId);
                    }
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ContentDBConnString, this.NintexDBConnectionString);
                    if (string.IsNullOrEmpty(this.NintexDBConnectionString))
                    {
                        return false;
                    }
                    SPWorkflowProcessorRuntime.NintexContentDBConnection = new SqlConnection(this.NintexDBConnectionString);
                    SPWorkflowProcessorRuntime.NintexContentDBConnection.Open();
                    logger.Info("Get Nintex Content DB successfully.{0}",AvePoint.Wrapper.Common.AveQueryException.InternalCrypto.EncryptMessage(SPWorkflowProcessorRuntime.NintexContentDBConnection.ConnectionString));
                }
                else
                {
                    logger.Info("Begin to get Nintex Content DB.");
                    if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWDBConnectionStringOfRestore") && (parentItem.ItemType != WorkflowSubItemType.Schedule))
                    {
                        this.NintexDBConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionStringOfRestore"];
                    }

                    this.NintexDBConnectionString = NintexDatabaseUtility.GetContentDBConnectionString(parentUnit.FixupParameters.mSiteIdDic.GetValue(0));
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ContentDBConnString, this.NintexDBConnectionString);

                    SPWorkflowProcessorRuntime.NintexContentDBConnection = new SqlConnection(this.NintexDBConnectionString);
                    SPWorkflowProcessorRuntime.NintexContentDBConnection.Open();
                    logger.Info("Get Nintex Content DB successfully.{0}", AvePoint.Wrapper.Common.AveQueryException.InternalCrypto.EncryptMessage(SPWorkflowProcessorRuntime.NintexContentDBConnection.ConnectionString));
                }
            }
            if (string.IsNullOrEmpty(this.NintexDBConnectionString))
            {
                return false;
            }
            return true;
        }

        private bool EnsureNintexConfigDBConnection(bool isRestore = false)
        {
            if (!SPWorkflowProcessorRuntime.HasSetNintexConfigDBConnection && SPWorkflowProcessorRuntime.NintexConfigDBConnection == null)
            {
                if (SPWorkflowProcessorRuntime.IsNintexDllInstalled)
                {
                    if (!isRestore && SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWConfigDBConnectionStringOfBackup"))
                    {
                        this.NintexDBConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWConfigDBConnectionStringOfBackup"];
                    }
                    else
                    {
                        this.NintexDBConnectionString = NintexDatabaseUtility.GetConfigDBConnectionString();
                    }

                    if (string.IsNullOrEmpty(this.NintexDBConnectionString))
                    {
                        return false;
                    }
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ContentDBConnString, this.NintexDBConnectionString);
                    SPWorkflowProcessorRuntime.NintexConfigDBConnection = new SqlConnection(this.NintexDBConnectionString);
                    SPWorkflowProcessorRuntime.NintexConfigDBConnection.Open();
                    logger.Info("Get Nintex Config DB successfully.{0}", AvePoint.Wrapper.Common.AveQueryException.InternalCrypto.EncryptMessage(SPWorkflowProcessorRuntime.NintexConfigDBConnection.ConnectionString));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        #endregion



        #region Restore Region

        public void RestoreCustomWorkflowData(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreCustomWorkflowData");
            if (SPWorkflowProcessorRuntime.IsNintexDllInstalled)
            {
                try
                {
                    if (parentItem.ItemType == WorkflowSubItemType.Instance)
                    {
                        if (!EnsureNintexContentDBConnection(parentUnit, parentItem, false))
                        {
                            return;
                        }

                        foreach (SPWorkflowSubItemUnit instanceChild in parentItem.ChildUnits)
                        {
                            if (instanceChild.ItemType == WorkflowSubItemType.Custom && instanceChild.UnitId.Equals("Nintex.WorkflowInstance", StringComparison.OrdinalIgnoreCase))
                            {
                                //Nintex Workflow在显示action时,是根据Workflow Instance表中XOML和Rules字段中的数值来选择xoml,rules文件的version
                                if (!string.IsNullOrEmpty(parentUnit.ParentAssociationUnit.XomlVersionLabel) && instanceChild.Properties.Contains("#XOML"))
                                {
                                    instanceChild.Properties["#XOML"] = parentUnit.ParentAssociationUnit.XomlVersionLabel.Substring(0, parentUnit.ParentAssociationUnit.XomlVersionLabel.IndexOf('.'));
                                }
                                if (!string.IsNullOrEmpty(parentUnit.ParentAssociationUnit.RulesVersionLabel) && instanceChild.Properties.Contains("#Rules"))
                                {
                                    instanceChild.Properties["#Rules"] = parentUnit.ParentAssociationUnit.RulesVersionLabel.Substring(0, parentUnit.ParentAssociationUnit.RulesVersionLabel.IndexOf('.'));
                                }
                                //
                                HandleWorkflowInstance(instanceChild, parentUnit.FixupParameters);
                            }
                        }
                    }
                    else if (parentItem.ItemType == WorkflowSubItemType.Task)
                    {
                        AveSPField aveField = null;
                        long humanWorkflowId = 0;
                        long newHumanWorkflowId = 0;
                        if (parentUnit.ParentAssociationUnit.mTaskListUnit.FieldProcessor.AveFieldCollection[Nintex_HumanWorkflowIDColStaticName] == null)
                        {
                            humanWorkflowId = -1;
                        }
                        else
                        {
                            aveField = parentUnit.ParentAssociationUnit.mTaskListUnit.FieldProcessor.AveFieldCollection.GetAveFieldByInternalName(Nintex_HumanWorkflowIDColStaticName);
                            if (!parentItem.Properties.Contains("#" + aveField.SerializableData.mDstColName))
                            {
                                humanWorkflowId = -1;
                            }
                            else
                            {
                                humanWorkflowId = (long)(int)parentItem.Properties["#" + aveField.SerializableData.mDstColName];
                            }
                        }
                        #region Find New Human Workflow ID
                        bool found = false;
                        foreach (SPWorkflowSubItemUnit insChild in parentItem.ParentUnit.ChildUnits)
                        {
                            if (insChild.ItemType == WorkflowSubItemType.Custom && insChild.UnitId.Equals("Nintex.WorkflowInstance", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (SPWorkflowSubItemUnit progressChild in insChild.ChildUnits)
                                {
                                    if (progressChild.ItemType == WorkflowSubItemType.Custom && progressChild.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase))
                                    {
                                        foreach (SPWorkflowSubItemUnit humansChild in progressChild.ChildUnits)
                                        {
                                            if (humansChild.ItemType == WorkflowSubItemType.Custom && humansChild.UnitId.Equals("Nintex.HumanWorkflow", StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (humansChild.Properties.Contains("~HumanWorkflowID"))
                                                {
                                                    long temp = (long)humansChild.Properties["~HumanWorkflowID"];//old humanworkflowid
                                                    if (temp == humanWorkflowId)
                                                    {
                                                        newHumanWorkflowId = (long)humansChild.Properties["#HumanWorkflowID"];//new humanworkflowid
                                                        found = true;
                                                        foreach (SPWorkflowSubItemUnit approversChild in humansChild.ChildUnits)
                                                        {
                                                            if (approversChild.ItemType == WorkflowSubItemType.Custom && progressChild.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                int tempTaskId = (int)approversChild.Properties["#SPTaskID"];
                                                                if (parentItem.Properties.ContainsKey("#tp_ID") && (int)parentItem.Properties["#tp_ID"] == tempTaskId)
                                                                {
                                                                    HandleHumanWorkflowApprovers(approversChild, parentUnit.FixupParameters);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    }
                                                    else if (humanWorkflowId == -1)
                                                    {
                                                        newHumanWorkflowId = (long)humansChild.Properties["#HumanWorkflowID"];//new humanworkflowid
                                                        foreach (SPWorkflowSubItemUnit approversChild in humansChild.ChildUnits)
                                                        {
                                                            if (approversChild.ItemType == WorkflowSubItemType.Custom && progressChild.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                int tempTaskId = (int)approversChild.Properties["#SPTaskID"];
                                                                if (parentItem.Properties.ContainsKey("#tp_ID") && (int)parentItem.Properties["#tp_ID"] == tempTaskId)
                                                                {
                                                                    found = true;
                                                                    humanWorkflowId = (long)humansChild.Properties["~HumanWorkflowID"];
                                                                    HandleHumanWorkflowApprovers(approversChild, parentUnit.FixupParameters);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        if (found)
                                            break;
                                    }
                                }
                                if (found)
                                    break;
                            }
                            if (found)
                                break;
                        }
                        #endregion

                        #region Add Replace Dictionary

                        if (parentUnit.HasInstanceData)
                        {
                            if (parentUnit.ParentAssociationUnit.mNonSerializedCustomData != null
                                && parentUnit.ParentAssociationUnit.mNonSerializedCustomData is Dictionary<string, NintexActivityMemberInfo>)
                            {
                                Dictionary<string, NintexActivityMemberInfo> customData = (Dictionary<string, NintexActivityMemberInfo>)parentUnit.ParentAssociationUnit.mNonSerializedCustomData;
                                foreach (KeyValuePair<string, NintexActivityMemberInfo> pair in customData)
                                {
                                    if (!pair.Value.Flag)
                                    {
                                        switch (pair.Key)
                                        {
                                            case "HumanWorkflowId":
                                                if (humanWorkflowId > 0 && newHumanWorkflowId > 0)
                                                {
                                                    foreach (string parameter in pair.Value.Parameters)
                                                    {
                                                        LS.BinarySerialization.Replacer.LSMemberDataInfo dependInfo = new LS.BinarySerialization.Replacer.LSMemberDataInfo(humanWorkflowId, newHumanWorkflowId, LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty, parameter);
                                                        parentUnit.FixupParameters.mCustomDic1.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty + parameter, dependInfo);
                                                    }
                                                }
                                                break;
                                            case "TaskListItemId":
                                                foreach (string parameter in pair.Value.Parameters)
                                                {
                                                    LS.BinarySerialization.Replacer.LSMemberDataInfo dependInfo = new LS.BinarySerialization.Replacer.LSMemberDataInfo(parentUnit.FixupParameters.mLastTaskItemIdDic.GetKey(0), parentUnit.FixupParameters.mLastTaskItemIdDic.GetValue(0), LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty, parameter);
                                                    parentUnit.FixupParameters.mCustomDic1.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty + parameter, dependInfo);
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        //foreach (string parameter in pair.Value.Parameters)
                                        //{
                                        //    int index = parameter.IndexOf('.');
                                        //    if (index < 0)
                                        //        continue;
                                        //    string profix = parameter.Substring(0, index);
                                        //    LS.BinarySerialization.Replacer.LSMemberDataInfo info = null;
                                        //    switch (profix)
                                        //    {
                                        //        case LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember:
                                        //            info = new LS.BinarySerialization.Replacer.LSMemberDataInfo(humanWorkflowId, newHumanWorkflowId, LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, parameter.Substring(index + 1));
                                        //            break;
                                        //        case LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfSetVariable:
                                        //            info = new LS.BinarySerialization.Replacer.LSMemberDataInfo(humanWorkflowId, newHumanWorkflowId, LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfSetVariable, "Variable");
                                        //            break;
                                        //        default:
                                        //            break;
                                        //    }
                                        //    if (info != null)
                                        //        parentUnit.FixupParameters.mCustomDic1.AddEx(parameter, info);
                                        //}
                                        pair.Value.Flag = true;
                                        //break;
                                    }
                                }
                            }

                            for (int i = 0; i < 1000; i++)
                            {
                                string taskItemIDKey = LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember + "." + i.ToString() + ".TaskListItemId";
                                if (parentUnit.FixupParameters.mCustomDic1.ContainsKey(taskItemIDKey))
                                    continue;
                                parentUnit.FixupParameters.mCustomDic1.AddEx(taskItemIDKey, new LS.BinarySerialization.Replacer.LSMemberDataInfo(parentUnit.FixupParameters.mLastTaskItemIdDic.GetKey(0), parentUnit.FixupParameters.mLastTaskItemIdDic.GetValue(0), LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, "TaskListItemId"));
                                break;
                            }
                            if (humanWorkflowId > 0 && newHumanWorkflowId > 0)
                            {
                                LS.BinarySerialization.Replacer.LSMemberDataInfo dependInfo1 = new LS.BinarySerialization.Replacer.LSMemberDataInfo(humanWorkflowId, newHumanWorkflowId, LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty, "HumanWorkflowId");
                                parentUnit.FixupParameters.mCustomDic1.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty + ".HumanWorkflowId", dependInfo1);
                            }
                        }

                        if (aveField == null || !parentItem.Properties.ContainsKey("#" + aveField.SerializableData.mDstColName) || (int)parentItem.Properties["#" + aveField.SerializableData.mDstColName] != humanWorkflowId)
                            return;
                        parentItem.Properties["#" + aveField.SerializableData.mDstColName] = newHumanWorkflowId;
                        #endregion
                    }
                    else if (parentItem.ItemType == WorkflowSubItemType.Schedule)
                    {
                        if (!EnsureNintexConfigDBConnection(true))
                        {
                            return;
                        }
                        foreach (SPWorkflowSubItemUnit scheduleChild in parentItem.ChildUnits)
                        {
                            if (scheduleChild.ItemType == WorkflowSubItemType.Schedule && scheduleChild.UnitId.Equals("Nintex.WorkflowSchedule", StringComparison.OrdinalIgnoreCase))
                            {
                                HandleWorkflowSchedule(scheduleChild, parentUnit.FixupParameters);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_NativeRestoreException, e.Message);
                    logger.Warn("An exception occurred while restore custom workflow data. exception:{0}", e.ToString());
                }
                finally
                {                   
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreCustomWorkflowData");
                }
            }
        }

        private bool HandleWorkflowSchedule(SPWorkflowSubItemUnit currentUnit, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleWorkflowSchedule");
            Hashtable data = currentUnit.Properties;
            int scheduleId = 0;
            data["#ListId"] = fixupParams.mListIdDic.GetValue(0);
            data["#WebId"] = fixupParams.mWebIdDic.GetValue(0);
            data["#SiteID"] = fixupParams.mSiteIdDic.GetValue(0);
            data["#ItemID"] = fixupParams.mItemIdDic.GetValue(0);
            data["#WorkflowID"] = fixupParams.mParentAssociationBaseIdDic.GetValue(0);
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexConfigDBConnection;
                cmd.Parameters.AddWithValue("@ListId", data["#ListId"]);
                cmd.Parameters.AddWithValue("@WebId", data["#WebId"]);
                cmd.Parameters.AddWithValue("@SiteID", data["#SiteID"]);
                cmd.Parameters.AddWithValue("@ItemID", data["#ItemID"]);
                cmd.Parameters.AddWithValue("@WorkflowID", data["#WorkflowID"]);
                cmd.CommandText = "SELECT TOP(1) ScheduleId FROM WorkflowSchedule WITH(NOLOCK) WHERE ItemId=@ItemID AND ListId=@ListId AND WebId=@WebId AND SiteId=@SiteId AND WorkflowId=@WorkflowId ORDER BY ScheduleId DESC";
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    if (sdr.Read())
                    {
                        scheduleId = sdr.GetInt32(0);
                    }
                }

                if (scheduleId > 0)
                {

                    NintexDatabaseUtility.ExecuteUpdateCommand(cmd, "UPDATE WorkflowSchedule SET {0} WHERE ItemId=@ItemId AND ListId=@ListId AND WebId=@WebId AND SiteId=@SiteId AND WorkflowId=@WorkflowId", ",ItemId,ListId,WebId,SiteId,WorkflowId,", ",ScheduleId,", data);
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT TOP(1) ScheduleId FROM WorkflowSchedule WITH(NOLOCK) ORDER BY ScheduleId DESC";
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            scheduleId = sdr.GetInt32(0);
                        }
                    }
                    scheduleId = scheduleId + 1;
                    data["#ScheduleId"] = scheduleId;
                    NintexDatabaseUtility.ExecuteInsertCommand(cmd, "WorkflowSchedule", data);
                }
                return true;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, "WorkflowSchedule", e.Message);
                logger.Warn("An exception occurred while handle nintex workflow schedule. exception:{0}", e.ToString());
                return false;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleWorkflowSchedule");
            }
        }

        private bool HandleWorkflowInstance(SPWorkflowSubItemUnit currentUnit, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleWorkflowInstance");
            Hashtable data = currentUnit.Properties;
            long instanceIndex = 0;
            long oldInstanceIndex = (long)data["#InstanceID"];
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;
                cmd.Parameters.AddWithValue("@SiteId", fixupParams.mSiteIdDic.GetValue(0));
                cmd.Parameters.AddWithValue("@WebId", fixupParams.mWebIdDic.GetValue(0));
                cmd.Parameters.AddWithValue("@ListId", fixupParams.mListIdDic.GetValue(0));
                cmd.Parameters.AddWithValue("@ItemId", fixupParams.mItemIdDic.GetValue(0));
                cmd.Parameters.AddWithValue("@WorkflowInstanceId", fixupParams.mInstanceIdDic.GetValue(0));
                cmd.CommandText = "SELECT InstanceID FROM WorkflowInstance WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND ItemId=@ItemId AND WorkflowInstanceId=@WorkflowInstanceId";
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
                    cmd.CommandText = "SELECT TOP(1) InstanceID FROM WorkflowInstance WITH(NOLOCK) ORDER BY InstanceID DESC";
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
                try
                {
                    currentUnit.ChildUnits.Sort(new SPWorkflowSubItemUnitComparer());
                    logger.Info("Sort nintex workflow progress.");
                }
                catch (Exception sortEx)
                {
                    logger.Error("Sort exception: {0}.", sortEx.ToString());
                }
                foreach (SPWorkflowSubItemUnit child in currentUnit.ChildUnits)
                {
                    if (child.ItemType == WorkflowSubItemType.Custom && child.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase))
                    {
                        child.Properties["#InstanceID"] = currentUnit.Properties["#InstanceID"];
                        HandleWorkflowProgress(child, fixupParams);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, "WorkflowInstance", e.Message);
                logger.Warn("An exception occurred while handle workflow instance. exception:{0}", e.ToString());
                return false;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleWorkflowInstance");
            }
        }

        internal class SPWorkflowSubItemUnitComparer : IComparer<SPWorkflowSubItemUnit>
        {
            public int Compare(SPWorkflowSubItemUnit x, SPWorkflowSubItemUnit y)
            {
                if (x.ItemType == WorkflowSubItemType.Custom && x.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase)
                    && y.ItemType == WorkflowSubItemType.Custom && y.UnitId.Equals("Nintex.WorkflowProgress", StringComparison.OrdinalIgnoreCase))
                {
                    if (x.Properties.ContainsKey("#WorkflowProgressID") && y.Properties.ContainsKey("#WorkflowProgressID"))
                    {
                        if (Int64.Parse(x.Properties["#WorkflowProgressID"].ToString()) > Int64.Parse(y.Properties["#WorkflowProgressID"].ToString()))
                        {
                            return 1;
                        }
                        else if (Int64.Parse(x.Properties["#WorkflowProgressID"].ToString()) < Int64.Parse(y.Properties["#WorkflowProgressID"].ToString()))
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else if (x.Properties.ContainsKey("#TimeStamp") && y.Properties.ContainsKey("#TimeStamp"))
                    {
                        if (((DateTime)x.Properties["#TimeStamp"]).Ticks > ((DateTime)y.Properties["#TimeStamp"]).Ticks)
                        {
                            return 1;
                        }
                        else if (((DateTime)x.Properties["#TimeStamp"]).Ticks < ((DateTime)y.Properties["#TimeStamp"]).Ticks)
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
                return 0;
            }
        }

        private bool HandleWorkflowProgress(SPWorkflowSubItemUnit currentUnit, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleWorkflowProgress");
            Hashtable data = currentUnit.Properties;
            long progressIndex = 0;
            long oldProgressIndex = (long)data["#WorkflowProgressID"];
            logger.Debug("WorkflowProgressID is:{0}", data["#WorkflowProgressID"]);
            long instanceIndex = (long)data["#InstanceID"];
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;
                cmd.Parameters.AddWithValue("@InstanceIndex", instanceIndex);
                cmd.Parameters.AddWithValue("@ActivityComplete", data["#ActivityComplete"]);
                cmd.Parameters.AddWithValue("@SequenceID", data["#SequenceID"]);
                cmd.CommandText = "SELECT TOP(1) WorkflowProgressID FROM WorkflowProgress WITH(NOLOCK) WHERE InstanceId=@InstanceIndex AND ActivityComplete=@ActivityComplete AND SequenceID=@SequenceID ORDER BY WorkflowProgressID DESC";
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
                    cmd.CommandText = "SELECT TOP(1) WorkflowProgressID FROM WorkflowProgress WITH(NOLOCK) ORDER BY WorkflowProgressID DESC";
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
                        HandleHumanWorkflow(child, fixupParams);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, "WorkflowProgress", e.Message);
                logger.Warn("An exception occurred while handle nintex workflow progress. exception:{0}", e.ToString());
                return false;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleWorkflowProgress");
            }
        }

        private bool HandleHumanWorkflow(SPWorkflowSubItemUnit currentUnit, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleHumanWorkflow");
            Hashtable data = currentUnit.Properties;
            HandleHumanworkflowCustomOutcome(data);
            long humanWorkflowIndex = 0;
            long oldHumanWorkflowIndex = (long)data["#HumanWorkflowID"];
            long progressIndex = (long)data["#WorkflowProgressID"];
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;
                cmd.Parameters.AddWithValue("@ProgressIndex", progressIndex);
                cmd.CommandText = "SELECT HumanWorkflowID FROM HumanWorkflow WITH(NOLOCK) WHERE WorkflowProgressID=@ProgressIndex ORDER BY HumanWorkflowID DESC";
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
                    cmd.CommandText = "SELECT TOP(1) HumanWorkflowID FROM HumanWorkflow WITH(NOLOCK) ORDER BY HumanWorkflowID DESC";
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
                        HandleHumanWorkflowApprovers(child, fixupParams);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, "HumanWorkflow", e.Message);
                logger.Warn("An exception occurred while handle nintex human workflow. exception:{0}", e.ToString());
                return false;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleHumanWorkflow");
            }
        }

        private void HandleHumanworkflowCustomOutcome(Hashtable data)
        {
            try
            {
                if (data.ContainsKey("#CustomOutcomeName"))
                {
                    string databaseName = SPWorkflowProcessorRuntime.NintexContentDBConnection.Database;
                    string outcomeName = data["#CustomOutcomeName"].ToString();
                    Dictionary<string, string> nameIds = new Dictionary<string, string>();
                    if (!SPWorkflowProcessorRuntime.CustomOutcomeNameIDs.ContainsKey(databaseName))
                    {
                        nameIds = GetCustomOutcomeIdNames(false);
                    }
                    else
                    {
                        nameIds = SPWorkflowProcessorRuntime.CustomOutcomeNameIDs[databaseName];
                    }
                    if (!nameIds.ContainsKey(outcomeName))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;
                            cmd.CommandText = "INSERT INTO ConfiguredOutcomes(Name) VALUES ('" + outcomeName + "')";
                            cmd.ExecuteNonQuery();
                        }
                        nameIds = GetCustomOutcomeIdNames(false);
                    }
                    if (nameIds.ContainsKey(outcomeName))
                    {
                        data["#CustomOutcome"] = nameIds[outcomeName];
                    }
                    data.Remove("#CustomOutcomeName");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("HandleHumanworkflowCustomOutcome error.Error is:{0}", ex);
            }
        }

        private long HandleHumanWorkflowApprovers(SPWorkflowSubItemUnit currentUnit, WorkflowFixupParams fixupParams)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleHumanWorkflowApprovers");
            Hashtable data = currentUnit.Properties;
            long approverIndex = 0;
            int sourceSPTaskId = 0;
            int targetSPTaskId = 0;
            HandleHumanworkflowCustomOutcome(data);
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
                cmd.Connection = SPWorkflowProcessorRuntime.NintexContentDBConnection;
                cmd.Parameters.AddWithValue("@HumanWorkflowID", humanWorkflowIndex);
                cmd.Parameters.AddWithValue("@SPTaskID", targetSPTaskId);
                cmd.CommandText = "SELECT ApproverID FROM HumanWorkflowApprovers WITH(NOLOCK) WHERE HumanWorkflowID=@HumanWorkflowID AND SPTaskID=@SPTaskID ORDER BY ApproverID DESC";
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
                    cmd.CommandText = "SELECT TOP(1) ApproverID FROM HumanWorkflowApprovers WITH(NOLOCK) ORDER BY ApproverID DESC";
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
                logger.Warn("An exception occurred while handle nintex human workflow approvers. exception:{0}", e.ToString());
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleHumanWorkflowApprovers");
            }
            return approverIndex;
        }

        public void ResetData(SPWFInstanceUnit parentUnit)
        {
            if (parentUnit.HasInstanceData)
            {
                if (parentUnit.ParentAssociationUnit.mNonSerializedCustomData != null)
                {
                    Dictionary<string, NintexActivityMemberInfo> customFields = (Dictionary<string, NintexActivityMemberInfo>)parentUnit.ParentAssociationUnit.mNonSerializedCustomData;
                    foreach (KeyValuePair<string, NintexActivityMemberInfo> pair in customFields)
                    {
                        pair.Value.Flag = false;
                    }
                }
            }
        }
        /// <summary>
        /// if use this method, please change the way to get the ninntex DB connonction.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="instanceId"></param>
        public void OnSPInstanceDeleted(Guid siteId, List<Guid> instanceId)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "OnSPInstanceDeleted");
            if (SPWorkflowProcessorRuntime.IsNintexDllInstalled)
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
                    cmd.CommandText = "SELECT InstanceID FROM WorkflowInstance WITH(NOLOCK) WHERE WorkflowInstanceID=@WorkflowInstanceID";
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
                    cmd.CommandText = "SELECT WorkflowProgressID FROM WorkflowProgress WITH(NOLOCK) WHERE InstanceID=@InstanceID";
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
                    cmd.CommandText = "SELECT HumanWorkflowID FROM HumanWorkflow WITH(NOLOCK) WHERE WorkflowProgressID=@WorkflowProgressID";
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
                    logger.Warn("An exception occurred while doing delete nintex workflow instance. exception:{0}", e.ToString());
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

