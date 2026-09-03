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
using System.IO;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;
using System.Diagnostics.CodeAnalysis;
namespace LS.SPWorkflowProcessor
{
    public class NintexWorkflowAssociationProc : ICustomWorkflowAssociationProc
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
        public static bool NintexPublishedWf = false;
        public Dictionary<Guid, string> ReusableWfName = new Dictionary<Guid, string>();
        private const string NintexNoCodeWorkflowLibName = "NintexWorkflows";
        public void BackupNintexWorklfowAssociationDB(SPWFAssociationUnit parentAsso)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupNintexWorkflowAssociationDb");
            if (IsNintexDllInstalled)
            {
                if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWDBConnectionStringOfBackup"))
                {
                    this.NintexDBConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionStringOfBackup"];
                }
                else
                {
                    this.NintexDBConnectionString = NintexDatabaseUtility.GetContentDBConnectionString((Guid)parentAsso.SPSiteId);
                }
                if (string.IsNullOrEmpty(this.NintexDBConnectionString))
                {
                    return;
                }

                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ContentDBConnString, this.NintexDBConnectionString);

                // fix Unreleased Resource: Database
                using (SqlConnection nintexConn = new SqlConnection())
                {
                    nintexConn.ConnectionString = this.NintexDBConnectionString;
                    nintexConn.Open();

                    BackupNintexPublishedWorkflow(nintexConn, parentAsso);
                }

                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupCustomWorkflowData");
            }
        }

        public void BackupCustomWorkflowData(SPWFAssociationUnit parentAsso)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupCustomWorkflowData");
            IAveWeb parentWeb = parentAsso.ParentWeb;
            object listObj = null;
            listObj = parentWeb.GetListByName(NintexNoCodeWorkflowLibName, false);// LSInvoker.CallMethod(parentWeb.Lists, "GetListByName", new Type[] { typeof(string), typeof(bool) }, new object[] { NintexNoCodeWorkflowLibName, false });

            IAveList mList = listObj as IAveList;
            if (mList == null)
            { return; }
            try
            {
                if ((mList.Title != string.Empty) && (mList.Title == NintexNoCodeWorkflowLibName))
                {

                }
            }
            catch (Exception ex)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_TemplateLibraryMissing, NintexNoCodeWorkflowLibName, ex.Message);
                return;
            }
            if (listObj == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_TemplateLibraryMissing, NintexNoCodeWorkflowLibName);
                return;
            }
            BackupNintexWorklfowAssociationDB(parentAsso);
            IAveList nintexNoCodeWorkflowLib = (IAveList)listObj;
            foreach (IAveFolder parentFolder in nintexNoCodeWorkflowLib.RootFolder.SubFolders)
            {
                if (!this.ReusableWfName.ContainsKey(parentAsso.mSPAssociation.BaseId))
                {
                    if (parentFolder.Name.Equals(parentAsso.SerializableData.mName))
                    {
                        SPWorkflowSubListUnit listUnit = SPWorkflowSubListUnit.GetSubListInfo(nintexNoCodeWorkflowLib);
                        listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateSPFileUnitCollection(parentFolder, -1);
                        listUnit.SerializableData.mUnitId = "NintexWorkflow";
                        parentAsso.SerializableData.mSerializableCustomData = listUnit.FixupSerializableData();
                        return;
                    }
                }
                else
                {
                    if (parentFolder.Name.Equals(this.ReusableWfName[parentAsso.mSPAssociation.BaseId]))
                    {
                        SPWorkflowSubListUnit listUnit = SPWorkflowSubListUnit.GetSubListInfo(nintexNoCodeWorkflowLib);
                        listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateSPFileUnitCollection(parentFolder, -1);
                        listUnit.SerializableData.mUnitId = "NintexWorkflow";
                        parentAsso.SerializableData.mSerializableCustomData = listUnit.FixupSerializableData();
                        return;
                    }

                    if (parentFolder.Name.Equals("__globallyReusable", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (IAveFolder subFolder in parentFolder.Folders)
                        {
                            if (subFolder.Name.Equals(this.ReusableWfName[parentAsso.mSPAssociation.BaseId]))
                            {
                                SPWorkflowSubListUnit listUnit = SPWorkflowSubListUnit.GetSubListInfo(nintexNoCodeWorkflowLib);
                                listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateSPFileUnitCollection(subFolder, -1);
                                listUnit.SerializableData.mUnitId = "NintexWorkflow";
                                parentAsso.SerializableData.mIsNintexSiteCollectionReusableWorklfow = true;
                                parentAsso.SerializableData.mSerializableCustomData = listUnit.FixupSerializableData();
                                return;
                            }
                        }
                    }
                }
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupCustomWorkflowData");
        }

        public void BackupNintexPublishedWorkflow(SqlConnection nintexConn, SPWFAssociationUnit parentAsso)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = nintexConn;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@WorkflowID", parentAsso.mSPAssociation.BaseId);
                    cmd.CommandText = "SELECT * FROM PublishedWorkflows WHERE WorkflowID=@WorkflowID AND (WorkflowType = 'Reusable' or WorkflowType = 'GloballyReusable') AND Version = 1";
                    using (SqlDataAdapter nwAdapter = new SqlDataAdapter())
                    {
                        nwAdapter.SelectCommand = cmd;
                        using (DataTable nwMemoryTable = new DataTable())
                        {
                            nwAdapter.Fill(nwMemoryTable);
                            foreach (DataRow dr in nwMemoryTable.Rows)
                            {
                                SPWFAssociationSerializableData tempUnit = new SPWFAssociationSerializableData();
                                tempUnit.SetPropsFromDataRow(dr, nwMemoryTable.Columns);
                                parentAsso.SerializableData.ChildUnits.Add(tempUnit);
                                if (!ReusableWfName.ContainsKey(parentAsso.mSPAssociation.BaseId))
                                {
                                    this.ReusableWfName.Add(parentAsso.mSPAssociation.BaseId, tempUnit.Properties["#WorkflowName"].ToString());
                                }
                            }
                            if (nwMemoryTable.Rows.Count > 0)
                            {
                                nwMemoryTable.Clear();
                                parentAsso.SerializableData.mIsNintexReusableWorkflow = true;
                                NintexWorkflowAssociationProc.NintexPublishedWf = true;
                            }
                            else
                            {
                                nwMemoryTable.Clear();
                                parentAsso.SerializableData.mIsNintexReusableWorkflow = false;
                                NintexWorkflowAssociationProc.NintexPublishedWf = false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, e.Message);
            }
        }

        public void RestoreCustomWorkflowData(SPWFAssociationUnit parentAsso)
        {
        }
      


    }

    internal class NintexActivityMemberInfo
    {
        internal bool Flag;
        internal List<string> Parameters = new List<string>();
    }
}
