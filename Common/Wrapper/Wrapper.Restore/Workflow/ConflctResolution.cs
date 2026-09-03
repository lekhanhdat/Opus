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
using System.Linq;
using System.Text;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using LS.SPWorkflowProcessor;
using AvePoint.Common;
using System.IO;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using System.Globalization;
using AvePoint.Wrapper.Common.AveWorkflowAssociationCollection;
using AvePoint.Wrapper.Restore.NintexForm;
using LS.SPWorkflowProcessor.SerializableObjects;

namespace AvePoint.Wrapper.Restore
{
    public class WFConflictResolution : IReportable, IDisposable
    {
        #region [******Members******]

        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        [ThreadStatic]
        private static volatile WFConflictResolution instance;

        private const string mConfigFile = @"AgentCommonSPWorkflowConfiguration.xml";
        private readonly static object syncRoot = new Object();
        private AveWorkflowRestoreCore workflowRestoreCore = new AveWorkflowRestoreCore();
        private Dictionary<Guid, AveWorkflowInfo> mUnitsOfBackup = new Dictionary<Guid, AveWorkflowInfo>();
        private IReport reportor = new AveWrapperReport();

        public WFTemplateConflictResolutionOption TemplateOption { get; set; }
        public WFAssociationConflictResolutionOption AssociationOption { get; set; }
        public WFInstanceConflictResolutionOption InstanceOption { get; set; }
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

        private WorkflowAssociationParentObject mAssociationParentObject = null;
        /// <summary>
        /// Must be a IAveList or IAveContentType
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

                        //默认情况还原running instance
                        SPWorkflowProcessorRuntime.RestoreHistoryOnly = true;
                        //默认情况下只还原Builtin Workflow
                        SPWorkflowProcessorRuntime.RestoreBuiltinOnly = false;
                        //User Mapping Service
                        SPWorkflowProcessorRuntime.AddService(typeof(AveUserMappingService), null);
                        //FilterData Service
                        SPWorkflowProcessorRuntime.AddService(typeof(LS.SPWorkflowProcessor.Services.WFTaskAndHistoryDataFilter), null);
                        //Cache Service
                        Dictionary<string, string> param = new Dictionary<string, string>();
                        param.Add("RootDirectory", AveEnv.AgentTempFolder);
                        SPWorkflowProcessorRuntime.AddService(typeof(LS.SPWorkflowProcessor.FileCacheService), param);
                        //Postpone Service
                        SPWorkflowProcessorRuntime.AddService(typeof(LS.SPWorkflowProcessor.WebPostponeActionService), param);
                        if (ParentSite.AveLanguageProcesser != null && ParentSite.AveLanguageProcesser.ListMapping.Count > 0)
                        {
                            SPWorkflowProcessorRuntime.AddService(typeof(NativeLanguageMappingService), GetLanguageMappingParam(ParentSite.AveLanguageProcesser.ListMapping));
                        }

                        string configFilePath = System.IO.Path.Combine(AveEnv.AgentDataPath, "SP2010/WrapperCommon", mConfigFile);
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
        // ListMapping转成NativeLanguageMappingService可用格式
        private static Dictionary<string, string> GetLanguageMappingParam(AveVolatileCache<string, string> oldListMapping)
        {
            Dictionary<string, string> newListMapping = new Dictionary<string, string>();
            StringBuilder sbListMapping = new StringBuilder();
            foreach (string item in oldListMapping.Keys)
            {
                sbListMapping.Append(item + ",");
                sbListMapping.Append(oldListMapping[item].ToString() + ";");
            }
            newListMapping[LanguageMappingScopeEnum.ListTitle.ToString()] = sbListMapping.ToString().TrimEnd(';');
            return newListMapping;
        }

        public static void ClearResolution()
        {
            //SAAS-21766 在还原一个新的web之前，dispose上一个web使用的workflow信息
            if (instance != null)
            {
                instance = null;
            }
            if (SPWorkflowProcessorRuntime.CustomTemplateContentProcessors != null)
            {
                SPWorkflowProcessorRuntime.CustomTemplateContentProcessors.Clear();
                SPWorkflowProcessorRuntime.CustomTemplateContentProcessors = null;
            }
            WFConflictResolutionInternal.CleanResolutionInternalModel();
        }
        #endregion

        public void SetNWDBConnectionString(string connStr)
        {
            try
            {
                if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWDBConnectionStringOfRestore"))
                {
                    SPWorkflowProcessorRuntime.AllProcessorParams["NWDBConnectionStringOfRestore"] = connStr;
                }
                else
                {
                    SPWorkflowProcessorRuntime.AllProcessorParams.Add("NWDBConnectionStringOfRestore", connStr);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, "A error occurred while set nintex workflow (restore)parameter.detail:{0}", e.ToString());
            }
        }

        #region [******Handle PostponeAction ******]

        public void ExecutePostAction(AveSPSite site)
        {
            var spSite = site.SPSite;
            var mapping = site.MappingManager;
            RestoreWFDefinitionEventHandler definitionExecution = new RestoreWFDefinitionEventHandler(RestoreDefinitionExecuted);
            var postService=SPWorkflowProcessorRuntime.GetWebPostponeActionService();
            if (postService != null)
            {
                postService.Execute(spSite, mapping, definitionExecution);
            }
            //workflowRestoreCore.WFAssociationProcessor.RestoreWFDefinitionEvent += new RestoreWFDefinitionEventHandler(RestoreDefinitionExecuted);
            //workflowRestoreCore.WFInstanceProcessor.RestoreWFInstanceEvent += new RestoreWFInstanceEventHandler(RestoreInstanceExecuted);
            //workflowRestoreCore.WFAssociationProcessor.Site = site.SPSite;
            //workflowRestoreCore.ExecutePostAction();
            //workflowRestoreCore.WFAssociationProcessor.RestoreWFDefinitionEvent -= new RestoreWFDefinitionEventHandler(RestoreDefinitionExecuted);
            //workflowRestoreCore.WFInstanceProcessor.RestoreWFInstanceEvent -= new RestoreWFInstanceEventHandler(RestoreInstanceExecuted);
        }

        internal void RestoreInstanceExecuted(object sender, RestoreWFInstanceEventArgs e)
        {
            //switch (e.ParentObjectType)
            //{
            //    case SPWFAssociationParentType.Web:
            //        HandleInstanceConflict((SPWFInstanceUnit)sender, (IAveWeb)e.ParentObject);
            //        break;
            //    case SPWFAssociationParentType.List:
            //    case SPWFAssociationParentType.ListContentType:
            //    case SPWFAssociationParentType.WebContentType:
            //        HandleInstanceConflict((SPWFInstanceUnit)sender, (IAveListItem)e.ParentObject);
            //        break;
            //    default:
            //        break;
            //}
        }

