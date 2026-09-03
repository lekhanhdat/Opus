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
using System.IO;
using System.Reflection;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;
using LS.SPWorkflowProcessor;

namespace AvePoint.Wrapper.Backup
{
    [AveCodeReview("2012/06/08", "qinglong.luo@avepoint.com", "kexin.guo@AvePoint.com", new string[0] { }, null, true)]
    public class AveWorkflow
    {
        private SPWFAssociationProc WFAssociationProcessorProject = null;
        private SPWFAssociationProc WFAssociationProcessor = null;
        private SPWFAssociationProc WFAssociationProcessor13Model = null;
        private const string mConfigFile = @"AgentCommonSPWorkflowConfiguration.xml";
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveWorkflow()
        {
            string configFilePath = AveEnv.AgentDataPath + "\\SP2010\\WrapperCommon\\" + mConfigFile;
            SPWorkflowProcessorRuntime.LoadConfiguration(configFilePath, AveEnv.AgentRootFolder);
            WFAssociationProcessor = SPWFAssociationProc.CreateInstance(SPWFProcessorType.API);
            WFAssociationProcessor.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            WFAssociationProcessor13Model = SPWFAssociationProc.CreateInstance(SPWFProcessorType.API13Model);
            WFAssociationProcessor13Model.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            WFAssociationProcessorProject = SPWFAssociationProc.CreateInstance(SPWFProcessorType.Project);
            WFAssociationProcessorProject.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            SPWorkflowFileContentProc.CustomContentProcessors = SPWorkflowProcessorRuntime.CustomTemplateContentProcessors;
        }

        public void SetNWDBConnectionString(string connStr)
        {
            try
            {
                log.Error("Test: SetConnstring:" + connStr);
                if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWDBConnectionStringOfBackup"))
                {
                    SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionStringOfBackup"] = connStr;
                }
                else
                {
                    SPWorkflowProcessorRuntime.AllProcessorParams.Add("NWDBConnectionStringOfBackup", connStr);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, "A error occurred while set nintex workflow (backup)parameter.detail:{0}", e.ToString());
            }
        }

        public bool ForceBackupAssoiciation = false;

        public void ExportWebContentTypeWFAssociation(IAveBackupStream stream, AveSPWeb web, List<string> ctname, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            ExportWebContentTypeWFAssociation(stream, web, true, ctname, filterFunc);
        }

        private void ExportWebContentTypeWFAssociation(IAveBackupStream stream, AveSPWeb web, bool useCTName, List<string> ctname, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportWebContentTypeWFAssociation"))
            {
                if (SPWorkflowProcessorRuntime.ProcessAssociation || ForceBackupAssoiciation)
                {
                    List<AveWorkflowInfo> associations = new List<AveWorkflowInfo>();
                    foreach (IAveContentType contenttype in web.SPWeb.ContentTypes)
                    {
                        if (!useCTName || (ctname != null && ctname.Contains(contenttype.Name)))
                        {
                            associations.AddRange(GetAssociations(contenttype, SPWFAssociationParentType.WebContentType, filterFunc));
                        }
                    }
                    log.Info("Totally {0} Site ContentType Workflows are backed up on current web:{1}.Size:{2}", associations.Count, web.SPWeb.Url, CalculateDataSize(associations));
                    stream.WriteMetadata(AveMetadataType.WebCTWorkflowAssociation, associations);
                }
            }
        }

        public void ExportWebContentTypeWFAssociation(IAveBackupStream stream, AveSPWeb web, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            ExportWebContentTypeWFAssociation(stream, web, false, null, filterFunc);
        }

        public void ExportListContentTypeWFAssociation(IAveBackupStream stream, AveSPList list, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ExportListContentTypeWFAssociation"))
            {
                if (SPWorkflowProcessorRuntime.ProcessAssociation || ForceBackupAssoiciation)
                {
                    List<AveWorkflowInfo> associations = new List<AveWorkflowInfo>();
                    foreach (IAveContentType contenttype in list.SPList.ContentTypes)
                    {
                        associations.AddRange(GetAssociations(contenttype, SPWFAssociationParentType.ListContentType, filterFunc));
                    }
                    log.Info("Totally {0} List ContentType Workflows are backed up on current List:[{1}][{2}].Size:{3}", associations.Count, list.ParentWeb.SPWeb.Url, list.SPList.Title, CalculateDataSize(associations));
                    stream.WriteMetadata(AveMetadataType.ListCTWorkflowAssociation, associations);
                }
            }
        }

