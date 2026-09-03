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
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using LS.SPWorkflowProcessor;
using System.Linq;
using AvePoint.Wrapper.Resource.Backup;

namespace AvePoint.Wrapper.Backup
{
    [AveCodeReview("2012/06/08", "qinglong.luo@avepoint.com", "kexin.guo@AvePoint.com", new string[0] { }, null, true)]
    public class AveWorkflow
    {
        private SPWFAssociationProc WFAssociationProcessor = null;
        private SPWFAssociationProc WFAssociationProcessor13Model = null;
        private const string mConfigFile = @"AgentCommonSPWorkflowConfiguration.xml";
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveWorkflow()
        {
            string configFilePath = AveEnv.AgentDataFolder + "\\WrapperCommon\\" + mConfigFile;
            SPWorkflowProcessorRuntime.LoadConfiguration(configFilePath, AveEnv.AgentRootFolder);
            WFAssociationProcessor = SPWFAssociationProc.CreateInstance(SPWFProcessorType.API);
            WFAssociationProcessor.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            WFAssociationProcessor13Model = SPWFAssociationProc.CreateInstance(SPWFProcessorType.API13Model);
            WFAssociationProcessor13Model.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            SPWorkflowFileContentProc.CustomContentProcessors = SPWorkflowProcessorRuntime.CustomTemplateContentProcessors;
            //BackupWorkflowAssocationToExportedFile = true;
        }

        public void SetNWDBConnectionString(string connStr)
        {
            try
            {
                SPWorkflowProcessorRuntime.AllProcessorParams["NWContentDBConnectionStringOfBackup"] = connStr;
                //if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWContentDBConnectionStringOfBackup"))
                //{
                //    SPWorkflowProcessorRuntime.AllProcessorParams["NWContentDBConnectionStringOfBackup"] = connStr;
                //}
                //else
                //{
                //    SPWorkflowProcessorRuntime.AllProcessorParams.Add("NWContentDBConnectionStringOfBackup", connStr);
                //}
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, "A error occurred while set nintex workflow (backup)parameter.detail:{0}", e.ToString());
            }
        }