        internal void RestoreDefinitionExecuted(object sender, RestoreWFDefinitionEventArgs e)
        {
            AssociationParentObject = e.ParentObject;
            WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(((SPWFAssociationUnit)sender).WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
            {
                conflictResolutionInternal.HandleAssociationConflictInternal((SPWFAssociationUnit)sender, (WorkflowAssociationParentObject)AssociationParentObject,new AssociationHelper { SiteMapping=e.Mapping});
            }
        }

        #endregion

        #region [******Handle Workflow Association******]

        public void RestoreProjectAssociationData(AveWorkflowInfo wfInfo, AveSPWeb parentWeb=null)
        {
            SPWFAssociationUnit assoUnit = CacheProjectAssociationData(wfInfo);
            if (!SPWorkflowProcessorRuntime.ProcessAssociation)
            {
                return;
            }
            RestoreAssociationDataInternal(wfInfo, assoUnit,new AssociationHelper { SiteMapping=parentWeb.ParentSite.MappingManager});
            ParentSite.MappingManager.ProjectMappingManager.AddWorkflowSubscriptionIdMapping(assoUnit.SerializableData.mSourceId, assoUnit.SerializableData.mId);
        }

        /// <summary>
        /// 还原web(web contentType)上的workflow definition
        /// </summary>
        /// <param name="wfInfo">workflow的备份数据</param>
        /// <param name="web">parent AveSPWeb</param>
        /// <param name="contentType">还原web workflow association时为null,还原web contentType workflow时为关联的web contentType</param>
        public void RestoreAssociationData(AveWorkflowInfo wfInfo, AveSPWeb web, IAveContentType contentType = null)
        {
            WebContentTypeAssociation = false;
            //ParentSPWeb = web as AveSPWeb;
            //AveUserMappingService.AveSite = ParentSPWeb.ParentSite;
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
            var helper = new AssociationHelper
            {
                SiteMapping = web.ParentSite.MappingManager
            };
            SPWFAssociationUnit assoUnit = CacheAssociationData(wfInfo);
            if (!SPWorkflowProcessorRuntime.ProcessAssociation)
            {
                return;
            }
            RestoreAssociationDataInternal(wfInfo, assoUnit, helper);
        }

        /// <summary>
        /// 还原list(list contentType)上的workflow definition
        /// </summary>
        /// <param name="wfInfo">workflow definition的备份数据</param>
        /// <param name="list">parent AveSPList</param>
        /// <param name="contentType">还原web workflow association时为null,还原web contentType workflow时为关联的list contentType</param>
        public void RestoreAssociationData(AveWorkflowInfo wfInfo, AveSPList list, IAveContentType contentType = null)
        {
            WebContentTypeAssociation = false;
            if (list != null)
            {
                //ParentSPWeb = list.ParentWeb as AveSPWeb;
                // AveUserMappingService.AveSite = ParentSPWeb.ParentSite;
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
            var helper = new AssociationHelper
            {
                SiteMapping = list?.ParentSite.MappingManager
            };
            SPWFAssociationUnit assoUnit = CacheAssociationData(wfInfo);
            if (!SPWorkflowProcessorRuntime.ProcessAssociation)
            {
                return;
            }
            RestoreAssociationDataInternal(wfInfo, assoUnit, helper);
        }

        [Obsolete]
        public void RestoreAssociationData(AveWorkflowInfo wfInfo)
        {
#if PerformanceLog
                using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.RestoreAssociationData"))
                {
#endif
            SPWFAssociationUnit assoUnit = CacheAssociationData(wfInfo);
            if (!SPWorkflowProcessorRuntime.ProcessAssociation)
            {
                return;
            }
            RestoreAssociationDataInternal(wfInfo, assoUnit);
#if PerformanceLog
                }
#endif
        }

        private void RestoreAssociationDataInternal(AveWorkflowInfo wfInfo, SPWFAssociationUnit assoUnit, AssociationHelper helper)
        {
            try
            {
                WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(assoUnit.WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
                {
                    conflictResolutionInternal.RestoreAssociationDataInternal(wfInfo, assoUnit,helper, true);
                }
            }
            catch (SPWFProcessorException procException)
            {
                log.Log(AveLogLevel.INFO, "Skip restore workflow association, a known error occurred while restore workflow association, reason:{0}.", procException.Message);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An unknown error occurred while restore workflow association.", e);
            }
        }

        [Obsolete]
        private void RestoreAssociationDataInternal(AveWorkflowInfo wfInfo, SPWFAssociationUnit assoUnit)
        {
            try
            {
                WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(assoUnit.WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
                {
                    conflictResolutionInternal.RestoreAssociationDataInternal(wfInfo, assoUnit,null, true);
                }
            }
            catch (SPWFProcessorException procException)
            {
                log.Log(AveLogLevel.INFO, "Skip restore workflow association, a known error occurred while restore workflow association, reason:{0}.", procException.Message);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An unknown error occurred while restore workflow association.", e);
            }
        }

        public SPWFAssociationUnit CacheAssociationData(AveWorkflowInfo wfInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.CacheAssociationData"))
            {
#endif
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
                    return null;
                }
#if PerformanceLog
            }
#endif
        }

        public SPWFAssociationUnit CacheProjectAssociationData(AveWorkflowInfo wfInfo)
        {
            var associationUnit = CacheAssociationData(wfInfo);
            //兼容老备份数据
            associationUnit.SerializableData.IsProjectWorkflow = true;
            return associationUnit;

        }

        public void RestoreWorkflowTemplates(AveSPWeb web, List<AveWorkflowInfo> templates, WFTemplateConflictResolutionOption option)
        {
            AveUserMappingService.AveSite = web.ParentSite;
            AssociationParentObject = web.SPWeb;
            List<AveWorkflowInfo> laterTemplates = new List<AveWorkflowInfo>();
            foreach (var template in templates)
            {
                RestoreSingleWorkflowTemplate(template, web.SPWeb, option);
            }
        }

        private void RestoreSingleWorkflowTemplate(AveWorkflowInfo templateInfo, IAveWeb web, WFTemplateConflictResolutionOption option)
        {
            SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(templateInfo.AssociationUnit);
            if (assoUnit == null)
            {
                return;
            }
            assoUnit.ParentObject = web;
            assoUnit.ParentObjectType = SPWFAssociationParentType.Web;

            string name = assoUnit.SerializableData.mName;
            string objTitle = assoUnit.ParentWeb.Title;
            //const AveReportObjectType reportObjectType = AveReportObjectType.WorkflowTemplate;
            SPWFInternalPlatform platformType = SPWFInternalPlatform.Default;
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
                        conflictResolutionInternal.RestoreTemplateDataInternal(assoUnit, option);
                    }
                }
            }
            catch (AveWrapperSkipException exception)
            {
                log.Warn("Skip restoring the workflow template. Name: {0} Error: {1}", name, exception);
            }
            catch (SPWFProcessorException exception)
            {
                if (exception.ErrorCode == (int)SPWFProcessorErrorCode.PutIntoPostAction)
                {
                    //donot support post action restore workflow tempalte at the moment, log it and add failed report
                    log.Warn("Failed restoring the workflow template. Name: {0},ErrorCode:{1}, Error: {2}", name, exception.ErrorCode, exception);
                }
                else
                {
                    log.Error("An error occurred while restoring the workflow template. Name: {0} Error: {1}", name, exception);
                }
            }
            catch (Exception exception)
            {
                log.Error("An error occurred while restoring the workflow template. Name: {0} Error: {1}", name, exception);
            }

        }
        #endregion
        #region obsolute
        #region [******Handle Workflow Instance******]
        [Obsolete]
        public void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveListItem item)
        {
            WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(instanceUnit.WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
            {
                conflictResolutionInternal.HandleInstanceConflict(instanceUnit, item);
            }
        }
        [Obsolete]
        public void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveWeb web)
        {
            WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(instanceUnit.WFInternalPlatform, GetWFConflictResolutionParametersBySelf());
            {
                conflictResolutionInternal.HandleInstanceConflict(instanceUnit, web);
            }
        }

        #endregion