        public void ExportListWFAssociation(IAveBackupStream stream, AveSPList list, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ExportListWFAssociation"))
            {
                if (SPWorkflowProcessorRuntime.ProcessAssociation || ForceBackupAssoiciation)
                {
                    List<AveWorkflowInfo> associations = GetAssociations(list.SPList, SPWFAssociationParentType.List, filterFunc);
                    log.Info("Totally {0} List Workflows are backed up on current List:[{1}][{2}].Size:{3}", associations.Count,list.ParentWeb.SPWeb.Url,list.SPList.Title, CalculateDataSize(associations));
                    stream.WriteMetadata(AveMetadataType.ListWorkflowAssociation, associations);
                }
            }
        }

        public void ExportReusableWorkflowTemplates(IAveBackupStream stream, AveSPWeb web, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportReusableWorkflowTemplates"))
            {
                if (SPWorkflowProcessorRuntime.ProcessAssociation || ForceBackupAssoiciation)
                {
                    List<AveWorkflowInfo> templates = GetReusableWFTemplates(web.SPWeb, SPWFAssociationParentType.Web, filterFunc);
                    log.Info("Totally {0} workflow templates are backed up on current web:{1}.Size:{2}", templates.Count, web.SPWeb.Url,CalculateDataSize(templates));
                    stream.WriteMetadata(AveMetadataType.ReusableWorkflowTemplate, templates);
                }
            }
        }

        private static string CalculateDataSize(List<AveWorkflowInfo> templates)
        {
            long size = 0;
            try
            {
                templates.ForEach(t => { if (t != null) { size += t.AssociationUnit.LongLength; } });
            }
            catch (Exception e)
            {
                log.Warn("Calculate workflow backup Data Size failed.Error:{0}",e);
            }
            
            return string.Format("{0} , MB", size / 1024/1024);
        }

        public void ExportWebWFAssociation(IAveBackupStream stream, AveSPWeb web, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportWebWFAssociation"))
            {
                if (SPWorkflowProcessorRuntime.ProcessAssociation || ForceBackupAssoiciation)
                {
                    List<AveWorkflowInfo> associations = GetAssociations(web.SPWeb, SPWFAssociationParentType.Web, filterFunc);
                    log.Info("Totally {0} Site Workflows are backed up on current web:{1}.Size:{2}", associations.Count, web.SPWeb.Url, CalculateDataSize(associations));
                    stream.WriteMetadata(AveMetadataType.WebWorkflowAssociation, associations);
                }
            }
        }

        public void ExportProjectWFAssociation(IAveBackupStream stream, AveSPWeb web, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportProjectWFAssociation"))
            {
                if (SPWorkflowProcessorRuntime.ProcessAssociation || ForceBackupAssoiciation)
                {
                    List<AveWorkflowInfo> associations = new List<AveWorkflowInfo>();
                    string wFInfoColumnName = "AssociationUnit";
                    string wFInfoTableName = "WorkflowAssociation";
                    string wFInfoCTId = string.Empty;
                    string wFInfoCTName = string.Empty;
                    WFAssociationProcessorProject.ParentObject = web.SPWeb;
                    WFAssociationProcessorProject.ParentObjectType = SPWFAssociationParentType.Web;
                    WFAssociationProcessorProject.FilterWorkflowFunction = filterFunc;
                    var units = WFAssociationProcessorProject.Backup();
                    AddAssociationBackupDataToList(units, wFInfoColumnName, wFInfoTableName, wFInfoCTId, wFInfoCTName, associations);
                    log.Info("Totally {0} Project Workflows are backed up on current web:{1}.Size:{2}", associations.Count, web.SPWeb.Url, CalculateDataSize(associations));
                    stream.WriteMetadata(AveMetadataType.ProjectWorkflowAssociation, associations);
                }
            }
        }

