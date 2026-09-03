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
using System.Linq;
using System.Reflection;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPRestore;
using LS.SPWorkflowProcessor;
using AvePoint.Common;
using System.IO;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using System.Globalization;
using AvePoint.Wrapper.Mapping;
using LS.SPWorkflowProcessor.SerializableObjects;
using System.Diagnostics.CodeAnalysis;
using LS.SPWorkflowProcessor.Common;
using AvePoint.Wrapper.Common.AveWorkflowAssociationCollection;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{

    public class WFConflictResolution : IReportable, IWFConflictResolution
    {
        #region [******Members******]

        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        [ThreadStatic]
        private static volatile WFConflictResolution instance;

        private const string mConfigFile = @"AgentCommonSPWorkflowConfiguration.xml";
        private static object syncRoot = new Object();
        private AveWorkflowRestoreCore workflowRestoreCore = new AveWorkflowRestoreCore();
        private ThreadSafeDictionary<Guid, AveWorkflowInfo> mUnitsOfBackup = new ThreadSafeDictionary<Guid, AveWorkflowInfo>();
        private IReport reportor = new AveWrapperReport();


        //private WFConflictResolutionInternal mWFConflictResolutionInternal = null;
        /// <summary>
        /// 还原workflow template时使用
        /// </summary>
        public WFTemplateConflictResolutionOption TemplateOption { get; set; }
        /// <summary>
        /// 还原workflow definition时使用
        /// </summary>
        public WFAssociationConflictResolutionOption AssociationOption { get; set; }
        /// <summary>
        /// 还原workflow instance 过程中反插workflow definition时使用
        /// </summary>
        public WFAssociationConflictResolutionOption ParentAssociationOption { get; set; }
        /// <summary>
        /// 还原workflow instance时使用
        /// </summary>
        public WFInstanceConflictResolutionOption InstanceOption { get; set; }

        /// <summary>
        ///是否使用Export Workflow Association的方式备份Nintex Workflow
        /// </summary>
        public bool BackupNintexWorklfowToExportedFile { set; get; }

        public bool? RestartRunningInstance { get; set; }

        /// <summary>
        /// 当前还原的workflow association是否是web contentType workflow association
        /// </summary>
        public bool WebContentTypeAssociation { get; set; }
        private static AveSPSite mParentSite;
        public static AveSPSite ParentSite
        {
            get
            { return mParentSite; }
            set
            {
                mParentSite = value;
                AveUserMappingService.AveSite = mParentSite;
            }
        }

        /// <summary>
        /// 缓存的AveSPWeb
        /// </summary>
        internal AveSPWeb ParentSPWeb;

        private WorkflowAssociationParentObject mAssociationParentObject = null;
        /// <summary>
        /// Must be a IAveList，IAveWeb or IAveContentType
        /// </summary>
        public object AssociationParentObject
        {
            get
            {
                return mAssociationParentObject;
            }
            set
            {
                mAssociationParentObject = WFConflictResolutionInternal.GetInstance(SPWFInternalPlatform.Default, null).SetAssociationParentObject(value, ref workflowRestoreCore, WebContentTypeAssociation);
            }
        }

        private WFConflictResolution()
        {
            TemplateOption = WFTemplateConflictResolutionOption.NotOverwrite;
            AssociationOption = WFAssociationConflictResolutionOption.NotOverwrite;
            ParentAssociationOption = WFAssociationConflictResolutionOption.ForceUse;
            InstanceOption = WFInstanceConflictResolutionOption.Overwrite;
        }

        public static WFConflictResolution Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new WFConflictResolution();

                        #region ====设置Runtime默认参数和Services====
                        //User Mapping Service
                        SPWorkflowProcessorRuntime.AddService(typeof(AveUserMappingService), null);
                        //FilterData Service
                        SPWorkflowProcessorRuntime.AddService(typeof(LS.SPWorkflowProcessor.Services.WFTaskAndHistoryDataFilter), null);
                        //Cache Service
                        Dictionary<string, string> param = new Dictionary<string, string>();
                        param.Add("RootDirectory", SPWorkflowProcessorRuntime.CurrentProcessTempLocation);
                        SPWorkflowProcessorRuntime.AddService(typeof(LS.SPWorkflowProcessor.FileCacheService), param);
                        //Postpone Service
                        param.Add("RootDirectory", SPWorkflowProcessorRuntime.CurrentProcessTempLocation);
                        SPWorkflowProcessorRuntime.AddService(typeof(LS.SPWorkflowProcessor.WebPostponeActionService), param);

                        string configFilePath = AveEnv.AgentDataFolder + "\\WrapperCommon\\" + mConfigFile;
                        if (File.Exists(configFilePath))
                        {
                            SPWorkflowProcessorRuntime.LoadConfiguration(configFilePath, AveEnv.AgentRootFolder);
                        }
                        #endregion

                    }
                }

                return instance;
            }
        }

        #endregion

        public void SetNWDBConnectionString(string connStr)
        {
            try
            {
                SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionStringOfRestore"] = connStr;
                //if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWDBConnectionStringOfRestore"))
                //{
                //    SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionStringOfRestore"] = connStr;
                //}
                //else
                //{
                //    SPWorkflowProcessorRuntime.AllProcessorParams.Add("NWDBConnectionStringOfRestore", connStr);
                //}
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, "A error occurred while set nintex workflow (restore)parameter.detail:{0}", e.ToString());
            }
        }

        /// <summary>
        /// Assign value to properties of 'SPWorkflowProcessorRuntime' in this way..
        /// </summary>
        /// <param name="restoreHistoryOnly"></param>
        public void SetRestoreHistoryOnlyRuntimeProperty(bool restoreHistoryOnly)
        {
            SPWorkflowProcessorRuntime.RestoreHistoryOnly = restoreHistoryOnly;
        }

        /// <summary>
        /// 暂时加到list,web post action中调用
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        public void ClearCache(Guid siteId, Guid webId)
        {
            if (siteId != Guid.Empty && webId != Guid.Empty)
            {
                //SPWorkflowProcessorRuntime.UDAMappingManager.Clear(siteId, webId);
                SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Remove(siteId, webId);
            }
            if (workflowRestoreCore != null)
            {
                ClearSPWFAssociationProc(workflowRestoreCore.WFAssociationProcessor);
                ClearSPWFAssociationProc(workflowRestoreCore.WFAssociationProcessor13Model);
            }
        }
        private void ClearSPWFAssociationProc(SPWFAssociationProc processor)
        {
            if (processor != null)
            {
                processor.Dispose();
            }
        }

        #region [******Handle PostponeAction ******]

        internal void ExecutePostAction(AveSPWeb parentSPWeb)
        {
            ParentSPWeb = parentSPWeb;
            workflowRestoreCore.WFAssociationProcessor.NoRestoredWFCache.Clear();
            workflowRestoreCore.WFAssociationProcessor.RestoreWFDefinitionEvent += new RestoreWFDefinitionEventHandler(RestoreDefinitionExecuted);
            workflowRestoreCore.WFInstanceProcessor.RestoreWFInstanceEvent += new RestoreWFInstanceEventHandler(RestoreInstanceExecuted);
            workflowRestoreCore.ExecutePostAction();
            workflowRestoreCore.WFAssociationProcessor.RestoreWFDefinitionEvent -= new RestoreWFDefinitionEventHandler(RestoreDefinitionExecuted);
            workflowRestoreCore.WFInstanceProcessor.RestoreWFInstanceEvent -= new RestoreWFInstanceEventHandler(RestoreInstanceExecuted);
            RecalculateRunningInstanceCount(parentSPWeb);
        }

        private void RecalculateRunningInstanceCount(AveSPWeb parentSPWeb)
        {
            if (parentSPWeb != null)
            {
                AveWorkflowRunningInstanceRecalculationService.RecalculateRunningInstanceCount(parentSPWeb.SPWeb, parentSPWeb.QueryService);
            }

        }

        internal void RestoreInstanceExecuted(object sender, RestoreWFInstanceEventArgs e)
        {
            switch (e.ParentObjectType)
            {
                case SPWFAssociationParentType.Web:
                    HandleInstanceConflict((SPWFInstanceUnit)sender, (IAveWeb)e.ParentObject, true);
                    break;
                case SPWFAssociationParentType.List:
                case SPWFAssociationParentType.ListContentType:
                case SPWFAssociationParentType.WebContentType:
                    HandleInstanceConflict((SPWFInstanceUnit)sender, (IAveListItem)e.ParentObject, true);
                    break;
                default:
                    break;
            }
        }

        internal void RestoreDefinitionExecuted(object sender, RestoreWFDefinitionEventArgs e)
        {
            AssociationParentObject = e.ParentObject;
            WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(((SPWFAssociationUnit)sender).WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
            {
                string parentTitle = string.Empty;
                var type = conflictResolutionInternal.GetParentObjectType((WorkflowAssociationParentObject)AssociationParentObject, out parentTitle);
                conflictResolutionInternal.ParentSPWeb = ParentSPWeb;
                AveUserMappingService.AveSite = ParentSPWeb.ParentSite;
                if (SPWorkflowProcessorRuntime.MappingManager == null)
                {
                    if (ParentSPWeb != null)
                    {
                        SPWorkflowProcessorRuntime.MappingManager = ParentSPWeb.ParentSite.MappingManager;
                    }
                    else
                    {
                        SPWorkflowProcessorRuntime.MappingManager = WrapperRuntime.CurrentContext.MappingManager;
                    }
                }
                try
                {
                    log.Info("Process {0} in post action", ((SPWFAssociationUnit)sender).SerializableData.mOriginalName);
                    var association = (SPWFAssociationUnit)sender;
                    conflictResolutionInternal.InitAssociationUnitWorkflowType(association);
                    if (association.SerializableData.isReusableWrokflow)
                    {
                        association.ParentObject = e.ParentObject;
                        association.ParentObjectType = SPWFAssociationParentType.Web;
                        conflictResolutionInternal.RestoreTemplateDataInternal(association);
                    }
                    else
                    {
                        conflictResolutionInternal.HandleAssociationConflictInternal(association, (WorkflowAssociationParentObject)AssociationParentObject, new WFAveSPObjectCache(ParentSPWeb, null), true);
                    }
                    BackupWFStartOptionForOnline(association);
                    this.reportor.AddDetail(
                                new AveWrapperReportDto(((SPWFAssociationUnit)sender).SerializableData.mOriginalName, parentTitle, type, AveStatus.Successful, string.Empty));
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while restore workflow definition in post action. ParentTitle:{0},type:{1},Error:{2}", parentTitle, type, ex.ToString());
                    if ((SPWFAssociationUnit)sender != null && ((SPWFAssociationUnit)sender).SerializableData != null)
                    {
                        Guid associationId = ((SPWFAssociationUnit)sender).SerializableData.mId;
                        if (conflictResolutionInternal.WFCRParameters != null
                            && conflictResolutionInternal.WFCRParameters.workflowRestoreCore != null
                            && conflictResolutionInternal.WFCRParameters.workflowRestoreCore.WFAssociationProcessor != null
                            && conflictResolutionInternal.WFCRParameters.workflowRestoreCore.WFAssociationProcessor.NeedPostActionAssociations.Contains(associationId))
                        {
                            conflictResolutionInternal.WFCRParameters.workflowRestoreCore.WFAssociationProcessor.NeedPostActionAssociations.Remove(associationId);
                        }
                        this.reportor.AddDetail(
                                    GetReportDto(((SPWFAssociationUnit)sender).SerializableData.mOriginalName, parentTitle, type, AveStatus.Failed, ex, string.Empty));
                    }
                    else
                    {
                        log.Warn("Invalid association data in post action.Parent Title:{0}", parentTitle);
                    }
                    throw;
                }
            }

        }

        private void BackupWFStartOptionForOnline(SPWFAssociationUnit association)
        {
            if (ParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
            {
                IAveList list = null;
                if (association.ParentObjectType == SPWFAssociationParentType.List)
                {
                    list = association.ParentObject as IAveList;
                }
                else if (association.ParentObjectType == SPWFAssociationParentType.ListContentType)
                {
                    list = (association.ParentObject as IAveContentType)?.ParentList;
                }
                if (list != null)
                {
                    log.Info("Being to backup workflow start option in post action for list {0}", list.Title);
                    var cache = list.BackupWorkflowStartOption(this.ParentSPWeb.SPWeb.Url, list.ParentWeb.ID, list.ID);
                    ParentSite.WorkflowCache.AddCache(this.ParentSPWeb.SPWeb.ID, list.ID, cache);
                    log.Debug("Finish backup workflow start option in post action for list {0}", list.Title);
                }
            }
        }

        #endregion

        #region [******Handle Workflow Association******]

        #region new public method for restore association

        /// <summary>
        /// 还原web(web contentType)上的workflow definition
        /// </summary>
        /// <param name="wfInfo">workflow的备份数据</param>
        /// <param name="web">parent AveSPWeb</param>
        /// <param name="contentType">还原web workflow association时为null,还原web contentType workflow时为关联的web contentType</param>
        public void RestoreAssociationData(AveWorkflowInfo wfInfo, IAveSPWeb web, IAveContentType contentType = null)
        {
            WebContentTypeAssociation = false;
            ParentSPWeb = web as AveSPWeb;
            AveUserMappingService.AveSite = ParentSPWeb.ParentSite;
            if (contentType != null)
            {
                WebContentTypeAssociation = true;
                AssociationParentObject = contentType;
            }
            else
            {
                if (web != null)
                { AssociationParentObject = web.SPWeb; }
                else
                {
                    throw new ArgumentNullException("web");
                }
            }

            RestoreAssociationDataInternally(wfInfo, ParentSPWeb, new WFAveSPObjectCache(web, null));
        }

        /// <summary>
        /// 还原list(list contentType)上的workflow definition
        /// </summary>
        /// <param name="wfInfo">workflow definition的备份数据</param>
        /// <param name="list">parent AveSPList</param>
        /// <param name="contentType">还原web workflow association时为null,还原web contentType workflow时为关联的list contentType</param>
        public void RestoreAssociationData(AveWorkflowInfo wfInfo, IAveSPList list, IAveContentType contentType = null)
        {
            WebContentTypeAssociation = false;
            if (list != null)
            {
                ParentSPWeb = list.ParentWeb as AveSPWeb;
                AveUserMappingService.AveSite = ParentSPWeb.ParentSite;
            }
            if (contentType != null)
            {
                AssociationParentObject = contentType;
            }
            else
            {
                if (list != null)
                {
                    AssociationParentObject = list.SPList;
                }
                else
                {
                    throw new ArgumentNullException("list");
                }
            }

            RestoreAssociationDataInternally(wfInfo, ParentSPWeb, new WFAveSPObjectCache(ParentSPWeb, list));
        }

        #endregion

        //WFTemplateConflictResolutionOption option  need to be delted.
        public void RestoreWorkflowTemplates(IAveSPWeb web, List<AveWorkflowInfo> templates, WFTemplateConflictResolutionOption option)
        {
            WebContentTypeAssociation = false;
            ParentSPWeb = web as AveSPWeb;
            AveUserMappingService.AveSite = ParentSPWeb.ParentSite;
            SPWorkflowProcessorRuntime.MappingManager = ParentSPWeb.ParentSite.MappingManager;
            AssociationParentObject = web.SPWeb;
            foreach (var template in templates)
            {
                RestoreSingleWorkflowTemplate(template, web.SPWeb);
            }
        }

        private string GetReportTemplateName(SPWFAssociationUnit unit)
        {
            string name = unit.SerializableData.mName;
            bool isVersion = false;
            if (unit.TemplateLibUnit != null && unit.TemplateLibUnit.mTemplateFileUnits.Count > 0)
            {
                foreach (SPWorkflowSubFileUnit fileUnit in unit.TemplateLibUnit.mTemplateFileUnits)
                {
                    if (!fileUnit.SerializableData.mIsCurrentVersion)
                    {
                        isVersion = true;
                        break;
                    }
                }
            }
            if (isVersion)
            {
                if (!string.IsNullOrEmpty(unit.SerializableData.mInternalName))
                {
                    string noCodeWorkflowName = null;
                    int cfgFileItemId = -1;
                    int cfgFileVersion = -1;
                    Guid listId;
                    try
                    {
                        SPWorkflowSubListUnit.GetInfoFromInternalName(unit.SerializableData.mInternalName, out noCodeWorkflowName, out listId, out cfgFileItemId, out cfgFileVersion);
                        name = string.Format("{0}:{1}", name, cfgFileVersion);
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while getting name from internal name. Name:{0}, Error:{1}", unit.SerializableData.mInternalName, e);
                    }
                }
            }
            return name;
        }

        private void RestoreSingleWorkflowTemplate(AveWorkflowInfo templateInfo, IAveWeb web)
        {
            SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(templateInfo.AssociationUnit);
            if (assoUnit == null)
            {
                return;
            }
            assoUnit.ParentObject = web;
            assoUnit.ParentObjectType = SPWFAssociationParentType.Web;

            string reportName = GetReportTemplateName(assoUnit);
            string name = assoUnit.SerializableData.mName;
            string objTitle = assoUnit.ParentWeb.Title;
            const AveReportObjectType reportObjectType = AveReportObjectType.WorkflowTemplate;
            AveWrapperReportDto report = null;
            SPWFInternalPlatform platformType = SPWFInternalPlatform.Default;
            log.Debug("Begin to restore the reusable workflow template.Name:{0},RelatedObjectTitle:{1},Type:{2}", reportName, objTitle, templateInfo.TableName);
            try
            {
                if (SPWorkflowProcessorRuntime.ProcessAssociation)
                {

                    switch (templateInfo.TableName)
                    {
                        case "WF2010PlatformType":
                            platformType = SPWFInternalPlatform.WF2010PlatformType;
                            break;
                        case "WF2013PlatformType":
                            platformType = SPWFInternalPlatform.WF2013PlatformType;
                            break;
                        default:
                            throw new ArgumentException("Invalid platform type for restoring workflow template.");
                    }

                    WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(platformType, GetWFConflictResolutionParametersBySelf());
                    {
                        conflictResolutionInternal.ParentSPWeb = ParentSPWeb;
                        conflictResolutionInternal.RestoreTemplateDataInternal(assoUnit);
                    }
                }
                report = new AveWrapperReportDto(reportName, objTitle, reportObjectType, AveStatus.Successful, "");
            }
            catch (AveWrapperSkipException exception)
            {
                log.Warn("Skip restoring the workflow template. Name: {0} Error: {1}", name, exception);
                report = GetReportDto(reportName, objTitle, reportObjectType, AveStatus.Skipped, exception, "");
            }
            catch (SPWFProcessorException exception)
            {
                if (exception.ErrorCode == SPWFProcessorErrorCode.PutIntoPostAction)
                {
                    log.Info("The workflow definition will be restored later, workflow definition name:{0}.", assoUnit.SerializableData.mOriginalName);
                }
                else
                {
                    log.Error("An error occurred while restoring the workflow template. Name: {0} Error: {1}", name, exception);
                    report = GetReportDto(reportName, objTitle, reportObjectType, AveStatus.Failed, exception, "");
                }
            }
            catch (Exception exception)
            {
                log.Error("An error occurred while restoring the workflow template. Name: {0} Error: {1}", name, exception);
                report = GetReportDto(reportName, objTitle, reportObjectType, AveStatus.Failed, exception, "");
            }

            this.reportor.AddDetail(report);
        }

        private void RestoreAssociationDataInternally(AveWorkflowInfo wfInfo, AveSPWeb parentSPWeb, WFAveSPObjectCache spObjectCache)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.RestoreAssociationData"))
            {
                SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(wfInfo.AssociationUnit); // CacheAssociationData(wfInfo); 下面开始还原wf 了，没必要cache 了 

                if (SPWorkflowProcessorRuntime.ProcessAssociation)
                {
                    try
                    {
                        WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(assoUnit.WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
                        {
                            SPWorkflowProcessorRuntime.MappingManager = parentSPWeb.ParentSite.MappingManager;
                            conflictResolutionInternal.ParentSPWeb = parentSPWeb;
                            conflictResolutionInternal.ProcessAssociationDataPreCondition(assoUnit);
                            conflictResolutionInternal.RestoreAssociationDataInternal(wfInfo, assoUnit, true, spObjectCache);
                        }
                    }
                    catch (SPWFProcessorException procException)
                    {
                        log.Log(AveLogLevel.INFO, "Skip restore workflow association, a known error occurred while restore workflow association, reason:{0}.", procException);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An unknown error occurred while restore workflow association.", e);
                    }
                }
                else
                {
                    log.Log(AveLogLevel.INFO, "Skip restore workflow associations, the option is not to restore workflow association");
                }

            }

        }

        public SPWFAssociationUnit CacheAssociationData(AveWorkflowInfo wfInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.CacheAssociationData"))
            {

                if (workflowRestoreCore != null)
                {
                    SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(wfInfo.AssociationUnit);
                    if (assoUnit != null && (assoUnit.IsBuiltinBaseId || !SPWorkflowProcessorRuntime.RestoreBuiltinOnly))
                    {
                        if (!mUnitsOfBackup.ContainsKey(assoUnit.SerializableData.mId))
                        {
                            mUnitsOfBackup.Add(assoUnit.SerializableData.mId, wfInfo);
                        }
                        else
                        {
                            mUnitsOfBackup[assoUnit.SerializableData.mId] = wfInfo;
                        }
                        wfInfo.OrigAssoId = assoUnit.SerializableData.mId;
                        wfInfo.OrigBaseId = assoUnit.SerializableData.mBaseId;
                        wfInfo.OrigAssoName = assoUnit.SerializableData.mOriginalName;
                    }
                    return assoUnit;
                }
                else
                {
                    log.Log(AveLogLevel.DEBUG, "Cannot cache workflow  association data, due to the workflow restore core object is null.");
                    return null;
                }

            }

        }

        #endregion

        #region [******Handle Workflow Instance******]

        internal void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveListItem item, bool isPostAction = false)
        {
            WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(instanceUnit.WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
            if (SPWorkflowProcessorRuntime.MappingManager == null)
            {
                if (ParentSPWeb != null)
                {
                    SPWorkflowProcessorRuntime.MappingManager = ParentSPWeb.ParentSite.MappingManager;
                }
                else
                {
                    SPWorkflowProcessorRuntime.MappingManager = WrapperRuntime.CurrentContext.MappingManager;
                }
            }
            instanceUnit.RestartRunningInstance = RestartRunningInstance.HasValue ? RestartRunningInstance.Value : SPWorkflowProcessorRuntime.RestartRunningInstance;
            conflictResolutionInternal.ParentSPWeb = ParentSPWeb;
            conflictResolutionInternal.HandleInstanceConflict(instanceUnit, item);
        }

        internal void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveWeb web, bool isPostAction = false)
        {
            WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(instanceUnit.WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
            {
                if (SPWorkflowProcessorRuntime.MappingManager == null)
                {
                    if (ParentSPWeb != null)
                    {
                        SPWorkflowProcessorRuntime.MappingManager = ParentSPWeb.ParentSite.MappingManager;
                    }
                    else
                    {
                        SPWorkflowProcessorRuntime.MappingManager = WrapperRuntime.CurrentContext.MappingManager;
                    }
                }
                instanceUnit.RestartRunningInstance = RestartRunningInstance.HasValue ? RestartRunningInstance.Value : SPWorkflowProcessorRuntime.RestartRunningInstance;
                conflictResolutionInternal.ParentSPWeb = ParentSPWeb;
                conflictResolutionInternal.HandleInstanceConflict(instanceUnit, web);
            }
        }

        #endregion

        #region [******Handle Workflow Scheduel******]
        public void RestoreScheduleData(AveWorkflowInfo wfInfo, IAveListItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (item.Web.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
            {
                log.Warn("Skip restoring workflow schedule data because of lacking of permission.");
                return;
            }
            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            workflowRestoreCore.RestoreSchedule(wfAssociationUnit, item);
        }

        public void RestoreScheduleData(AveWorkflowInfo wfInfo, IAveWeb web)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }
            if (web.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
            {
                log.Warn("Skip restoring workflow schedule data because of lacking of permission.");
                return;
            }
            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            if (SPWorkflowProcessorRuntime.ProcessAssociation)
            {
                workflowRestoreCore.RestoreSchedule(wfAssociationUnit, web);
            }
        }
        #endregion

        #region Workflow Templates
        public void RestoreNintexWorkflowTemplates(AveWorkflowInfo wfInfo, IAveWeb web)
        {
            byte[] workflowTemplatesData = wfInfo.AssociationUnit;
            try
            {
                SPWorkflowProcessorRuntime.RestoreCustomData(web, workflowTemplatesData, false);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "A error occurred while restore workflow templates , detail:{0}.", e.ToString());
            }
        }
        #endregion

        void IWFConflictResolution.CacheAssociationData(AveWorkflowInfo wfInfo)
        {
            CacheAssociationData(wfInfo);
        }

        #region new public methods for restore instance

        /// <summary>
        /// 还原listItem上的workflow instance
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="item"></param>
        public void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveSPListItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (item.ParentSite.SPSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
            {
                log.Warn("Skip restoring workflow instance because of lacking of permission.");
                return;
            }
            ParentSPWeb = item.ParentWeb as AveSPWeb;
            AveUserMappingService.AveSite = ParentSPWeb.ParentSite;

            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            HandleInstanceConflict(wfAssociationUnit, item.SPListItem);
        }

        /// <summary>
        /// 还原document上的workflow instance
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="doc"></param>
        public void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveSPDoc doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            if (doc.ParentSite.SPSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
            {
                log.Warn("Skip restoring workflow instance because of lacking of permission.");
                return;
            }
            ParentSPWeb = doc.ParentWeb as AveSPWeb;
            AveUserMappingService.AveSite = ParentSPWeb.ParentSite;

            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            HandleInstanceConflict(wfAssociationUnit, doc.SPListItem);
        }

        /// <summary>
        /// 还原folder上的workflow instance
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="folder"></param>
        public void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveSPFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }
            if (folder.ParentSite.SPSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
            {
                log.Warn("Skip restoring workflow instance because of lacking of permission.");
                return;
            }
            ParentSPWeb = folder.ParentWeb as AveSPWeb;
            AveUserMappingService.AveSite = ParentSPWeb.ParentSite;

            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            HandleInstanceConflict(wfAssociationUnit, folder.SPListItem);
        }

        /// <summary>
        /// 还原web上的workflow instance
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="web"></param>
        public void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveSPWeb web)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }
            if (web.ParentSite.SPSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
            {
                log.Warn("Skip restoring workflow instance because of lacking of permission.");
                return;
            }
            ParentSPWeb = web as AveSPWeb;
            AveUserMappingService.AveSite = ParentSPWeb.ParentSite;

            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            HandleInstanceConflict(wfAssociationUnit, web.SPWeb);
        }

        #endregion

        public void SetWorkflowProcessorRuntime(AveSPWorkflowRestoreOption workflowRestoreOption)
        {
            if (workflowRestoreOption.ProcessAssociation.HasValue)
            { SPWorkflowProcessorRuntime.ProcessAssociation = workflowRestoreOption.ProcessAssociation.Value; }
            if (workflowRestoreOption.ProcessInstance.HasValue)
            { SPWorkflowProcessorRuntime.ProcessInstance = workflowRestoreOption.ProcessInstance.Value; }
            if (workflowRestoreOption.RestartRunningInstance.HasValue)
            { SPWorkflowProcessorRuntime.RestartRunningInstance = workflowRestoreOption.RestartRunningInstance.Value; }
            if (workflowRestoreOption.RestoreParentAssociationIfNotFound.HasValue)
            { SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound = workflowRestoreOption.RestoreParentAssociationIfNotFound.Value; }
            if (workflowRestoreOption.SkipRunningInstance.HasValue)
            { SPWorkflowProcessorRuntime.SkipRunningInstance = workflowRestoreOption.SkipRunningInstance.Value; }
            if (workflowRestoreOption.AllowDuplicateSPDAndNintexInSameWeb.HasValue)
            { SPWorkflowProcessorRuntime.IsAllowDuplicateSPDAndNintexInSameWeb = workflowRestoreOption.AllowDuplicateSPDAndNintexInSameWeb.Value; }
        }

        /// <summary>
        /// WFConflictResolution 和 WFConflictResolutionInternal中重复加了，以后考虑单提出一个方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objTitle"></param>
        /// <param name="type"></param>
        /// <param name="status"></param>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected AveWrapperReportDto GetReportDto(string name, string objTitle, AveReportObjectType type, AveStatus status, Exception ex, string message)
        {
            if (ex == null)
            {
                return new AveWrapperReportDto(name, objTitle, type, status, message);
            }
            if (ex is AveWrapperBaseException && !string.IsNullOrEmpty((ex as AveWrapperBaseException).I18NKey))
            {
                return new AveWrapperReportDto((ex as AveWrapperBaseException).I18NKey, name, objTitle, type, status, (ex as AveWrapperBaseException).Parameters);
            }
            else
            {
                return new AveWrapperReportDto(name, objTitle, type, status, ex.Message);
            }
        }

        public IReport GetReport()
        {
            return this.reportor;
        }

        public void Dispose()
        {
            reportor.Dispose();
        }

        internal WFConflictResolutionParameters GetWFConflictResolutionParametersBySelf()
        {
            WFConflictResolutionParameters parameterInstance = new WFConflictResolutionParameters();
            parameterInstance.TemplateOption = this.TemplateOption;
            parameterInstance.AssociationOption = this.AssociationOption;
            parameterInstance.ParentAssociationOption = this.ParentAssociationOption;
            parameterInstance.InstanceOption = this.InstanceOption;
            parameterInstance.mAssociationParentObject = this.mAssociationParentObject;
            parameterInstance.mUnitsOfBackup = this.mUnitsOfBackup;
            parameterInstance.reportor = this.reportor;
            parameterInstance.WebContentTypeAssociation = this.WebContentTypeAssociation;
            parameterInstance.workflowRestoreCore = this.workflowRestoreCore;
            parameterInstance.BackupNintexWorklfowToExportedFile = this.BackupNintexWorklfowToExportedFile;
            return parameterInstance;
        }

        //public void UpdateWorkflowStartOptions()
        //{
        //    WFConflictResolutionParameters parameters = GetWFConflictResolutionParametersBySelf();
        //    var units10Mode = parameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored;
        //    var units13Mode = parameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored;
        //    foreach (var unit in units10Mode.Values)
        //    {
        //        WorkflowBusinessBehaviorController.GetStartOptionPostBehavior(unit, unit.SPAssociation).Run();
        //    }
        //    foreach (var unit in units13Mode.Values)
        //    {
        //        WorkflowBusinessBehaviorController.GetStartOptionPostBehavior(unit, unit.WorkflowSubscription).Run();
        //    }
        //}
        
    }

    internal class WFConflictResolutionInternal
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected const int MaxRenameTimes = 50;

        protected const string AppendSuffix = "_";

        protected bool needAssociationConflictResolution
        {
            get
            {
                return mWFCRParameters.AssociationOption != WFAssociationConflictResolutionOption.NotOverwrite;
            }
        }

        protected WFConflictResolutionParameters mWFCRParameters = null;

        internal WFConflictResolutionParameters WFCRParameters
        {
            get { return mWFCRParameters; }
            set { mWFCRParameters = value; }
        }

        public AveSPWeb ParentSPWeb { get; set; }

        protected WFConflictResolutionInternal()
        { }

        protected WFConflictResolutionInternal(WFConflictResolutionParameters parameters)
        {
            mWFCRParameters = parameters;
        }

        [ThreadStatic]
        private static WFConflictResolutionInternalFor10Model mConflictResolutionInternal10Model = null;
        private static WFConflictResolutionInternalFor10Model GetConflictResolutionInternal10Model(WFConflictResolutionParameters parameters)
        {
            if (mConflictResolutionInternal10Model == null)
            {
                mConflictResolutionInternal10Model = new WFConflictResolutionInternalFor10Model(parameters);
            }
            else
            {
                mConflictResolutionInternal10Model.Update(parameters);
            }
            return mConflictResolutionInternal10Model;
        }
        [ThreadStatic]
        private static WFConflictResolutionInternalFor13Model mConflictResolutionInternal13Model = null;
        [ThreadStatic]
        private static WFConflictResolutionInternalForExportedNintex mConflictResolutionInternalExportedNintex;

        private static WFConflictResolutionInternalFor13Model GetConflictResolutionInternal13Model(WFConflictResolutionParameters parameters)
        {
            if (mConflictResolutionInternal13Model == null)
            {
                mConflictResolutionInternal13Model = new WFConflictResolutionInternalFor13Model(parameters);
            }
            else
            {
                mConflictResolutionInternal13Model.Update(parameters);
            }
            return mConflictResolutionInternal13Model;
        }
        private static WFConflictResolutionInternal GetConflictResolutionInternalExportedNintex(WFConflictResolutionParameters parameters)
        {
            if (mConflictResolutionInternalExportedNintex == null)
            {
                mConflictResolutionInternalExportedNintex = new WFConflictResolutionInternalForExportedNintex(parameters);
            }
            else
            {
                mConflictResolutionInternalExportedNintex.Update(parameters);
            }
            return mConflictResolutionInternalExportedNintex;
        }

        public virtual bool UpdateWorkflowStartOptions(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation desWorkflowAssociation = null) { return false; }

        public static WFConflictResolutionInternal GetInstance(SPWFInternalPlatform platformType, WFConflictResolutionParameters parameters)
        {
            switch (platformType)
            {
                case SPWFInternalPlatform.Default:
                    return new WFConflictResolutionInternal();
                case SPWFInternalPlatform.WF2010PlatformType:
                    return GetConflictResolutionInternal10Model(parameters);
                case SPWFInternalPlatform.WF2013PlatformType:
                    return GetConflictResolutionInternal13Model(parameters);
                case SPWFInternalPlatform.WFExportedNintex:
                    return GetConflictResolutionInternalExportedNintex(parameters);
                default:
                    throw new NotSupportedException("Not support platform type.");
            }
        }


        public void Update(WFConflictResolutionParameters parameters)
        {
            this.mWFCRParameters = parameters;
        }

        public virtual void HandleAssociationConflictInternal(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, WFAveSPObjectCache spObjectCache, bool isPostAction) { }

        public virtual void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveListItem item) { }

        public virtual void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveWeb web) { }

        public WorkflowAssociationParentObject SetAssociationParentObject(object value, ref AveWorkflowRestoreCore workflowRestoreCore, bool webContentTypeAssociation)
        {
            WorkflowAssociationParentObject associationParentObject = null;
            if (value is IAveList)
            {
                associationParentObject = new WorkflowAssociationParentObject((IAveList)value);
            }
            else if (value is IAveContentType)
            {
                if (webContentTypeAssociation || (value as IAveContentType).ParentList == null)
                {
                    associationParentObject = new WorkflowAssociationParentObject((IAveContentType)value, SPWFAssociationParentType.WebContentType);
                }
                else
                {
                    associationParentObject = new WorkflowAssociationParentObject((IAveContentType)value, SPWFAssociationParentType.ListContentType);
                }
            }
            else if (value is IAveWeb)
            {
                associationParentObject = new WorkflowAssociationParentObject((IAveWeb)value);
            }
            if (associationParentObject == null)
            {
                throw new InvalidDataException(string.Format("associationParentObject is not in an expected range.Type:{0}", value == null ? "NULL" : value.GetType().ToString()));
            }
            SetWorkflowRestoreCore(associationParentObject.ParentType, ref workflowRestoreCore);
            return associationParentObject;
        }

        private void SetWorkflowRestoreCore(SPWFAssociationParentType parentType, ref AveWorkflowRestoreCore workflowRestoreCore)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.SetWorkflowRestoreCore"))
            {

                if (workflowRestoreCore == null || workflowRestoreCore.ParentType != parentType)
                {
                    workflowRestoreCore = AveWorkflowRestoreCoreFactory.GetWorkflowRestoreCore(parentType, workflowRestoreCore.WFAssociationProcessor, workflowRestoreCore.WFAssociationProcessor13Model, workflowRestoreCore.WFInstanceProcessor, workflowRestoreCore.WFInstanceProcessor13Model);
                }

            }

        }

        /// <summary>
        /// 还原workflow definition前对workflow association 数据处理的方法
        /// </summary>
        /// <param name="assoUnit"></param>
        internal virtual void ProcessAssociationDataPreCondition(SPWFAssociationUnit assoUnit)
        { }


        protected virtual bool IsNeedRestoreWorkflow(SPWFAssociationUnit assoUnit)
        {
            return true;
        }

        /// <summary>
        /// 还原workflow definition，无论是正常还原还是反插都会走这个方法
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="assoUnit"></param>
        /// <param name="needReport"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "assoUnit,wfInfo are argument names.")]
        internal void RestoreAssociationDataInternal(AveWorkflowInfo wfInfo, SPWFAssociationUnit assoUnit, bool needReport, WFAveSPObjectCache spObjectCache)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.RestoreAssociationDataInternal"))
            {

                if (wfInfo == null)
                {
                    throw new AveArgumentNullException("Invalid argument, argument wfInfo can not be null.");
                }

                if (this.mWFCRParameters.mAssociationParentObject == null)
                {
                    throw new AveArgumentNullException("Invalid argument, argument mWFCRParameters.mAssociationParentObject can not be null.");
                }

                if (assoUnit == null)
                {
                    throw new AveArgumentNullException("Invalid argument, argument assoUnit can not be null.");
                }

                InitAssociationUnitParentObjectInfo(assoUnit);

                //local -> online filter out nintex workflow
                if (!IsNeedRestoreWorkflow(assoUnit))
                {
                    return;
                }

                string parentTitle;
                assoUnit.reusableWFContentTypeName = wfInfo.CTName;
                var type = GetParentObjectType(mWFCRParameters.mAssociationParentObject, out parentTitle);
                InitAssociationUnitWorkflowType(assoUnit);
                ParentSPWeb.HasNintexWF |= assoUnit.WorkflowType == WorkflowType.NintexWorkflowLocal;

                try
                {
                    if (SPWorkflowProcessorRuntime.RestoreBuiltinOnly && !assoUnit.IsBuiltinBaseId)
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.SkipNonBuiltinAssociationException, AveInternalResourceKey.Wrapper_Exception_Workflow_NotBuildinAssociationSkipException, new object[] { assoUnit.SerializableData.mOriginalName });
                    }

                    ActiveDependencyFeatures(assoUnit, WFCRParameters.mAssociationParentObject);

                    if (!HasNotRestoredWF(assoUnit))
                    {
                        HandleAssociationConflictInternal(assoUnit, mWFCRParameters.mAssociationParentObject, spObjectCache, false);
                        if (needReport)
                        {
                            mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Successful, string.Empty));
                        }
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    if (needReport)
                    {
                        switch (procException.ErrorCode)
                        {
                            case SPWFProcessorErrorCode.PutIntoPostAction:
                                log.Info("The workflow definition will be restored later, workflow definition name:{0}.", assoUnit.SerializableData.mOriginalName);
                                break;
                            case SPWFProcessorErrorCode.WorkflowServiceNotAvailableException:
                                log.Info("Workflow service is not available for current site. error message: {0}", procException);
                                mWFCRParameters.reportor.AddDetail(GetReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Skipped, procException, string.Empty));
                                break;
                            case SPWFProcessorErrorCode.SkipNonBuiltinAssociationException:
                                log.Info("Skip restoring the workflow as it is not a Builtin workflow and the option is only restoring Builtin workflow. Error message: {0}", procException);
                                mWFCRParameters.reportor.AddDetail(GetReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Skipped, procException, string.Empty));
                                break;
                            case SPWFProcessorErrorCode.AssociationDependencyFeatureNotActivatedError:
                                log.Warn("Failed restoring the workflow association as the dependency features of this workflow association are not activated. Error message: {0}", procException);
                                mWFCRParameters.reportor.AddDetail(GetReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Failed, procException, string.Empty));
                                break;
                            case SPWFProcessorErrorCode.WebServiceOperationNotSupported:
                                log.Warn("Workflow defination option is not availible, maybe it's disable at webapplication level , error message: {0}", procException);
                                mWFCRParameters.reportor.AddDetail(GetReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Skipped, procException, string.Empty));
                                break;
                            default:
                                log.Warn("An unexpected error occurred while restoring workflow association, error message: {0}", procException);
                                mWFCRParameters.reportor.AddDetail(GetReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Failed, procException, string.Empty));
                                break;
                        }
                    }
                    throw;
                }
                catch (AveWrapperSkipException ex)
                {
                    log.Log(AveLogLevel.INFO, "Skip restore workflow association, reason:{0}.", ex.Message);
                    if (needReport)
                    {
                        mWFCRParameters.reportor.AddDetail(GetReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Skipped, ex, string.Empty));
                    }
                }
                catch (AveWrapperWorkflowException ex)
                {
                    log.Warn("An AveWrapperWorkflowException was thrown while handling association conflict. Association:{0}\n{1}", assoUnit.SerializableData.mOriginalName, ex);
                    if (needReport)
                    {
                        mWFCRParameters.reportor.AddDetail(GetReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Failed, ex.InnerException, string.Empty));
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An Error occurred while handle association conflict.Association:{0}\n{1}", assoUnit.SerializableData.mOriginalName, ex);
                    mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(assoUnit.SerializableData.mOriginalName, parentTitle, type, AveStatus.Skipped, AveReportResource.Wrapper_Report_RestoreWFWithoutPermission));
                }
                catch (Exception ex)
                {
                    log.Warn("An Error occurred while handle association conflict.Association:{0}\n{1}", assoUnit.SerializableData.mOriginalName, ex);
                }
            }
        }

        private void InitAssociationUnitParentObjectInfo(SPWFAssociationUnit assoUnit)
        {
            assoUnit.ParentObject = mWFCRParameters.mAssociationParentObject.ParentObject;
            assoUnit.ParentObjectType = mWFCRParameters.mAssociationParentObject.ParentType;
        }

        internal virtual void InitAssociationUnitWorkflowType(SPWFAssociationUnit associationUnit)
        {
            if (NintexWorkflowUtility.IsNintexWorkflow(associationUnit))
            {
                associationUnit.WorkflowType = WorkflowType.NintexWorkflowLocal;
            }
            else
            {
                associationUnit.WorkflowType = WorkflowType.SP2010PlatformWorkflow;
            }
        }

        private bool HasNotRestoredWF(SPWFAssociationUnit assoUnit)
        {
            if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.NoRestoredWFCache.Contains(assoUnit.SerializableData.mBaseId))
            {
                log.Debug("The workflow {0} will be restored later, because previous version is not restored.", assoUnit.SerializableData.mName);
                this.ParentSPWeb.ParentSite.MappingManager.SiteMappingManager.AddWorkflowIdMapping(assoUnit.SerializableData.mSourceId, assoUnit.SerializableData.mId);
                byte[] cacheData = SPWFAssociationUnit.Save(assoUnit);
                var parentObject = WFCRParameters.mAssociationParentObject;
                var web = parentObject.ParentWeb;
                var listId = string.Empty;
                var parentId = string.Empty;
                switch (parentObject.ParentType)
                {
                    case SPWFAssociationParentType.Web:
                        break;
                    case SPWFAssociationParentType.List:
                        listId = parentObject.ID.ToString();
                        parentId = parentObject.ID.ToString();
                        break;
                    case SPWFAssociationParentType.ListContentType:
                        listId = parentObject.ParentContentType.ParentList.ID.ToString();
                        parentId = parentObject.ParentContentType.ID.ToString();
                        break;
                    case SPWFAssociationParentType.WebContentType:
                        listId = string.Empty;
                        parentId = parentObject.ParentContentType.ID.ToString();
                        break;
                    default:
                        break;
                }
                if (parentObject.ParentType != SPWFAssociationParentType.Invalid)
                {
                    SPWorkflowProcessorRuntime.OnCacheData(web.Site.Url, web.Site.ID.ToString(), web.ID.ToString(), listId, parentId, 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                }
                if (!this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.NeedPostActionAssociations.Contains(assoUnit.SerializableData.mId))
                {
                    this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.NeedPostActionAssociations.Add(assoUnit.SerializableData.mId);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        internal virtual void RestoreTemplateDataInternal(SPWFAssociationUnit assoUnit)
        {
        }

        #region active feature

        private void ActiveDependencyFeatures(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parentObject)
        {
            if (BuiltinWorkflowBaseIdCollection.IsBuiltinBaseId(assoUnit.SerializableData.mBaseId))
            {
                ActiveDependencyFeaturesForBuiltin(parentObject, assoUnit.SerializableData.mBaseId);
            }
        }

        private void ActiveDependencyFeaturesForBuiltin(WorkflowAssociationParentObject parentObject, Guid templateBaseId)
        {
            bool isNeedReload = false;
            IAveSite site = parentObject.ParentWeb.Site;
            try
            {
                if (BuiltinWorkflowBaseIdCollection.ApprovalWorkflowTemplateBaseIdForSP10.Contains(templateBaseId) ||
                    BuiltinWorkflowBaseIdCollection.Collect_FeedbackWorkflowTemplateBaseIdForSP10.Contains(templateBaseId) ||
                    BuiltinWorkflowBaseIdCollection.Collect_SignatureWorkflowTemplateBaseIdForSP10.Contains(templateBaseId))//feature: Workflows
                {
                    //"a42f749f-8633-48b7-9b22-403b40190407" "a42f749f-8633-48b7-9b22-403b40190409" "3bc0c1e1-b7d5-4e82-afd7-9f7e59b60407" "3bc0c1e1-b7d5-4e82-afd7-9f7e59b60409"
                    //激活依赖的feature，上面四个hidden feature会在Workflows feature开启关闭时同时开启或关闭当前web语言对应的feature，不需要额外处理
                    //先激活四个dependency  feature，再激活Workflows feature
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.OffWFCommon, site);
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.ReviewWorkflowsSPD, site);
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.SignaturesWorkflowSPD, site);
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.TranslationWorkflow, site);
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.Workflows, site); //Workflows
                }
                else if (BuiltinWorkflowBaseIdCollection.Disposition_ApprovalWorkflowTemplateBaseIdForSP10.Contains(templateBaseId)) //feature: Disposition Approval Workflow
                {
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.OffWFCommon, site);
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.ExpirationWorkflow, site); //Disposition Approval Workflow
                }
                else if (BuiltinWorkflowBaseIdCollection.Publishing_ApprovalWorkflowTemplateBaseIdForSP10.Contains(templateBaseId)) //feature: Publishing Approval Workflow
                {
                    //"19f5f68e-1b92-4a02-b04d-61810ead0407" "19f5f68e-1b92-4a02-b04d-61810ead0409" 
                    //这两个hidden feature会在开启关闭ReviewPublishingSPD时同时开启或关闭，不需要额外处理
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.OffWFCommon, site);
                    isNeedReload |= SafeAddSiteFeature(AveSP2010FeatureDefinitions.ReviewPublishingSPD, site); ////Publishing Approval Workflow feature
                }
                else if (BuiltinWorkflowBaseIdCollection.Three_StateWorkflowTemplateBaseIdForSP10.Contains(templateBaseId))
                {
                    isNeedReload = SafeAddSiteFeature(AveSP2010FeatureDefinitions.IssueTrackingWorkflow, site); //Three State Workflow
                }
                if (isNeedReload)
                {
                    parentObject.ReloadParentWeb();
                }
            }
            catch (Exception e)
            {
                log.Info("An error occurred while trigger workflow dependency site features, template base id: {0}, error message: {1}", templateBaseId, e);
                if (isNeedReload)
                {
                    parentObject.ReloadParentWeb();
                }
            }
        }

        private bool SafeAddSiteFeature(Guid featureId, IAveSite site)
        {
            bool needReload = false;
            try
            {
                if (site.Features[featureId] == null)
                {
                    needReload = true;
                    site.Features.Add(featureId, true);
                }
            }
            catch (Exception e)
            {
                log.Debug("An error occurred while add single workflow dependency feature to site collection, feature id: {0}, error message: {1}", featureId, e);
            }
            return needReload;
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objTitle"></param>
        /// <param name="type"></param>
        /// <param name="status"></param>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected AveWrapperReportDto GetReportDto(string name, string objTitle, AveReportObjectType type, AveStatus status, Exception ex, string message)
        {
            if (ex == null)
            {
                return new AveWrapperReportDto(name, objTitle, type, status, message);
            }
            if (ex is AveWrapperBaseException && !string.IsNullOrEmpty((ex as AveWrapperBaseException).I18NKey))
            {
                return new AveWrapperReportDto((ex as AveWrapperBaseException).I18NKey, name, objTitle, type, status, (ex as AveWrapperBaseException).Parameters);
            }
            else
            {
                return new AveWrapperReportDto(name, objTitle, type, status, ex.Message);
            }
        }

        internal AveReportObjectType GetParentObjectType(WorkflowAssociationParentObject parent, out string parentTitle)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.GetParentObjectType"))
            {

                var result = AveReportObjectType.Undefined;
                parentTitle = string.Empty;
                if (parent != null)
                {
                    object parentObject = parent.ParentObject;
                    switch (parent.ParentType)
                    {
                        case SPWFAssociationParentType.ListContentType:
                            result = AveReportObjectType.ListCTWorkflowDefinition;
                            parentTitle = (parentObject as IAveContentType).Name;
                            break;
                        case SPWFAssociationParentType.WebContentType:
                            result = AveReportObjectType.WebCTWorkflowDefinition;
                            parentTitle = (parentObject as IAveContentType).Name;
                            break;
                        case SPWFAssociationParentType.List:
                            result = AveReportObjectType.ListWorkflowDefinition;
                            parentTitle = (parentObject as IAveList).Title;
                            break;
                        case SPWFAssociationParentType.Web:
                            result = AveReportObjectType.WebWorkflowDefinition;
                            parentTitle = (parentObject as IAveWeb).Title;
                            break;
                        default:
                            break;
                    }
                }
                return result;

            }

        }

        protected void AddFieldMapping(SPWFInstanceUnit instanceUnit, Guid listId)
        {
            IAveFieldMapping listMapping;
            if (ParentSPWeb != null && ParentSPWeb.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromListFieldsMapping(listId, out listMapping))
            {
                MergeMapping(instanceUnit.mWFFieldIDMapping, listMapping.AddFieldIdMapping);
                instanceUnit.mWFFieldIDMapping.Clear();
                MergeMapping(instanceUnit.mWFFieldInternalNameMapping, listMapping.AddFieldInternalNameMapping);
                instanceUnit.mWFFieldInternalNameMapping.Clear();
                MergeMapping(instanceUnit.mWFFieldDisplayNameMapping, listMapping.AddFieldDisplayNameMapping);
                instanceUnit.mWFFieldDisplayNameMapping.Clear();
            }
        }

        private void MergeMapping<TKey, TValue>(Dictionary<TKey, TValue> src, Action<TKey, TValue> add)
        {
            foreach (var value in src)
            {
                add(value.Key, value.Value);
            }
        }
    }

    /// <summary>
    /// Create Instance by WFConflictResolutionInternal
    /// </summary>
    internal sealed class WFConflictResolutionInternalFor10Model : WFConflictResolutionInternal
    {
        public WFConflictResolutionInternalFor10Model(WFConflictResolutionParameters parameters)
            : base(parameters)
        {

        }

        /// <summary>
        /// 由于当前on-premise Nintex Workflow还原到online nintex Workflow 有两套备份数据，因此会还原两遍，
        /// 而10mode对于nintex Workflow的还原实际上是直接过滤的，为了不导致同一个nintex Workflow还原产生两个job report，
        /// 对于10mode 的nitex workflow的此种特殊情况 暂时先不加到job report中
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <returns></returns>
        protected override bool IsNeedRestoreWorkflow(SPWFAssociationUnit assoUnit)
        {
            //目前只有nintex workflow的custom data中有数据，目前365不支持还原nintex workflow，所以在此处直接禁掉,nintex workflow都是10mode workflow，所以在此处进行判断即可
            if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel && NintexWorkflowUtility.IsNintexWorkflow(assoUnit))
            {
                log.Info("Skip restoring nintex workflow with WF2010PlatformType, workflow name is {0}", assoUnit.SerializableData.mName);
                return false;
            }
            return true;
        }

        #region [******Handle WorkflowAssociation******]

        public override void HandleAssociationConflictInternal(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.HandleAssociationConflictInternal"))
            {
                log.Log(AveLogLevel.DEBUG, "Begin to restore workflow association for 10 model internally, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
                assoUnit.IsPostAction = isPostAction;
                CheckNeedRestoreByDependencyFeature(assoUnit, this.WFCRParameters.mAssociationParentObject.ParentWeb);

                //不直接使用comare结果确定workflow是否还原，先走restore option，确定需要还原再comare
                //一些情况，比如skip,append 其实不需要check template冲突
                IAveWorkflowAssociation asso = FindAssociation(assoUnit.SerializableData.mName, parent.WorkflowAssociations);
                //reset ForceUpdate to default value "true" before restoring workflow definition
                this.mWFCRParameters.workflowRestoreCore.ForceUpdate = true;
                if (asso == null)
                {
                    //没冲突，直接还原
                    mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                }
                else
                {
                    //先处理option，在一些需要compare的option中再进行compare
                    #region Compare and Conflict Resolution
                    AssociationConflictResolution(assoUnit, parent, asso, spObjectCache, isPostAction);
                }
                #endregion
            }

        }

        internal override void RestoreTemplateDataInternal(SPWFAssociationUnit assoUnit)
        {
            if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
            {
                throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Workflow_TemplateTypeNotSupportIn365);
            }
            SPWFAssociationProc proc = SPWFAssociationProc.CreateInstance(SPWFProcessorType.API);
            proc.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            InitAssociationUnitWorkflowType(assoUnit);
            IAveWorkflowTemplate template;
            IAveWorkflowTemplate templatebyname = null;
            switch (this.mWFCRParameters.TemplateOption)
            {
                case WFTemplateConflictResolutionOption.NotOverwrite:
                    {
                        template = GetTemplateByBaseId(assoUnit);
                        //baseid没有取到拿name再取一次
                        if (template == null)
                        {
                            log.Debug("Begin getting workflow template by name:{0}", assoUnit.SerializableData.mName);
                            templatebyname = GetTemplateByName(assoUnit, CultureInfo.CurrentUICulture);
                        }
                        //1.第一次还原，或者之前还原的都不是current version
                        //2.baseid与name都取不到的情况才会还原
                        if (template != null && !SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(assoUnit.ParentWeb.Site.ID, assoUnit.ParentWeb.ID, template.ID) || template == null && templatebyname == null)
                        {
                            proc.ParentObject = assoUnit.ParentWeb;
                            proc.ParentObjectType = SPWFAssociationParentType.Web;
                            proc.RestoreReusableWFTemplate(assoUnit);
                        }
                        else
                        {
                            var webid = Guid.Empty;
                            if (!assoUnit.mTemplateLibUnit.SerializableData.IsRootWebList)
                            {
                                webid = assoUnit.ParentWeb.ID;
                            }
                            if (!SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(assoUnit.ParentWeb.Site.ID, webid, assoUnit.SerializableData.mBaseId))
                            {
                                //Global Reusable Workflow Template ,而且不在cache中
                                SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Add(assoUnit.ParentWeb.Site.ID, webid, assoUnit.SerializableData.mBaseId, template != null ? template.ID : templatebyname.ID);
                            }
                            throw new AveWrapperSkipException("Skip restoring the workflow template as it conflict with destination.");
                        }
                        break;
                    }
                case WFTemplateConflictResolutionOption.Overwrite:
                    {
                        proc.ParentObject = assoUnit.ParentWeb;
                        proc.ParentObjectType = SPWFAssociationParentType.Web;
                        proc.RestoreReusableWFTemplate(assoUnit);
                        break;
                    }
            }

            ParentSPWeb.HasNintexWF |= assoUnit.WorkflowType == WorkflowType.NintexWorkflowLocal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <returns></returns>
        private IAveWorkflowTemplate GetTemplateByBaseId(SPWFAssociationUnit assoUnit)
        {
            return assoUnit.ParentWeb.WorkflowTemplates.GetTemplateByBaseID(assoUnit.SerializableData.mBaseId);
        }
        private IAveWorkflowTemplate GetTemplateByName(SPWFAssociationUnit assoUnit, CultureInfo cultureInfo)
        {
            return assoUnit.ParentWeb.WorkflowTemplates.GetTemplateByNmae(assoUnit.SerializableData.mName, cultureInfo);
        }

        #region check feature dependency ,check feauture逻辑都加在此处，目前暂时只处理nintex的

        private void CheckNeedRestoreByDependencyFeature(SPWFAssociationUnit assoUnit, IAveWeb parentWeb)
        {
            switch (GetTemplateType(assoUnit))
            {
                case AveWorkflowType.Builtin:
                    break;
                case AveWorkflowType.Nintex:
                    if (!IsNintexFeatureActivated(parentWeb))
                    {
                        log.Warn("Skipped restoring current workflow association [{0}] as dependency nintex workflow feature is not activated.", assoUnit.SerializableData.mName);
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationDependencyFeatureNotActivatedError, AveInternalResourceKey.Wrapper_Exception_Workflow_DependencyFeatureNotActivated);
                    }
                    break;
                case AveWorkflowType.SPD:
                    break;
                default:
                    break;
            }
        }

        private bool IsNintexFeatureActivated(IAveWeb parentWeb)
        {
            return parentWeb.Site.Features[AveSP2010FeatureDefinitions.NintexWorkflow] != null
                && parentWeb.Features[AveSP2010FeatureDefinitions.NintexWorkflowWeb] != null;
        }

        private AveWorkflowType GetTemplateType(SPWFAssociationUnit assoUnit)
        {
            var workflowType = AveWorkflowType.SPD;

            if (BuiltinWorkflowBaseIdCollection.IsBuiltinBaseId(assoUnit.SerializableData.mBaseId))
            {
                workflowType = AveWorkflowType.Builtin;
            }
            else if (NintexWorkflowUtility.IsNintexWorkflow(assoUnit))
            {
                workflowType = AveWorkflowType.Nintex;
            }

            return workflowType;
        }
        //todo:wbhu,后续改成用WorkflowType替代
        internal enum AveWorkflowType
        {
            Builtin,
            SPD,
            Nintex
        }

        #endregion

        /// <summary>
        /// 还原workflow definition前对workflow association 数据处理的方法
        /// </summary>
        /// <param name="assoUnit"></param>
        internal override void ProcessAssociationDataPreCondition(SPWFAssociationUnit assoUnit)
        {
            base.ProcessAssociationDataPreCondition(assoUnit);

            ProcessAssociatedFieldPreCondition(assoUnit);

            ProcessNintexWebFields(assoUnit);
        }

        private void ProcessNintexWebFields(SPWFAssociationUnit assoUnit)
        {
            if (ParentSPWeb != null &&
                ParentSPWeb.SPWeb != null &&
                NintexWorkflowUtility.IsNintexWorkflow(assoUnit))
            {
                NintexWorkflowUtility.EnsureNintexWebFields(ParentSPWeb.SPWeb, ParentSPWeb.ParentSite.ObjectModelFactory);
            }
        }

        /// <summary>
        /// 还原workflow definition前 反插associated field + 替换template文件中的对应的field 信息
        /// 只有10mode workflow中有
        /// </summary>
        /// <param name="assoUnit"></param>  
        private void ProcessAssociatedFieldPreCondition(SPWFAssociationUnit assoUnit)
        {
            try
            {
                var associatedFieldsBasicInfos = SPWorkflowSubFileUnit.GetAssociatedFields(assoUnit.TemplateLibUnit);
                //restore association data ,restore instance时赋值
                if (ParentSPWeb != null && associatedFieldsBasicInfos != null && associatedFieldsBasicInfos.Count > 0)
                {
                    //找到没有还原过，而且需要还原的field的备份数据
                    var fields = ParentSPWeb.Fields.XmlFields.
                        Where(field =>
                            associatedFieldsBasicInfos.ContainsKey(field.Value.ID) //在解析出来的备份数据中
                            && ParentSPWeb.Fields.FieldMapping.GetMappingRestoredFieldId(field.Value.ID) == Guid.Empty) //没有还原过
                        .ToDictionary(field => field.Key, field => field.Value);
                    //还原web fields
                    ParentSPWeb.Fields.RestoreFields(fields, new AveFieldRestoreOption());

                    SPWorkflowSubFileUnit.UpdateAssociatedFields(assoUnit.TemplateLibUnit, ParentSPWeb.Fields.FieldMapping);

                    ProcessAssociatedFieldPreconditionForNintex(assoUnit, ParentSPWeb.Fields.FieldMapping);
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while process associated field pre condition for 10 mode workflow. Error: {0}", e);
            }
        }

        /// <summary>
        /// 处理nintex workflow template中associated field数据
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="fieldMapping"></param>
        private void ProcessAssociatedFieldPreconditionForNintex(SPWFAssociationUnit assoUnit, IAveFieldMapping fieldMapping)
        {
            if (NintexWorkflowUtility.IsNintexWorkflow(assoUnit))
            {
                SPWorkflowSubListUnit listUnit = new SPWorkflowSubListUnit((SPWorkflowSubListSerializableData)assoUnit.SerializableData.mSerializableCustomData);

                SPWorkflowSubFileUnit.UpdateAssociatedFields(listUnit, fieldMapping);

                assoUnit.SerializableData.mSerializableCustomData = listUnit.Save();
            }
        }

        private IAveWorkflowAssociation FindAssociation(string assoName, IAveWorkflowAssociationCollection assoCollection)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.FindAssociation"))
            {
                //Todo:find association,考虑是否需要check 13 mode workflow
                return assoCollection.GetAssociationByName(assoName, CultureInfo.CurrentUICulture);

            }

        }

        /// <summary>
        /// 根据workflow association的baseId进行比对，返回实际的compare结果，不受任何option的影响
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="asso"></param>
        /// <returns></returns>
        protected WFAssociationConflictType CompareAssociation(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.CompareAssociation"))
            {

                //Do Compare
                if (!assoUnit.SerializableData.mBaseId.Equals(asso.BaseId))
                {
                    return WFAssociationConflictType.Template;
                }
                return WFAssociationConflictType.None;

            }

        }

        private void AssociationConflictResolution(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, IAveWorkflowAssociation asso, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.AssociationConflictResolution"))
            {
                log.Log(AveLogLevel.DEBUG, "Begin to handle workflow association conflict for 10 model, association option is {0}", this.mWFCRParameters.AssociationOption.ToString());
                switch (this.mWFCRParameters.AssociationOption)
                {
                    case WFAssociationConflictResolutionOption.Append:
                        //Rename workflow ，不需要compare template,因为rename后肯定不允许冲突
                        string oldName = assoUnit.SerializableData.mName;
                        string newName = RenameAssociation(oldName, parent);
                        assoUnit.SerializableData.mName = newName;
                        assoUnit.SerializableData.mOriginalName = newName;

                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        break;
                    case WFAssociationConflictResolutionOption.Overwrite:
                        //不需要compare。因为会remove，即使不remove也是throw exception，而不是restore
                        // ADO-188311 if the option is overwrite, don't need to check the instance count, just remove it
                        parent.WorkflowAssociations.Remove(asso);
                        if (parent.ParentWeb.Site.APIType == AveAPIType.Server)
                        {
                            NintexDatabaseUtility.DeleteNintexPublishedWorkflowRecord(asso.BaseId, parent.ParentWeb.Site.ID, parent.ParentWeb.Site.WebApplication.ID);
                        }
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        //int count = parent.WorkflowManager.CountWorkflows(asso);
                        //if (count == 0)
                        //{
                        //todo:是否需要remove template
                        //}
                        //else
                        //{

                        //    log.Warn("Skip to restore the workflow {0} with conflict option {1} as destination conflict workflow association has instances,destination instance count:{2}", asso.Name, mWFCRParameters.AssociationOption, count);
                        //    throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, AveInternalResourceKey.Wrapper_Exception_Workflow_AssociationConflictWithInstanceExist, new object[] { asso.Name, mWFCRParameters.AssociationOption, count });
                        //}

                        break;
                    case WFAssociationConflictResolutionOption.ForceOverwrite:
                        //不需要compare。因为会remove
                        //todo:是否需要remove template
                        parent.WorkflowAssociations.Remove(asso);
                        if (parent.ParentWeb.Site.APIType == AveAPIType.Server)
                        {
                            NintexDatabaseUtility.DeleteNintexPublishedWorkflowRecord(asso.BaseId, parent.ParentWeb.Site.ID, parent.ParentWeb.Site.WebApplication.ID);
                        }
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        break;
                    case WFAssociationConflictResolutionOption.UpdateOverwrite:
                        //需要check template
                        switch (CompareAssociation(assoUnit, asso))
                        {
                            case WFAssociationConflictType.Template:
                                throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, AveInternalResourceKey.Wrapper_Exception_Restore_WFDeNameOrTemplateConflictError);
                            case WFAssociationConflictType.Configuration:
                            case WFAssociationConflictType.Same:
                                //暂时还没有
                                break;
                            case WFAssociationConflictType.None:
                                mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                                break;
                        }
                        break;
                    case WFAssociationConflictResolutionOption.ForceUse:
                        //Todo:需要考虑是否需要check tempalte冲突，以及workflow 类型冲突，check冲突意义不大
                        //现在先保持原有逻辑，以后看看是否有必要保留这个option
                        this.mWFCRParameters.workflowRestoreCore.ForceUpdate = false;
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        break;
                    case WFAssociationConflictResolutionOption.NotOverwrite:
                        //skip也不需要comare
                        log.Log(AveLogLevel.INFO, "Skip restore workflow association due to the association {0} has already existed in the destination and option is not to force restoring the association", assoUnit.SerializableData.mOriginalName);
                        throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, AveInternalResourceKey.Wrapper_Exception_Restore_WFDeConflictError);
                    default:
                        //枚举类型都处理过了，这种情况一般不会出现
                        throw new AveWrapperException(AveWrapperErrorCode.DefinitionConflictResolutionOptionInvalid, "Association conflict resolution invalid.");
                }

            }

        }

        private string RenameAssociation(string oldName, WorkflowAssociationParentObject parent)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.RenameAssociation"))
            {

                string newName = oldName;
                int counter = 1;
                while (true)
                {
                    //走到这里，name肯定冲突，需要先append一次，再check
                    newName = oldName + AppendSuffix + counter;

                    if (FindAssociation(newName, parent.WorkflowAssociations) == null)
                    {
                        break;
                    }

                    if (counter > MaxRenameTimes)
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationRenameError);
                    }
                    counter++;
                }
                log.Debug(string.Format("Rename association\nOld name:{0}\nNew name{1}", oldName, newName));
                return newName;

            }

        }

        #endregion

        #region[******Handle WorkflowInstance******]

        public override void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveListItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.HandleInstanceConflict"))
            {

                if (CheckInstanceNeedSkip(instanceUnit, item))
                {
                    log.Info("SKip restoring current workflow instance,ParentItemUrl:{0}", item == null ? string.Empty : item.Url);
                    return;
                }
                log.Log(AveLogLevel.DEBUG, "Begin to restore 10 model workflow instance for item: {0}", item.Url);
                string parentAssociationName = null;
                try
                {
                    Guid parentAssoId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("#TemplateId");
                    if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(parentAssoId))
                    {
                        if (SPWorkflowProcessorRuntime.RestoreBuiltinOnly && !SPWFAssociationUnit.Load(this.mWFCRParameters.mUnitsOfBackup[parentAssoId].AssociationUnit).IsBuiltinBaseId)
                        {
                            log.Log(AveLogLevel.INFO, "Skip restoring 10 model workflow instance for item: {0}, because the option is only to restore built-in and current workflow association is not a built-in", item.Url);
                            return;
                        }
                    }
                    parentAssociationName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                    //Guid parentAssoBaseId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationBaseId");
                    bool needReport = true;
                    if (!TryFindParentAssociation(parentAssoId, item, instanceUnit, ref needReport))
                    {
                        if (!needReport)
                        {
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction);
                        }
                        throw new WFDefinitionNotFoundException(parentAssociationName, item.Name, item.ParentList.Title, item.ParentList.ParentWeb.Url);
                    }
                    if (!mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.ContainsKey(parentAssoId))
                    {
                        throw new WFDefinitionNotFoundException(parentAssociationName, item.Name, item.ParentList.Title, item.ParentList.ParentWeb.Url);
                    }
                    instanceUnit.ParentAssociationUnit = this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored[parentAssoId];

                    #region deal with InstanceConflict
                    log.Info("WorkflowInstance Conflict {0}", "Item Name is " + item.Name + " InstanceState is " + instanceUnit.InstanceItem.Properties["#InternalState"].ToString());
                    if ((int)instanceUnit.InstanceItem.Properties["#InternalState"] == 2)
                    {
                        var runningWorkflow = TryGetRunningInstance(item, instanceUnit);
                        if (runningWorkflow == null)
                        {
                            var createTimeWorkflow = TryGetAllInstance(item, instanceUnit);
                            if (createTimeWorkflow == null)
                            {
                                this.mWFCRParameters.workflowRestoreCore.RestoreInstance(instanceUnit, item);
                            }
                            else
                            {
                                InstanceConflictResolution(instanceUnit, item, createTimeWorkflow);
                            }
                        }
                        else
                        {
                            InstanceConflictResolution(instanceUnit, item, runningWorkflow);
                        }

                    }
                    else
                    {
                        var workflow = TryGetAllInstance(item, instanceUnit);
                        if (workflow == null)
                        {
                            this.mWFCRParameters.workflowRestoreCore.RestoreInstance(instanceUnit, item);
                        }
                        else
                        {
                            InstanceConflictResolution(instanceUnit, item, workflow);
                        }
                    }
                    #endregion
                    //var workflow = TryGetRunningInstance(item, instanceUnit);
                    //if (workflow == null)
                    //{
                    //    workflowRestoreCore.RestoreInstance(instanceUnit, item);
                    //}
                    //else
                    //{
                    //    InstanceConflictResolution(instanceUnit, item, workflow);
                    //}

                    if (!instanceUnit.ParentAssociationUnit.isCreateField)
                    {
                        AddFieldMapping(instanceUnit, item.ParentList.ID);
                        instanceUnit.ParentAssociationUnit.isCreateField = true;
                    }
                    this.mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(parentAssociationName, item.Name, AveReportObjectType.WorkflowInstance, AveStatus.Successful, string.Empty));

                }
                catch (SPWFProcessorException ex)
                {
                    if (ex.ErrorCode == SPWFProcessorErrorCode.PutIntoPostAction)
                    {
                        log.Debug("Skip to restore the workflow instance of {0} at this moment, it will be restored later. Message:{1}", parentAssociationName, ex.ToString());
                    }
                    else
                    {
                        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreWorkflowInstanceFailedEventMessage(parentAssociationName, ex));
                        this.mWFCRParameters.reportor.AddDetail(GetReportDto(parentAssociationName, item.Name, AveReportObjectType.WorkflowInstance, AveStatus.Skipped, ex, string.Empty));
                    }

                }
                catch (WFDefinitionNotFoundException ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreWorkflowInstanceFailedEventMessage(parentAssociationName, ex));
                    this.mWFCRParameters.reportor.AddDetail(GetReportDto(parentAssociationName, item.Name, AveReportObjectType.WorkflowInstance, AveStatus.Skipped, ex, string.Empty));
                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreWorkflowInstanceFailedEventMessage(parentAssociationName, ex));
                    this.mWFCRParameters.reportor.AddDetail(GetReportDto(parentAssociationName, item.Name, AveReportObjectType.WorkflowInstance, AveStatus.Failed, ex, string.Empty));
                }
            }

        }

        public override void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveWeb web)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.HandleInstanceConflict_Web"))
            {
                if (CheckInstanceNeedSkip(instanceUnit, web))
                {
                    log.Info("SKip restoring current workflow instance,ParentWebUrl:{0}", web == null ? string.Empty : web.Url);
                    return;
                }
                string parentAssociationName = null;
                try
                {
                    log.Log(AveLogLevel.DEBUG, "Begin to restore 10 model workflow instance for web: {0}", web.Url);
                    Guid parentAssoId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("#TemplateId");
                    parentAssociationName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                    if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(parentAssoId))
                    {
                        if (SPWorkflowProcessorRuntime.RestoreBuiltinOnly && !SPWFAssociationUnit.Load(this.mWFCRParameters.mUnitsOfBackup[parentAssoId].AssociationUnit).IsBuiltinBaseId)
                        {
                            log.Log(AveLogLevel.DEBUG, "Skip restoring 10 model workflow instance for web: {0}, because the option is only to restore built-in and current workflow association is not a built-in", web.Url);
                            return;
                        }
                    }

                    //Guid parentAssoBaseId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationBaseId");
                    bool needReport = true;
                    if (!TryFindParentAssociation(parentAssoId, web, instanceUnit, ref needReport))
                    {
                        if (!needReport)
                        {
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction);
                        }
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.ParentAssociationCannotBeFound);
                    }
                    if (!mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.ContainsKey(parentAssoId))
                    {
                        throw new WFDefinitionNotFoundException(parentAssociationName, web.ID.ToString(), web.Title, web.Url);
                    }
                    instanceUnit.ParentAssociationUnit = this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored[parentAssoId];

                    AveStatus workflowInstanceRestoreStatus = AveStatus.Successful;
                    #region deal with InstanceConflict
                    log.Info("WorkflowInstance Conflict", " Web Title is " + web.Title + " Item Name is " + web.Name + " InstanceState is " + instanceUnit.InstanceItem.Properties["#InternalState"].ToString());
                    if ((int)instanceUnit.InstanceItem.Properties["#InternalState"] == 2)
                    {
                        var runningWorkflow = TryGetRunningInstance(web, instanceUnit);
                        if (runningWorkflow == null)
                        {
                            var createTimeWorkflow = TryGetAllInstance(web, instanceUnit);
                            if (createTimeWorkflow == null)
                            {
                                this.mWFCRParameters.workflowRestoreCore.RestoreInstance(instanceUnit, web);
                            }
                            else
                            {
                                workflowInstanceRestoreStatus = InstanceConflictResolution(instanceUnit, web, createTimeWorkflow);
                            }
                        }
                        else
                        {
                            workflowInstanceRestoreStatus = InstanceConflictResolution(instanceUnit, web, runningWorkflow);
                        }

                    }
                    else
                    {
                        var workflow = TryGetAllInstance(web, instanceUnit);
                        if (workflow == null)
                        {
                            this.mWFCRParameters.workflowRestoreCore.RestoreInstance(instanceUnit, web);
                        }
                        else
                        {
                            workflowInstanceRestoreStatus = InstanceConflictResolution(instanceUnit, web, workflow);
                        }
                    }
                    #endregion

                    this.mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(parentAssociationName, web.Title, AveReportObjectType.WorkflowInstance, workflowInstanceRestoreStatus, string.Empty));
                }
                catch (SPWFProcessorException ex)
                {
                    if (ex.ErrorCode == SPWFProcessorErrorCode.PutIntoPostAction)
                    {
                        log.Debug("Skip to restore the workflow instance .Web: {0} at this moment, it will be restored later. Message:{1}", web.Url, ex.ToString());
                    }
                    else
                    {
                        log.Warn("An WFProcessorException occurred while handle instance conflict.Web:{0}\r\n{1}",
                       web.Url, ex.ErrorCodeString);
                        this.mWFCRParameters.reportor.AddDetail(GetReportDto(parentAssociationName, web.Title, AveReportObjectType.WorkflowInstance, AveStatus.Failed, ex, string.Empty));
                    }

                }
                catch (WFDefinitionNotFoundException ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreWorkflowInstanceFailedEventMessage(parentAssociationName, ex));
                    this.mWFCRParameters.reportor.AddDetail(GetReportDto(parentAssociationName, web.Title, AveReportObjectType.WorkflowInstance, AveStatus.Skipped, ex, string.Empty));
                }
                catch (Exception ex)
                {
                    log.Warn("An Error occurred while handle instance conflict.Web:{0}\r\n{1}", web.Url, ex);
                    this.mWFCRParameters.reportor.AddDetail(GetReportDto(parentAssociationName, web.Title, AveReportObjectType.WorkflowInstance, AveStatus.Failed, ex, string.Empty));
                }

            }

        }

        private bool CheckInstanceNeedSkip(SPWFInstanceUnit instanceUnit, object parentObject)
        {
            if (instanceUnit == null)
            {
                log.Warn("Argument instanceUnit is null while checking workflow instance need skip,the workflow instance will not be restored.");
                return true;

            }
            if (parentObject == null)
            {
                log.Warn("Argument parentObject is null while checking workflow instance need skip,the workflow instance will not be restored.");
                return true;
            }

            if (!SPWorkflowProcessorRuntime.ProcessInstance)
            {
                log.Log(AveLogLevel.INFO, "The option ProcessInstance for restoring workflow instance is false,the workflow instance will not be restored.");
                return true;
            }
            if (SPWorkflowProcessorRuntime.ObjectModelFactory != null && SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
            {
                log.Log(AveLogLevel.INFO, "Current object model is Client, workflow instance will not be restored.");
                return true;
            }
            if (SPWorkflowProcessorRuntime.SkipRunningInstance && ((int)instanceUnit.InstanceItem.Properties["#InternalState"] == 2 || (int)instanceUnit.InstanceItem.Properties["#Status1"] == 2))
            {
                log.Log(AveLogLevel.INFO, "The option SkipRunningInstance is false,current workflow instance is in running state,the workflow instance will not be restored.");
                return true;
            }
            return false;
        }

        private void InstanceConflictResolution(SPWFInstanceUnit assoUnit, IAveListItem item, IAveWorkflow workflow)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.InstanceConflictResolution"))
            {
                log.Log(AveLogLevel.DEBUG, "The option for restoring 10 model workflow instance for item is {0}", this.mWFCRParameters.InstanceOption.ToString());
                switch (this.mWFCRParameters.InstanceOption)
                {
                    case WFInstanceConflictResolutionOption.Overwrite:
                        item.ParentList.ParentWeb.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
                        this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, item);
                        break;
                    case WFInstanceConflictResolutionOption.NotOverwrite:
                        break;
                    case WFInstanceConflictResolutionOption.OverwriteByModifiedTime:
                        if ((DateTime)assoUnit.InstanceItem.Properties["#Modified"] > workflow.Modified)
                        {
                            item.ParentList.ParentWeb.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
                            this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, item);
                            break;
                        }
                        else
                        {
                            return;
                        }
                    default:
                        break;
                }

            }

        }

        private AveStatus InstanceConflictResolution(SPWFInstanceUnit assoUnit, IAveWeb web, IAveWorkflow workflow)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.InstanceConflictResolution_web"))
            {
                AveStatus result = AveStatus.Successful;
                log.Log(AveLogLevel.DEBUG, "The option for restoring 10 model workflow instance for web is {0}", this.mWFCRParameters.InstanceOption.ToString());
                switch (this.mWFCRParameters.InstanceOption)
                {
                    case WFInstanceConflictResolutionOption.Overwrite:
                        web.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
                        this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, web);
                        result = AveStatus.Successful;
                        break;
                    case WFInstanceConflictResolutionOption.NotOverwrite:
                        result = AveStatus.Skipped;
                        break;
                    case WFInstanceConflictResolutionOption.OverwriteByModifiedTime:
                        if ((DateTime)assoUnit.InstanceItem.Properties["#Modified"] > workflow.Modified)
                        {
                            web.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
                            this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, web);
                            result = AveStatus.Successful;

                        }
                        else
                        {
                            //return;
                            result = AveStatus.Skipped;
                        }
                        break;
                    default:
                        result = AveStatus.Skipped;
                        break;
                }

                return result;
            }

        }

        private IAveWorkflow TryGetRunningInstance(IAveListItem item, SPWFInstanceUnit instanceUnit)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetRunningInstance"))
            {

                try
                {

                    IAveWorkflowCollection wfColl = item.ParentList.ParentWeb.Site.WorkflowManager.GetItemActiveWorkflows(item);
                    foreach (var wf in wfColl)
                    {
                        if (wf.AssociationId == instanceUnit.ParentAssociationUnit.Id)
                        {
                            return wf;
                        }
                    }
                    return null;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetWFRunningInstance, e);
                    return null;
                }

            }

        }

        private IAveWorkflow TryGetRunningInstance(IAveWeb web, SPWFInstanceUnit instanceUnit)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetRunningInstance_web"))
            {

                try
                {

                    IAveWorkflowCollection wfColl = web.Workflows;
                    foreach (var wf in wfColl)
                    {
                        if ((wf.AssociationId == instanceUnit.ParentAssociationUnit.Id) && ((int)wf.InternalState == 2))
                        {
                            return wf;
                        }
                    }
                    return null;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetWFRunningInstance, e);
                    return null;
                }

            }

        }

        private IAveWorkflow TryGetAllInstance(IAveListItem item, SPWFInstanceUnit instanceUnit)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetAllInstance"))
            {

                try
                {
                    List<IAveWorkflow> wfColl = item.ParentList.ParentWeb.Site.WorkflowManager.GetItemWorkflows(item, instanceUnit.ParentAssociationUnit.Id);
                    foreach (var wf in wfColl)
                    {
                        if ((DateTime)instanceUnit.InstanceItem.Properties["#Created"] == wf.Created)
                        {
                            return wf;
                        }
                    }
                    return null;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetWFRunningInstance, e);
                    return null;
                }

            }

        }

        private IAveWorkflow TryGetAllInstance(IAveWeb web, SPWFInstanceUnit instanceUnit)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetAllInstance_web"))
            {

                try
                {
                    IAveWorkflowCollection wfColl = web.Workflows;
                    foreach (var wf in wfColl)
                    {
                        if (((DateTime)instanceUnit.InstanceItem.Properties["#Created"] == wf.Created) && (wf.AssociationId == instanceUnit.ParentAssociationUnit.Id))
                        {
                            return wf;
                        }
                    }
                    return null;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetWFRunningInstance, e);
                    return null;
                }

            }

        }

        #endregion

        #region[******反插还原workflow definition******]

        /// <summary>
        /// 通过之前restore association的mapping，cache check当前instance的association是否还原过
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="parentAssociationId"></param>
        /// <param name="item"></param>
        /// <param name="instanceUnit"></param>
        /// <returns></returns>
        private bool HasParentAssociationRestored(WFConflictResolutionParameters parameter, Guid parentAssociationId, IAveListItem item, SPWFInstanceUnit instanceUnit)
        {
            bool hasParentAssociationRestored = false;
            //修改逻辑后UnitsOfRestored集合中应该只有一个元素,就是最后还原的Association
            if (parameter.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.ContainsKey(parentAssociationId))
            {
                hasParentAssociationRestored = true;
            }
            else
            {
                string origName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();

                string dicValue;
                if (parameter.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestoredNameMapping.TryGetValue(origName, out dicValue))
                {
                    SPWFAssociationUnit associationUnit = null;
                    IAveWorkflowAssociation association = GetParentAssociation(item, dicValue, parentAssociationId, out associationUnit);
                    if (association != null)
                    {
                        parameter.workflowRestoreCore.WFAssociationProcessor.SetRestoredUnit(associationUnit, association);
                        hasParentAssociationRestored = true;
                    }
                }
            }
            return hasParentAssociationRestored;
        }

        /// <summary>
        /// 反插还原workflow definition
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="parentAssoId"></param>
        /// <param name="item"></param>
        /// <param name="instanceUnit"></param>
        /// <returns></returns>
        private bool RestoreParentAssociation(WFConflictResolutionParameters parameter, Guid parentAssoId, IAveListItem item, SPWFInstanceUnit instanceUnit)
        {
            bool parentAssociationRestoredSuccessful = false;
            //之前没有还原过这个workflow association，那么需要拿到备份数据，重新还原
            if (parameter.mUnitsOfBackup.ContainsKey(parentAssoId))
            {
                AveWorkflowInfo cacheData = parameter.mUnitsOfBackup[parentAssoId];
                if (string.IsNullOrEmpty(cacheData.CTId))
                {
                    parameter.mAssociationParentObject = SetAssociationParentObject(item.ParentList, ref parameter.workflowRestoreCore, parameter.WebContentTypeAssociation);
                }
                else
                {
                    //ADO-196002 由于还原Workflow存在reload 逻辑，而item不会reload，导致通过item 获取的content type对象是老的，进而导致对象不一致，现改为获取list上的content type
                    parameter.mAssociationParentObject = SetAssociationParentObject(item.ParentList.ContentTypes[item.ContentType.ID], ref parameter.workflowRestoreCore, parameter.WebContentTypeAssociation);
                }

                WFAssociationConflictResolutionOption temp = parameter.AssociationOption;
                parameter.AssociationOption = parameter.ParentAssociationOption;
                try
                {
                    string origName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                    if (SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound || FindAssociation(origName, parameter.mAssociationParentObject.WorkflowAssociations) != null)
                    {
                        SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(cacheData.AssociationUnit);
                        ProcessAssociationDataPreCondition(assoUnit);
                        RestoreAssociationDataInternal(cacheData, assoUnit, true, new WFAveSPObjectCache(ParentSPWeb, null));
                        //有skip的情况,只有放到UnitsOfRestored中的才是成功还原的association
                        parentAssociationRestoredSuccessful = mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.ContainsKey(parentAssoId);
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    log.Log(AveLogLevel.DEBUG, "An processor error occurred while trying to find parent workflow association, error message: {0}", procException);
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
                    try
                    {
                        if (procException.ErrorCode == SPWFProcessorErrorCode.PutIntoPostAction)
                        {
                            SPWorkflowProcessorRuntime.OnCacheData(item.ParentList.ParentWeb.Site.Url, item.ParentList.ParentWeb.Site.ID.ToString(), item.ParentList.ParentWeb.ID.ToString(), item.ParentList.ID.ToString(), item.ParentList.ID.ToString(), item.ID, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.RestoreAssociationDataInternalError, e);
                }
                finally
                {
                    parameter.AssociationOption = temp;
                }
            }
            return parentAssociationRestoredSuccessful;
        }

        /// <summary>
        /// 还原instance时反找parent definition
        /// </summary>
        /// <param name="parentAssoId"></param>
        /// <param name="item"></param>
        /// <param name="instanceUnit"></param>
        /// <param name="needReport"></param>
        /// <returns></returns>
        private bool TryFindParentAssociation(Guid parentAssoId, IAveListItem item, SPWFInstanceUnit instanceUnit, ref bool needReport)
        {
            needReport = true;
            bool associationFound = false;
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryFindParentAssociation"))
            {

                try
                {
                    if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.NeedPostActionAssociations.Contains(parentAssoId))
                    {
                        //the instance that has been cached not need to be reported. We will add it to report while restoring it in post action.
                        try
                        {
                            SPWorkflowProcessorRuntime.OnCacheData(item.ParentList.ParentWeb.Site.Url, item.ParentList.ParentWeb.Site.ID.ToString(), item.ParentList.ParentWeb.ID.ToString(), item.ParentList.ID.ToString(), item.ParentList.ID.ToString(), item.ID, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));

                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                        }
                        needReport = false;
                    }
                    else
                    {
                        if (HasParentAssociationRestored(mWFCRParameters, parentAssoId, item, instanceUnit))
                        {
                            associationFound = true;
                        }
                        else
                        {
                            associationFound = RestoreParentAssociation(mWFCRParameters, parentAssoId, item, instanceUnit);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindParentAssociationFailed, e);
                }
                return associationFound;

            }

        }

        /// <summary>
        /// 过之前restore association的mapping，cache check当前instance的association是否还原过
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="parentAssociationId"></param>
        /// <param name="web"></param>
        /// <param name="instanceUnit"></param>
        /// <returns></returns>
        private bool HasParentAssociationRestored(WFConflictResolutionParameters parameter, Guid parentAssociationId, IAveWeb web, SPWFInstanceUnit instanceUnit)
        {
            bool hasParentAssociationRestored = false;
            //修改逻辑后UnitsOfRestored集合中应该只有一个元素,就是最后还原的Association
            if (parameter.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.ContainsKey(parentAssociationId))
            {
                hasParentAssociationRestored = true;
            }
            else
            {
                string origName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();

                string dicValue;
                if (parameter.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestoredNameMapping.TryGetValue(origName, out dicValue))
                {
                    SPWFAssociationUnit associationUnit;
                    IAveWorkflowAssociation association = GetParentAssociation(web, dicValue, parentAssociationId, out associationUnit);
                    if (association != null)
                    {
                        parameter.workflowRestoreCore.WFAssociationProcessor.SetRestoredUnit(associationUnit, association);
                        hasParentAssociationRestored = true;
                    }
                }
            }
            return hasParentAssociationRestored;
        }

        /// <summary>
        /// 反插还原workflow definition
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="parentAssoId"></param>
        /// <param name="web"></param>
        /// <param name="instanceUnit"></param>
        /// <returns></returns>
        private bool RestoreParentAssociation(WFConflictResolutionParameters parameter, Guid parentAssoId, IAveWeb web, SPWFInstanceUnit instanceUnit)
        {
            bool parentAssociationRestoredSuccessful = false;
            //之前没有还原过这个workflow association，那么需要拿到备份数据，重新还原
            if (parameter.mUnitsOfBackup.ContainsKey(parentAssoId))
            {
                AveWorkflowInfo cacheData = mWFCRParameters.mUnitsOfBackup[parentAssoId];
                if (string.IsNullOrEmpty(cacheData.CTId))
                {
                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(web, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                }

                bool isAssociationOptionChanged = false;
                WFAssociationConflictResolutionOption temp = mWFCRParameters.AssociationOption;
                //反插definition的option暴露给外围，让外围控制
                if (parameter.AssociationOption != parameter.ParentAssociationOption)
                {
                    isAssociationOptionChanged = true;
                    parameter.AssociationOption = parameter.ParentAssociationOption;
                }
                try
                {
                    string origName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                    if (SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound || FindAssociation(origName, parameter.mAssociationParentObject.WorkflowAssociations) != null)
                    {
                        RestoreAssociationDataInternal(cacheData, SPWFAssociationUnit.Load(cacheData.AssociationUnit), true, new WFAveSPObjectCache(ParentSPWeb, null));
                        parentAssociationRestoredSuccessful = true;
                    }
                    //else//RestoreParentAssociationIfNotFound is false,and cannot find parent association, set parentAssociationRestoredSuccessful to false
                    //{
                    //    parentAssociationRestoredSuccessful = false;
                    //}
                }
                catch (SPWFProcessorException procException)
                {
                    log.Log(AveLogLevel.DEBUG, "An processor error occurred while trying to find parent workflow association, error message: {0}", procException);
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
                    try
                    {
                        if (procException.ErrorCode == SPWFProcessorErrorCode.PutIntoPostAction)
                        {
                            SPWorkflowProcessorRuntime.OnCacheData(web.Site.Url, web.Site.ID.ToString(), web.ID.ToString(), string.Empty, string.Empty, int.MinValue, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance. {0}", ex);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.RestoreAssociationDataInternalError, e);
                }
                finally
                {
                    if (isAssociationOptionChanged)
                    {
                        mWFCRParameters.AssociationOption = temp;
                    }
                }
            }
            return parentAssociationRestoredSuccessful;
        }

        /// <summary>
        /// 还原instance时反找parent definition
        /// </summary>
        /// <param name="parentAssoId"></param>
        /// <param name="web"></param>
        /// <param name="instanceUnit"></param>
        /// <param name="needReport"></param>
        /// <returns></returns>
        private bool TryFindParentAssociation(Guid parentAssoId, IAveWeb web, SPWFInstanceUnit instanceUnit, ref bool needReport)
        {
            needReport = true;
            bool associationFound = false;
            try
            {
                if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.NeedPostActionAssociations.Contains(parentAssoId))
                {
                    //Cache instance data because association has already beeen cached.
                    try
                    {
                        SPWorkflowProcessorRuntime.OnCacheData(web.Site.Url, web.Site.ID.ToString(), web.ID.ToString(), string.Empty, string.Empty, int.MinValue, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                    }
                    needReport = false;
                }
                else
                {
                    if (HasParentAssociationRestored(mWFCRParameters, parentAssoId, web, instanceUnit))
                    {
                        associationFound = true;
                    }
                    else
                    {
                        associationFound = RestoreParentAssociation(mWFCRParameters, parentAssoId, web, instanceUnit);
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindParentAssociationFailed, e);
            }
            return associationFound;
        }

        private IAveWorkflowAssociation GetParentAssociation(IAveListItem item, string parentAssoName, Guid origAssoId, out SPWFAssociationUnit assoUnit)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.GetParentAssociation"))
            {

                assoUnit = null;
                AveWorkflowInfo wfInfo = null;
                if (mWFCRParameters.mUnitsOfBackup.ContainsKey(origAssoId))
                {
                    wfInfo = mWFCRParameters.mUnitsOfBackup[origAssoId];
                    assoUnit = SPWFAssociationUnit.Load(wfInfo.AssociationUnit);
                }

                IAveWorkflowAssociation asso = null;
                if (string.IsNullOrEmpty(wfInfo.CTId))
                {
                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ParentList, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                    assoUnit.ParentObjectType = SPWFAssociationParentType.List;
                    assoUnit.ParentObject = item.ParentList;

                }
                else
                {
                    var contentType = item.ParentList.ContentTypes[item.ContentType.ID];
                    assoUnit.ParentObjectType = SPWFAssociationParentType.ListContentType;
                    assoUnit.ParentObject = contentType;
                }

                asso = mWFCRParameters.mAssociationParentObject.WorkflowAssociations.GetAssociationByName(parentAssoName, System.Globalization.CultureInfo.CurrentUICulture);
                return asso;

            }

        }

        private IAveWorkflowAssociation GetParentAssociation(IAveWeb web, string parentAssoName, Guid origAssoId, out SPWFAssociationUnit assoUnit)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.GetParentAssociation_web"))
            {

                assoUnit = null;
                AveWorkflowInfo wfInfo = null;
                if (mWFCRParameters.mUnitsOfBackup.ContainsKey(origAssoId))
                {
                    wfInfo = mWFCRParameters.mUnitsOfBackup[origAssoId];
                    assoUnit = SPWFAssociationUnit.Load(wfInfo.AssociationUnit);
                }

                IAveWorkflowAssociation asso = null;
                if (string.IsNullOrEmpty(wfInfo.CTId))
                {
                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(web, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                    assoUnit.ParentObjectType = SPWFAssociationParentType.Web;
                    assoUnit.ParentObject = web;

                }
                else
                {
                    //AssociationParentObject = web.ContentTypes;
                    //assoUnit.ParentObjectType = SPWFAssociationParentType.WebContentType;
                    //assoUnit.ParentObject = web.ContentTypes;
                }

                asso = mWFCRParameters.mAssociationParentObject.WorkflowAssociations.GetAssociationByName(parentAssoName, System.Globalization.CultureInfo.CurrentUICulture);
                return asso;

            }

        }

        #endregion

        /// <summary>
        /// 目前就这个类用到了，先写到这里
        /// </summary>
        class WFDefinitionNotFoundException : AveWrapperBaseException
        {
            public WFDefinitionNotFoundException(string parentWorkflowDefinationName, string itemName, string listTitle, string webURL)
                : base(AveInternalResourceKey.Wrapper_Exception_Workflow_DefinitionNotFound)
            {
                this.Contexts.Add(ContextKeys.SharePoint.WorkflowDefinationName, parentWorkflowDefinationName);
                this.Contexts.Add(ContextKeys.SharePoint.ItemName, itemName);
                this.Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
                this.Contexts.Add(ContextKeys.SharePoint.SiteURL, webURL);
            }
        }

        public override bool UpdateWorkflowStartOptions(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation desWorkflowAssociation)
        {
            bool needUpdate = false;
            if (desWorkflowAssociation != null)
            {
                bool autoStartChange = (((Configuration)assoUnit.SerializableData.mConfiguration & Configuration.AutoStartChange) != Configuration.None);
                bool autoStartCreate = (((Configuration)assoUnit.SerializableData.mConfiguration & Configuration.AutoStartAdd) != Configuration.None);
                if (desWorkflowAssociation.AutoStartChange != autoStartChange)
                {
                    desWorkflowAssociation.AutoStartChange = autoStartChange;
                    needUpdate = true;
                }
                if (desWorkflowAssociation.AutoStartCreate != autoStartCreate)
                {
                    desWorkflowAssociation.AutoStartCreate = autoStartCreate;
                    needUpdate = true;
                }
            }
            return needUpdate;
        }
    }

    /// <summary>
    /// Create Instance by WFConflictResolutionInternal
    /// </summary>
    internal sealed class WFConflictResolutionInternalFor13Model : WFConflictResolutionInternal
    {
        public IAveWorkflowSubscriptionService WFSubscriptionService
        {
            get
            {
                return Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).WFSubscriptionService;
            }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                IAveWeb web = null;
                switch (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.ParentObjectType)
                {
                    case SPWFAssociationParentType.List:
                        web = ((IAveList)this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.ParentObject).ParentWeb;
                        break;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        web = ((IAveContentType)this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.ParentObject).ParentWeb;
                        break;
                    case SPWFAssociationParentType.Web:
                        web = (IAveWeb)this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.ParentObject;
                        break;
                    default:
                        break;
                }
                return web;
            }
        }

        public WFConflictResolutionInternalFor13Model(WFConflictResolutionParameters parameters)
            : base(parameters)
        {

        }

        internal override void RestoreTemplateDataInternal(SPWFAssociationUnit assoUnit)
        {
            SPWFAssociationProc proc = SPWFAssociationProc.CreateInstance(SPWFProcessorType.API13Model);
            WorkflowServiceFactory workflowSvcFacory = InitWorkflowServiceFactory(assoUnit.ParentWeb);
            if (!workflowSvcFacory.IsCurrentWorkflowServiceConnected())
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.WorkflowServiceNotAvailableException, AveInternalResourceKey.Wrapper_Exception_Workflow_ServiceNotAvailable, assoUnit.ParentWeb.Url);
            }
            IAveWorkflowDefinition definition;
            switch (this.mWFCRParameters.TemplateOption)
            {
                case WFTemplateConflictResolutionOption.NotOverwrite:
                    {
                        definition = FindWorkflowDefinition(assoUnit, workflowSvcFacory.WFDeploymentService);
                        if (definition == null)
                        {
                            proc.ParentObject = assoUnit.ParentWeb;
                            proc.ParentObjectType = SPWFAssociationParentType.Web;
                            proc.RestoreReusableWFTemplate(assoUnit);
                        }
                        else
                        {
                            throw new AveWrapperSkipException("Skip restoring the 2013 mode workflow template as it conflict with destination.");
                        }
                        break;
                    }
                case WFTemplateConflictResolutionOption.Overwrite:
                    {
                        proc.ParentObject = assoUnit.ParentWeb;
                        proc.ParentObjectType = SPWFAssociationParentType.Web;
                        proc.RestoreReusableWFTemplate(assoUnit);
                        break;
                    }
            }
        }

        #region [******Handle WorkflowAssociation******]

        public override void HandleAssociationConflictInternal(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {
            log.Log(AveLogLevel.DEBUG, "Begin to restore workflow association for 13 model internally, workflow association name: {0}.", assoUnit.SerializableData.mOriginalName);
            WorkflowServiceFactory workflowSvcFacory = InitWorkflowServiceFactory(parent.ParentWeb);
            if (!workflowSvcFacory.IsCurrentWorkflowServiceConnected())
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.WorkflowServiceNotAvailableException, AveInternalResourceKey.Wrapper_Exception_Workflow_ServiceNotAvailable, parent.ParentWeb.Url);
            }
            assoUnit.IsPostAction = isPostAction;
            List<IAveWorkflowSubscription> workflowSubscriptions = FindWorkflowSubscription(assoUnit.SerializableData.mOriginalName, parent.WorkflowSubscriptionCollection);
            if (workflowSubscriptions == null || workflowSubscriptions.Count == 0)
            {
                mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                return;
            }
            AssociationConflictResolution(assoUnit, parent, workflowSubscriptions[0], spObjectCache, isPostAction);
        }

        internal override void InitAssociationUnitWorkflowType(SPWFAssociationUnit associationUnit)
        {
            associationUnit.WorkflowType = WorkflowType.SP2013PlatformWorkflow;
        }

        private IAveWorkflowDefinition FindWorkflowDefinition(SPWFAssociationUnit assoUnit, IAveWorkflowDeploymentService deploymentService)
        {
            if (deploymentService == null)
            {
                throw new ArgumentNullException("deploymentService");
            }
            IAveWorkflowDefinition wfDefinition = deploymentService.GetDefinition(assoUnit.SerializableData.mBaseId);
            if (wfDefinition == null)
            {
                string restrictToType = string.Empty;
                if (assoUnit.SerializableData.Properties.ContainsKey(SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION))
                {
                    Dictionary<string, object> workflowDefinitionProps = (Dictionary<string, object>)assoUnit.SerializableData.Properties[SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION];
                    if (workflowDefinitionProps.ContainsKey("RestrictToType"))
                    {
                        restrictToType = workflowDefinitionProps["RestrictToType"].ToString();
                    }
                }
                foreach (IAveWorkflowDefinition definition in deploymentService.EnumerateDefinitions(true))
                {
                    if (definition.DisplayName.Equals(assoUnit.SerializableData.mName) &&
                        restrictToType.Equals(definition.RestrictToType))
                    {
                        wfDefinition = definition;
                        break;
                    }
                }
            }
            return wfDefinition;
        }

        private List<IAveWorkflowSubscription> FindWorkflowSubscription(string assoName, IAveWorkflowSubscriptionCollection subscriptionCollection)
        {
            return subscriptionCollection.Where(subscrip => subscrip.Name.Equals(assoName)).ToList<IAveWorkflowSubscription>();
        }

        private WorkflowServiceFactory InitWorkflowServiceFactory(IAveWeb web)
        {
            WorkflowServiceFactory workflowServiceFactory = Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { web });
            workflowServiceFactory.UpdateWorkflowServiceManager(web);
            return workflowServiceFactory;
        }

        private void AssociationConflictResolution(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, IAveWorkflowSubscription subscription, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.AssociationConflictResolution"))
            {
                log.Log(AveLogLevel.DEBUG, "Begin to handle workflow association conflict for 13 model, association option is {0}", this.mWFCRParameters.AssociationOption.ToString());
                switch (this.mWFCRParameters.AssociationOption)
                {
                    case WFAssociationConflictResolutionOption.Append:
                        //Rename workflow 
                        string oldName = assoUnit.SerializableData.mName;
                        string newName = RenameAssociation(oldName, parent);
                        assoUnit.SerializableData.mName = newName;
                        assoUnit.SerializableData.mOriginalName = newName;

                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        break;
                    case WFAssociationConflictResolutionOption.Overwrite:
                        var count = Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { parent.ParentWeb }).WFInstanceService.CountInstances(subscription);
                        if (count == 0)
                        {
                            //删除association，会删除所有应用此association的subscription；不删除，则会出现两个Workflow Template，需要解决。
                            Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { parent.ParentWeb }).WFSubscriptionService.DeleteSubscription(subscription.Id);
                            //Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { parent.ParentWeb }).WFDeploymentService.DeleteDefinition(subscription.DefinitionId);
                            this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        }
                        else
                        {
                            log.Warn("Skip to restore the workflow {0} with conflict option {1} as destination conflict workflow association has instances,destination instance count:{2}", subscription.Name, mWFCRParameters.AssociationOption, count);
                            throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, AveInternalResourceKey.Wrapper_Exception_Workflow_AssociationConflictWithInstanceExist, new object[] { subscription.Name, mWFCRParameters.AssociationOption, count });
                        }
                        break;
                    case WFAssociationConflictResolutionOption.ForceOverwrite:
                        Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { parent.ParentWeb }).WFSubscriptionService.DeleteSubscription(subscription.Id);
                        Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { parent.ParentWeb }).WFDeploymentService.DeleteDefinition(subscription.DefinitionId);
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        break;
                    case WFAssociationConflictResolutionOption.UpdateOverwrite:
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        break;
                    case WFAssociationConflictResolutionOption.ForceUse:
                        this.mWFCRParameters.workflowRestoreCore.ForceUpdate = false;
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
                        break;
                    case WFAssociationConflictResolutionOption.NotOverwrite:
                        //not overwrite时，找到就skip,在对应的option中处理即可
                        log.Log(AveLogLevel.DEBUG, "RestoreWorkflowDefinitionSkip.");
                        throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, AveInternalResourceKey.Wrapper_Exception_Restore_WFDeConflictError);
                    default:
                        throw new AveWrapperException(AveWrapperErrorCode.DefinitionConflictResolutionOptionInvalid, "Association conflict resolution invalid.");
                }

            }

        }

        private string RenameAssociation(string oldName, WorkflowAssociationParentObject parent)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.RenameAssociation"))
            {

                string newName = oldName;
                int counter = 1;
                while (true)
                {
                    newName = oldName + AppendSuffix + counter;

                    List<IAveWorkflowSubscription> subscriptions = FindWorkflowSubscription(newName, parent.WorkflowSubscriptionCollection);
                    if (subscriptions == null || subscriptions.Count == 0)
                    {
                        break;
                    }

                    if (counter > MaxRenameTimes)
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationRenameError);
                    }
                    counter++;
                }
                log.Debug(string.Format("Rename association\nOld name:{0}\nNew name{1}", oldName, newName));
                return newName;

            }

        }

        public override bool UpdateWorkflowStartOptions(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation desWorkflowAssociation = null)
        {
            //记录event receiver状态
            bool eventFiringDisabled = LS.SPWorkflowProcessor.SPEventManagerWrapper.EventFiringDisabled;
            try
            {
                SPEventManagerWrapper.EnableEventFiring();

                if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored.ContainsKey(assoUnit.SerializableData.mId))
                {

                    SPWFAssociationUnit desAssoUnit = this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored[assoUnit.SerializableData.mId];
                    IAveWorkflowSubscription definitionSubscription = desAssoUnit.WorkflowSubscription;

                    var tempweb = ParentWeb;
                    tempweb.ReloadWeb();
                    Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { tempweb }).UpdateWorkflowServiceManager(tempweb);
                    var newdefinitionSubscription = this.WFSubscriptionService.GetSubscription(desAssoUnit.WorkflowSubscription.Id);
                    var newdd = this.WFSubscriptionService.PublishSubscription(newdefinitionSubscription);

                    Dictionary<string, object> propertyDefinitions = null;
                    if (assoUnit.SerializableData.Properties.Contains("Props.13Model") && ((Dictionary<string, object>)assoUnit.SerializableData.Properties["Props.13Model"]).ContainsKey("SharePointWorkflowContext.Subscription.EventType"))
                    {
                        propertyDefinitions = assoUnit.SerializableData.Properties["Props.13Model"] as Dictionary<string, object>;
                        string[] eventTypes = ((Dictionary<string, object>)assoUnit.SerializableData.Properties["Props.13Model"])["SharePointWorkflowContext.Subscription.EventType"].ToString().Split(new string[] { "#;" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string eventType in eventTypes)
                        {
                            if ((string.Equals(eventType, "ItemAdded") || string.Equals(eventType, "ItemUpdated")) && !definitionSubscription.EventTypes.Contains(eventType))
                            {
                                definitionSubscription.EventTypes.Add(eventType);
                            }
                        }
                    }
                    Guid workflowSubscriptionId = Guid.Empty;
                    if (desAssoUnit.ParentObjectType == SPWFAssociationParentType.List)
                    {
                        workflowSubscriptionId = this.WFSubscriptionService.PublishSubscriptionForList(definitionSubscription, desAssoUnit.ParentList.ID);
                    }
                    else if (desAssoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                    {
                        workflowSubscriptionId = this.WFSubscriptionService.PublishSubscriptionForList(definitionSubscription, desAssoUnit.ParentContentType.ParentList.ID);
                    }
                    else if (desAssoUnit.ParentObjectType == SPWFAssociationParentType.Web)
                    {
                        workflowSubscriptionId = this.WFSubscriptionService.PublishSubscription(definitionSubscription);
                    }
                }
                return false;
            }
            finally
            {
                if (eventFiringDisabled)
                {
                    SPEventManagerWrapper.DisableEventFiring();
                }
                else
                {
                    SPEventManagerWrapper.EnableEventFiring();
                }
            }
        }
        #endregion

        #region [******Handle WorkflowInstance******]
        public override void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveWeb web)
        {
        }

        public override void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveListItem item)
        {
            //disbale backup&restore sp2013 mode workflow instance
            //using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.HandleInstanceConflict"))
            //{

            //    if (!SPWorkflowProcessorRuntime.ProcessInstance)
            //    {
            //        return;
            //    }

            //    if (instanceUnit == null || item == null)
            //    {
            //        throw new AveException("Invalid argument, argument can not be null.");
            //    }
            //    string parentAssociationName = null;
            //    try
            //    {
            //        Guid parentAssoId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("#TemplateId");
            //        if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(parentAssoId))
            //        {
            //            if (SPWorkflowProcessorRuntime.RestoreBuiltinOnly && !SPWFAssociationUnit.Load(this.mWFCRParameters.mUnitsOfBackup[parentAssoId].AssociationUnit).IsBuiltinBaseId)
            //            {
            //                return;
            //            }
            //        }
            //        parentAssociationName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
            //        //Guid parentAssoBaseId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationBaseId");
            //        if (!TryFindParentAssociation(parentAssoId, item, instanceUnit))
            //        {
            //            throw new WorkflowDefinitionNotFoundException(parentAssociationName, item.Name, item.ParentList.Title, item.ParentList.ParentWeb.Url);
            //        }
            //        else
            //        {
            //            instanceUnit.ParentAssociationUnit = this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored[parentAssoId];
            //        }
            //        #region deal with InstanceConflict
            //        log.Info("WorkflowInstance Conflict", " Item Title is " + item.Title + " Item Name is " + item.Name + " InstanceState is " + instanceUnit.InstanceItem.Properties["Status"].ToString());
            //        if ((AveWorkflowStatus13Model)instanceUnit.InstanceItem.Properties["Status"] == AveWorkflowStatus13Model.Started)
            //        {
            //            var runningWorkflow = TryGetRunningInstance(item, instanceUnit);
            //            if (runningWorkflow == null)
            //            {
            //                var createTimeWorkflow = TryGetAllInstance(item, instanceUnit);
            //                if (createTimeWorkflow == null)
            //                {
            //                    this.mWFCRParameters.workflowRestoreCore.RestoreInstance(instanceUnit, item);
            //                }
            //                else
            //                {
            //                    InstanceConflictResolution(instanceUnit, item, createTimeWorkflow);
            //                }
            //            }
            //            else
            //            {
            //                InstanceConflictResolution(instanceUnit, item, runningWorkflow);
            //            }

            //        }
            //        else
            //        {
            //            var workflow = TryGetAllInstance(item, instanceUnit);
            //            if (workflow == null)
            //            {
            //                this.mWFCRParameters.workflowRestoreCore.RestoreInstance(instanceUnit, item);
            //            }
            //            else
            //            {
            //                InstanceConflictResolution(instanceUnit, item, workflow);
            //            }
            //        }
            //        #endregion
            //        //var workflow = TryGetRunningInstance(item, instanceUnit);
            //        //if (workflow == null)
            //        //{
            //        //    workflowRestoreCore.RestoreInstance(instanceUnit, item);
            //        //}
            //        //else
            //        //{
            //        //    InstanceConflictResolution(instanceUnit, item, workflow);
            //        //}

            //        if (!instanceUnit.ParentAssociationUnit.isCreateField)
            //        {
            //            AddFieldMapping(instanceUnit, item.ParentList.ID);
            //            instanceUnit.ParentAssociationUnit.isCreateField = true;
            //        }
            //        this.mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(parentAssociationName, item.Title, AveReportObjectType.WorkflowInstance, AveStatus.Successful, string.Empty));

            //    }
            //    catch (Exception ex)
            //    {
            //        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreWorkflowInstanceFailedEventMessage(parentAssociationName, ex));
            //        this.mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(parentAssociationName, item.Title, AveReportObjectType.WorkflowInstance, AveStatus.Skipped, ex.Message));
            //    }

            //}

        }

        #region   13 mode instance不支持还原，先注释掉
        //private bool TryFindParentAssociation(Guid workflowSubscriptionId, IAveListItem item, SPWFInstanceUnit instanceUnit)
        //{

        //    using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryFindParentAssociation"))
        //    {

        //        try
        //        {
        //            //如果发现parentAssociation已经Cache则Instance直接Cache.
        //            if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.NeedPostActionAssociations.Contains(workflowSubscriptionId))
        //            {
        //                try
        //                {
        //                    SPWorkflowProcessorRuntime.OnCacheData(item.ParentList.ParentWeb.Site.ID.ToString(), item.ParentList.ParentWeb.ID.ToString(), item.ParentList.ID.ToString(), item.ParentList.ID.ToString(), item.ID, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
        //                }
        //                catch (Exception ex)
        //                {
        //                    log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
        //                }
        //                return false;
        //            }
        //            //修改逻辑后UnitsOfRestored集合中应该只有一个元素,就是最后还原的Association
        //            if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored.ContainsKey(workflowSubscriptionId))
        //            {
        //                return true;
        //            }

        //            //之前已经还原过这个workflow association
        //            string origName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
        //            if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestoredNameMapping.ContainsKey(origName.ToLowerInvariant()))
        //            {
        //                SPWFAssociationUnit assoUnit = null;
        //                IAveWorkflowSubscription workflowSubscription = GetParentWorkflowSubscription(item, this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestoredNameMapping[origName.ToLowerInvariant()], workflowSubscriptionId, out assoUnit);
        //                if (workflowSubscription != null)
        //                {
        //                    this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.SetRestoredUnit(assoUnit, workflowSubscription);
        //                    return true;
        //                }
        //            }


        //            //之前没有还原过这个workflow association，那么需要拿到备份数据，重新还原
        //            if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(workflowSubscriptionId))
        //            {
        //                AveWorkflowInfo cacheData = this.mWFCRParameters.mUnitsOfBackup[workflowSubscriptionId];
        //                if (string.IsNullOrEmpty(cacheData.CTId))
        //                {
        //                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ParentList, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
        //                }
        //                else
        //                {
        //                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ContentType, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
        //                }

        //                WFAssociationConflictResolutionOption temp = mWFCRParameters.AssociationOption;
        //                mWFCRParameters.AssociationOption = WFAssociationConflictResolutionOption.ForceUse;
        //                try
        //                {
        //                    List<IAveWorkflowSubscription> workflowSubscriptions = FindWorkflowSubscription(origName, mWFCRParameters.mAssociationParentObject.WorkflowSubscriptionCollection);
        //                    if ((workflowSubscriptions == null || workflowSubscriptions.Count == 0) && !SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound)
        //                        return false;
        //                    RestoreAssociationDataInternal(cacheData, SPWFAssociationUnit.Load(cacheData.AssociationUnit), true);
        //                }
        //                catch (SPWFProcessorException procException)
        //                {
        //                    log.Log(AveLogLevel.DEBUG, "An processor error occurred while trying to find parent association for 13 model, error message: {0}", procException);
        //                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
        //                    try
        //                    {
        //                        if (procException.ErrorCode == 9999)
        //                        {
        //                            SPWorkflowProcessorRuntime.OnCacheData(item.ParentList.ParentWeb.Site.ID.ToString(), item.ParentList.ParentWeb.ID.ToString(), item.ParentList.ID.ToString(), item.ParentList.ID.ToString(), item.ID, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
        //                    }
        //                    return false;
        //                }
        //                catch (Exception e)
        //                {
        //                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.RestoreAssociationDataInternalError, e);
        //                }
        //                finally
        //                {
        //                    mWFCRParameters.AssociationOption = temp;
        //                }
        //                return true;
        //            }
        //            return false;
        //        }
        //        catch (Exception e)
        //        {
        //            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindParentAssociationFailed, e);
        //            return false;
        //        }

        //    }

        //}

        //private IAveWorkflowSubscription GetParentWorkflowSubscription(IAveListItem item, string parentAssoName, Guid workflowSubscriptionId, out SPWFAssociationUnit assoUnit)
        //{

        //    using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.GetParentAssociation"))
        //    {

        //        assoUnit = null;
        //        AveWorkflowInfo wfInfo = null;
        //        if (mWFCRParameters.mUnitsOfBackup.ContainsKey(workflowSubscriptionId))
        //        {
        //            wfInfo = mWFCRParameters.mUnitsOfBackup[workflowSubscriptionId];
        //            assoUnit = SPWFAssociationUnit.Load(wfInfo.AssociationUnit);
        //        }

        //        IAveWorkflowSubscription asso = null;
        //        if (string.IsNullOrEmpty(wfInfo.CTId))
        //        {
        //            mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ParentList, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
        //            assoUnit.ParentObjectType = SPWFAssociationParentType.List;
        //            assoUnit.ParentObject = item.ParentList;

        //        }
        //        else
        //        {
        //            mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ContentType, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
        //            assoUnit.ParentObjectType = SPWFAssociationParentType.ListContentType;
        //            assoUnit.ParentObject = item.ContentType;
        //        }

        //        asso = mWFCRParameters.mAssociationParentObject.WorkflowSubscriptionCollection.GetSubscriptionByName(parentAssoName);
        //        return asso;

        //    }

        //}

        //private IAveWorkflowInstance TryGetRunningInstance(IAveListItem item, SPWFInstanceUnit instanceUnit)
        //{

        //    using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetRunningInstance"))
        //    {

        //        try
        //        {

        //            IAveWorkflowInstanceCollection wfColl = Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.ParentList.ParentWeb }).WFInstanceService.EnumerateInstancesForListItem(item.ParentList.ID, item.ID);
        //            foreach (var wf in wfColl)
        //            {
        //                if (wf.WorkflowSubscriptionId == instanceUnit.ParentAssociationUnit.Id && wf.Status == AveWorkflowStatus13Model.Started)
        //                {
        //                    return wf;
        //                }
        //            }
        //            return null;
        //        }
        //        catch (Exception e)
        //        {
        //            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetWFRunningInstance, e);
        //            return null;
        //        }

        //    }

        //}

        //private IAveWorkflowInstance TryGetAllInstance(IAveListItem item, SPWFInstanceUnit instanceUnit)
        //{

        //    using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetAllInstance"))
        //    {

        //        try
        //        {
        //            IAveWorkflowInstanceCollection wfColl = Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.ParentList.ParentWeb }).WFInstanceService.EnumerateInstancesForListItem(item.ParentList.ID, item.ID);
        //            foreach (var wf in wfColl)
        //            {
        //                if ((DateTime)instanceUnit.InstanceItem.Properties["InstanceCreated"] == wf.InstanceCreated && wf.WorkflowSubscriptionId == instanceUnit.ParentAssociationUnit.Id)
        //                {
        //                    return wf;
        //                }
        //            }
        //            return null;
        //        }
        //        catch (Exception e)
        //        {
        //            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetWFRunningInstance, e);
        //            return null;
        //        }

        //    }

        //}

        //private void InstanceConflictResolution(SPWFInstanceUnit assoUnit, IAveListItem item, IAveWorkflowInstance workflow)
        //{

        //    using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.InstanceConflictResolution"))
        //    {

        //        switch (this.mWFCRParameters.InstanceOption)
        //        {
        //            case WFInstanceConflictResolutionOption.Overwrite:
        //                //can not remove
        //                //item.ParentList.ParentWeb.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow); 

        //                this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, item);
        //                break;
        //            case WFInstanceConflictResolutionOption.NotOverwrite:
        //                break;
        //            case WFInstanceConflictResolutionOption.OverwriteByModifiedTime:
        //                // List<IAveWorkflow> wfs = item.ParentList.ParentWeb.Site.WorkflowManager.GetItemWorkflows(item, assoUnit.ParentAssociationUnit.Id);
        //                // foreach (IAveWorkflow wf in wfs)
        //                //{
        //                //DateTime UtcCreatedTime = wf.Created.ToUniversalTime();
        //                //DateTime UtcModifiedtime = wf.Modified.ToUniversalTime();
        //                // if ((DateTime)assoUnit.InstanceItem.Properties["#Created"] == wf.Created)
        //                //{
        //                if ((DateTime)assoUnit.InstanceItem.Properties["LastUpdated"] > workflow.LastUpdated)
        //                {
        //                    //item.ParentList.ParentWeb.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
        //                    this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, item);
        //                    break;
        //                }
        //                else
        //                {
        //                    return;
        //                }
        //            default:
        //                break;
        //        }

        //    }

        //}
        #endregion

        #endregion
    }

    internal sealed class WFConflictResolutionInternalForExportedNintex : WFConflictResolutionInternal
    {
        public WFConflictResolutionInternalForExportedNintex(WFConflictResolutionParameters parameters)
            : base(parameters)
        {
        }

        public override void HandleAssociationConflictInternal(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {
            using (new AvePerformanceScope("Restore.WFConflictResolution.HandleAssociationConflictInternal"))
            {
                log.Log(AveLogLevel.DEBUG, "Begin to restore workflow association for exported nintex workflow, workflow association name: {0}", assoUnit.SerializableData.mName);
                //暂不做冲突处理
                assoUnit.IsPostAction = isPostAction;
                mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject, spObjectCache, isPostAction);
            }
        }

        protected override bool IsNeedRestoreWorkflow(SPWFAssociationUnit assoUnit)
        {
            //只需要往online上restore
            bool isOnlineSite = assoUnit.ParentWeb.Site.IsOnlineSite;
            if (!isOnlineSite)
            {
                log.Info("Skip migrating Nintex Workflow {0} to destination, as destination site {1} is not Online site.", assoUnit.SerializableData.mName, assoUnit.ParentWeb.Url);
            }
            return isOnlineSite;
        }

        internal override void InitAssociationUnitWorkflowType(SPWFAssociationUnit associationUnit)
        {
            associationUnit.WorkflowType = WorkflowType.NintexWorkflowOnline;
        }
    }
    internal class WorkflowInstanceParentObject
    {
        public WorkflowInstanceParentObject(IAveWorkflowAssociation asso, object parent, SPWFAssociationParentType parentType)
        {
            Association = asso;
            ParentObj = parent;
            ParentType = parentType;
        }
        public IAveWorkflowAssociation Association { get; private set; }
        public object ParentObj { get; private set; }
        public SPWFAssociationParentType ParentType { get; private set; }
    }

    internal class WorkflowAssociationParentObject
    {
        private IAveList mList;
        private IAveContentType mContentType;
        private IAveWeb mWeb;
        private SPWFAssociationParentType mParentType;
        private IAveWorkflowManager mWorkflowManager;
        private IAveWorkflowAssociationCollection mWorkflowAssociations;

        public WorkflowAssociationParentObject(IAveList list)
        {
            mList = list;
            mParentType = SPWFAssociationParentType.List;
        }
        public WorkflowAssociationParentObject(IAveContentType ct, SPWFAssociationParentType parentType)
        {
            mContentType = ct;
            mParentType = parentType;
        }
        public WorkflowAssociationParentObject(IAveWeb web)
        {
            mWeb = web;
            mParentType = SPWFAssociationParentType.Web;
        }

        public void ReloadParentWeb()
        {
            switch (this.mParentType)
            {

                case SPWFAssociationParentType.List:
                    mList.ParentWeb.ReloadWeb();
                    mList.Reload();
                    break;
                case SPWFAssociationParentType.Web:
                    mWeb.ReloadWeb();
                    break;
                case SPWFAssociationParentType.ListContentType:
                    mContentType.ParentList.ParentWeb.ReloadWeb();
                    mContentType.ParentList.Reload();
                    mContentType = mContentType.ParentList.ContentTypes[mContentType.ID];
                    break;
                case SPWFAssociationParentType.WebContentType:
                    mContentType.ParentWeb.ReloadWeb();
                    mContentType = mContentType.ParentWeb.ContentTypes[mContentType.ID];
                    break;
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
            }
        }

        internal IAveContentType ParentContentType
        {
            get
            {
                if (mParentType == SPWFAssociationParentType.ListContentType ||
                    mParentType == SPWFAssociationParentType.WebContentType)
                {
                    return mContentType;
                }
                return null;
            }
        }

        internal IAveWeb ParentWeb
        {
            get
            {
                switch (mParentType)
                {
                    case SPWFAssociationParentType.List:
                        return mList.ParentWeb;
                    case SPWFAssociationParentType.Web:
                        return mWeb;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        return mContentType.ParentWeb;
                    default:
                        return null;
                }
            }
        }

        public object ParentObject
        {
            get
            {
                switch (mParentType)
                {
                    case SPWFAssociationParentType.List:
                        return mList;
                    case SPWFAssociationParentType.Web:
                        return mWeb;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        return mContentType;
                    default:
                        return null;
                }
            }
        }

        public SPWFAssociationParentType ParentType
        {
            get { return mParentType; }
        }

        public IAveWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                if (mWorkflowAssociations == null)
                {
                    switch (mParentType)
                    {
                        case SPWFAssociationParentType.List:
                            //mWorkflowAssociations = mList.WorkflowAssociations;
                            mWorkflowAssociations = mList.ParentWeb.Lists[mList.ID].WorkflowAssociations;
                            break;
                        case SPWFAssociationParentType.Web:
                            mWorkflowAssociations = mWeb.WorkflowAssociations;
                            break;
                        case SPWFAssociationParentType.ListContentType:
                            mWorkflowAssociations = mContentType.ParentList.ParentWeb.Lists[mContentType.ParentList.ID].ContentTypes[mContentType.ID].WorkflowAssociations;
                            break;
                        case SPWFAssociationParentType.WebContentType:
                            mWorkflowAssociations = mContentType.WorkflowAssociations;
                            break;
                        default:
                            return null;
                    }
                }
                return mWorkflowAssociations;
            }
        }

        private IAveWorkflowSubscriptionCollection mWorkflowSubscriptionCollection = null;
        public IAveWorkflowSubscriptionCollection WorkflowSubscriptionCollection
        {
            get
            {
                if (mWorkflowSubscriptionCollection == null)
                {
                    WorkflowServiceFactory factory = Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb });
                    factory.UpdateWorkflowServiceManager(ParentWeb);
                    switch (mParentType)
                    {
                        case SPWFAssociationParentType.List:
                            mWorkflowSubscriptionCollection = factory.WFSubscriptionService.EnumerateSubscriptionsByList(mList.ID);
                            break;
                        case SPWFAssociationParentType.Web:
                            mWorkflowSubscriptionCollection = factory.WFSubscriptionService.EnumerateSubscriptionsByEventSource(mWeb.ID);
                            break;
                        case SPWFAssociationParentType.ListContentType:
                            mWorkflowSubscriptionCollection = factory.WFSubscriptionService.EnumerateSubscriptionsByList(mContentType.ParentList.ID);
                            break;
                        case SPWFAssociationParentType.WebContentType:
                            break;
                        default:
                            break;
                    }
                }
                return mWorkflowSubscriptionCollection;
            }
        }

        public Guid ID
        {
            get
            {
                switch (mParentType)
                {
                    case SPWFAssociationParentType.List:
                        return mList.ID;
                    case SPWFAssociationParentType.Web:
                        return mWeb.ID;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        return mList.ID;
                    default:
                        return Guid.Empty;
                }
            }
        }

        public string Name
        {
            get
            {
                switch (mParentType)
                {
                    case SPWFAssociationParentType.List:
                        return mList.Title;
                    case SPWFAssociationParentType.Web:
                        return mWeb.Name;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        return mContentType.Name;
                    default:
                        return string.Empty;
                }
            }
        }

        public IAveWorkflowManager WorkflowManager
        {
            get
            {
                if (mWorkflowManager == null)
                {
                    switch (mParentType)
                    {
                        case SPWFAssociationParentType.List:
                            mWorkflowManager = mList.ParentWeb.Site.WorkflowManager;
                            break;
                        case SPWFAssociationParentType.Web:
                            mWorkflowManager = mWeb.Site.WorkflowManager;
                            break;
                        case SPWFAssociationParentType.ListContentType:
                        case SPWFAssociationParentType.WebContentType:
                            mWorkflowManager = mContentType.ParentWeb.Site.WorkflowManager;
                            break;
                        default:
                            break;
                    }
                }
                return mWorkflowManager;
            }
        }
    }

    internal class WFConflictResolutionParameters
    {
        public AveWorkflowRestoreCore workflowRestoreCore = null;
        public Dictionary<Guid, AveWorkflowInfo> mUnitsOfBackup = null;
        public IReport reportor = null;
        public WFTemplateConflictResolutionOption TemplateOption;
        public WFAssociationConflictResolutionOption AssociationOption;
        public WFAssociationConflictResolutionOption ParentAssociationOption;
        public WFInstanceConflictResolutionOption InstanceOption;
        public WorkflowAssociationParentObject mAssociationParentObject = null;
        public bool WebContentTypeAssociation;
        public bool BackupNintexWorklfowToExportedFile;
    }




}