        #region [******Handle Workflow Scheduel******]
        [Obsolete]
        public void RestoreScheduleData(AveWorkflowInfo wfInfo, IAveListItem item)
        {
            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            if (SPWorkflowProcessorRuntime.ProcessAssociation)
            {
                workflowRestoreCore.RestoreSchedule(wfAssociationUnit, item);
            }
        }
        [Obsolete]
        public void RestoreScheduleData(AveWorkflowInfo wfInfo, IAveWeb web)
        {
            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            if (SPWorkflowProcessorRuntime.ProcessAssociation)
            {
                workflowRestoreCore.RestoreSchedule(wfAssociationUnit, web);
            }
        }
        #endregion
        [Obsolete]
        public void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveListItem item)
        {
            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            HandleInstanceConflict(wfAssociationUnit, item);
        }
        [Obsolete]
        public void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveWeb web)
        {
            var wfAssociationUnit = SPWFInstanceUnit.Load(wfInfo.AssociationUnit);
            HandleInstanceConflict(wfAssociationUnit, web);
        }
        #endregion obsolute
        public void SetWorkflowProcessorRuntime(AveSPWorkflowRestoreOption workflowRestoreOption)
        {
            SPWorkflowProcessorRuntime.ProcessAssociation = workflowRestoreOption.ProcessAssociation;
            SPWorkflowProcessorRuntime.ProcessInstance = workflowRestoreOption.ProcessInstance;
            SPWorkflowProcessorRuntime.RestartRunningInstance = workflowRestoreOption.RestartRunningInstance;
            SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound = workflowRestoreOption.RestoreParentAssociationIfNotFound;
            SPWorkflowProcessorRuntime.SkipRunningInstance = workflowRestoreOption.SkipRunningInstance;
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
            parameterInstance.AssociationOption = this.AssociationOption;
            parameterInstance.InstanceOption = this.InstanceOption;
            parameterInstance.mAssociationParentObject = this.mAssociationParentObject;
            parameterInstance.mUnitsOfBackup = this.mUnitsOfBackup;
            parameterInstance.reportor = this.reportor;
            parameterInstance.workflowRestoreCore = this.workflowRestoreCore;
            parameterInstance.WebContentTypeAssociation = this.WebContentTypeAssociation;

            return parameterInstance;
        }

        /// <summary>
        /// 更新workflow的start options在post action中更新，目前只支持2010
        /// </summary>
        public void UpdateWorkflowStartOptions(IAveWorkflowAssociationCollection workflowAssociations)
        {
            WFConflictResolutionParameters parameters = GetWFConflictResolutionParametersBySelf();
            foreach (KeyValuePair<Guid, AveWorkflowInfo> pair in mUnitsOfBackup)
            {
                AveWorkflowInfo workflowInfo = pair.Value;
                SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(workflowInfo.AssociationUnit);
                WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(assoUnit.WFInternalPlatform, parameters);
                try
                {
                    switch (assoUnit.WFInternalPlatform)
                    {
                        case SPWFInternalPlatform.WF2010PlatformType:
                            SPWFAssociationUnit desAssoUnit = null;
                            if (parameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.TryGetValue(assoUnit.SerializableData.mId, out desAssoUnit))
                            {
                                IAveWorkflowAssociation desWorkflowAssociation = workflowAssociations.GetAssociationByName(desAssoUnit.SerializableData.mName, CultureInfo.CurrentUICulture);
                                if (conflictResolutionInternal.UpdateWorkflowStartOptions(assoUnit, desWorkflowAssociation))
                                {
                                    desAssoUnit.UpdateWorkflowAssociation(desWorkflowAssociation);
                                }
                            }
                            break;
                        case SPWFInternalPlatform.WF2013PlatformType:
                            conflictResolutionInternal.UpdateWorkflowStartOptions(assoUnit);
                            break;
                        default:
                            log.Log(AveLogLevel.WARN, "Not support platform type :{0}.", assoUnit.WFInternalPlatform);
                            break;
                    }
                }
                catch (Exception e)
                {

                    log.Log(AveLogLevel.WARN, "An error occurred while update {0} workflow \"Start Options\" workflow association. ErrorMessage:{1}", assoUnit.WFInternalPlatform, e.ToString());
                }
            }
            //this.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.Clear();
            //this.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored.Clear();
        }

        public void UpdateWorkflowStartOptions(AveSPWeb aveSPWeb)
        {
            if (aveSPWeb.SPWeb == null)
            {
                return;
            }
            WFConflictResolutionParameters parameters = GetWFConflictResolutionParametersBySelf();
            List<SPWFAssociationUnit> associationUnits10Modes = parameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.Values.Where(value => value.ParentWeb != null && value.ParentWeb.ID == aveSPWeb.SPWeb.ID).ToList<SPWFAssociationUnit>();
            List<SPWFAssociationUnit> associationUnitsunits13Modes = parameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored.Values.Where(value => value.ParentWeb != null && value.ParentWeb.ID == aveSPWeb.SPWeb.ID).ToList<SPWFAssociationUnit>();
            if (associationUnits10Modes.Count > 0)
            {
                UpdateSP10WorkflowStartOptions(associationUnits10Modes, parameters);
            }
            if (associationUnitsunits13Modes.Count > 0)
            {
                UpdateSP13WorkflowStartOptions(associationUnitsunits13Modes, parameters);
            }
        }

        public void UpdateWorkflowStartOptions(AveSPList aveSPList)
        {
            if (aveSPList.SPList == null)
            {
                return;
            }
            WFConflictResolutionParameters parameters = GetWFConflictResolutionParametersBySelf();
            List<SPWFAssociationUnit> associationUnits10Modes = parameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.Values.Where(value => value.ParentList != null && value.ParentList.ID == aveSPList.SPList.ID).ToList<SPWFAssociationUnit>();
            List<SPWFAssociationUnit> associationUnitsunits13Modes = parameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored.Values.Where(value => value.ParentList != null && value.ParentList.ID == aveSPList.SPList.ID).ToList<SPWFAssociationUnit>();
            if (associationUnits10Modes.Count > 0)
            {
                UpdateSP10WorkflowStartOptions(associationUnits10Modes, parameters);
            }
            if (associationUnitsunits13Modes.Count > 0)
            {
                UpdateSP13WorkflowStartOptions(associationUnitsunits13Modes, parameters);
            }
        }

        private void UpdateSP10WorkflowStartOptions(List<SPWFAssociationUnit> desAssociationUnits, WFConflictResolutionParameters conflictResolutionParameters)
        {
            WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(SPWFInternalPlatform.WF2010PlatformType, conflictResolutionParameters);
            try
            {
                foreach (var desAssociationUnit in desAssociationUnits)
                {
                    SPWFAssociationUnit assoUnit = GetSourceSPWFAssociationUnit(desAssociationUnit.SourceId);
                    if (assoUnit != null && conflictResolutionInternal.UpdateWorkflowStartOptions(assoUnit, desAssociationUnit.SPAssociation))
                    {
                        desAssociationUnit.UpdateWorkflowAssociation(desAssociationUnit.SPAssociation);
                    }
                    conflictResolutionParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.Remove(desAssociationUnit.SourceId);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while update {0} workflow \"Start Options\" workflow association. ErrorMessage:{1}", SPWFInternalPlatform.WF2010PlatformType, e);
            }
        }

        private void UpdateSP13WorkflowStartOptions(List<SPWFAssociationUnit> desAssociationUnits, WFConflictResolutionParameters conflictResolutionParameters)
        {
            WFConflictResolutionInternal conflictResolutionInternal = WFConflictResolutionInternal.GetInstance(SPWFInternalPlatform.WF2013PlatformType, conflictResolutionParameters);
            try
            {
                foreach (var desAssociationUnit in desAssociationUnits)
                {
                    SPWFAssociationUnit assoUnit = GetSourceSPWFAssociationUnit(desAssociationUnit.SourceId);
                    if (assoUnit != null)
                    {
                        conflictResolutionInternal.UpdateWorkflowStartOptions(assoUnit);
                    }
                    conflictResolutionParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored.Remove(desAssociationUnit.SourceId);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while update {0} workflow \"Start Options\" workflow association. ErrorMessage:{1}", SPWFInternalPlatform.WF2013PlatformType, e);
            }
        }

        public SPWFAssociationUnit GetSourceSPWFAssociationUnit(Guid sourceId)
        {
            SPWFAssociationUnit assoUnit = null;
            AveWorkflowInfo aveWorkflowInfo = null;
            if (mUnitsOfBackup.TryGetValue(sourceId, out aveWorkflowInfo))
            {
                assoUnit = SPWFAssociationUnit.Load(aveWorkflowInfo.AssociationUnit);
            }
            return assoUnit;
        }
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

        protected WFConflictResolutionInternal()
        { }

        protected WFConflictResolutionInternal(WFConflictResolutionParameters parameters)
        {
            mWFCRParameters = parameters;
        }

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

        private static WFConflictResolutionInternalFor13Model mConflictResolutionInternal13Model = null;
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

        private static WFConflictResolutionInternalForProject mConflictResolutionInternalProject = null;
        private static WFConflictResolutionInternalFor13Model GetConflictResolutionInternalProject(WFConflictResolutionParameters parameters)
        {
            if (mConflictResolutionInternalProject == null)
            {
                mConflictResolutionInternalProject = new WFConflictResolutionInternalForProject(parameters);
            }
            else
            {
                mConflictResolutionInternalProject.Update(parameters);
            }
            return mConflictResolutionInternalProject;
        }

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
                case SPWFInternalPlatform.WFProjectPlatformType:
                    return GetConflictResolutionInternalProject(parameters);
                default:
                    throw new NotSupportedException("Not support platform type.");

            }
        }

        public static void CleanResolutionInternalModel()
        {
            if (mConflictResolutionInternal10Model != null)
            {
                mConflictResolutionInternal10Model = null;
            }
            if (mConflictResolutionInternal13Model != null)
            {
                mConflictResolutionInternal13Model = null;
            }
        }
        public void Update(WFConflictResolutionParameters parameters)
        {
            this.mWFCRParameters = parameters;
        }

        public virtual bool UpdateWorkflowStartOptions(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation desWorkflowAssociation = null) { return false; }

        public virtual void HandleAssociationConflictInternal(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent,AssociationHelper helper) { }

        public virtual void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveListItem item) { }

        public virtual void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveWeb web) { }

        public WorkflowAssociationParentObject SetAssociationParentObject(object value, ref AveWorkflowRestoreCore workflowRestoreCore, bool webContentTypeWorkflow)
        {
            WorkflowAssociationParentObject associationParentObject = null;
            if (value is IAveList)
            {
                associationParentObject = new WorkflowAssociationParentObject((IAveList)value);
            }
            else if (value is IAveContentType)
            {
                if (webContentTypeWorkflow)
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
            else if (value == null)
            {
                associationParentObject = null;
            }
            SetWorkflowRestoreCore((SPWFAssociationParentType)(associationParentObject?.ParentType), ref workflowRestoreCore);
            return associationParentObject;
        }

        private void SetWorkflowRestoreCore(SPWFAssociationParentType parentType, ref AveWorkflowRestoreCore workflowRestoreCore)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.SetWorkflowRestoreCore"))
            {
#endif
                if (workflowRestoreCore == null || workflowRestoreCore.ParentType != parentType)
                {
                    Common.ArgumentCheck.CheckNotNull(workflowRestoreCore);
                    workflowRestoreCore = AveWorkflowRestoreCoreFactory.GetWorkflowRestoreCore(parentType, workflowRestoreCore?.WFAssociationProcessor, workflowRestoreCore?.WFAssociationProcessor13Model, workflowRestoreCore?.WFInstanceProcessor, workflowRestoreCore?.WFInstanceProcessor13Model);
                }
#if PerformanceLog
            }
#endif
        }

        internal virtual void RestoreTemplateDataInternal(SPWFAssociationUnit assoUnit, WFTemplateConflictResolutionOption option)
        {
        }

        internal void RestoreAssociationDataInternal(AveWorkflowInfo wfInfo, SPWFAssociationUnit assoUnit,AssociationHelper helper, bool needReport)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.RestoreAssociationDataInternal"))
            {
#endif
                if (wfInfo == null || this.mWFCRParameters.mAssociationParentObject == null)
                {
                    throw new AveException("Invalid argument, argument can not be null.");
                }

                if (assoUnit == null || (SPWorkflowProcessorRuntime.RestoreBuiltinOnly && !assoUnit.IsBuiltinBaseId))
                {
                    return;
                }
                string parentTitle = null;
                assoUnit.reusableWFContentTypeName = wfInfo.CTName;
                var type = GetParentObjectType(mWFCRParameters.mAssociationParentObject as WorkflowAssociationParentObject, out parentTitle);
                if (assoUnit.SerializableData.IsProjectWorkflow)
                {
                    type = AveReportObjectType.ProjectWorkflowDefinition;
                }
                try
                {
                    log.Log(AveLogLevel.INFO, "Start to restore AssociationData, assoUnit name:{0},parent title:{1}", assoUnit.SerializableData.mName, parentTitle);
                    ActiveDependencyFeatures(assoUnit, this.mWFCRParameters.mAssociationParentObject);
                    HandleAssociationConflictInternal(assoUnit, mWFCRParameters.mAssociationParentObject,helper);
                    if (needReport)
                    {
                        log.Log(AveLogLevel.INFO, "Finish restore AssociationData successfully,assoUnit name:{0},parent title:{1}", assoUnit.SerializableData.mName, parentTitle);
                        mWFCRParameters.reportor.AddDetail(
                            new AveWrapperReportDto(assoUnit.SerializableData.mName, parentTitle, type, AveStatus.Successful, string.Empty));
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    if (needReport)
                    {
                        mWFCRParameters.reportor.AddDetail(
                            new AveWrapperReportDto(assoUnit.SerializableData.mName, parentTitle, type, AveStatus.Skipped, procException.Message));

                    }
                    throw;
                }
                catch (AveWrapperSkipException ex)
                {
                    log.Log(AveLogLevel.INFO, "Skip restore workflow association, reason:{0}.", ex.Message);
                    if (needReport)
                    {
                        mWFCRParameters.reportor.AddDetail(
                            new AveWrapperReportDto(assoUnit.SerializableData.mName, parentTitle, type, AveStatus.Skipped, ex.Message));

                    }
                }
                catch (AveWrapperWorkflowException ex)
                {
                    log.Warn("An AveWrapperWorkflowException was thrown while handling association conflict. Association:{0}\n{1}",
                        assoUnit.SerializableData.mName, ex);
                    if (needReport)
                    {
                        mWFCRParameters.reportor.AddDetail(
                            new AveWrapperReportDto(assoUnit.SerializableData.mName, parentTitle, type, AveStatus.Failed, ex.InnerException.Message));
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An Error occurred while handle association conflict.Association:{0}\n{1}",
                        assoUnit.SerializableData.mName, ex);
                    mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(assoUnit.SerializableData.mName, parentTitle, type, AveStatus.Skipped, WrapperWorkflowResource.RestoreWFWithoutPermission));
                }
                catch (Exception ex)
                {
                    log.Warn("An Error occurred while handle association conflict.Association:{0}\n{1}",
                        assoUnit.SerializableData.mName, ex);
                }
#if PerformanceLog
            }