        public List<AveWorkflowInfo> GetAssociations(Object obj, SPWFAssociationParentType type, Func<AveWorkflowAssociationInfo, bool> filterFun)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveWorkflow.GetAssociations"))
            {
                List<AveWorkflowInfo> associations = new List<AveWorkflowInfo>();
                try
                {
                    //提前处理workflow info中的一些基本信息，避免循环中反复处理
                    string wFInfoColumnName = "AssociationUnit";
                    string wFInfoTableName = "WorkflowAssociation";
                    string wFInfoCTId = string.Empty;
                    string wFInfoCTName = string.Empty;
                    if (type == SPWFAssociationParentType.ListContentType || type == SPWFAssociationParentType.WebContentType)
                    {
                        wFInfoTableName = "CTWorkflowAssociation";
                        wFInfoCTId = (obj as IAveContentType).ID.ToString();
                        wFInfoCTName = (obj as IAveContentType).Name;
                    }
                    List<byte[]> units = null;

                    WFAssociationProcessor.ParentObject = obj;
                    WFAssociationProcessor.ParentObjectType = type;
                    WFAssociationProcessor.FilterWorkflowFunction = filterFun;
                    units = WFAssociationProcessor.Backup();
                    AddAssociationBackupDataToList(units, wFInfoColumnName, wFInfoTableName, wFInfoCTId, wFInfoCTName, associations);

                    WFAssociationProcessor13Model.ParentObject = obj;
                    WFAssociationProcessor13Model.ParentObjectType = type;
                    WFAssociationProcessor13Model.FilterWorkflowFunction = filterFun;
                    units = WFAssociationProcessor13Model.Backup();
                    AddAssociationBackupDataToList(units, wFInfoColumnName, wFInfoTableName, wFInfoCTId, wFInfoCTName, associations);
                }
                catch (SPWFProcessorException ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBGetAssociationsError, ex.ToString());
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBGetAssociationsError, ex.ToString());
                }
                return associations;
            }
        }
        private List<AveWorkflowInfo> GetReusableWFTemplates(Object obj, SPWFAssociationParentType type, Func<AveWorkflowAssociationInfo, bool> filterFun)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveWorkflow.GetAssociations"))
            {
                List<AveWorkflowInfo> associations = new List<AveWorkflowInfo>();
                try
                {
                    List<byte[]> units = null;
                    //client does not support to backup and restore 10 mode workflow template,as some API are not supported
                    WFAssociationProcessor13Model.ParentObject = obj;
                    WFAssociationProcessor13Model.ParentObjectType = SPWFAssociationParentType.Web;
                    units = WFAssociationProcessor13Model.BackupWFReusableTemplates();
                    AddTemplateBackupDataToList(units, "ReusableWorkflowTemplate", "WF2013PlatformType", associations);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBGetAssociationsError, ex.ToString());
                }
                return associations;
            }
        }
        private void AddTemplateBackupDataToList(List<byte[]> data, string columnName, string tableName, List<AveWorkflowInfo> info)
        {
            AddAssociationBackupDataToList(data, columnName, tableName, string.Empty, string.Empty, info);
        }
        private void AddInstanceBackupDataToList(List<byte[]> data, string columnName, string tableName, List<AveWorkflowInfo> info)
        {
            AddAssociationBackupDataToList(data, columnName, tableName, string.Empty, string.Empty, info);
        }

        private void AddAssociationBackupDataToList(List<byte[]> data, string columnName, string tableName, string CTId, string CTName, List<AveWorkflowInfo> Info)
        {
            foreach (byte[] serializedData in data)
            {
                AveWorkflowInfo workflowInfo = new AveWorkflowInfo
                {
                    ColumnName = columnName,
                    TableName = tableName,
                    AssociationUnit = serializedData,
                    CTId = CTId,
                    CTName = CTName
                };
                Info.Add(workflowInfo);
            }
        }
        public void ExportWorkflowInstance(IAveBackupStream stream, AveSPItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.ExportWorkflowInstance"))
            {
                if (SPWorkflowProcessorRuntime.ProcessInstance)
                {
                    if (item.RowId <= 0)
                        return;
                    List<Guid> tempIds = new List<Guid>();
                    try
                    {
                        item.QueryService.GetWorkflowId(tempIds, item.ParentSite.SPSite.ID, item.AveSPList.ParentWeb.SPWeb.ID, item.RowId, item.AveSPList.SPList.ID);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "Get workflow instance error.Error Message:{0}.", e);
                        return;
                    }
                    List<AveWorkflowInfo> instances = new List<AveWorkflowInfo>();
                    List<byte[]> units = null;
                    try
                    {
                        if (tempIds.Count != 0)
                        {
                            using (SPWFInstanceProc instanceProc = SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native))
                            {
                                InstanceProcCreationParam param = new InstanceProcCreationParam();
                                param.ParentItem = item.SPListItem;
                                param.QueryService = item.QueryService;
                                param.ProcType = SPWFProcessorType.Native;
                                param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                                param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                                instanceProc.ParentItem = new AveWFParentItem();
                                instanceProc.ParentItem.ParentItemType = WFParentItemType.ListItem;
                                instanceProc.SetInstanceProcParameters(param);
                                units = instanceProc.Backup();
                                AddInstanceBackupDataToList(units, "InstanceUnit", "WorkflowInstance", instances);
                            }
                        }

                        if (item.ParentSite.ObjectModelFactory.ContextKind == AveContextKind.Server13ObjectModel
                            && (Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.SPListItem.Web }).WFInstanceService != null
                            || Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.SPListItem.Web }).UpdateWorkflowServiceManager(item.SPListItem.Web) && Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.SPListItem.Web }).WFInstanceService != null))
                        {
                            using (SPWFInstanceProc instanceProc = SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native13Model))
                            {
                                InstanceProcCreationParam param = new InstanceProcCreationParam();
                                param.ParentItem = item.SPListItem;
                                param.QueryService = item.QueryService;
                                param.ProcType = SPWFProcessorType.Native13Model;
                                param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                                param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                                instanceProc.ParentItem = new AveWFParentItem();
                                instanceProc.ParentItem.ParentItemType = WFParentItemType.ListItem;
                                instanceProc.SetInstanceProcParameters(param);
                                units = instanceProc.Backup();
                                AddInstanceBackupDataToList(units, "InstanceUnit", "WorkflowInstance", instances);
                            }
                        }

                        stream.WriteMetadata(AveMetadataType.WorkflowInstance, instances);
                    }

                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBExportItemWFInsError, item.Id, ex.ToString());
                    }
                }
            }
        }

        public void ExportWebWorkflowInstance(IAveBackupStream stream, AveSPWeb web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportWebWorkflowInstance"))
            {
                if (SPWorkflowProcessorRuntime.ProcessInstance)
                {
                    List<AveWorkflowInfo> instances = new List<AveWorkflowInfo>();
                    List<byte[]> units = null;
                    try
                    {
                        using (SPWFInstanceProc instanceProc = SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native))
                        {
                            InstanceProcCreationParam param = new InstanceProcCreationParam();
                            //param.ParentItem = item.SPListItem;
                            //param.QueryService = item.QueryService;
                            param.ParentWeb = web.SPWeb;
                            param.QueryService = web.QueryService;
                            param.ProcType = SPWFProcessorType.Native;
                            param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                            param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                            instanceProc.ParentItem = new AveWFParentItem();
                            instanceProc.ParentItem.ParentItemType = WFParentItemType.Web;
                            instanceProc.SetInstanceProcParameters(param);
                            units = instanceProc.Backup();
                            AddInstanceBackupDataToList(units, "InstanceUnit", "WorkflowInstance", instances);
                            stream.WriteMetadata(AveMetadataType.WebWorkflowInstance, instances);
                        }
                    }

                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBExportWebWFInsError, web.SPWeb.ID, ex.ToString());
                    }
                }
            }
        }
    }
}