        public void SetNWConfigDBConnectionString(string configConnStr)
        {
            try
            {
                SPWorkflowProcessorRuntime.AllProcessorParams["NWConfigDBConnectionStringOfBackup"] = configConnStr;
                //if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWConfigDBConnectionStringOfBackup"))
                //{
                //    SPWorkflowProcessorRuntime.AllProcessorParams["NWConfigDBConnectionStringOfBackup"] = configConnStr;
                //}
                //else
                //{
                //    SPWorkflowProcessorRuntime.AllProcessorParams.Add("NWConfigDBConnectionStringOfBackup", configConnStr);
                //}
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, "A error occurred while set nintex workflow (backup)parameter.detail:{0}", e.ToString());
            }
        }

        public bool BackupWorkflowAssocationToExportedFile { get; set; }
        public bool ForceBackupAssoiciation { get; set; }
        public bool ForceBackupInstance { get; set; }

        public void ExportReusableWorkflowTemplates(IAveBackupStream stream, AveSPWeb web, Func<AveReusableWorkflowTemplateInfo, bool> TemplateFilterFunc)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportReusableWorkflowTemplates"))
            {
                List<AveWorkflowInfo> templates = GetReusableWFTemplates(web, TemplateFilterFunc);
                log.Debug("Totally {0} workflow templates are backed up on current web:{1}.", templates.Count, web.SPWeb.Url);
                stream.WriteMetadata(AveMetadataType.ReusableWorkflowTemplate, templates);
            }
        }

        public void ExportWebContentTypeWFAssociation(IAveBackupStream stream, AveSPWeb web, Func<AveWorkflowAssociationInfo, bool> filterFunc)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportWebContentTypeWFAssociation"))
            {
                List<AveWorkflowInfo> associations = new List<AveWorkflowInfo>();
                foreach (IAveContentType contenttype in web.SPWeb.ContentTypes)
                {
                    associations.AddRange(GetAssociations(contenttype, SPWFAssociationParentType.WebContentType, web.ParentSite.QueryService, filterFunc));
                }
                log.Debug("Totally {0} web contentType workflow associations are backed up on current web:{1}.", associations.Count, web.Name);
                stream.WriteMetadata(AveMetadataType.WebCTWorkflowAssociation, associations);
            }
        }

        public void ExportListContentTypeWFAssociation(IAveBackupStream stream, AveSPList list, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ExportListContentTypeWFAssociation"))
            {
                List<AveWorkflowInfo> associations = new List<AveWorkflowInfo>();
                foreach (IAveContentType contenttype in list.SPList.ContentTypes)
                {
                    associations.AddRange(GetAssociations(contenttype, SPWFAssociationParentType.ListContentType, list.ParentSite.QueryService, filterFunc));
                }
                log.Debug("Totally {0} list contentType workflow associations are backed up on current list:{1}.", associations.Count, list.Title);
                stream.WriteMetadata(AveMetadataType.ListCTWorkflowAssociation, associations);
            }
        }

        public void ExportListWFAssociation(IAveBackupStream stream, AveSPList list, Func<AveWorkflowAssociationInfo, bool> filterFunc = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ExportListWFAssociation"))
            {
                List<AveWorkflowInfo> associations = GetAssociations(list.SPList, SPWFAssociationParentType.List, list.ParentSite.QueryService, filterFunc);
                log.Debug("Totally {0} list workflow associations are backed up on current list:{1}.", associations.Count, list.Title);
                stream.WriteMetadata(AveMetadataType.ListWorkflowAssociation, associations);
            }
        }

        public void ExportWebWFAssociation(IAveBackupStream stream, AveSPWeb web)
        {
            this.ExportWebWFAssociation(stream, web, null);
        }

        public void ExportWebWFAssociation(IAveBackupStream stream, AveSPWeb web, Func<AveWorkflowAssociationInfo, bool> filterFunc)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportWebWFAssociation"))
            {
                List<AveWorkflowInfo> associations = GetAssociations(web.SPWeb, SPWFAssociationParentType.Web, web.ParentSite.QueryService, filterFunc);
                log.Debug("Totally {0} web workflow associations are backed up on current web:{1}.", associations.Count, web.Name);
                stream.WriteMetadata(AveMetadataType.WebWorkflowAssociation, associations);
            }
        }

        private List<AveWorkflowInfo> GetReusableWFTemplates(AveSPWeb parentSPWeb, Func<AveReusableWorkflowTemplateInfo, bool> TemplateFilterFunc)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveWorkflow.GetAssociations"))
            {
                List<AveWorkflowInfo> associations = new List<AveWorkflowInfo>();
                try
                {
                    IAveWeb web = parentSPWeb.SPWeb;

                    List<byte[]> units = null;
                    //client does not support to backup and restore 10 mode workflow template,as some API are not supported
                    //对于reusable workflow template，10 mode workflow支持local 10和local 13这两个SP版本.
                    if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind.IsServerMode())
                    {
                        WFAssociationProcessor.ParentObject = web;
                        WFAssociationProcessor.ParentObjectType = SPWFAssociationParentType.Web;
                        WFAssociationProcessor.QueryService = parentSPWeb.QueryService;
                        WFAssociationProcessor.FilterReusableWorkflowTemplateFunction = TemplateFilterFunc;
                        units = WFAssociationProcessor.BackupWFReusableTemplates();
                        AddTemplateBackupDataToList(units, "ReusableWorkflowTemplate", "WF2010PlatformType", associations);
                    }
                    //对于reusable workflow template，13 mode workflow支持local 13和Office 365(13模拟和真实365,10模拟会空走)这两个SP版本.
                    if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind.IsServerMode13Upper()
                        || SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                    {
                        WFAssociationProcessor13Model.ParentObject = web;
                        WFAssociationProcessor13Model.ParentObjectType = SPWFAssociationParentType.Web;
                        WFAssociationProcessor13Model.FilterReusableWorkflowTemplateFunction = TemplateFilterFunc;
                        units = WFAssociationProcessor13Model.BackupWFReusableTemplates();
                        AddTemplateBackupDataToList(units, "ReusableWorkflowTemplate", "WF2013PlatformType", associations);
                    }
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



        private bool NeedBackupNintexWorkflowAssociationForUpgrade(bool BackupWorkflowAssocationToExportedFile)
        {
            return BackupWorkflowAssocationToExportedFile
                && (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind != AveContextKind.Server07ObjectModel);//仅针对07做特殊判断 不支持nintex 因为没有测试过07
        }
        private List<AveWorkflowInfo> GetAssociations(Object obj, SPWFAssociationParentType type, IAveBackupRestoreQueryService queryService, Func<AveWorkflowAssociationInfo, bool> filterFunc)
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
                    if (NeedBackupNintexWorkflowAssociationForUpgrade(BackupWorkflowAssocationToExportedFile))
                    {
                        var nintexWorkflowProcessor = new SPExportedNintexWorkflowAssociation
                        {
                            ParentObject = obj,
                            ParentObjectType = type,
                            QueryService = queryService,
                            FilterWorkflowFunction = filterFunc
                        };
                        units = nintexWorkflowProcessor.Backup();
                        AddAssociationBackupDataToList(units, wFInfoColumnName, wFInfoTableName, wFInfoCTId, wFInfoCTName, associations);
                    }

                    #region 10 mode workflow

                    WFAssociationProcessor.ParentObject = obj;
                    WFAssociationProcessor.ParentObjectType = type;
                    WFAssociationProcessor.QueryService = queryService;
                    WFAssociationProcessor.FilterWorkflowFunction = filterFunc;
                    units = WFAssociationProcessor.Backup();
                    AddAssociationBackupDataToList(units, wFInfoColumnName, wFInfoTableName, wFInfoCTId, wFInfoCTName, associations);

                    #endregion



                    #region 13 mode workflow

                    if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind.IsServerMode13Upper()
                        || SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                    {
                        WFAssociationProcessor13Model.ParentObject = obj;
                        WFAssociationProcessor13Model.ParentObjectType = type;
                        WFAssociationProcessor13Model.FilterWorkflowFunction = filterFunc;
                        units = WFAssociationProcessor13Model.Backup();
                        AddAssociationBackupDataToList(units, wFInfoColumnName, wFInfoTableName, wFInfoCTId, wFInfoCTName, associations);
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBGetAssociationsError, ex.ToString());
                }
                return associations;
            }
        }

        public void ExportWorkflowInstance(IAveBackupStream stream, AveSPItem item)
        {
            var workflowInstance = ExportWorkflowInstance(item);

            if (workflowInstance != null)
            {
                stream.WriteMetadata(AveMetadataType.WorkflowInstance, workflowInstance);
            }
        }

        private List<AveWorkflowInfo> ExportWorkflowInstance(AveSPItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.ExportWorkflowInstance"))
            {
                if (item.RowId <= 0)
                    return null;
                List<Guid> tempIds = new List<Guid>();
                try
                {
                    item.QueryService.GetWorkflowId(tempIds, item.ParentSite.SPSite.ID, item.AveSPList.ParentWeb.SPWeb.ID, item.RowId, item.AveSPList.SPList.ID);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "Get workflow instance error.Error Message:{0}.", e);
                    return null;
                }

                List<AveWorkflowInfo> instances = new List<AveWorkflowInfo>();
                List<byte[]> units = null;
                try
                {
                    if (tempIds.Count != 0)
                    {
                        using (SPWFInstanceProc instanceProc = SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native))
                        {
                            try
                            {
                                InstanceProcCreationParam param = new InstanceProcCreationParam();
                                param.NeedReloadParent = false;
                                param.ParentItem = item.SPListItem;
                                param.QueryService = item.QueryService;
                                param.ProcType = SPWFProcessorType.Native;
                                param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                                param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                                instanceProc.ParentItem = new AveWFParentItem();
                                instanceProc.ParentItem.ParentItemType = WFParentItemType.ListItem;
                                instanceProc.SetInstanceProcParameters(param);
                                instanceProc.SetWorkflowIds(tempIds);
                                units = instanceProc.Backup();
                                foreach (byte[] serializedData in units)
                                {
                                    AveWorkflowInfo workflowInfo = new AveWorkflowInfo();
                                    workflowInfo.ColumnName = "InstanceUnit";
                                    workflowInfo.TableName = "WorkflowInstance";
                                    workflowInfo.AssociationUnit = new byte[serializedData.Length];
                                    workflowInfo.AssociationUnit = serializedData;
                                    instances.Add(workflowInfo);
                                }
                                log.Debug("Totally {0} workflow instances are backed up on current item:{1}.", instances.Count, item.Name);
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.WARN, "An error occurred while backup 2010 workflow instance. {0}", e.ToString());
                            }
                        }
                    }
                    //disbale backup&restore sp2013 mode workflow instance
                    //if (item.ParentSite.ObjectModelFactory.ContextKind == AveContextKind.Server13ObjectModel 
                    //    && (Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.SPListItem.Web }).WFInstanceService != null
                    //    || Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.SPListItem.Web }).UpdateWorkflowServiceManager(item.SPListItem.Web) && Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.SPListItem.Web }).WFInstanceService != null))
                    //{
                    //    using (SPWFInstanceProc instanceProc = SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native13Model))
                    //    {
                    //        try
                    //        {
                    //            InstanceProcCreationParam param = new InstanceProcCreationParam();
                    //            param.ParentItem = item.SPListItem;
                    //            param.QueryService = item.QueryService;
                    //            param.ProcType = SPWFProcessorType.Native13Model;
                    //            param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                    //            param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                    //            instanceProc.ParentItem = new AveWFParentItem();
                    //            instanceProc.ParentItem.ParentItemType = WFParentItemType.ListItem;
                    //            instanceProc.SetInstanceProcParameters(param);
                    //            units = instanceProc.Backup();
                    //            foreach (byte[] serializedData in units)
                    //            {
                    //                AveWorkflowInfo workflowInfo = new AveWorkflowInfo();
                    //                workflowInfo.ColumnName = "InstanceUnit";
                    //                workflowInfo.TableName = "WorkflowInstance";
                    //                workflowInfo.AssociationUnit = new byte[serializedData.Length];
                    //                workflowInfo.AssociationUnit = serializedData;
                    //                instances.Add(workflowInfo);
                    //            }
                    //        }
                    //        catch (Exception e) 
                    //        {
                    //            log.Log(AveLogLevel.WARN, "An error occurred while backup 2013 workflow instance. {0}", e.ToString());
                    //        }
                    //    }
                    //}

                    //stream.WriteMetadata(AveMetadataType.WorkflowInstance, instances);
                    return instances;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBExportItemWFInsError, item.Id, ex.ToString());
                }
            }

            return null;
        }

        public void ExportWebWorkflowInstance(IAveBackupStream stream, AveSPWeb web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportWebWorkflowInstance"))
            {
                List<Guid> tempIds = new List<Guid>();
                try
                {
                    web.QueryService.GetWorkflowId(tempIds, web.ParentSite.SPSite.ID, web.SPWeb.ID, -1, web.SPWeb.ID);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "Get web workflow instance error.Error Message:{0}.", e);
                    return;
                }
                if (tempIds.Count == 0)
                {
                    return;
                }
                List<AveWorkflowInfo> instances = new List<AveWorkflowInfo>();
                List<byte[]> units = null;
                try
                {
                    using (SPWFInstanceProc instanceProc = SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native))
                    {
                        InstanceProcCreationParam param = new InstanceProcCreationParam();
                        //param.ParentItem = item.SPListItem;
                        //param.QueryService = item.QueryService;
                        param.NeedReloadParent = false;
                        param.ParentWeb = web.SPWeb;
                        param.QueryService = web.QueryService;
                        param.ProcType = SPWFProcessorType.Native;
                        param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                        param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                        instanceProc.ParentItem = new AveWFParentItem();
                        instanceProc.ParentItem.ParentItemType = WFParentItemType.Web;
                        instanceProc.SetInstanceProcParameters(param);
                        instanceProc.SetWorkflowIds(tempIds);
                        units = instanceProc.Backup();
                        foreach (byte[] serializedData in units)
                        {
                            AveWorkflowInfo workflowInfo = new AveWorkflowInfo();
                            workflowInfo.ColumnName = "InstanceUnit";
                            workflowInfo.TableName = "WorkflowInstance";
                            workflowInfo.AssociationUnit = new byte[serializedData.Length];
                            workflowInfo.AssociationUnit = serializedData;
                            instances.Add(workflowInfo);
                        }
                        log.Debug("Totally {0} workflow instances are backed up on current web:{1}.", instances.Count, web.Name);
                        stream.WriteMetadata(AveMetadataType.WebWorkflowInstance, instances);
                    }
                }

                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBExportWebWFInsError, web.SPWeb.ID, ex.ToString());
                }
            }
        }

        public void ExportWorkflowSchedule(IAveBackupStream stream, AveSPItem item)
        {
            var schedules = ExportWorkflowSchedule(item);
            if (schedules != null)
            {
                stream.WriteMetadata(AveMetadataType.WorkflowSchedule, schedules);
            }
        }

        private List<AveWorkflowInfo> ExportWorkflowSchedule(AveSPItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem..ExportWebWorkflowSchedule"))
            {
                if (item.RowId <= 0)
                    return null;
                List<Guid> tempIds = new List<Guid>();
                try
                {
                    item.QueryService.GetWorkflowAssociationId(tempIds, item.ParentSite.SPSite.ID, item.AveSPList.ParentWeb.SPWeb.ID, item.AveSPList.SPList.ID);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "Get workflow association error.Error Message:{0}.", e);
                    return null;
                }
                if (tempIds.Count == 0)
                {
                    return null;
                }

                List<AveWorkflowInfo> schedules = new List<AveWorkflowInfo>();
                List<byte[]> units = null;
                try
                {
                    using (SPWFInstanceProc instanceProc = SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native))
                    {
                        InstanceProcCreationParam param = new InstanceProcCreationParam();
                        param.NeedReloadParent = false;
                        param.ParentItem = item.SPListItem;
                        param.QueryService = item.QueryService;
                        param.ProcType = SPWFProcessorType.Native;
                        param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                        param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                        instanceProc.ParentItem = new AveWFParentItem();
                        instanceProc.ParentItem.ParentItemType = WFParentItemType.ListItem;
                        instanceProc.SetInstanceProcParameters(param);
                        instanceProc.SetTempBaseIds(tempIds);
                        units = instanceProc.BackupSchedules();
                        foreach (byte[] serializedData in units)
                        {
                            AveWorkflowInfo workflowInfo = new AveWorkflowInfo();
                            workflowInfo.ColumnName = "ScheduleUnit";
                            workflowInfo.TableName = "WorkflowSchedule";
                            workflowInfo.AssociationUnit = new byte[serializedData.Length];
                            workflowInfo.AssociationUnit = serializedData;
                            schedules.Add(workflowInfo);
                        }
                        log.Debug("Totally {0} workflow schedule items are backed up on current item:{1}.", schedules.Count, item.Name);
                        //stream.WriteMetadata(AveMetadataType.WorkflowSchedule, schedules);
                        return schedules;
                    }
                }

                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBExportItemWFInsError, item.Id, ex.ToString());
                }
            }

            return null;
        }

        public void ExportWebWorkflowSchedule(IAveBackupStream stream, AveSPWeb web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportWebWorkflowSchedule"))
            {
                if (web.QueryService == null)
                {//Office 365 暂时不支持Schedule
                    return;
                }
                List<Guid> tempIds = new List<Guid>();
                try
                {


                    web.QueryService.GetWorkflowAssociationId(tempIds, web.ParentSite.SPSite.ID, web.SPWeb.ID, web.SPWeb.ID);

                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "Get web workflow association error.Error Message:{0}.", e);
                    return;
                }
                if (tempIds.Count == 0)
                {
                    return;
                }

                List<AveWorkflowInfo> instances = new List<AveWorkflowInfo>();
                List<byte[]> units = null;
                try
                {
                    using (SPWFInstanceProc instanceProc = SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native))
                    {
                        InstanceProcCreationParam param = new InstanceProcCreationParam();
                        //param.ParentItem = item.SPListItem;
                        //param.QueryService = item.QueryService;
                        param.NeedReloadParent = false;
                        param.ParentWeb = web.SPWeb;
                        param.QueryService = web.QueryService;
                        param.ProcType = SPWFProcessorType.Native;
                        param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                        param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                        instanceProc.ParentItem = new AveWFParentItem();
                        instanceProc.ParentItem.ParentItemType = WFParentItemType.Web;
                        instanceProc.SetInstanceProcParameters(param);
                        instanceProc.SetTempBaseIds(tempIds);
                        units = instanceProc.BackupSchedules();
                        foreach (byte[] serializedData in units)
                        {
                            AveWorkflowInfo workflowInfo = new AveWorkflowInfo();
                            workflowInfo.ColumnName = "ScheduleUnit";
                            workflowInfo.TableName = "WorkflowSchedule";
                            workflowInfo.AssociationUnit = new byte[serializedData.Length];
                            workflowInfo.AssociationUnit = serializedData;
                            instances.Add(workflowInfo);
                        }
                        log.Debug("Totally {0} workflow schedule items are backed up on current web:{1}.", instances.Count, web.Name);
                        stream.WriteMetadata(AveMetadataType.WebWorkflowSchedule, instances);
                    }
                }

                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBExportWebWFInsError, web.SPWeb.ID, ex.ToString());
                }
            }
        }

        public void ExportNintexWorkflowTemplates(IAveBackupStream stream, AveSPWeb web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportNintexWorkflowTemplates"))
            {
                List<AveWorkflowInfo> templates = new List<AveWorkflowInfo>();
                byte[] units = null;
                try
                {

                    units = SPWorkflowProcessorRuntime.BackupCustomData(web.ParentSite.SPSite.ID, web.SPWeb.ID);
                    if (units == null)
                    {
                        return;
                    }
                    AveWorkflowInfo workflowInfo = new AveWorkflowInfo();
                    workflowInfo.ColumnName = "TemplatesUnit";
                    workflowInfo.TableName = "NintexWorkflowTemplates";
                    workflowInfo.AssociationUnit = new byte[units.Length];
                    workflowInfo.AssociationUnit = units;
                    templates.Add(workflowInfo);

                    stream.WriteMetadata(AveMetadataType.WorkflowTemplate, templates);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "A error occurred while restore workflow templates {0}", ex.ToString());
                }
            }
        }

        #region IAveWorkflow Members

        public void ExportListContentTypeWFAssociation(IAveBackupStream stream, IAveSPList list)
        {
            this.ExportListContentTypeWFAssociation(stream, list as AveSPList);
        }

        public void ExportListWFAssociation(IAveBackupStream stream, IAveSPList list)
        {
            this.ExportListWFAssociation(stream, list as AveSPList);
        }

        public void ExportWebContentTypeWFAssociation(IAveBackupStream stream, IAveSPWeb web)
        {
            this.ExportWebContentTypeWFAssociation(stream, web as AveSPWeb, null);
        }

        public void ExportWebWFAssociation(IAveBackupStream stream, IAveSPWeb web)
        {
            this.ExportWebWFAssociation(stream, web as AveSPWeb);
        }

        public void ExportWebWorkflowInstance(IAveBackupStream stream, IAveSPWeb web)
        {
            this.ExportWebWorkflowInstance(stream, web as AveSPWeb);
        }

        public void ExportWorkflowInstance(IAveBackupStream stream, IAveSPItem item)
        {
            this.ExportWorkflowInstance(stream, item as AveSPItem);
        }

        #endregion

        internal SPWorkflowDto GetWorkflowDto(AveSPItem item, SPWorkflowBackupOption backupOption)
        {
            var workflow = new SPWorkflowDto();

            if ((!backupOption.BackupInstance) && (!backupOption.BackupSchedule))
            {
                throw new ArgumentNullException("backupOption");
            }

            if (backupOption.BackupInstance)
            {
                workflow.Instances = ExportWorkflowInstance(item);
            }

            if (backupOption.BackupSchedule)
            {
                workflow.Schedules = ExportWorkflowSchedule(item);
            }

            return workflow;
        }


    }
}