#endif
        }

        #region active feature
        private void ActiveDependencyFeatures(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parentObject)
        {
            try
            {
                if (assoUnit.IsBuiltinBaseIdForSP2010)
                {
                    IAveSite site = parentObject.ParentWeb.Site;
                    string tempBuiltinBaseId = assoUnit.BuiltinEnglishBaseIdForSP2010;
                    switch (tempBuiltinBaseId)
                    {
                        case AveConstants.Three_State:
                            SafeAddSiteFeature(AveConstants.IssueTrackingWorkflow, site); //SC Three-state workflow Feature
                            break;
                        case AveConstants.Disposition_Approval:
                            SafeAddSiteFeature(AveConstants.ExpirationWorkflow, site);//SC Disposition Approval Workflow Feature
                            break;
                        case AveConstants.COLLECT_FEEDBACK_BASEID:
                        case AveConstants.COLLECT_SIGNATURE_BASEID:
                        case AveConstants.APPROVAL_BASEID:
                            SafeAddSiteFeature(AveConstants.Workflows, site);//SC Workflows Feature
                            break;
                        case AveConstants.PUBLISHING_APPROVAL_BASEID:
                            SafeAddSiteFeature(AveConstants.ReviewPublishingSPD, site);//SC Publishing Approval Workflow Feature
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                log.Info("An error occurred while trigger workflow dependency site features, template base id: {0}, error message: {1}", assoUnit.SerializableData.mBaseId, e);
            }
        }

        private void SafeAddSiteFeature(Guid featureId, IAveSite site)
        {
            try
            {
                if (site.Features[featureId] == null)
                {
                    site.Features.Add(featureId, true);
                }
            }
            catch (Exception e)
            {
                log.Debug("An error occurred while add single workflow dependency feature to site collection, feature id: {0}, error message: {1}", featureId, e);
            }
        }
        #endregion

        private AveReportObjectType GetParentObjectType(WorkflowAssociationParentObject parent, out string parentTitle)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.GetParentObjectType"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        protected void AddFieldMapping(SPWFInstanceUnit instanceUnit, Guid ListId)
        {
            if (WFConflictResolution.ParentSite.MappingManager.SiteMappingManager.ListFieldsMapping.ContainsKey(ListId))
            {
                var listMapping = WFConflictResolution.ParentSite.MappingManager.SiteMappingManager.ListFieldsMapping[ListId];
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

        #region [******Handle WorkflowAssociation******]

        public override void HandleAssociationConflictInternal(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, AssociationHelper helper)
        {
            log.Log(AveLogLevel.INFO, "Begin to restore workflow association for 10 model internally, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
            IAveWorkflowAssociation asso = FindAssociation(assoUnit.SerializableData.mName, parent.WorkflowAssociations);
            if (asso == null)
            {
                mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
            }
            else
            {
                if (mWFCRParameters.AssociationOption != WFAssociationConflictResolutionOption.ForceUse && !needAssociationConflictResolution)
                {
                    throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, WrapperWorkflowResource.WFDeConflictError);
                }
                #region Compare and Conflict Resolution
                switch (CompareAssociation(assoUnit, asso))
                {
                    case WFAssociationConflictType.Template:
                        throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, WrapperWorkflowResource.WFDeNameOrTemplateConflictError);
                    default:
                        AssociationConflictResolution(assoUnit, parent, asso, parent.WorkflowAssociations);
                        break;
                }
            }
            if (assoUnit!=null&&
                assoUnit.SerializableData.mId != Guid.Empty&&
                helper!=null&&
                helper.SiteMapping!=null)
            {
               helper.SiteMapping.SiteMappingManager.AddWorkflowIdMapping(assoUnit.SerializableData.mSourceId, assoUnit.SerializableData.mId);
            }
            #endregion
        }

        public override bool UpdateWorkflowStartOptions(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation desWorkflowAssociation)
        {
            bool needUpdate = false;
            if (desWorkflowAssociation != null)
            {
                bool allowManualStart = (((Configuration)assoUnit.SerializableData.mConfiguration & Configuration.AllowManualStart) != Configuration.None);
                bool autoStartChange = (((Configuration)assoUnit.SerializableData.mConfiguration & Configuration.AutoStartChange) != Configuration.None);
                bool autoStartCreate = (((Configuration)assoUnit.SerializableData.mConfiguration & Configuration.AutoStartAdd) != Configuration.None);
                bool enabled = assoUnit.SerializableData.mEnable;
                if (desWorkflowAssociation.AllowManual != allowManualStart)
                {
                    desWorkflowAssociation.AllowManual = allowManualStart;
                    needUpdate = true;
                }
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
                if (desWorkflowAssociation.Enabled != enabled)
                {
                    desWorkflowAssociation.Enabled = enabled;
                    needUpdate = true;
                }
            }
            return needUpdate;
        }

        private IAveWorkflowAssociation FindAssociation(string assoName, IAveWorkflowAssociationCollection assoCollection)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.FindAssociation"))
            {
#endif
                return assoCollection.GetAssociationByName(assoName, System.Globalization.CultureInfo.CurrentUICulture);
#if PerformanceLog
            }
#endif
        }

        protected WFAssociationConflictType CompareAssociation(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.CompareAssociation"))
            {
#endif
                //Do Compare
                if (!assoUnit.SerializableData.mBaseId.Equals(asso.BaseId) && this.mWFCRParameters.AssociationOption != WFAssociationConflictResolutionOption.ForceUse)
                {
                    return WFAssociationConflictType.Template;
                }
                return WFAssociationConflictType.None;
#if PerformanceLog
            }
#endif
        }

        private void AssociationConflictResolution(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, IAveWorkflowAssociation asso, IAveWorkflowAssociationCollection workflowAssociations)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.AssociationConflictResolution"))
            {
#endif
                switch (this.mWFCRParameters.AssociationOption)
                {
                    case WFAssociationConflictResolutionOption.Append:
                        //Rename workflow 
                        string oldName = assoUnit.SerializableData.mName;
                        string newName = RenameAssociation(oldName, parent);
                        assoUnit.SerializableData.mName = newName;
                        assoUnit.SerializableData.mOriginalName = newName;

                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        break;
                    case WFAssociationConflictResolutionOption.Overwrite:
                        int count = parent.WorkflowManager.CountWorkflows(asso);
                        if (count == 0)
                        {
                            parent.WorkflowAssociations.Remove(asso);
                            this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        }

                        break;
                    case WFAssociationConflictResolutionOption.ForceOverwrite:
                        parent.WorkflowAssociations.Remove(asso);
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        break;
                    case WFAssociationConflictResolutionOption.UpdateOverwrite:
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        break;
                    case WFAssociationConflictResolutionOption.ForceUse:
                        this.mWFCRParameters.workflowRestoreCore.ForceUpdate = false;
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        break;
                    case WFAssociationConflictResolutionOption.NotOverwrite:
                        log.Log(AveLogLevel.DEBUG, "RestoreWorkflowDefinitionSkip.");
                        break;
                    default:
                        throw new AveWrapperException(AveWrapperErrorCode.DefinitionConflictResolutionOptionInvalid, "Association conflict resolution invalid.");
                }
#if PerformanceLog
            }
#endif
        }

        private string RenameAssociation(string oldName, WorkflowAssociationParentObject parent)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.RenameAssociation"))
            {
#endif
                string newName = oldName;
                int counter = 1;
                while (true)
                {
                    newName = oldName + AppendSuffix + counter;

                    if (parent.WorkflowAssociations.GetAssociationByName(newName, System.Globalization.CultureInfo.CurrentUICulture) == null)
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
#if PerformanceLog
            }
#endif
        }

        #endregion

        #region[******Handle WorkflowInstance******]

        public override void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveListItem item)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.HandleInstanceConflict"))
            {
#endif
                if (!SPWorkflowProcessorRuntime.ProcessInstance)
                {
                    return;
                }

                if (instanceUnit == null || item == null)
                {
                    throw new AveException("Invalid argument, argument can not be null.");
                }
                string parentAssociationName = null;
                try
                {
                    Guid parentAssoId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("#TemplateId");
                    if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(parentAssoId))
                    {
                        if (SPWorkflowProcessorRuntime.RestoreBuiltinOnly && !SPWFAssociationUnit.Load(this.mWFCRParameters.mUnitsOfBackup[parentAssoId].AssociationUnit).IsBuiltinBaseId)
                        {
                            return;
                        }
                    }
                    parentAssociationName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                    //Guid parentAssoBaseId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationBaseId");
                    if (!TryFindParentAssociation(parentAssoId, item, instanceUnit))
                    {
                        throw new WorkflowDefinitionNotFoundException(parentAssociationName, item.Name, item.ParentList.Title, item.ParentList.ParentWeb.Url);
                    }
                    else
                    {
                        instanceUnit.ParentAssociationUnit = this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored[parentAssoId];
                    }
                    #region deal with InstanceConflict
                    log.Info("WorkflowInstance Conflict", " Item Title is " + item.Title + " Item Name is " + item.Name + " InstanceState is " + instanceUnit.InstanceItem.Properties["#InternalState"].ToString());
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
                    this.mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(parentAssociationName, item.Title, AveReportObjectType.WorkflowInstance, AveStatus.Successful, string.Empty));

                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreWorkflowInstanceFailedEventMessage(parentAssociationName, ex));
                    this.mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(parentAssociationName, item.Title, AveReportObjectType.WorkflowInstance, AveStatus.Skipped, ex.Message));
                }
#if PerformanceLog
            }
#endif
        }

        public override void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.HandleInstanceConflict_Web"))
            {
#endif
                if (!SPWorkflowProcessorRuntime.ProcessInstance)
                {
                    return;
                }

                if (instanceUnit == null || web == null)
                {
                    throw new AveException("Invalid argument, argument can not be null.");
                }
                try
                {
                    Guid parentAssoId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("#TemplateId");
                    if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(parentAssoId))
                    {
                        if (SPWorkflowProcessorRuntime.RestoreBuiltinOnly && !SPWFAssociationUnit.Load(this.mWFCRParameters.mUnitsOfBackup[parentAssoId].AssociationUnit).IsBuiltinBaseId)
                        {
                            return;
                        }
                    }

                    //Guid parentAssoBaseId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationBaseId");
                    if (!TryFindParentAssociation(parentAssoId, web, instanceUnit))
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.ParentAssociationCannotBeFound);
                    }
                    else
                    {
                        instanceUnit.ParentAssociationUnit = this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored[parentAssoId];
                    }


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
                                InstanceConflictResolution(instanceUnit, web, createTimeWorkflow);
                            }
                        }
                        else
                        {
                            InstanceConflictResolution(instanceUnit, web, runningWorkflow);
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
                            InstanceConflictResolution(instanceUnit, web, workflow);
                        }
                    }
                    #endregion


                    //workflowRestoreCore.RestoreInstance(instanceUnit, web);
                    //IAveWorkflow workflow = TryGetRunningInstance(item, instanceUnit);
                    //if (workflow == null)
                    //{
                    //    workflowRestoreCore.RestoreInstance(instanceUnit, item);
                    //}
                    //else
                    //{
                    //    InstanceConflictResolution(instanceUnit, item, workflow);
                    //}

                    //if (!instanceUnit.ParentAssociationUnit.isCreateField)
                    //{
                    //    AddFieldMapping(instanceUnit, item.ParentList.ID);
                    //    instanceUnit.ParentAssociationUnit.isCreateField = true;
                    //}

                }
                catch (SPWFProcessorException ex)
                {
                    log.Warn("An WFProcessorException occurred while handle instance conflict.Web:{0}\r\n{1}",
                        web.Url, ex.ErrorCodeString);
                }
                catch (Exception ex)
                {
                    log.Warn("An Error occurred while handle instance conflict.Web:{0}\r\n{1}", web.Url, ex);
                }
#if PerformanceLog
            }
#endif
        }

        private void InstanceConflictResolution(SPWFInstanceUnit assoUnit, IAveListItem item, IAveWorkflow workflow)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.InstanceConflictResolution"))
            {
#endif
                switch (this.mWFCRParameters.InstanceOption)
                {
                    case WFInstanceConflictResolutionOption.Overwrite:
                        item.ParentList.ParentWeb.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
                        this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, item);
                        break;
                    case WFInstanceConflictResolutionOption.NotOverwrite:
                        break;
                    case WFInstanceConflictResolutionOption.OverwriteByModifiedTime:
                        // List<IAveWorkflow> wfs = item.ParentList.ParentWeb.Site.WorkflowManager.GetItemWorkflows(item, assoUnit.ParentAssociationUnit.Id);
                        // foreach (IAveWorkflow wf in wfs)
                        //{
                        //DateTime UtcCreatedTime = wf.Created.ToUniversalTime();
                        //DateTime UtcModifiedtime = wf.Modified.ToUniversalTime();
                        // if ((DateTime)assoUnit.InstanceItem.Properties["#Created"] == wf.Created)
                        //{
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
#if PerformanceLog
            }
#endif
        }

        private void InstanceConflictResolution(SPWFInstanceUnit assoUnit, IAveWeb web, IAveWorkflow workflow)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.InstanceConflictResolution_web"))
            {
#endif
                switch (this.mWFCRParameters.InstanceOption)
                {
                    case WFInstanceConflictResolutionOption.Overwrite:
                        web.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
                        this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, web);
                        break;
                    case WFInstanceConflictResolutionOption.NotOverwrite:
                        break;
                    case WFInstanceConflictResolutionOption.OverwriteByModifiedTime:
                        // List<IAveWorkflow> wfs = item.ParentList.ParentWeb.Site.WorkflowManager.GetItemWorkflows(item, assoUnit.ParentAssociationUnit.Id);
                        // foreach (IAveWorkflow wf in wfs)
                        //{
                        //DateTime UtcCreatedTime = wf.Created.ToUniversalTime();
                        //DateTime UtcModifiedtime = wf.Modified.ToUniversalTime();
                        // if ((DateTime)assoUnit.InstanceItem.Properties["#Created"] == wf.Created)
                        //{
                        if ((DateTime)assoUnit.InstanceItem.Properties["#Modified"] > workflow.Modified)
                        {
                            web.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
                            this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, web);
                            break;
                        }
                        else
                        {
                            return;
                        }
                    default:
                        break;
                }
#if PerformanceLog
            }
#endif
        }

        private IAveWorkflow TryGetRunningInstance(IAveListItem item, SPWFInstanceUnit instanceUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetRunningInstance"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        private IAveWorkflow TryGetRunningInstance(IAveWeb web, SPWFInstanceUnit instanceUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetRunningInstance_web"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        private IAveWorkflow TryGetAllInstance(IAveListItem item, SPWFInstanceUnit instanceUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetAllInstance"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        private IAveWorkflow TryGetAllInstance(IAveWeb web, SPWFInstanceUnit instanceUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetAllInstance_web"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        private bool TryFindParentAssociation(Guid parentAssoId, IAveListItem item, SPWFInstanceUnit instanceUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryFindParentAssociation"))
            {
#endif
                try
                {
                    //如果发现parentAssociation已经Cache则Instance直接Cache.
                    if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.NeedPostActionAssociations.Contains(parentAssoId))
                    {
                        try
                        {
                            SPWorkflowProcessorRuntime.OnCacheData(item.ParentList.ParentWeb.Site.ID.ToString(), item.ParentList.ParentWeb.ID.ToString(), item.ParentList.ID.ToString(), item.ParentList.ID.ToString(), item.ID, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                        }
                        return false;
                    }
                    //修改逻辑后UnitsOfRestored集合中应该只有一个元素,就是最后还原的Association
                    if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.ContainsKey(parentAssoId))
                    {
                        return true;
                    }

                    //之前已经还原过这个workflow association
                    string origName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                    if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestoredNameMapping.ContainsKey(origName.ToLower()))
                    {
                        SPWFAssociationUnit assoUnit = null;
                        IAveWorkflowAssociation asso = GetParentAssociation(item, this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestoredNameMapping[origName.ToLower()], parentAssoId, out assoUnit);
                        if (asso != null)
                        {
                            this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.SetRestoredUnit(assoUnit, asso);
                            return true;
                        }
                    }


                    //之前没有还原过这个workflow association，那么需要拿到备份数据，重新还原
                    if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(parentAssoId))
                    {
                        AveWorkflowInfo cacheData = this.mWFCRParameters.mUnitsOfBackup[parentAssoId];
                        if (string.IsNullOrEmpty(cacheData.CTId))
                        {
                            mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ParentList, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                        }
                        else
                        {
                            mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ContentType, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                        }

                        WFAssociationConflictResolutionOption temp = mWFCRParameters.AssociationOption;
                        mWFCRParameters.AssociationOption = WFAssociationConflictResolutionOption.ForceUse;
                        try
                        {
                            IAveWorkflowAssociation asso = FindAssociation(origName, mWFCRParameters.mAssociationParentObject.WorkflowAssociations);
                            if (asso == null && !SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound)
                                return false;
                            RestoreAssociationDataInternal(cacheData, SPWFAssociationUnit.Load(cacheData.AssociationUnit),null, false);
                        }
                        catch (SPWFProcessorException procException)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
                            try
                            {
                                if (procException.ErrorCode == 9999)
                                {
                                    SPWorkflowProcessorRuntime.OnCacheData(item.ParentList.ParentWeb.Site.ID.ToString(), item.ParentList.ParentWeb.ID.ToString(), item.ParentList.ID.ToString(), item.ParentList.ID.ToString(), item.ID, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                            }
                            return false;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.RestoreAssociationDataInternalError, e);
                        }
                        finally
                        {
                            mWFCRParameters.AssociationOption = temp;
                        }
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindParentAssociationFailed, e);
                    return false;
                }
#if PerformanceLog
            }
#endif
        }

        private bool TryFindParentAssociation(Guid parentAssoId, IAveWeb web, SPWFInstanceUnit instanceUnit)
        {
            try
            {
                if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.NeedPostActionAssociations.Contains(parentAssoId))
                {
                    try
                    {
                        SPWorkflowProcessorRuntime.OnCacheData(web.Site.ID.ToString(), web.ID.ToString(), string.Empty, string.Empty, int.MinValue, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                    }
                    return false;
                }
                //修改逻辑后UnitsOfRestored集合中应该只有一个元素,就是最后还原的Association
                if (mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestored.ContainsKey(parentAssoId))
                {
                    return true;
                }

                //之前已经还原过这个workflow association
                string origName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                if (mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestoredNameMapping.ContainsKey(origName.ToLower()))
                {
                    SPWFAssociationUnit assoUnit = null;
                    IAveWorkflowAssociation asso = GetParentAssociation(web, mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestoredNameMapping[origName.ToLower()], parentAssoId, out assoUnit);
                    if (asso != null)
                    {
                        mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.SetRestoredUnit(assoUnit, asso);
                        return true;
                    }
                }


                //之前没有还原过这个workflow association，那么需要拿到备份数据，重新还原
                if (mWFCRParameters.mUnitsOfBackup.ContainsKey(parentAssoId))
                {
                    AveWorkflowInfo cacheData = mWFCRParameters.mUnitsOfBackup[parentAssoId];
                    if (string.IsNullOrEmpty(cacheData.CTId))
                    {
                        mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(web, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                    }
                    else
                    {
                        //AssociationParentObject = item.ContentType;
                    }

                    WFAssociationConflictResolutionOption temp = mWFCRParameters.AssociationOption;
                    mWFCRParameters.AssociationOption = WFAssociationConflictResolutionOption.ForceUse;
                    try
                    {
                        IAveWorkflowAssociation asso = FindAssociation(origName, mWFCRParameters.mAssociationParentObject.WorkflowAssociations);
                        if (asso == null && !SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound)
                            return false;
                        RestoreAssociationDataInternal(cacheData, SPWFAssociationUnit.Load(cacheData.AssociationUnit),null, false);
                    }
                    catch (SPWFProcessorException procException)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
                        try
                        {
                            if (procException.ErrorCode == 9999)
                            {
                                SPWorkflowProcessorRuntime.OnCacheData(web.Site.ID.ToString(), web.ID.ToString(), string.Empty, string.Empty, int.MinValue, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                        }
                        return false;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.RestoreAssociationDataInternalError, e);
                    }
                    finally
                    {
                        mWFCRParameters.AssociationOption = temp;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindParentAssociationFailed, e);
                return false;
            }
        }

        private IAveWorkflowAssociation GetParentAssociation(IAveListItem item, string parentAssoName, Guid origAssoId, out SPWFAssociationUnit assoUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.GetParentAssociation"))
            {
#endif
                assoUnit = null;
                AveWorkflowInfo wfInfo = null;
                if (mWFCRParameters.mUnitsOfBackup.ContainsKey(origAssoId))
                {
                    wfInfo = mWFCRParameters.mUnitsOfBackup[origAssoId];
                    assoUnit = SPWFAssociationUnit.Load(wfInfo.AssociationUnit);
                }

                IAveWorkflowAssociation asso = null;
                if (string.IsNullOrEmpty(wfInfo?.CTId))
                {
                    Common.ArgumentCheck.CheckNotNull(assoUnit);
                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ParentList, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                    assoUnit.ParentObjectType = SPWFAssociationParentType.List;
                    assoUnit.ParentObject = item.ParentList;

                }
                else
                {
                    Common.ArgumentCheck.CheckNotNull(assoUnit);
                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ContentType, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                    assoUnit.ParentObjectType = SPWFAssociationParentType.ListContentType;
                    assoUnit.ParentObject = item.ContentType;
                }

                asso = mWFCRParameters.mAssociationParentObject.WorkflowAssociations.GetAssociationByName(parentAssoName, System.Globalization.CultureInfo.CurrentUICulture);
                return asso;
#if PerformanceLog
            }
#endif
        }

        private IAveWorkflowAssociation GetParentAssociation(IAveWeb web, string parentAssoName, Guid origAssoId, out SPWFAssociationUnit assoUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.GetParentAssociation_web"))
            {
#endif
                assoUnit = null;
                AveWorkflowInfo wfInfo = null;
                if (mWFCRParameters.mUnitsOfBackup.ContainsKey(origAssoId))
                {
                    wfInfo = mWFCRParameters.mUnitsOfBackup[origAssoId];
                    assoUnit = SPWFAssociationUnit.Load(wfInfo.AssociationUnit);
                }

                IAveWorkflowAssociation asso = null;
                Common.ArgumentCheck.CheckNotNull(wfInfo);
                if (string.IsNullOrEmpty(wfInfo?.CTId))
                {
                    Common.ArgumentCheck.CheckNotNull(assoUnit);
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
#if PerformanceLog
            }
#endif
        }

        #endregion

    }

    internal class WFConflictResolutionInternalForProject : WFConflictResolutionInternalFor13Model
    {
        public WFConflictResolutionInternalForProject(WFConflictResolutionParameters parameters)
            : base(parameters)
        {

        }

        public override void HandleAssociationConflictInternal(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent,AssociationHelper helper)
        {
            Guid originalSubscriptionId = assoUnit.SerializableData.mId;
            log.Log(AveLogLevel.INFO, "Begin to restore workflow association for Project Workflow internally, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
            WorkflowServiceFactory workflowSvcFacory = InitWorkflowServiceFactory(parent.ParentWeb);
            var workflowSubscriptionCollection = workflowSvcFacory.WFSubscriptionService.EnumerateSubscriptionsByEventSource(AveProjectConstants.ProjectWorkflow_EventSourceId);
            List<IAveWorkflowSubscription> workflowSubscriptions = FindWorkflowSubscription(assoUnit.SerializableData.mOriginalName, workflowSubscriptionCollection);
            if (workflowSubscriptions == null || workflowSubscriptions.Count == 0)
            {
                mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, ParentWeb);
            }
            else
            {
                if (mWFCRParameters.AssociationOption != WFAssociationConflictResolutionOption.ForceUse && !needAssociationConflictResolution)
                {
                    throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, WrapperWorkflowResource.WFDeConflictError);
                }
                AssociationConflictResolution(assoUnit, parent, workflowSubscriptions[0], workflowSvcFacory);
            }
            if (assoUnit != null &&
               assoUnit.SerializableData.mId != Guid.Empty &&
               helper != null &&
               helper.SiteMapping != null)
            {
                helper.SiteMapping.SiteMappingManager.AddWorkflowIdMapping(originalSubscriptionId, assoUnit.SerializableData.mId);
            }
        }

        private void AssociationConflictResolution(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, IAveWorkflowSubscription subscription, WorkflowServiceFactory workflowSvcFacory)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.AssociationConflictResolution.ProjectWorkflowAssociation"))
            {
#endif
                switch (this.mWFCRParameters.AssociationOption)
                {
                    case WFAssociationConflictResolutionOption.Append:
                        //Rename workflow 
                        string oldName = assoUnit.SerializableData.mName;
                        var subscriptionCollection = workflowSvcFacory.WFSubscriptionService.EnumerateSubscriptionsByEventSource(AveProjectConstants.ProjectWorkflow_EventSourceId);
                        string newName = RenameAssociation(oldName, subscriptionCollection);
                        assoUnit.SerializableData.mName = newName;
                        assoUnit.SerializableData.mOriginalName = newName;

                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentWeb);
                        break;
                    case WFAssociationConflictResolutionOption.Overwrite:
                    case WFAssociationConflictResolutionOption.ForceOverwrite:
                        Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { parent.ParentWeb }).WFSubscriptionService.DeleteSubscription(subscription.Id);
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentWeb);
                        break;
                    case WFAssociationConflictResolutionOption.UpdateOverwrite:
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentWeb);
                        break;
                    case WFAssociationConflictResolutionOption.ForceUse:
                        this.mWFCRParameters.workflowRestoreCore.ForceUpdate = false;
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentWeb);
                        break;
                    case WFAssociationConflictResolutionOption.NotOverwrite:
                        assoUnit.SerializableData.mId = subscription.Id;
                        log.Log(AveLogLevel.WARN, "RestoreWorkflowDefinitionSkip.");
                        break;
                    default:
                        throw new AveWrapperException(AveWrapperErrorCode.DefinitionConflictResolutionOptionInvalid, "Association conflict resolution invalid.");
                }
#if PerformanceLog
            }
#endif
        }

        public override IAveWeb ParentWeb
        {
            get
            {
                return (IAveWeb)mWFCRParameters.workflowRestoreCore.WFAssociationProcessorProject.ParentObject;
            }
        }
    }

    /// <summary>
    /// Create Instance by WFConflictResolutionInternal
    /// </summary>
    internal class WFConflictResolutionInternalFor13Model : WFConflictResolutionInternal
    {
        public WFConflictResolutionInternalFor13Model(WFConflictResolutionParameters parameters)
            : base(parameters)
        {

        }
        internal override void RestoreTemplateDataInternal(SPWFAssociationUnit assoUnit, WFTemplateConflictResolutionOption option)
        {
            SPWFAssociationProc proc = SPWFAssociationProc.CreateInstance(SPWFProcessorType.API13Model);
            WorkflowServiceFactory workflowSvcFacory = InitWorkflowServiceFactory(assoUnit.ParentWeb);
            IAveWorkflowDefinition definition;
            switch (option)
            {
                case WFTemplateConflictResolutionOption.NotOverwrite:
                    {
                        definition = FindWorkflowDefinition(assoUnit, workflowSvcFacory.WFDeploymentService);
                        if (definition.Id == Guid.Empty)
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
        protected IAveWorkflowDefinition FindWorkflowDefinition(SPWFAssociationUnit assoUnit, IAveWorkflowDeploymentService deploymentService)
        {
            if (deploymentService == null)
            {
                throw new ArgumentNullException("deploymentService");
            }
            IAveWorkflowDefinition wfDefinition = deploymentService.GetDefinition(assoUnit.SerializableData.mBaseId);
            if (wfDefinition.Id == Guid.Empty)
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
        public IAveWorkflowSubscriptionService WFSubscriptionService
        {
            get
            {
                return Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).WFSubscriptionService;
            }
        }

        public virtual IAveWeb ParentWeb
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

        #region [******Handle WorkflowAssociation******]

        public override void HandleAssociationConflictInternal(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent,AssociationHelper helper)
        {
            Guid originalSubscriptionId = assoUnit.SerializableData.mId;
            log.Log(AveLogLevel.INFO, "Begin to restore workflow association for 13 model internally, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
            WorkflowServiceFactory workflowSvcFacory = InitWorkflowServiceFactory(parent.ParentWeb);
            List<IAveWorkflowSubscription> workflowSubscriptions = FindWorkflowSubscription(assoUnit.SerializableData.mOriginalName, parent.WorkflowSubscriptionCollection);
            if (workflowSubscriptions == null || workflowSubscriptions.Count == 0)
            {
                var formUnits = assoUnit.SerializableData.mFormFileUnit;
                if (formUnits != null && formUnits.Count > 0)
                //if (assoUnit.SerializableData.mFormFileUnit.Count > 0)
                {
                    mWFCRParameters.workflowRestoreCore.SetWorkflowParentTypeAndObject(assoUnit.WFInternalPlatform, parent.ParentObject);
                    ReplaceNintexFormFileContent(formUnits, ParentWeb);
                }

                mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
            }
            else
            {
                if (mWFCRParameters.AssociationOption != WFAssociationConflictResolutionOption.ForceUse && !needAssociationConflictResolution)
                {
                    throw new AveWrapperSkipException(AveWrapperErrorCode.WorkflowDefinitionConflict, WrapperWorkflowResource.WFDeConflictError);
                }
                AssociationConflictResolution(assoUnit, parent, workflowSubscriptions[0]);
            }
            if (assoUnit != null &&
               assoUnit.SerializableData.mId != Guid.Empty &&
               helper != null &&
               helper.SiteMapping != null)
            {
                helper.SiteMapping.SiteMappingManager.AddWorkflowIdMapping(originalSubscriptionId, assoUnit.SerializableData.mId);
            }
        }
        private void ReplaceNintexFormFileContent(List<SPWorkflowSubFileSerializableData> formFiles, IAveWeb parentWeb)
        {
            foreach (var formFile in formFiles)
            {
                var fileContent = Encoding.UTF8.GetString(formFile.mContent);
                var parentSPWeb = new AveSPWeb(WFConflictResolution.ParentSite, null, parentWeb.ServerRelativeUrl);
                var contentProcessor = new NintexFormContentProcessor(parentSPWeb, (IAveList)this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.ParentObject);
                var result = contentProcessor.ReplaceFormContent(fileContent, string.Empty, true);
                formFile.mContent = Encoding.UTF8.GetBytes(result);
            }

        }

        protected virtual List<IAveWorkflowSubscription> FindWorkflowSubscription(string assoName, IAveWorkflowSubscriptionCollection subscriptionCollection)
        {
            return subscriptionCollection.Where(subscrip => subscrip.Name.Equals(assoName)).ToList<IAveWorkflowSubscription>();
        }

        protected WorkflowServiceFactory InitWorkflowServiceFactory(IAveWeb web)
        {
            WorkflowServiceFactory workflowServiceFactory = Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { web });
            workflowServiceFactory.UpdateWorkflowServiceManager(web);
            return workflowServiceFactory;
        }

        private void AssociationConflictResolution(SPWFAssociationUnit assoUnit, WorkflowAssociationParentObject parent, IAveWorkflowSubscription subscription)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.AssociationConflictResolution"))
            {
#endif
                switch (this.mWFCRParameters.AssociationOption)
                {
                    case WFAssociationConflictResolutionOption.Append:
                        //Rename workflow 
                        string oldName = assoUnit.SerializableData.mName;
                        string newName = RenameAssociation(oldName, parent.WorkflowSubscriptionCollection);
                        assoUnit.SerializableData.mName = newName;
                        assoUnit.SerializableData.mOriginalName = newName;

                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        break;
                    case WFAssociationConflictResolutionOption.Overwrite:
                    case WFAssociationConflictResolutionOption.ForceOverwrite:
                        Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { parent.ParentWeb }).WFSubscriptionService.DeleteSubscription(subscription.Id);
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        break;
                    case WFAssociationConflictResolutionOption.UpdateOverwrite:
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        break;
                    case WFAssociationConflictResolutionOption.ForceUse:
                        this.mWFCRParameters.workflowRestoreCore.ForceUpdate = false;
                        this.mWFCRParameters.workflowRestoreCore.RestoreWorkflowAssociation(assoUnit, parent.ParentObject);
                        break;
                    case WFAssociationConflictResolutionOption.NotOverwrite:
                        assoUnit.SerializableData.mId = subscription.Id;
                        log.Log(AveLogLevel.WARN, "RestoreWorkflowDefinitionSkip.");
                        break;
                    default:
                        throw new AveWrapperException(AveWrapperErrorCode.DefinitionConflictResolutionOptionInvalid, "Association conflict resolution invalid.");
                }
#if PerformanceLog
            }
#endif
        }

        protected virtual string RenameAssociation(string oldName, IAveWorkflowSubscriptionCollection subscriptionCollection)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.RenameAssociation"))
            {
#endif
                string newName = oldName;
                int counter = 1;
                while (true)
                {
                    newName = oldName + AppendSuffix + counter;

                    List<IAveWorkflowSubscription> subscriptions = FindWorkflowSubscription(newName, subscriptionCollection);
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
#if PerformanceLog
            }
#endif
        }

        public override bool UpdateWorkflowStartOptions(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation desWorkflowAssociation = null)
        {
            if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored.ContainsKey(assoUnit.SerializableData.mId))
            {
                SPWFAssociationUnit desAssoUnit = this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored[assoUnit.SerializableData.mId];
                IAveWorkflowSubscription definitionSubscription = desAssoUnit.WorkflowSubscription;
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
        #endregion

        #region [******Handle WorkflowInstance******]
        public override void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveWeb web)
        {
        }

        public override void HandleInstanceConflict(SPWFInstanceUnit instanceUnit, IAveListItem item)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.HandleInstanceConflict"))
            {
#endif
                if (!SPWorkflowProcessorRuntime.ProcessInstance)
                {
                    return;
                }

                if (instanceUnit == null || item == null)
                {
                    throw new AveException("Invalid argument, argument can not be null.");
                }
                string parentAssociationName = null;
                try
                {
                    Guid parentAssoId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("#TemplateId");
                    if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(parentAssoId))
                    {
                        if (SPWorkflowProcessorRuntime.RestoreBuiltinOnly && !SPWFAssociationUnit.Load(this.mWFCRParameters.mUnitsOfBackup[parentAssoId].AssociationUnit).IsBuiltinBaseId)
                        {
                            return;
                        }
                    }
                    parentAssociationName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                    //Guid parentAssoBaseId = (Guid)instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationBaseId");
                    if (!TryFindParentAssociation(parentAssoId, item, instanceUnit))
                    {
                        throw new WorkflowDefinitionNotFoundException(parentAssociationName, item.Name, item.ParentList.Title, item.ParentList.ParentWeb.Url);
                    }
                    else
                    {
                        instanceUnit.ParentAssociationUnit = this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored[parentAssoId];
                    }
                    #region deal with InstanceConflict
                    log.Info("WorkflowInstance Conflict", " Item Title is " + item.Title + " Item Name is " + item.Name + " InstanceState is " + instanceUnit.InstanceItem.Properties["Status"].ToString());
                    if ((AveWorkflowStatus13Model)instanceUnit.InstanceItem.Properties["Status"] == AveWorkflowStatus13Model.Started)
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
                    this.mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(parentAssociationName, item.Title, AveReportObjectType.WorkflowInstance, AveStatus.Successful, string.Empty));

                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreWorkflowInstanceFailedEventMessage(parentAssociationName, ex));
                    this.mWFCRParameters.reportor.AddDetail(new AveWrapperReportDto(parentAssociationName, item.Title, AveReportObjectType.WorkflowInstance, AveStatus.Skipped, ex.Message));
                }
#if PerformanceLog
            }
#endif
        }

        private bool TryFindParentAssociation(Guid workflowSubscriptionId, IAveListItem item, SPWFInstanceUnit instanceUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryFindParentAssociation"))
            {
#endif
                try
                {
                    //如果发现parentAssociation已经Cache则Instance直接Cache.
                    if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.NeedPostActionAssociations.Contains(workflowSubscriptionId))
                    {
                        try
                        {
                            SPWorkflowProcessorRuntime.OnCacheData(item.ParentList.ParentWeb.Site.ID.ToString(), item.ParentList.ParentWeb.ID.ToString(), item.ParentList.ID.ToString(), item.ParentList.ID.ToString(), item.ID, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                        }
                        return false;
                    }
                    //修改逻辑后UnitsOfRestored集合中应该只有一个元素,就是最后还原的Association
                    if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestored.ContainsKey(workflowSubscriptionId))
                    {
                        return true;
                    }

                    //之前已经还原过这个workflow association
                    string origName = instanceUnit.InstanceItem.Properties.GetEx("LS.ParentAssociationName").ToString();
                    if (this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.UnitsOfRestoredNameMapping.ContainsKey(origName.ToLower()))
                    {
                        SPWFAssociationUnit assoUnit = null;
                        IAveWorkflowSubscription workflowSubscription = GetParentWorkflowSubscription(item, this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor.UnitsOfRestoredNameMapping[origName.ToLower()], workflowSubscriptionId, out assoUnit);
                        if (workflowSubscription != null)
                        {
                            this.mWFCRParameters.workflowRestoreCore.WFAssociationProcessor13Model.SetRestoredUnit(assoUnit, workflowSubscription);
                            return true;
                        }
                    }


                    //之前没有还原过这个workflow association，那么需要拿到备份数据，重新还原
                    if (this.mWFCRParameters.mUnitsOfBackup.ContainsKey(workflowSubscriptionId))
                    {
                        AveWorkflowInfo cacheData = this.mWFCRParameters.mUnitsOfBackup[workflowSubscriptionId];
                        if (string.IsNullOrEmpty(cacheData.CTId))
                        {
                            mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ParentList, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                        }
                        else
                        {
                            mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ContentType, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                        }

                        WFAssociationConflictResolutionOption temp = mWFCRParameters.AssociationOption;
                        mWFCRParameters.AssociationOption = WFAssociationConflictResolutionOption.ForceUse;
                        try
                        {
                            List<IAveWorkflowSubscription> workflowSubscriptions = FindWorkflowSubscription(origName, mWFCRParameters.mAssociationParentObject.WorkflowSubscriptionCollection);
                            if ((workflowSubscriptions == null || workflowSubscriptions.Count == 0) && !SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound)
                                return false;
                            RestoreAssociationDataInternal(cacheData, SPWFAssociationUnit.Load(cacheData.AssociationUnit),null, false);
                        }
                        catch (SPWFProcessorException procException)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
                            try
                            {
                                if (procException.ErrorCode == 9999)
                                {
                                    SPWorkflowProcessorRuntime.OnCacheData(item.ParentList.ParentWeb.Site.ID.ToString(), item.ParentList.ParentWeb.ID.ToString(), item.ParentList.ID.ToString(), item.ParentList.ID.ToString(), item.ID, instanceUnit.InstanceItem.Properties["#Id"].ToString(), SPWFInstanceUnit.Save(instanceUnit));
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.DEBUG, "An error occurred while on cache workflow instance.", ex);
                            }
                            return false;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.RestoreAssociationDataInternalError, e);
                        }
                        finally
                        {
                            mWFCRParameters.AssociationOption = temp;
                        }
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindParentAssociationFailed, e);
                    return false;
                }
#if PerformanceLog
            }
#endif
        }

        private IAveWorkflowSubscription GetParentWorkflowSubscription(IAveListItem item, string parentAssoName, Guid workflowSubscriptionId, out SPWFAssociationUnit assoUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.GetParentAssociation"))
            {
#endif
                assoUnit = null;
                AveWorkflowInfo wfInfo = null;
                if (mWFCRParameters.mUnitsOfBackup.ContainsKey(workflowSubscriptionId))
                {
                    wfInfo = mWFCRParameters.mUnitsOfBackup[workflowSubscriptionId];
                    assoUnit = SPWFAssociationUnit.Load(wfInfo.AssociationUnit);
                }

                IAveWorkflowSubscription asso = null;
                Common.ArgumentCheck.CheckNotNull(wfInfo);
                if (string.IsNullOrEmpty(wfInfo?.CTId))
                {
                    Common.ArgumentCheck.CheckNotNull(assoUnit);
                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ParentList, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                    assoUnit.ParentObjectType = SPWFAssociationParentType.List;
                    assoUnit.ParentObject = item.ParentList;

                }
                else
                {
                    Common.ArgumentCheck.CheckNotNull(assoUnit);
                    mWFCRParameters.mAssociationParentObject = SetAssociationParentObject(item.ContentType, ref mWFCRParameters.workflowRestoreCore, mWFCRParameters.WebContentTypeAssociation);
                    assoUnit.ParentObjectType = SPWFAssociationParentType.ListContentType;
                    assoUnit.ParentObject = item.ContentType;
                }

                asso = mWFCRParameters.mAssociationParentObject.WorkflowSubscriptionCollection.GetSubscriptionByName(parentAssoName);
                return asso;
#if PerformanceLog
            }
#endif
        }

        private IAveWorkflowInstance TryGetRunningInstance(IAveListItem item, SPWFInstanceUnit instanceUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetRunningInstance"))
            {
#endif
                try
                {

                    IAveWorkflowInstanceCollection wfColl = Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.ParentList.ParentWeb }).WFInstanceService.EnumerateInstancesForListItem(item.ParentList.ID, item.ID);
                    foreach (var wf in wfColl)
                    {
                        if (wf.WorkflowSubscriptionId == instanceUnit.ParentAssociationUnit.Id && wf.Status == AveWorkflowStatus13Model.Started)
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
#if PerformanceLog
            }
#endif
        }

        private IAveWorkflowInstance TryGetAllInstance(IAveListItem item, SPWFInstanceUnit instanceUnit)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.TryGetAllInstance"))
            {
#endif
                try
                {
                    IAveWorkflowInstanceCollection wfColl = Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { item.ParentList.ParentWeb }).WFInstanceService.EnumerateInstancesForListItem(item.ParentList.ID, item.ID);
                    foreach (var wf in wfColl)
                    {
                        if ((DateTime)instanceUnit.InstanceItem.Properties["InstanceCreated"] == wf.InstanceCreated && wf.WorkflowSubscriptionId == instanceUnit.ParentAssociationUnit.Id)
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
#if PerformanceLog
            }
#endif
        }

        private void InstanceConflictResolution(SPWFInstanceUnit assoUnit, IAveListItem item, IAveWorkflowInstance workflow)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WFConflictResolution.InstanceConflictResolution"))
            {
#endif
                switch (this.mWFCRParameters.InstanceOption)
                {
                    case WFInstanceConflictResolutionOption.Overwrite:
                        //can not remove
                        //item.ParentList.ParentWeb.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow); 

                        this.mWFCRParameters.workflowRestoreCore.RestoreInstance(assoUnit, item);
                        break;
                    case WFInstanceConflictResolutionOption.NotOverwrite:
                        break;
                    case WFInstanceConflictResolutionOption.OverwriteByModifiedTime:
                        // List<IAveWorkflow> wfs = item.ParentList.ParentWeb.Site.WorkflowManager.GetItemWorkflows(item, assoUnit.ParentAssociationUnit.Id);
                        // foreach (IAveWorkflow wf in wfs)
                        //{
                        //DateTime UtcCreatedTime = wf.Created.ToUniversalTime();
                        //DateTime UtcModifiedtime = wf.Modified.ToUniversalTime();
                        // if ((DateTime)assoUnit.InstanceItem.Properties["#Created"] == wf.Created)
                        //{
                        if ((DateTime)assoUnit.InstanceItem.Properties["LastUpdated"] > workflow.LastUpdated)
                        {
                            //item.ParentList.ParentWeb.Site.WorkflowManager.RemoveWorkflowFromListItem(workflow);
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
#if PerformanceLog
            }
#endif
        }
        #endregion
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
                            mWorkflowAssociations = mList.WorkflowAssociations;
                            break;
                        case SPWFAssociationParentType.Web:
                            mWorkflowAssociations = mWeb.WorkflowAssociations;
                            break;
                        case SPWFAssociationParentType.ListContentType:
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
        public WFAssociationConflictResolutionOption AssociationOption;
        public WFInstanceConflictResolutionOption InstanceOption;
        public WorkflowAssociationParentObject mAssociationParentObject = null;
        public bool WebContentTypeAssociation;
    }

}