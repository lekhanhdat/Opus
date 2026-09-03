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
namespace LS.SPWorkflowProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using AvePoint.Wrapper.Common;
    using System.Globalization;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using System.Diagnostics.CodeAnalysis;
    using AvePoint.Common;
    using System.Linq;
    using System.IO;
    using Native13NinTexWorkflowEntity;
    using System.Xml.Linq;
    using System.Net;
    using AvePoint.Wrapper.Restore;
    using AvePoint.GCommon.Utility;
    using WorkflowConfiguration = AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration;
    using SerializableObjects;
    using AvePoint.Wrapper.Resource.Workflow;

    public class SPWFAssociationProc : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string BesideAssmProfix = ", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        protected SPWFAssociationParentType mObjectType = SPWFAssociationParentType.Invalid;
        private WFAveSPObjectCache mAveSPObjectCache = new WFAveSPObjectCache();
        public SPWFAssociationParentType ParentObjectType
        {
            get { return mObjectType; }
            set { mObjectType = value; }
        }
        protected object mParentObject;
        public object ParentObject
        {
            get { return mParentObject; }
            set { mParentObject = value; }
        }
        public WFAveSPObjectCache AveSPObjectCache
        {
            get { return mAveSPObjectCache; }
            set { mAveSPObjectCache = value; }
        }


        protected SPWFProcessorType mProcType;
        protected Dictionary<Guid, SPWorkflowSubListUnit> mCachedSubListUnits;
        public Dictionary<Guid, SPWorkflowSubListUnit> CachedSubListUnits
        {
            get { return mCachedSubListUnits; }
        }
        protected LSPerformanceMonitor mPerformanceMonitor;
        protected string mMainMonitorLog = string.Empty;
        public IAveBackupRestoreQueryService QueryService { get; set; }
        private Dictionary<Guid, SPFieldProcessor> mWebLevelFieldProcessorCollection;
        public Dictionary<Guid, SPFieldProcessor> WebLevelFieldProcessorCollection
        {
            get
            {
                if (mWebLevelFieldProcessorCollection == null)
                    mWebLevelFieldProcessorCollection = new Dictionary<Guid, SPFieldProcessor>();
                return mWebLevelFieldProcessorCollection;
            }
            set { mWebLevelFieldProcessorCollection = value; }
        }

        private List<ICustomWorkflowAssociationProc> mCustomProcs;
        public List<ICustomWorkflowAssociationProc> CustomProcessors
        {
            get
            {
                if (mCustomProcs == null)
                    mCustomProcs = new List<ICustomWorkflowAssociationProc>();
                return mCustomProcs;
            }
            set
            {
                mCustomProcs = value;
            }
        }

        protected Dictionary<Guid, byte[]> mUnitsOfBackup;
        protected Dictionary<Guid, byte[]> UnitsOfBackup
        {
            get
            {
                if (mUnitsOfBackup == null)
                    mUnitsOfBackup = new Dictionary<Guid, byte[]>();
                return mUnitsOfBackup;
            }
        }

        protected Dictionary<Guid, SPWFAssociationUnit> mUnitsOfRestored;
        public Dictionary<Guid, SPWFAssociationUnit> UnitsOfRestored
        {
            get { return mUnitsOfRestored; }
        }


        public Dictionary<string, string> UnitsOfRestoredNameMapping { get; protected set; }
        public List<Guid> NoRestoredWFCache = new List<Guid>();
        protected List<Guid> mNeedPostActionAssociations;
        public List<Guid> NeedPostActionAssociations
        {
            get
            {
                if (mNeedPostActionAssociations == null)
                {
                    mNeedPostActionAssociations = new List<Guid>();
                }
                return mNeedPostActionAssociations;
            }
        }

        protected List<SPWFProcessorException> mInnerExceptions;
        protected Dictionary<Guid, List<SPWFProcessorException>> mExceptions;
        public Dictionary<Guid, List<SPWFProcessorException>> Exceptions
        {
            get { return mExceptions; }
        }

        protected List<SPWFProcessorException> mInnerWarnings;
        public List<SPWFProcessorException> InnerWarnings
        {
            get
            {
                if (mInnerWarnings == null)
                    mInnerWarnings = new List<SPWFProcessorException>();
                return mInnerWarnings;
            }
        }

        public SPWFAssociationProc()
        {
            mCachedSubListUnits = new Dictionary<Guid, SPWorkflowSubListUnit>();
            mExceptions = new Dictionary<Guid, List<SPWFProcessorException>>();
            mUnitsOfBackup = new Dictionary<Guid, byte[]>();
            mUnitsOfRestored = new Dictionary<Guid, SPWFAssociationUnit>();
            UnitsOfRestoredNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            mInnerWarnings = new List<SPWFProcessorException>();
            mPerformanceMonitor = new LSPerformanceMonitor();
        }

        public void Dispose()
        {
            mCachedSubListUnits.Clear();

            foreach (KeyValuePair<Guid, List<SPWFProcessorException>> pair in mExceptions)
            {
                pair.Value.Clear();
            }
            mExceptions.Clear();
            mUnitsOfBackup.Clear();
            UnitsOfRestoredNameMapping.Clear();
            if (mPerformanceMonitor != null)
                mPerformanceMonitor.Dispose();
            var contextKind = SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind;
            if (contextKind.IsServerMode())
            {
                mUnitsOfRestored.Clear();
            }
            else
            {
                var collect = mUnitsOfRestored.Where(unit =>
                {
                    bool autoStartChange = (((WorkflowConfiguration)unit.Value.SerializableData.mConfiguration & WorkflowConfiguration.AutoStartChange) != WorkflowConfiguration.None);
                    bool autoStartCreate = (((WorkflowConfiguration)unit.Value.SerializableData.mConfiguration & WorkflowConfiguration.AutoStartAdd) != WorkflowConfiguration.None);
                    return autoStartChange || autoStartCreate;
                }).ToDictionary(item => item.Key, item => item.Value);
                mUnitsOfRestored.Clear();
                mUnitsOfRestored = collect;
            }
        }

        public event RestoreWFDefinitionEventHandler RestoreWFDefinitionEvent;
        public void OnRestoreWFAssociation(object sender, RestoreWFDefinitionEventArgs e)
        {
            RestoreWFDefinitionEvent(sender, e);
        }

        protected bool IsOnlineNintexWorkflow<T>(IDictionary<string, T> workflowProperties)
        {
            const string nintexWorkflowPropertyKey = "NWConfig.Designer";
            if (workflowProperties != null)
            {
                return workflowProperties.ContainsKey(nintexWorkflowPropertyKey);
            }
            return false;
        }

        /// <summary>
        /// Create a custom workflow association processor
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="procType"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static SPWFAssociationProc CreateInstance(SPWFProcessorType procType)
        {
            SPWFAssociationProc proc = null;
            switch (procType)
            {
                case SPWFProcessorType.API:
                    proc = new SPWFAssociationProcAPI();
                    break;
                case SPWFProcessorType.API13Model:
                    proc = new SPWFAssociationProc13ModelAPI();
                    break;
                case SPWFProcessorType.Native:
                default:
                    proc = new SPWFAssociationProc();
                    //proc = new SPWFAssociationProcNative();
                    break;
            }

            proc.mProcType = procType;
            return proc;
        }

        public virtual List<byte[]> Backup()
        {
            return new List<byte[]>();
        }

        public virtual SPWFAssociationUnit BackupOneAssociation(string monitor, IAveWorkflowAssociation asso)
        {
            return new SPWFAssociationUnit();
        }

        public virtual List<byte[]> BackupWFReusableTemplates()
        {
            return new List<byte[]>();
        }

        public virtual int Restore(SPWFAssociationUnit unit)
        {
            return 0;
        }

        public virtual int Restore(SPWFAssociationUnit unit, bool forceUpdate, bool isPostAction)
        {
            return 0;
        }

        public virtual int Restore(byte[] serializedData)
        {
            return 0;
        }

        public virtual int Restore(byte[] serializedData, bool forceUpdate)
        {
            return 0;
        }

        public virtual void RestoreReusableWFTemplate(SPWFAssociationUnit unit)
        {
        }

        public void SetCustomProc(List<ICustomWorkflowAssociationProc> customProcessors)
        {
            CustomProcessors = customProcessors;
        }

        protected virtual void AddAssociationToRestoredCollection(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assoUnit">SPWFAssociationUnit</param>
        /// <param name="asso">WorkflowAssociation(SP2010 plantfrom type) or WorkflowSubscription(13Model plantform type)</param>
        public virtual void SetRestoredUnit(SPWFAssociationUnit assoUnit, object asso)
        {
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalName">SPDWorkflowDemo <Xoml.4de94745_b540_42f1_a2d7_ed8729d36a59.2.512.-1.0.dll> <Cfg.4de94745_b540_42f1_a2d7_ed8729d36a59.3.512.></param>
        /// <returns>Xoml.4de94745_b540_42f1_a2d7_ed8729d36a59.2.512.-1.0</returns>
        protected string GetAssmShortNameFromInternalName(string internalName)
        {
            string assmName = null;
            string[] splitedName = internalName.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (splitedName.Length > 1)
            {
                string xomlName = splitedName[1].ToLower(CultureInfo.CurrentCulture);
                if (xomlName.StartsWith("<xoml.", StringComparison.OrdinalIgnoreCase) && xomlName.EndsWith(".dll>", StringComparison.OrdinalIgnoreCase))
                {
                    assmName = splitedName[1].Substring(1, splitedName[1].Length - 6);
                }
            }
            return assmName;
        }

        protected string GetAssmFullNameFromInternalName(string internalName)
        {
            return GetAssmShortNameFromInternalName(internalName) + BesideAssmProfix;
        }

        private void HandleIssueTrackingFieldNode(IAveFieldCollection fields, XmlNode node, bool get, string key, Dictionary<string, AveSPField> aveFields)
        {
            if (node != null)
            {
                if (get)
                {
                    Guid fieldId = new Guid(node.InnerText);
                    object fieldObj = fields.GetFieldById(fieldId, false);// LSInvoker.CallMethod(fields, "GetFieldById", new Type[] { typeof(Guid), typeof(bool) }, new object[] { fieldId, false });
                    if (fieldObj != null)
                    {
                        AveSPField aveField = new AveSPField(((IAveField)fieldObj).SchemaXml);
                        aveFields.Add(key, aveField);
                    }
                }
                else
                {
                    if (aveFields.ContainsKey(key))
                    {
                        AveSPField aveField = aveFields[key];
                        CreateIssueTrackingField(fields, aveField);
                        if (aveField.SPFieldInternal != null)
                            node.InnerText = aveField.SPFieldInternal.ID.ToString("B").ToUpper(CultureInfo.CurrentCulture);
                    }
                }
            }
        }

        protected void GetOrSetIssueTrackingFields(SPWFAssociationUnit assoUnit, bool get)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetOrSetIssueTrackingFields");

            XmlDocument doc = null;
            try
            {
                if (string.IsNullOrEmpty(assoUnit.SerializableData.mInstantiationParams))
                    return;
                doc = new XmlDocument();
                doc.LoadXml(assoUnit.SerializableData.mInstantiationParams);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("my", doc.DocumentElement.GetNamespaceOfPrefix("my"));
                XmlNode choiceFieldNode = doc.SelectSingleNode("/my:myFields/my:StatusField", nsmgr);

                XmlNode customMessageFieldNode = doc.SelectSingleNode("/my:myFields/my:CustomMessageField", nsmgr);
                XmlNode customMessageBodyFieldNode = doc.SelectSingleNode("/my:myFields/my:CustomMessageBodyField", nsmgr);
                XmlNode dueDateFieldNode = doc.SelectSingleNode("/my:myFields/my:DueDateField", nsmgr);
                XmlNode assignedToFieldNode = doc.SelectSingleNode("/my:myFields/my:AssignedToField", nsmgr);

                XmlNode customMessageField2Node = doc.SelectSingleNode("/my:myFields/my:CustomMessageField2", nsmgr);
                XmlNode customMessageBodyField2Node = doc.SelectSingleNode("/my:myFields/my:CustomMessageBodyField2", nsmgr);
                XmlNode dueDateField2Node = doc.SelectSingleNode("/my:myFields/my:DueDateField2", nsmgr);
                XmlNode assignedToField2Node = doc.SelectSingleNode("/my:myFields/my:AssignedToField2", nsmgr);


                IAveWeb web = assoUnit.ParentWeb;
                {
                    IAveList list = null;

                    #region Get Parent List
                    switch (assoUnit.ParentObjectType)
                    {
                        case SPWFAssociationParentType.ListContentType:
                            list = assoUnit.ParentContentType.ParentList;
                            break;
                        case SPWFAssociationParentType.List:
                            list = assoUnit.ParentList;
                            break;
                        default:
                            break;
                    }
                    #endregion

                    if (list != null)
                    {
                        HandleIssueTrackingFieldNode(list.Fields, choiceFieldNode, get, "IT_ChoiceField", assoUnit.IssueTrackingRefFields);

                        HandleIssueTrackingFieldNode(list.Fields, customMessageFieldNode, get, "IT_CustomMessageField", assoUnit.IssueTrackingRefFields);
                        HandleIssueTrackingFieldNode(list.Fields, customMessageBodyFieldNode, get, "IT_CustomMessageBodyField", assoUnit.IssueTrackingRefFields);
                        HandleIssueTrackingFieldNode(list.Fields, dueDateFieldNode, get, "IT_DueDateField", assoUnit.IssueTrackingRefFields);
                        HandleIssueTrackingFieldNode(list.Fields, assignedToFieldNode, get, "IT_AssignedToField", assoUnit.IssueTrackingRefFields);

                        HandleIssueTrackingFieldNode(list.Fields, customMessageField2Node, get, "IT_CustomMessageField2", assoUnit.IssueTrackingRefFields);
                        HandleIssueTrackingFieldNode(list.Fields, customMessageBodyField2Node, get, "IT_CustomMessageBodyField2", assoUnit.IssueTrackingRefFields);
                        HandleIssueTrackingFieldNode(list.Fields, dueDateField2Node, get, "IT_DueDateField2", assoUnit.IssueTrackingRefFields);
                        HandleIssueTrackingFieldNode(list.Fields, assignedToField2Node, get, "IT_AssignedToField2", assoUnit.IssueTrackingRefFields);
                    }
                }
                if (!get)
                    assoUnit.SerializableData.mInstantiationParams = doc.OuterXml;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_SetIssueTrackingFieldException, e.Message);
                logger.Warn("An exception occurred while set issue tracking field. exception:{0}", e);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetOrSetIssueTrackingFields");
            }
        }

        private void CreateIssueTrackingField(IAveFieldCollection spFields, AveSPField aveField)
        {
            try
            {
                object fieldObj = spFields.GetFieldById(aveField.SerializableData.mSrcId, false); //LSInvoker.CallMethod(spFields, "GetFieldById", new Type[] { typeof(Guid), typeof(bool) }, new object[] { aveField.SerializableData.mSrcId, false });
                if (fieldObj == null)
                    fieldObj = spFields.GetFieldByInternalName(aveField.SerializableData.mSrcInternalName, false);// LSInvoker.CallMethod(spFields, "GetFieldByInternalName", new Type[] { typeof(string), typeof(bool) }, new object[] { aveField.SerializableData.mSrcInternalName, false });
                if (fieldObj == null)
                {
                    AveSPFieldCollection fields = new AveSPFieldCollection();
                    fields.CurrentFieldCollection = spFields;
                    fieldObj = fields.CreateSPField(aveField.SerializableData.mSrcSchemaXMLString, true, AveAddFieldOptions.DefaultValue);
                }
                if (fieldObj != null)
                    aveField.SetAveFieldBySPField((IAveField)fieldObj);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, "An error occurred while creating issue tracking field in workflow association project, error message: {0}", e);
                mInnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.CreateStatusFieldException, e));
            }
            finally
            {
            }
        }

        internal static string GetStatusFieldSchema(IAveWorkflowAssociation asso, string internalName, bool skipReference, IAveBackupRestoreQueryService queryService)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetStatusFieldSchema");
            SPWorkflowProcessorRuntime.Log(Logs.AP_StatusFieldName, internalName);
            string schema = string.Empty;
            try
            {
                schema = SPWFAssociationProcNative.GetStatusFieldSchema(asso, internalName, skipReference, queryService);
                if (string.IsNullOrEmpty(schema))
                {
                    schema = asso.ParentList.Fields.GetFieldByInternalName(internalName).SchemaXml;
                }
                SPWorkflowProcessorRuntime.Log(Logs.AP_StatusFieldName, schema);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_GetStatusFieldSchemaException, e.Message);
                logger.Warn("An exception occurred while get status field schema. exception:{0}", e);
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetStatusFieldSchema");
            return schema;
        }

        public Func<AveWorkflowAssociationInfo, bool> FilterWorkflowFunction { get; set; }

        public Func<AveReusableWorkflowTemplateInfo, bool> FilterReusableWorkflowTemplateFunction { get; set; }

        protected void ParseExceptionToCacheData(SPWFProcessorException procException, SPWFAssociationUnit assoUnit, byte[] cacheData)
        {
            try
            {
                if (procException.ErrorCode == SPWFProcessorErrorCode.PutIntoPostAction)
                {
                    switch (assoUnit.ParentObjectType)
                    {
                        case SPWFAssociationParentType.Web:
                            logger.Log(AveLogLevel.DEBUG, "An processor error occurred while restoring workflow association unit, level: web, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
                            SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.Url, assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), string.Empty, string.Empty, 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                            break;
                        case SPWFAssociationParentType.List:
                            logger.Log(AveLogLevel.DEBUG, "An processor error occurred while restoring workflow association unit, level: list, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
                            SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.Url, assoUnit.ParentList.ParentWeb.Site.ID.ToString(), assoUnit.ParentList.ParentWeb.ID.ToString(), assoUnit.ParentList.ID.ToString(), assoUnit.ParentList.ID.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                            break;
                        case SPWFAssociationParentType.ListContentType:
                            logger.Log(AveLogLevel.DEBUG, "An processor error occurred while restoring workflow association unit, level: list content type, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
                            SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.Url, assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), assoUnit.ParentContentType.ParentList.ID.ToString(), assoUnit.ParentId.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                            break;
                        case SPWFAssociationParentType.WebContentType:
                            logger.Log(AveLogLevel.DEBUG, "An processor error occurred while restoring workflow association unit, level: web content type, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
                            SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.Url, assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), string.Empty, assoUnit.ParentId.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                            break;
                        default:
                            break;
                    }
                    if (!NeedPostActionAssociations.Contains(assoUnit.SerializableData.mId))
                    {
                        NeedPostActionAssociations.Add(assoUnit.SerializableData.mId);
                    }
                    if (!NoRestoredWFCache.Contains(assoUnit.SerializableData.mBaseId))
                    {
                        NoRestoredWFCache.Add(assoUnit.SerializableData.mBaseId);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.CacheDataError, e);
            }
        }
    }

    internal sealed class SPWFAssociationProcAPI : SPWFAssociationProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool mForceUpdate = true;
        public override List<byte[]> Backup()
        {
            return BackupAssociationUnit();
        }

        public override int Restore(SPWFAssociationUnit assoUnit)
        {
            return Restore(assoUnit, true, false);
        }

        public override int Restore(byte[] serializedData)
        {
            return Restore(serializedData, true);
        }

        public override int Restore(SPWFAssociationUnit unit, bool forceUpdate, bool isPostAction)
        {
            mForceUpdate = forceUpdate;
            return RestoreAssociationUnit(unit);
        }

        public override int Restore(byte[] serializedData, bool forceUpdate)
        {
            try
            {
                SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(serializedData);
                return Restore(assoUnit, forceUpdate, false);
            }
            catch (SPWFProcessorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e);
            }
        }

        /// <summary>
        /// 还原resuable workflow的template
        /// </summary>
        /// <param name="unit"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public override void RestoreReusableWFTemplate(SPWFAssociationUnit unit)
        {
            unit.ParentObject = mParentObject;
            unit.ParentObjectType = mObjectType;
            byte[] cacheData = SPWFAssociationUnit.Save(unit);
            try
            {
                bool hasConfigFile = unit.TemplateLibUnit.mTemplateFileUnits.Any(subFile => subFile.SerializableData.mName.EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase));

                bool isCurrent = true;
                foreach (SPWorkflowSubFileUnit fileUnit in unit.TemplateLibUnit.mTemplateFileUnits)
                {
                    isCurrent = isCurrent && fileUnit.SerializableData.mIsCurrentVersion;
                }
                //单独还原template时，只有current version才创建association

                if (hasConfigFile
                  && (isCurrent || !SPWorkflowProcessorRuntime.RestoreCurrentVersion)
                    && CheckNeedRestoreTemplate(unit.ParentWeb, unit, isCurrent))
                {

                    if (SPWorkflowSubFileUnit.HandleReusableTemplateSPFileUnits(unit, unit.TemplateLibUnit, isCurrent))
                    {
                        AddRestoredWorkflowTemplateToCache(unit);
                    }
                    else
                    {
                        logger.Error("Failed handling resuable workflow template restore opeartion.Name:{0}", unit.SerializableData.mName);
                    }
                }

                using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.RestoreCustomWorkflowData"))
                {
                    CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                    customProc.FireRestoreCustomWorkflowDataEvent(unit);
                }
            }
            catch (SPWFProcessorException procException)
            {
                ParseExceptionToCacheData(procException, unit, cacheData);
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, unit.SerializableData.mId);
            }
        }

        public override void SetRestoredUnit(SPWFAssociationUnit assoUnit, object asso)
        {
            assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.mTaskListUnit.SerializableData.mId];
            assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.mHistListUnit.SerializableData.mId];
            AddAssociationToRestoredCollection(assoUnit, (IAveWorkflowAssociation)asso);
            base.SetRestoredUnit(assoUnit, asso);
        }

        protected override void AddAssociationToRestoredCollection(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.AddAssociationToRestoredCollection"))
            {
                if (mForceUpdate)
                {
                    bool needReloadParent = false;
                    asso.AssociationData = assoUnit.SerializableData.mInstantiationParams;
                    if (assoUnit.IsBuiltinBaseIdForSP2010)
                    {
                        asso.AssociationData = ReplaceUserInAssociationDataForSP2010(asso.ParentWeb, asso.AssociationData, asso.BaseId);
                    }
                    else if (assoUnit.IsBuiltinBaseIdForSP2007)
                    {
                        asso.AssociationData = ReplaceUserInAssociationData(asso.ParentWeb, asso.AssociationData, asso.BaseId);
                    }
                    try
                    {
                        if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                        {
                            needReloadParent = true;
                            //todo:NativeUpdateAssociation
                            SPWFAssociationProcNative.UpdateAssociationName(asso, assoUnit.SerializableData.mOriginalName, assoUnit.IsRenamed);
                            //todo:NativeUpdateAssociation
                            SPWFAssociationProcNative.UpdateCreatedTime(asso, asso.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(assoUnit.SerializableData.mCreated));
                            //todo:NativeUpdateAssociation
                            SPWFAssociationProcNative.UpdateAuthor(asso, assoUnit.SerializableData.mAuthorLoginName);
                            //todo:API_UpdateSetting only builtin need update it,reusable does not need
                            asso.PermissionsManual = (AveBasePermissions)assoUnit.SerializableData.mPermissionsManual;
                            //if only restore association,but not restore instance
                            ModifyDefaultConfiguration(ref assoUnit.SerializableData.mConfiguration);
                        }
                        else
                        {
                            asso.Name = assoUnit.SerializableData.mOriginalName;
                        }
                        //if backed up status field is not null, but the column doesn't exist on destination, this function will throw an exception.
                        assoUnit.UpdateWorkflowAssociation(asso);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.ConfigurationUpdateError, e);
                    }
                    finally
                    {
                        //todo:NativeUpdateAssociation
                        if ((assoUnit.SerializableData.mConfiguration & 131072) == 0)
                        {
                            //Configuration问题，如再遇到Configuration相关问题，参考ADO-87374的comment。
                            if ((assoUnit.SerializableData.mConfiguration & 0x800) == 0x800)
                            {
                                assoUnit.SerializableData.mConfiguration = assoUnit.SerializableData.mConfiguration - 2048;
                            }
                            SPWFAssociationProcNative.UpdateConfiguration(asso, assoUnit.SerializableData.mConfiguration);
                        }
                    }
                    //native更新workflow association name后，需要reload parent
                    if (needReloadParent)
                    {
                        assoUnit.ReloadParentObject();
                    }
                    asso = assoUnit.SPAssoicationCollection[asso.ID];
                    try
                    {
                        if (assoUnit.SerializableData.mIsDefaultContentApprovalWorkflow)
                        {
                            IAveList list = assoUnit.ParentList;
                            list.DefaultContentApprovalWorkflowId = asso.ID;
                            list.Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.DefaultContentApprovalIdSetError, ex);
                    }
                }

                if (NeedPostActionAssociations.Contains(assoUnit.SerializableData.mId))
                {
                    NeedPostActionAssociations.Remove(assoUnit.SerializableData.mId);
                }
                if (SPWorkflowProcessorRuntime.CachedAssociationCount > 0)
                {
                    if (mUnitsOfRestored != null && mUnitsOfRestored.Count >= SPWorkflowProcessorRuntime.CachedAssociationCount)
                    {
                        logger.Log(AveLogLevel.INFO, "Restored workflow definitions count: {0}", mUnitsOfRestored.Count);
                        foreach (KeyValuePair<Guid, SPWFAssociationUnit> kv in mUnitsOfRestored)
                        {
                            logger.Log(AveLogLevel.INFO, "{0}  {1}", kv.Key, kv.Value.SerializableData.mName);
                        }
                        mUnitsOfRestored.Clear();
                    }
                    mUnitsOfRestored.AddEx(assoUnit.SerializableData.mSourceId, assoUnit);
                    logger.Debug("Add workflow association to restored units.ID:{0},Name:{1}", assoUnit.SerializableData.mSourceId, assoUnit.SerializableData.mName);
                    //reload association
                    var newAsso = assoUnit.SPAssoicationCollection[asso.ID];
                    UnitsOfRestoredNameMapping.AddEx(assoUnit.SerializableData.mOriginalName.ToLower(CultureInfo.CurrentCulture), null == newAsso ? asso.Name : newAsso.Name);
                }
                else//use mId for supporting old backed up data
                {
                    mUnitsOfRestored.AddEx(assoUnit.SerializableData.mId, assoUnit);
                }
                SetAssociationUnitPropBySPAssociationForRestore(assoUnit, asso);
                //SetAssociationUnitPropBySPAssociation(assoUnit, asso, false);

                if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
                {
                    //fixup GUID Mapping in template files
                    try
                    {
                        if (!assoUnit.IsBuiltinBaseId &&
                            assoUnit.SerializableData.mIsDeclarative &&
                            assoUnit.mTemplateLibUnit != null
                            && assoUnit.mTemplateLibUnit.mTemplateFileUnits != null
                            && assoUnit.mTemplateLibUnit.mTemplateFileUnits.Count > 0)
                        {
                            Dictionary<string, object> temp = new Dictionary<string, object>();
                            foreach (SPWorkflowSubFileUnit fileUnit in assoUnit.mTemplateLibUnit.mTemplateFileUnits)
                            {
                                SPWorkflowSubFileUnit.FixupDictionary(asso.ParentWeb, temp, fileUnit.SerializableData.mGUIDDictionary);
                            }

                            foreach (KeyValuePair<string, object> pair in temp)
                            {
                                if (SPWorkflowCommon.StringIsGUIDFormat(pair.Key)
                                    && SPWorkflowCommon.StringIsGUIDFormat(pair.Value as string))
                                {
                                    Guid key = new Guid(pair.Key);
                                    Guid value = new Guid(pair.Value as string);
                                    assoUnit.AllGUIDInTemplate.AddEx(key, value);
                                }
                                else
                                {
                                    logger.Debug("Invalid guid format in fix up dictionary.Key:{0},Value:{1}", pair.Key, pair.Value);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.GuidMappingFixingUpError, ex);
                    }
                }
            }
        }

        private void GetXomlAndRulesVersionLabel(IAveWorkflowAssociation asso, out string xomlVersionLabel, out string rulesVersionLabel)
        {
            xomlVersionLabel = null;
            rulesVersionLabel = null;

            string name = null;
            Guid libId = Guid.Empty;
            int cfgId = 0;
            int cfgFileVersion = 0;
            if (SPWorkflowSubListUnit.GetInfoFromInternalName(asso.InternalName, out name, out libId, out cfgId, out cfgFileVersion))
            {
                try
                {
                    //ADO-80069， site collection level的Reusable Nintex Workflow的template 文件存在Root web下的list “wfpub” 下，Nintex template 文件存在Root Web的list “NintexWorkflows”下。故需要特殊处理。
                    IAveList list = null;
                    try
                    {
                        list = asso.ParentWeb.GetList(libId);
                    }
                    catch (Exception e)
                    {
                        logger.Info("Get the template list failed,the template file may be in root web. Error message:{0}.", e);
                        list = asso.ParentWeb.Site.RootWeb.GetList(libId);
                    }
                    IAveListItem item = list.GetItemById(cfgId);
                    IAveFile file = item.File;
                    string charSetName = file.CharSetName;
                    if (string.IsNullOrEmpty(charSetName))
                        charSetName = "utf-8";

                    string strContent = string.Empty; ;
                    if (file.UIVersion == cfgFileVersion)
                    {
                        strContent = Encoding.GetEncoding(charSetName).GetString(file.OpenBinary());
                    }
                    else
                    {
                        IAveFileVersion version = file.Versions.GetVersionFromID(cfgFileVersion);
                        strContent = Encoding.GetEncoding(charSetName).GetString(version.OpenBinary());
                    }
                    XmlDocument xmlConfig = null;
                    try
                    {
                        xmlConfig = new XmlDocument();
                        xmlConfig.LoadXml(strContent);
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@XomlVersion") != null)
                        {
                            xomlVersionLabel = xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@XomlVersion").Value;
                            if (xomlVersionLabel.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                            {
                                xomlVersionLabel = xomlVersionLabel.Substring(1);
                            }
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@RulesVersion") != null)
                        {
                            rulesVersionLabel = xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@RulesVersion").Value;
                            if (rulesVersionLabel.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                            {
                                rulesVersionLabel = rulesVersionLabel.Substring(1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.Common_XmlFileHandleException, ex.Message);
                        logger.Warn("An exception occurred while handle xml file. exception:{0}", ex.ToString());
                    }
                    finally
                    {
                        if (xmlConfig != null)
                            xmlConfig.RemoveAll();
                    }

                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.AP_GetXomlAndRulesVersionLabelException, e.Message);
                    logger.Warn("An exception occurred while get xoml and rules version label. exception:{0}", e);
                }
            }
        }

        #region ************************Backup  Region************************

        /// <summary>
        /// restore过程中 根据SPWorkflowAssociation给assoUnit属性赋值的方法
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="asso"></param>
        private void SetAssociationUnitPropBySPAssociationForBackup(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {
            using (var pf = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation.SetAssociationUnitPropBySPAssociationForBackup"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetAssociationUnitPropBySPAssociationForBackup");
                string monitor = "Set Association Properties for backup";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    SetAssociationUnitPropBySPAssociationCommon(assoUnit, asso);

                    assoUnit.SerializableData.mSourceId = asso.ID;
                    assoUnit.SerializableData.mDescription = asso.Description;
                    assoUnit.SerializableData.mAutoCleanupDays = asso.AutoCleanupDays;
                    assoUnit.SerializableData.mConfiguration = (int)asso.Configuration;

                    //SPWorkflowAssociation.Created is a utc time,so for found the pre-restored preversion association, we must convert created to local time
                    assoUnit.SerializableData.mCreated = asso.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(asso.Created);

                    switch (this.ParentObjectType)
                    {
                        case SPWFAssociationParentType.List:
                            IAveList list = (IAveList)assoUnit.ParentObject;
                            assoUnit.SerializableData.mParentId = list.ID.ToString("B");
                            break;
                        case SPWFAssociationParentType.ListContentType:
                        case SPWFAssociationParentType.WebContentType:
                            IAveContentType ct = (IAveContentType)assoUnit.ParentObject;
                            assoUnit.SerializableData.mParentId = ct.ID.ToString();
                            break;
                        case SPWFAssociationParentType.Web:
                            IAveWeb web = (IAveWeb)assoUnit.ParentObject;
                            assoUnit.SerializableData.mParentId = web.ID.ToString("B");
                            break;
                        default:
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                    }
                    assoUnit.SerializableData.mOriginalParentId = assoUnit.SerializableData.mParentId;

                    //for nintex statistics
                    assoUnit.SerializableData.mModified = asso.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(asso.Modified);


                    assoUnit.SerializableData.mOriginalName = asso.Name;

                    assoUnit.SerializableData.mPermissionsManual = (int)asso.PermissionsManual;

                    assoUnit.SerializableData.mStatusFieldName = asso.InternalNameStatusField;
                    if (!string.IsNullOrEmpty(assoUnit.SerializableData.mStatusFieldName))
                    {
                        assoUnit.SerializableData.mStatusFieldSchema = SPWFAssociationProc.GetStatusFieldSchema(asso, assoUnit.SerializableData.mStatusFieldName, true, QueryService); //static mothed
                        SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "StatusFieldName", assoUnit.SerializableData.mStatusFieldName);
                    }
                    else
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "StatusFieldName", "NULL");
                    }

                    //IssueTracking(Three-state) Workflow
                    if (asso.BaseId.Equals(new Guid("C6964BFF-BF8D-41AC-AD5E-B61EC111731A")))
                    {
                        GetOrSetIssueTrackingFields(assoUnit, true);
                    }
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                    logger.Warn("An exception occurred while set association unit properties. exception:{0}", e);
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSetUnitException, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetAssociationUnitPropBySPAssociationForBackup");
                }
            }
        }

        private List<IAveWorkflowAssociation> ExecuteFilterFanction(IAveWorkflowAssociationCollection assoCollection, string ctName = null)
        {
            if (this.FilterWorkflowFunction != null)
            {
                return assoCollection.Where(ass => this.FilterWorkflowFunction(new AveWorkflowAssociationInfo() { WorkFlowModle = AveWorkflowModel.Model2010, AssociationId = ass.ID, AssociationBaseId = ass.BaseId, CTName = ctName, Name = ass.Name, IsCTWorkflowAssociation = ctName != null })).ToList();
            }
            return assoCollection.ToList();
        }

        private List<byte[]> BackupAssociationUnit()
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupAssociationUnit");
                string monitor = mMainMonitorLog = "Association Backup";
                if (mParentObject == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);

                List<IAveWorkflowAssociation> assoCollection;
                switch (mObjectType)
                {
                    case SPWFAssociationParentType.List:
                        assoCollection = ExecuteFilterFanction(((IAveList)mParentObject).WorkflowAssociations);
                        break;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        assoCollection = ExecuteFilterFanction(((IAveContentType)mParentObject).WorkflowAssociations, ((IAveContentType)mParentObject).Name);
                        break;
                    case SPWFAssociationParentType.Web:
                        assoCollection = ExecuteFilterFanction(((IAveWeb)mParentObject).WorkflowAssociations);
                        break;
                    default:
                        return base.Backup();
                }


                List<byte[]> rlt = new List<byte[]>();
                List<IAveWorkflowAssociation> assoCollectionSorted = new List<IAveWorkflowAssociation>();
                foreach (IAveWorkflowAssociation asso in assoCollection)
                {
                    assoCollectionSorted.Add(asso);
                }

                assoCollectionSorted.Sort(new SPWorkflowAssociationComparer());

                foreach (IAveWorkflowAssociation asso in assoCollectionSorted)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.AP_BackupBegin, asso.Name);
                    mInnerExceptions = new List<SPWFProcessorException>();
                    try
                    {
                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                        mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + asso.Name);
                        mPerformanceMonitor.StartMonitor(monitor);
                        #endregion
                        logger.Log(AveLogLevel.DEBUG, "Begin to assemble workflow association for 10 model one by one, association name: {0}, level: {1}", asso.Name, mObjectType.ToString());
                        SPWFAssociationUnit assoUnit = BackupOneAssociation(monitor, asso);

                        byte[] data = SPWFAssociationUnit.Save(assoUnit);
                        rlt.Add(data);

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Serialize Association Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion

                        UnitsOfBackup.AddEx(assoUnit.SerializableData.mId, data);
                    }
                    catch (SPWFProcessorException procException)
                    {
                        logger.Log(AveLogLevel.WARN, "An error occurred while backup workflow association.Name:{0}, defined Exception:{1}.", asso.Name, procException.ToString());
                        mInnerExceptions.Add(procException);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, "An error occurred while backup workflow association.Name:{0}, Exception:{1}.", asso.Name, e);
                        mInnerExceptions.Add(new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, asso.ID));
                    }
                    finally
                    {
                        #region Performance Monitor Region
                        mPerformanceMonitor.StopMonitor(monitor);
                        mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                        mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                        #endregion
                    }
                    if (mInnerExceptions.Count > 0)
                        mExceptions.AddEx(asso.ID, mInnerExceptions);
                    SPWorkflowProcessorRuntime.Log(Logs.AP_BackupFinish, asso.Name);
                }
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupAssociationUnit");
                return rlt;
            }
        }

        public override SPWFAssociationUnit BackupOneAssociation(string monitor, IAveWorkflowAssociation asso)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation"))
            {
                SPWFAssociationUnit assoUnit = new SPWFAssociationUnit();
                assoUnit.ParentObject = mParentObject;
                assoUnit.ParentObjectType = mObjectType;
                SetAssociationUnitPropBySPAssociationForBackup(assoUnit, asso);
                //SetAssociationUnitPropBySPAssociation(assoUnit, asso, true);

                #region Performance Monitor Region
                mPerformanceMonitor.ResetCurrentDuration(monitor);
                #endregion

                if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType || assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
                {
                    using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation.BackupTaskListUnit"))
                    {
                        if (mCachedSubListUnits.ContainsKey(assoUnit.SerializableData.mTaskListId))
                        {
                            assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.SerializableData.mTaskListId];
                        }
                        //ADO-94145 继承自site content type上的list content type workflow association关联的tasks list和history list可能还没有创建出来
                        else if (assoUnit.SerializableData.mTaskListId == Guid.Empty && assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                        {
                            //
                        }
                        else
                        {
                            assoUnit.mTaskListUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.Tasks);
                            mCachedSubListUnits.AddEx(assoUnit.mTaskListUnit.SerializableData.mId, assoUnit.mTaskListUnit);
                        }

                    }
                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Task List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation.BackupHistListUnit"))
                    {
                        if (mCachedSubListUnits.ContainsKey(assoUnit.SerializableData.mHistoryListId))
                        {
                            assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.SerializableData.mHistoryListId];
                        }
                        //ADO-94145 继承自site content type上的list content type workflow association关联的tasks list和history list可能还没有创建出来
                        else if (assoUnit.SerializableData.mHistoryListId == Guid.Empty && assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                        {
                            //
                        }
                        else
                        {
                            assoUnit.mHistListUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.WorkflowHistory);
                            mCachedSubListUnits.AddEx(assoUnit.mHistListUnit.SerializableData.mId, assoUnit.mHistListUnit);
                        }

                    }
                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup History List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion
                }

                using (AvePerformanceScope pf3 = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation.BackupTemplateLibUnit"))
                {
                    if (!assoUnit.IsBuiltinBaseId && assoUnit.SerializableData.mIsDeclarative)
                    {
                        assoUnit.mTemplateLibUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.NoCodeWorkflows);

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Template Library Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion
                    }

                }




                using (AvePerformanceScope pf4 = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation.BackupCustomWorkflowDataEvent"))
                {
                    CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                    customProc.FireBackupCustomWorkflowDataEvent(assoUnit);
                }

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Custom Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion
                return assoUnit;
            }
        }



        public override List<byte[]> BackupWFReusableTemplates()
        {
            IAveWeb parentWeb = ParentObject as IAveWeb;
            List<byte[]> templateUnits = new List<byte[]>();
            if (parentWeb == null)
            {
                logger.Warn("Invalid parentWeb object while backing up workflow template.");
                return templateUnits;
            }
            try
            {
                //need to get all template file internal names with all versions
                foreach (var template in parentWeb.WorkflowTemplates)
                {
                    if (BuiltinWorkflowBaseIdCollection.IsBuiltinBaseId(template.BaseId)
                        || (FilterReusableWorkflowTemplateFunction != null && !FilterReusableWorkflowTemplateFunction(new AveReusableWorkflowTemplateInfo
                        {
                            AllowDefaultContentApproval = template.AllowDefaultContentApproval,
                            AutoStartChange = template.AutoStartChange,
                            AutoStartCreate = template.AutoStartCreate,
                            Description = template.Description,
                            BaseId = template.BaseId,
                            ID = template.ID,
                            IsRootPublic = template.IsRootPublic,
                            Name = template.Name
                        })))
                    {
                        logger.Debug("The resuable workflow template {0} is filtered.", template.Name);
                        continue;
                    }
                    string tempName = (string)template["DeclarativeConfiguration"];
                    if (string.IsNullOrEmpty(tempName))
                    {
                        continue;
                    }
                    List<WFTemplateVersionInfo> templateVersionNames = SPWorkflowTemplateHelper.GetInternalNameForAllTemplateVersions(parentWeb, template, tempName);
#if DEBUG
                    try
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine("Begin to backup workflow templates:");
                        templateVersionNames.ForEach(version => builder.AppendLine(version.ToString()));
                        logger.Debug(builder.ToString());
                    }
                    catch (Exception e)
                    {

                        logger.Debug(e.ToString());
                    }
#endif
                    templateVersionNames.ForEach(version => templateUnits.Add(BackupOneWFTemplate(version, parentWeb)));
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while backing up workflow templates. Error: {0}", e);
            }
            return templateUnits;
        }

        private byte[] BackupOneWFTemplate(WFTemplateVersionInfo version, IAveWeb web)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneTemplateUnit"))
            {
                logger.Debug("Begin backing up the 10 mode workflow template: {0}", version);
                SPWFAssociationUnit assoUnit = new SPWFAssociationUnit
                {
                    ParentObject = web,
                    ParentObjectType = SPWFAssociationParentType.Web,
                    IsCurrentVersion = version.IsCurrent,
                };

                try
                {
                    //used for backup tempalte file
                    assoUnit.SerializableData.mInternalName = version.ToString();
                    //used for restore
                    assoUnit.SerializableData.mName = version.TemplateName;
                    assoUnit.SerializableData.mBaseId = version.BaseId;


                    using (AvePerformanceScope pf3 = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation.BackupTemplateLibUnit"))
                    {
                        assoUnit.mTemplateLibUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.NoCodeWorkflows);
                    }

                    using (AvePerformanceScope pf4 = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation.BackupCustomWorkflowDataEvent"))
                    {
                        CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                        customProc.FireBackupCustomWorkflowDataEvent(assoUnit);
                    }

                    if (NintexWorkflowUtility.IsNintexWorkflow(assoUnit))
                    {
                        assoUnit.SerializableData.mIsNintexReusableWorkflow = true;
                    }

                    assoUnit.SerializableData.isReusableWrokflow = true;

                    return SPWFAssociationUnit.Save(assoUnit);
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while backing up one workflow template. TemplateInfo:{0}. Error: {1}", version, ex);
                    return null;
                }
            }
        }
        #endregion

        #region ************************Restore Region************************

        /// <summary>
        /// restore过程中 根据SPWorkflowAssociation给assoUnit属性赋值的方法
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="asso"></param>
        private void SetAssociationUnitPropBySPAssociationForRestore(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {
            using (var pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.AddAssociationToRestoredCollection.SetAssociationUnitPropBySPAssociationForRestore"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetAssociationUnitPropBySPAssociationForRestore");
                string monitor = "Set Association Properties for restore";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);

                    SetAssociationUnitPropBySPAssociationCommon(assoUnit, asso);

                    assoUnit.SerializableData.mCreated = asso.Created;
                    assoUnit.SerializableData.mParentId = assoUnit.ParentId;
                    assoUnit.SerializableData.mModified = asso.Modified;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                    logger.Warn("An exception occurred while set association unit properties. exception:{0}", e);
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSetUnitException, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetAssociationUnitPropBySPAssociationForRestore");
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        private int RestoreAssociationUnit(SPWFAssociationUnit assoUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreAssociationUnit");
                string monitor = mMainMonitorLog = "Association Restore";
                assoUnit.ParentObject = mParentObject;
                assoUnit.ParentObjectType = mObjectType;
                assoUnit.ParentAveSPWeb = AveSPObjectCache.AveSPWeb;
                assoUnit.WebLevelFieldProcessorCollection = this.WebLevelFieldProcessorCollection;
                byte[] cacheData = SPWFAssociationUnit.Save(assoUnit);

                switch (assoUnit.ParentObjectType)
                {
                    case SPWFAssociationParentType.List:
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                    case SPWFAssociationParentType.Web:
                        break;
                    default:
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                }

                try
                {

                    if (assoUnit.ParentObject == null)
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + assoUnit.SerializableData.mName);
                    mPerformanceMonitor.StartMonitor(monitor);
                    #endregion

                    IAveWeb parentSPWeb = assoUnit.ParentWeb;
                    {
                        bool needUpdateTemplate = true;
                        IAveList taskList = null;
                        IAveList histList = null;
                        string taskListTitle = assoUnit.SerializableData.mTaskListTitle;
                        string histListTitle = assoUnit.SerializableData.mHistoryListTitle;

                        GetOrCreateWorkflowRelatedList(assoUnit, parentSPWeb, ref taskList, ref histList);
                        IAveWorkflowAssociation conflictAssociation;
                        ConflicStatus cStatus = HandleWorkflowAssociationConflict(assoUnit, out conflictAssociation);
                        if (cStatus == ConflicStatus.BaseId)
                            return 1;
                        else if (cStatus == ConflicStatus.None)
                        {
                            #region Performance Monitor Region
                            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Handle Conflict. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            #endregion

                            IAveWorkflowAssociation newAsso = null;
                            if (!assoUnit.IsBuiltinBaseId && assoUnit.SerializableData.mIsDeclarative)
                            {
                                using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.MarkupWorkflowAssociation"))
                                {
                                    #region Markup Workflow Association
                                    if (assoUnit.mTemplateLibUnit != null)
                                    {

                                        IAveWeb tempListParentWeb = (assoUnit.mTemplateLibUnit.SerializableData.IsRootWebList && !parentSPWeb.IsRootWeb) ? parentSPWeb.Site.RootWeb : parentSPWeb;
                                        IAveList tempList = SPWorkflowSubListUnit.GetOrCreateSPList(assoUnit, tempListParentWeb.Lists, assoUnit.mTemplateLibUnit, assoUnit.WebLevelFieldProcessorCollection);

                                        #region Performance Monitor Region
                                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Template Library Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                                        #endregion

                                        bool HasConfigFile = false;
                                        bool needCreateWorkflowAssociation = true;
                                        foreach (SPWorkflowSubFileUnit subFile in assoUnit.mTemplateLibUnit.mTemplateFileUnits)
                                        {
                                            if (subFile.SerializableData.mName.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                                            {
                                                HasConfigFile = true;
                                                if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                                                {
                                                    string content = Encoding.UTF8.GetString(subFile.SerializableData.mContent, 0, subFile.SerializableData.mContent.Length);
                                                    //ADO-197003  还原reusabletemplate 时create workflow association 会导致web.WorkflowAssociation 中多一个与template 同名的workflow
                                                    //当前发现nintex workflow存在该问题，SPD workflow不存在该问题，对于reusable workflow teamplte来说 createAssociation 为FALSE即可，没有影响
                                                    needCreateWorkflowAssociation = !IsReuableWorkflow(content);
                                                }
                                            }
                                        }
                                        bool isCurrent = true;
                                        foreach (SPWorkflowSubFileUnit unit in assoUnit.mTemplateLibUnit.mTemplateFileUnits)
                                        {
                                            isCurrent = isCurrent && unit.SerializableData.mIsCurrentVersion;
                                        }
                                        bool isGetTemplatesnull = CheckNeedRestoreTemplate(tempListParentWeb, assoUnit, isCurrent); // tempSPWeb.WorkflowTemplates.GetTemplateByBaseID(assoUnit.SerializableData.mBaseId) == null;
                                        if ((isGetTemplatesnull && (isCurrent || !SPWorkflowProcessorRuntime.RestoreCurrentVersion) && HasConfigFile))
                                        {
                                            logger.Debug("Begin to restore workflow template  for 10 model workflow association internally, workflow association name: {0}", assoUnit.SerializableData.mOriginalName);
                                            SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(assoUnit, assoUnit.mTemplateLibUnit, needCreateWorkflowAssociation, out newAsso);
                                            //只会cache reusable workflow template base id
                                            AddRestoredWorkflowTemplateToCache(assoUnit);
                                        }
                                        if (newAsso == null && parentSPWeb.WorkflowTemplates[assoUnit.SerializableData.mBaseId] != null)
                                        {
                                            IAveWorkflowTemplate wfTemplate = parentSPWeb.WorkflowTemplates[assoUnit.SerializableData.mBaseId];
                                            if (wfTemplate == null)
                                            {
                                                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationCannotGetWorkflowTemplate, AveInternalResourceKey.Wrapper_Exception_Workflow_NotFindWFDefinition);
                                            }
                                            IAveWorkflowAssociation temp = assoUnit.SPAssoicationCollection.GetAssociationByName(assoUnit.SerializableData.mName, CultureInfo.CurrentUICulture);
                                            if (temp == null)
                                            {
                                                //当有incremental或rerun job还原前，重新publish了reusable template，那么多出来的version需要用这种方式更新下再获取，
                                                //否则current version关联的不是最新的template version
                                                //此处API内部有提权操作，同时，还可能更新list column，如果以后出现问题，考虑是否需要reload，目前测试没有发现问题 
                                                if (assoUnit.mSPAssoicationCollection.UpdateAssociationsToLatestVersion())
                                                {
                                                    temp = assoUnit.SPAssoicationCollection.GetAssociationByName(assoUnit.SerializableData.mName, CultureInfo.CurrentUICulture);
                                                }
                                            }
                                            bool needAddToParentObject = false;
                                            if (temp == null)
                                            {
                                                if (assoUnit.ParentObjectType == SPWFAssociationParentType.WebContentType)
                                                {
                                                    temp = assoUnit.CreateSPWorkflowAssociation(wfTemplate, assoUnit.SerializableData.mName, assoUnit.SerializableData.mTaskListTitle, assoUnit.SerializableData.mHistoryListTitle);
                                                }
                                                else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType && (taskList == null || histList == null))
                                                {
                                                    //ADO-94145 继承自site content type上的list content type workflow association关联的tasks list和history list可能还没有创建出来
                                                    temp = assoUnit.CreateSPWorkflowAssociation(wfTemplate, assoUnit.SerializableData.mName, taskListTitle, histListTitle);
                                                }
                                                else
                                                {
                                                    temp = assoUnit.CreateSPWorkflowAssociation(wfTemplate, assoUnit.SerializableData.mName, taskList, histList);
                                                }
                                                needAddToParentObject = true;
                                            }
                                            if (temp.BaseId.Equals(new Guid("C6964BFF-BF8D-41AC-AD5E-B61EC111731A")))
                                            {
                                                GetOrSetIssueTrackingFields(assoUnit, false);
                                            }
                                            temp.AllowAsyncManualStart = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AllowAsyncManualStart);
                                            temp.AllowManual = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AllowManualStart);
                                            temp.AssociationData = assoUnit.SerializableData.mInstantiationParams;
                                            temp.AutoCleanupDays = assoUnit.SerializableData.mAutoCleanupDays;
                                            //temp.Description = assoUnit.SerializableData.mDescription;
                                            temp.MarkedForDelete = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.MarkedForDelete);
                                            temp.PermissionsManual = (AveBasePermissions)assoUnit.SerializableData.mPermissionsManual;
                                            newAsso = needAddToParentObject ? assoUnit.AddSPAssociationToParentObject(temp) : temp;
                                        }
                                        if (newAsso != null)
                                        {
                                            var workflowEnable = !CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.GloballyDisabled | AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.MarkedForDelete | AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.NoNewWorkflows);
                                            if (assoUnit.ParentList != null && (newAsso.AutoStartCreate || newAsso.AutoStartChange))// only for list level workflow and enable auto start
                                            {
                                                this.AveSPObjectCache.AveSPWeb.AddtoWFEnableCache(assoUnit.ParentList.ID, newAsso.ID, workflowEnable);
                                            }
                                            else
                                            {
                                                newAsso.Enabled = workflowEnable;
                                            }

                                            newAsso.AutoStartCreate = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartAdd);
                                            newAsso.AutoStartChange = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartChange);
                                            newAsso.Description = assoUnit.SerializableData.mDescription;
                                            newAsso.AutoCleanupDays = assoUnit.SerializableData.mAutoCleanupDays;
                                            //assoUnit.UpdateWorkflowAssociation(newAsso);//DOC-68808
                                        }
                                        #region Performance Monitor Region
                                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Associate Markup Workflow. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                                        #endregion
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.CreateWorkflowAssociation"))
                                {
                                    #region Code Workflow Association

                                    newAsso = GetOrCreateBuiltinWorkflowAssociation(assoUnit, parentSPWeb, taskList, histList, taskListTitle, histListTitle, true);

                                    #endregion

                                    #region Performance Monitor Region
                                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Associate Code Workflow. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                                    #endregion
                                }
                            }

                            if (newAsso != null)
                            {
                                AddAssociationToRestoredCollection(assoUnit, newAsso);

                                #region Performance Monitor Region
                                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Update Association Unit Properties. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                                #endregion
                            }
                        }
                        else if (cStatus == ConflicStatus.Configuration)
                        {
                            using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.ConfigurationWorkflowAssociation"))
                            {
                                if (!assoUnit.IsBuiltinBaseId && assoUnit.SerializableData.mIsDeclarative)
                                {
                                    //forceUse and conflict with destination do not create association
                                    //need consider reusable workflow
                                    needUpdateTemplate = mForceUpdate || conflictAssociation == null || conflictAssociation.BaseId == assoUnit.SerializableData.mBaseId;
                                    IAveWorkflowAssociation newAsso = null;
                                    if (!needUpdateTemplate)
                                    {
                                        newAsso = conflictAssociation;
                                    }
                                    else
                                    {
                                        newAsso = RestoreWorkflowTemplateAndCreateNewAssociation(assoUnit, parentSPWeb);
                                    }
                                    //两种情况 1.reusable workflow  2.反插的definition在目的端已存在的情况,可以直接get出来definition
                                    if (newAsso == null /*&& SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(assoUnit.SPSiteId, assoUnit.SPWebId, assoUnit.SerializableData.mBaseId)*/)
                                    {
                                        //reusable workflow
                                        newAsso = assoUnit.SPAssoicationCollection.GetAssociationByName(assoUnit.SerializableData.mName, CultureInfo.CurrentUICulture);
                                    }
                                    if (newAsso != null)
                                    {
                                        if (needUpdateTemplate)
                                        {
                                            var workflowEnable = !CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.GloballyDisabled | AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.MarkedForDelete | AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.NoNewWorkflows);
                                            if (assoUnit.ParentList != null && (newAsso.AutoStartCreate || newAsso.AutoStartChange))// only for list level workflow and enable auto start
                                            {
                                                this.AveSPObjectCache.AveSPWeb.AddtoWFEnableCache(assoUnit.ParentList.ID, newAsso.ID, workflowEnable);
                                            }
                                            else
                                            {
                                                newAsso.Enabled = workflowEnable;
                                            }
                                            newAsso.AutoStartCreate = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartAdd);
                                            newAsso.AutoStartChange = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartChange);
                                            newAsso.Description = assoUnit.SerializableData.mDescription;
                                        }
                                        AddAssociationToRestoredCollection(assoUnit, newAsso);
                                    }
                                }
                                else
                                {
                                    using (AvePerformanceScope pf3 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.ConfigurationCodeWorkflowAssociation"))
                                    {
                                        IAveWorkflowAssociation newAsso = null;
                                        newAsso = GetOrCreateBuiltinWorkflowAssociation(assoUnit, parentSPWeb, taskList, histList, taskListTitle, histListTitle, false);

                                        if (newAsso != null)
                                        {
                                            AddAssociationToRestoredCollection(assoUnit, newAsso);
                                        }
                                    }
                                }
                            }
                        }

                        if (needUpdateTemplate)
                        {
                            using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.RestoreCustomWorkflowData"))
                            {
                                CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                                customProc.FireRestoreCustomWorkflowDataEvent(assoUnit);
                            }
                        }
                        assoUnit.DisposeSubListUnits();

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Custom Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion

                        SPWorkflowProcessorRuntime.Log(Logs.AP_RestoreFinish, assoUnit.SerializableData.mOriginalName);
                    }

                }
                catch (SPWFProcessorException procException)
                {
                    ParseExceptionToCacheData(procException, assoUnit, cacheData);
                    throw;
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, assoUnit.SerializableData.mId);
                }
                finally
                {
                    #region Performance Monitor Region
                    mPerformanceMonitor.StopMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    #endregion
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreAssociationUnit");
                    if (assoUnit.SerializableData.mId != Guid.Empty)
                    {
                        SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.AddWorkflowIdMapping(assoUnit.SerializableData.mSourceId, assoUnit.SerializableData.mId);
                    }
                }


                return 0;
            }
        }
        private bool IsReuableWorkflow(string xomlFileContent)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(xomlFileContent);
                var node = document.SelectSingleNode("//Template");
                var att = node.Attributes["Visibility"];
                return att != null && (string.Equals("RootPublic", att.Value, StringComparison.Ordinal) || string.Equals("Public", att.Value, StringComparison.Ordinal));
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred shile get visibility attribute,xomlFile:{0} ,error: {0}", xomlFileContent, e);
            }
            return false;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        private IAveWorkflowAssociation RestoreWorkflowTemplateAndCreateNewAssociation(SPWFAssociationUnit associationUnit, IAveWeb parentSPWeb)
        {
            IAveWorkflowAssociation newAsso = null;
            var bolCreate = false;
            var tempRenamed = associationUnit.IsRenamed;

            if (associationUnit.mTemplateLibUnit == null ||
                associationUnit.mTemplateLibUnit.mTemplateFileUnits == null ||
                associationUnit.mTemplateLibUnit.mTemplateFileUnits.Count == 0)
            {
                logger.Info("AssociationUnit.mTemplateLibUnit is empty, can not create new association by it.");
                return null;
            }

            //UpdateCurrentVersion这个开关是用来判断是否Published Workflow Definition.
            if (SPWorkflowProcessorRuntime.UpdateCurrentVersion)
            {
                if (CheckPublicNewVersionCondition(associationUnit))
                {
                    bolCreate = associationUnit.IsCurrentVersion;
                    if (bolCreate)
                    {
                        associationUnit.IsRenamed = true;
                    }
                }
            }

            var HasConfigFile = false;
            foreach (var subFile in associationUnit.mTemplateLibUnit.mTemplateFileUnits)
            {
                if (subFile.SerializableData.mName.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                {
                    HasConfigFile = true;
                }
            }

            bool isCurrent = true;
            foreach (var unit in associationUnit.mTemplateLibUnit.mTemplateFileUnits)
            {
                isCurrent = isCurrent && unit.SerializableData.mIsCurrentVersion;
            }
            bool isGetTemplatesnull = CheckNeedRestoreTemplate(parentSPWeb, associationUnit, isCurrent); //= tempSPWeb.WorkflowTemplates.GetTemplateByBaseID(assoUnit.SerializableData.mBaseId) == null;
            //isCurrent用来判断当前file是否是Current Version.RestoreCurrentVersion控制是否还原Workflow Definition Current Version.
            //此处主要针对Reusable Workflow Definition.由于Reusable Workflow Definition的Template只还原一次，根据这些option来决定还原那个Template.
            if ((isGetTemplatesnull && (isCurrent || !SPWorkflowProcessorRuntime.RestoreCurrentVersion) && HasConfigFile))
            {
                logger.Debug("Begin to restore workflow template  for 10 model workflow association internally, workflow association name: {0}", associationUnit.SerializableData.mOriginalName);
                SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(associationUnit, associationUnit.mTemplateLibUnit, bolCreate, out newAsso);
                //只会cache reusable workflow template base id
                AddRestoredWorkflowTemplateToCache(associationUnit, bolCreate);
            }
            associationUnit.IsRenamed = tempRenamed;
            return newAsso;
        }

        private IAveWorkflowAssociation GetOrCreateBuiltinWorkflowAssociation(SPWFAssociationUnit assoUnit, IAveWeb parentSPWeb, IAveList taskList, IAveList histList, string taskListTitle, string histListTitle, bool createIfNotExist)
        {
            #region Code Workflow Association

            IAveWorkflowAssociation newAsso = null;

            IAveWorkflowTemplate wfTemplate = GetWorkflowTemplate(assoUnit, parentSPWeb);
            IAveWorkflowAssociation temp = assoUnit.SPAssoicationCollection.GetAssociationByName(assoUnit.SerializableData.mName, CultureInfo.CurrentUICulture);
            bool needAddToParentObject = false;
            if (temp == null && createIfNotExist)
            {
                if (assoUnit.ParentObjectType == SPWFAssociationParentType.WebContentType)
                {
                    temp = assoUnit.CreateSPWorkflowAssociation(wfTemplate, assoUnit.SerializableData.mName, assoUnit.SerializableData.mTaskListTitle, assoUnit.SerializableData.mHistoryListTitle);
                }
                else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType && (taskList == null || histList == null))
                {
                    //ADO-94145 继承自site content type上的list content type workflow association关联的tasks list和history list可能还没有创建出来
                    temp = assoUnit.CreateSPWorkflowAssociation(wfTemplate, assoUnit.SerializableData.mName, taskListTitle, histListTitle);
                }
                else
                {
                    temp = assoUnit.CreateSPWorkflowAssociation(wfTemplate, assoUnit.SerializableData.mName, taskList, histList);
                }
                needAddToParentObject = true;
            }

            if (temp == null)
            {
                logger.Info("Cannot find workflow definition {0} in destination workflow association collection, and createIfNotExist is false.", assoUnit.SerializableData.mName);
                return null;
            }

            if (temp.BaseId.Equals(new Guid("C6964BFF-BF8D-41AC-AD5E-B61EC111731A")))
            {
                GetOrSetIssueTrackingFields(assoUnit, false);
            }
            if (needAddToParentObject || this.mForceUpdate)//ADO-195413 fource use 不应该更新Workflow setting
            {
                temp.AllowAsyncManualStart = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AllowAsyncManualStart);
                temp.AllowManual = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AllowManualStart);
                temp.AssociationData = assoUnit.SerializableData.mInstantiationParams;
                temp.AutoCleanupDays = assoUnit.SerializableData.mAutoCleanupDays;
                temp.Description = assoUnit.SerializableData.mDescription;
                temp.MarkedForDelete = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.MarkedForDelete);
                temp.PermissionsManual = (AveBasePermissions)assoUnit.SerializableData.mPermissionsManual;
                //temp.InternalNameStatusField = assoUnit.SerializableData.mStatusFieldName;
                //LSInvoker.SetProperty(temp, "InternalNameStatusField", assoUnit.SerializableData.mStatusFieldName);
            }
            newAsso = needAddToParentObject ? assoUnit.AddSPAssociationToParentObject(temp) : temp;
            newAsso.Enabled = !CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.GloballyDisabled | AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.MarkedForDelete | AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.NoNewWorkflows);
            //assoUnit.UpdateWorkflowAssociation(newAsso);//DOC-68808

            //WorkflowBusinessBehaviorController.GetStartOptionPreBehavior(assoUnit, newAsso).Run();
            newAsso.AutoStartCreate = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartAdd);
            newAsso.AutoStartChange = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartChange);
            //newAsso.AutoStartCreate = false;
            //newAsso.AutoStartChange = false;

            #endregion

            return newAsso;
        }

        private bool CheckNeedRestoreTemplate(IAveWeb tempWeb, SPWFAssociationUnit assoUnit, bool isCurrent)
        {
            bool needRestore = false;
            Guid templateBaseId = assoUnit.SerializableData.mBaseId;
            var tempalteId = Guid.Empty;
            if (SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(tempWeb.Site.ID, tempWeb.ID, templateBaseId))
            {
                tempalteId = SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.GetDestinationTemplateId(tempWeb.Site.ID, tempWeb.ID, templateBaseId);
            }
            else if (SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(tempWeb.Site.ID, Guid.Empty, templateBaseId))
            {
                tempalteId = SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.GetDestinationTemplateId(tempWeb.Site.ID, Guid.Empty, templateBaseId);
            }

            if (tempalteId != Guid.Empty) // 先从cache 里找，找到就不需要还原了
            {
                logger.Debug("Change the workflow baseid.from:{0} to {1}", templateBaseId, tempalteId);
                assoUnit.SerializableData.mBaseId = tempalteId;
                needRestore = false;
            }
            else
            {
                //此处可以取出三类workflow template  Global Reusable Workflow, Site Reusable Workflow, Builtin Workflow
                IAveWorkflowTemplate template = tempWeb.WorkflowTemplates.GetTemplateByBaseID(templateBaseId);
                if (template != null)
                {
                    //site，list，contentType workflow 或者 目的端不存在的reusable workflow
                    needRestore = isCurrent;
                }
                else
                {
                    needRestore = true;
                }
            }
            return needRestore;
        }

        private void AddRestoredWorkflowTemplateToCache(SPWFAssociationUnit assoUnit)
        {
            var configFile = assoUnit.TemplateLibUnit.mTemplateFileUnits.Find(unit => unit.FileType() == SPWorkflowFileContentProcType.Config);
            bool isCurrent = configFile != null && configFile.SerializableData.mIsCurrentVersion;
            AddRestoredWorkflowTemplateToCache(assoUnit, isCurrent);
        }

        private void AddRestoredWorkflowTemplateToCache(SPWFAssociationUnit assoUnit, bool isCurrent)
        {
            IAveWeb parentWeb = assoUnit.ParentWeb;
            IAveWorkflowTemplate template = parentWeb.WorkflowTemplates.GetTemplateByBaseID(assoUnit.SerializableData.mBaseId);

            if (template == null || !isCurrent)
            {
                //it's not a reusable workflow
                //非current version不需要加入cache
                return;
            }

            if (assoUnit.mTemplateLibUnit.SerializableData.IsRootWebList)
            {
                if (!SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(parentWeb.Site.ID, Guid.Empty, assoUnit.SerializableData.mBaseId))
                {
                    //Global Reusable Workflow Template ,而且不在cache中
                    SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Add(parentWeb.Site.ID, Guid.Empty, assoUnit.SerializableData.mBaseId, template.ID);
                }
                //else
                //{
                //    //already in cache, not need to add again
                //}
            }
            else
            {
                //site reusable workflow
                if (!SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(parentWeb.Site.ID, parentWeb.ID, assoUnit.SerializableData.mBaseId))
                {
                    //Global Reusable Workflow Template ,而且不在cache中
                    SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Add(parentWeb.Site.ID, parentWeb.ID, assoUnit.SerializableData.mBaseId, template.ID);
                }
                //else
                //{
                //    //already in cache, not need to add again
                //}
            }
        }

        private void GetOrCreateWorkflowRelatedList(SPWFAssociationUnit assoUnit, IAveWeb parentSPWeb, ref IAveList taskList, ref IAveList historyList)
        {
            string monitor = mMainMonitorLog = "Association Restore";
            if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType || assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
            {

                #region Get or Create Task List
                if (assoUnit.mTaskListUnit != null)
                {
                    using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.RestoreTaskListUnit"))
                    {
                        if (mCachedSubListUnits.ContainsKey(assoUnit.mTaskListUnit.SerializableData.mId))
                        {
                            assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.mTaskListUnit.SerializableData.mId];
                            assoUnit.mTaskListUnit.mSPList = parentSPWeb.Lists.GetListById(assoUnit.mTaskListUnit.mSPList.ID, true);
                        }
                        else
                        {
                            SPWorkflowSubListUnit.GetOrCreateSPList(assoUnit, parentSPWeb.Lists, assoUnit.mTaskListUnit, assoUnit.WebLevelFieldProcessorCollection);
                            mCachedSubListUnits.AddEx(assoUnit.mTaskListUnit.SerializableData.mId, assoUnit.mTaskListUnit);
                        }
                        taskList = assoUnit.mTaskListUnit.mSPList;
                        SPWorkflowProcessorRuntime.Log(Logs.AP_RestoreSubListFinish, taskList == null ? "taskList is null" : taskList.Title);

                        //CI-40817 避免触发Workflow 导致发邮件
                        if (taskList.EnableAssignToEmail)
                        {
                            try
                            {
                                taskList.EnableAssignToEmail = false;
                                taskList.Update();
                                logger.Debug("Reset task list EnableAssignToEmail to false, task list title is {0}", taskList.Title);
                                SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.AddNeedEnableSendEmailList(parentSPWeb.ID, taskList.ID);

                            }
                            catch (Exception e)
                            {
                                logger.Warn("An error occurred when update task list EnableAssignToEmail to false, task list title: {0}, error: {1}.", taskList.Title, e);
                            }
                        }
                    }
                    if (taskList.EnableAssignToEmail)
                    {
                        try
                        {
                            taskList.EnableAssignToEmail = false;
                            taskList.Update();
                            logger.Debug("Reset task list EnableAssignToEmail to false, task list title is {0}", taskList.Title);
                            if (SPWorkflowProcessorRuntime.MappingManager != null)
                            {
                                SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.AddNeedEnableSendEmailList(parentSPWeb.ID, taskList.ID);
                            }
                            else
                            {
                                logger.Warn("Workflow instance mapping manager is null.");
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred when update task list EnableAssignToEmail to false, error: {0}.", e);
                        }
                        // mParentWeb
                        //this.ParentSite.MappingManager.SiteMappingManager.AddNeedEnableSendEmailList(this.ParentWeb.SPWeb.ID, mSPList.ID);
                    }
                }

                #endregion

                #region Get or Create History List
                if (assoUnit.mHistListUnit != null)
                {

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Task List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion


                    using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.RestoreHistListUnit"))
                    {
                        if (mCachedSubListUnits.ContainsKey(assoUnit.mHistListUnit.SerializableData.mId))
                        {
                            assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.mHistListUnit.SerializableData.mId];
                            assoUnit.mHistListUnit.mSPList = parentSPWeb.Lists.GetListById(assoUnit.mHistListUnit.mSPList.ID, true);
                        }
                        else
                        {
                            SPWorkflowSubListUnit.GetOrCreateSPList(assoUnit, parentSPWeb.Lists, assoUnit.mHistListUnit, assoUnit.WebLevelFieldProcessorCollection);
                            mCachedSubListUnits.AddEx(assoUnit.mHistListUnit.SerializableData.mId, assoUnit.mHistListUnit);
                        }
                        historyList = assoUnit.mHistListUnit.mSPList;
                    }


                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore History List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                }

                #endregion

            }
        }

        private bool CheckPublicNewVersionCondition(SPWFAssociationUnit assUint)
        {
            IAveWorkflowAssociationCollection mCollection = assUint.SPAssoicationCollection;
            IAveWorkflowAssociation asso = mCollection.GetAssociationByName(assUint.SerializableData.mName, System.Globalization.CultureInfo.CurrentCulture);
            return assUint.SerializableData.mCreated.Ticks > asso.Created.Ticks ? true : false;
        }

        private IAveWorkflowTemplate GetWorkflowTemplate(SPWFAssociationUnit assoUnit, IAveWeb web)
        {
            IAveWorkflowTemplate wfTemplate = web.WorkflowTemplates[assoUnit.SerializableData.mBaseId];
            if (wfTemplate == null)
            {
                int lc = web.UICulture.LCID;
                Guid mappingBaseId = Guid.Empty;
                string idPrefix = string.Empty;
                foreach (string id in SPWorkflowCommon.BuiltInWorkflowBaseID)
                {
                    idPrefix = id.Substring(0, 32);
                    if (assoUnit.SerializableData.mBaseId.ToString().StartsWith(idPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        mappingBaseId = new Guid(idPrefix + (lc / 256).ToString("X2") + (lc % 256).ToString("X2"));
                        break;
                    }
                }
                mappingBaseId = mappingBaseId == Guid.Empty ? assoUnit.SerializableData.mBaseId : mappingBaseId;
                wfTemplate = web.WorkflowTemplates[mappingBaseId];
                if (wfTemplate == null)
                {
                    //反插feature 逻辑已经提前到wrapper restore中，在此处不需要反插了
                    //TriggerRelatedWorkflowSiteFeatures(web, mappingBaseId);//ADO-125832 通过workflow base id反插site collection level feature
                    wfTemplate = web.WorkflowTemplates[mappingBaseId];
                    if (wfTemplate == null)
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationCannotGetWorkflowTemplate, AveInternalResourceKey.Wrapper_Exception_Workflow_NotFindWFDefinition);
                    }
                }
            }
            return wfTemplate;
        }

        private bool CheckConfiguration(int configuration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration check)
        {
            return (((AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration)configuration & check) != AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.None);
        }

        internal void ModifyDefaultConfiguration(ref int configuration)
        {

            //更改 StatusColumnShown
            if (!SPWorkflowProcessorRuntime.ProcessInstance && CheckConfiguration(configuration, (AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration)0x800))
            {
                ModifyConfiguration(ref configuration, (AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration)0x800, false);
            }
        }

        private void ModifyConfiguration(ref int configuration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration configurationFlags, bool bSetTrue)
        {
            AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration tempConfiguration = (AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration)configuration;
            if (bSetTrue)
            {
                tempConfiguration |= configurationFlags;
            }
            else
            {
                tempConfiguration &= ~configurationFlags;
            }
            configuration = (int)tempConfiguration;
        }

        private bool CheckAssociationConfigConflict(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {
            return false;
        }

        private ConflicStatus CheckWorkflowAssociationConflict(SPWFAssociationUnit assoUnit, out IAveWorkflowAssociation outAsso)
        {
            outAsso = null;
            IAveWorkflowAssociationCollection assoCollection = assoUnit.SPAssoicationCollection;
            IAveWorkflowAssociation asso = assoCollection.GetAssociationByName(assoUnit.SerializableData.mName, System.Globalization.CultureInfo.CurrentCulture);
            if (asso == null)
            {
                return ConflicStatus.None;
            }
            //else if (asso.BaseId != assoUnit.SerializableData.mBaseId)
            //{
            //    return ConflicStatus.BaseId;
            //}
            //else if (CheckAssociationConfigConflict(assoUnit, asso))
            //{
            //    return ConflicStatus.Configuration;
            //}
            //else if(string.IsNullOrEmpty(asso.TaskListTitle) || string.IsNullOrEmpty(asso.HistoryListTitle))
            //{
            //    outAsso = asso;
            //    return ConflicStatus.Orphan;
            //}
            else
            {
                outAsso = asso;
                return ConflicStatus.Configuration;
            }
        }

        private string assoConflictProfix = ".LS.";
        private int assoConfilictIndex = 0;
        private ConflicStatus HandleWorkflowAssociationConflict(SPWFAssociationUnit assoUnit, out IAveWorkflowAssociation equalAssociation)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.HandleWorkflowAssociationConflict"))
            {
                try
                {
                    equalAssociation = null;
                    ConflicStatus conflict = CheckWorkflowAssociationConflict(assoUnit, out equalAssociation);
                    switch (conflict)
                    {
                        case ConflicStatus.None:
                            return conflict;
                        case ConflicStatus.Configuration:
                        case ConflicStatus.Equal:
                        case ConflicStatus.Orphan:
                            if (mForceUpdate && (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType || assoUnit.ParentObjectType == SPWFAssociationParentType.Web))
                            {
                                if (assoUnit.mTaskListUnit != null && assoUnit.mTaskListUnit.mSPList != null)
                                {
                                    equalAssociation.SetTaskList(assoUnit.mTaskListUnit.mSPList);
                                }
                                if (assoUnit.mHistListUnit != null && assoUnit.mHistListUnit.mSPList != null)
                                {
                                    equalAssociation.SetHistoryList(assoUnit.mHistListUnit.mSPList);
                                }
                            }
                            //todo:workflow update need move outside
                            //AddAssociationToRestoredCollection(assoUnit, equalAssociation);
                            return conflict;
                        case ConflicStatus.BaseId:
                            InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.AssociationBaseIdConflict));
                            assoUnit.SerializableData.mName = assoUnit.SerializableData.mOriginalName + assoConflictProfix + assoConfilictIndex.ToString();
                            assoConfilictIndex++;
                            conflict = HandleWorkflowAssociationConflict(assoUnit, out equalAssociation);
                            return conflict;
                        default:
                            return conflict;

                    }
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationHandleConflictError, e);
                }
            }
        }

        private enum WorkflowType
        {
            Feedback2007,
            Feedback2010,
            Approval2007,
            Approval2010,
            ThreeState,
            Signatures2010,
            PublishingApproval,
            ScheduleWebAnalyticsAlerts,
            ScheduleWebAnalyticsReports,
        }
        /// <summary>
        /// 由于不同语言环境的workflow的baseid可能不同，
        /// 所以在这里把所有的语言环境的baseid统一换为英语环境下的baseid
        /// </summary>
        /// <param name="assoBaseId"></param>
        /// <returns></returns>
        private string ToEnglishBaseId(Guid assoBaseId)
        {
            string newId = assoBaseId.ToString().ToUpper(CultureInfo.InvariantCulture);
            string tail = newId.Substring(newId.Length - 4);
            if (tail.StartsWith("04", StringComparison.OrdinalIgnoreCase))
            {
                newId = string.Concat(newId.Substring(0, newId.Length - 4), "0409");
            }
            return newId;
        }
        public string ReplaceUserInAssociationData(IAveWeb web, string orgAssociationData, Guid assoBaseId)
        {
            if (string.IsNullOrEmpty(orgAssociationData))
            {
                return orgAssociationData;
            }
            string associationData = orgAssociationData;
            string baseId = ToEnglishBaseId(assoBaseId);
            switch (baseId)
            {
                case "46C389A4-6E18-476C-AA17-289B0C79FB8F": //Feedback2007
                case "3BFB07CB-5C6A-4266-849B-8D6711700409": //Feedback2010
                case "C6964BFF-BF8D-41AC-AD5E-B61EC111731C": //Approval2007
                case "8AD4D8F0-93A7-4941-9657-CF3706F00409": //Approval2010
                    associationData = ReplaceUserForFeedback(web, associationData);
                    break;
                case "C6964BFF-BF8D-41AC-AD5E-B61EC111731A": //Three-State
                    associationData = ReplaceUserForThreeState(web, associationData);
                    break;
                default:
                    break;
            }
            return associationData;
        }
        public string ReplaceUserInAssociationDataForSP2010(IAveWeb web, string orgAssociationData, Guid assoBaseId)
        {
            if (string.IsNullOrEmpty(orgAssociationData))
            {
                return orgAssociationData;
            }
            string associationData = orgAssociationData;
            string baseId = ToEnglishBaseId(assoBaseId);
            switch (baseId)
            {
                case "77C71F43-F403-484B-BCB2-303710E00409": //Signatures2010
                    associationData = ReplaceUserForSP2010(web, associationData, WorkflowType.Signatures2010);
                    break;
                case "46C389A4-6E18-476C-AA17-289B0C79FB8F": //Feedback2007
                case "3BFB07CB-5C6A-4266-849B-8D6711700409": //Feedback2010
                    associationData = ReplaceUserForSP2010(web, associationData, WorkflowType.Feedback2010);
                    break;
                case "C6964BFF-BF8D-41AC-AD5E-B61EC111731C": //Approval2007
                case "8AD4D8F0-93A7-4941-9657-CF3706F00409": //Approval2010
                case "E43856D2-1BB4-40EF-B08B-016D89A00409": //PublishingApproval
                    associationData = ReplaceUserForSP2010(web, associationData, WorkflowType.Approval2010);
                    break;
                case "C6964BFF-BF8D-41AC-AD5E-B61EC111731A": //Three-State
                    associationData = ReplaceUserForThreeState(web, associationData);
                    break;
                case "1BE2E16E-961B-4898-9DFD-D33D15981EAE": // Schedule Web Analytics Alerts
                    associationData = ReplaceUserForSP2010(web, associationData, WorkflowType.ScheduleWebAnalyticsAlerts);
                    break;
                case "49A1FFA8-B55F-486A-8D8B-0963C3027F45": // Schedule Web Analytics Alerts
                    associationData = ReplaceUserForSP2010(web, associationData, WorkflowType.ScheduleWebAnalyticsReports);
                    break;
                default:
                    break;
            }
            return associationData;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "dfs is a key")]
        private string ReplaceUserForSP2010(IAveWeb web, string associationData, WorkflowType workflowtype)
        {
            XmlDocument xmlDoc = null;
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(associationData);
                string nsPrefixdfs = xmlDoc.DocumentElement.GetNamespaceOfPrefix("dfs");
                string nsPrefixpc = xmlDoc.DocumentElement.GetNamespaceOfPrefix("pc");
                string nsPrefixd = xmlDoc.DocumentElement.GetNamespaceOfPrefix("d");
                string nsPrefixmy = xmlDoc.DocumentElement.GetNamespaceOfPrefix("my");
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("dfs", nsPrefixdfs);
                nsmgr.AddNamespace("pc", nsPrefixpc);
                nsmgr.AddNamespace("d", nsPrefixd);
                nsmgr.AddNamespace("my", nsPrefixmy);
                string selectString = string.Empty;
                if (workflowtype == WorkflowType.Approval2010)
                {
                    selectString = "/dfs:myFields/dfs:dataFields/d:SharePointListItem_RW/d:Approvers/d:Assignment/d:Assignee/pc:Person";
                }
                else if (workflowtype == WorkflowType.Signatures2010)
                {
                    selectString = "/dfs:myFields/dfs:dataFields/d:SharePointListItem_RW/d:Signers/d:Assignment/d:Assignee/pc:Person";
                }
                else if (workflowtype == WorkflowType.Feedback2010)
                {
                    selectString = "/dfs:myFields/dfs:dataFields/d:SharePointListItem_RW/d:Reviewers/d:Assignment/d:Assignee/pc:Person";
                }
                else if (workflowtype == WorkflowType.ScheduleWebAnalyticsAlerts)
                {
                    selectString = "/my:DDWorkflow_AssocFormFields/my:Recipient_Emails/pc:Person";
                }
                else if (workflowtype == WorkflowType.ScheduleWebAnalyticsReports)
                {
                    selectString = "/my:ScheduledWF_DataFields/my:Recipient_Emails/pc:Person";
                }
                //<d:Approvers>
                XmlNodeList reviewersNodes = xmlDoc.SelectNodes(selectString, nsmgr);
                ReplaceUserInNodesForSP2010(reviewersNodes, nsmgr, web);

                //<d:CC>
                XmlNodeList ccNodes = xmlDoc.SelectNodes("/dfs:myFields/dfs:dataFields/d:SharePointListItem_RW/d:CC/pc:Person", nsmgr);
                ReplaceUserInNodesForSP2010(ccNodes, nsmgr, web);

                return xmlDoc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ReplaceUserForSP2010Error, e);
                return associationData;
            }
            finally
            {
                if (xmlDoc != null)
                {
                    xmlDoc.RemoveAll();
                    xmlDoc = null;
                }
            }
        }
        private string ReplaceUserForFeedback(IAveWeb web, string associationData)
        {
            XmlDocument xmlDoc = null;
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(associationData);
                string nsPrefix = xmlDoc.DocumentElement.GetNamespaceOfPrefix("my");
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("my", nsPrefix);

                //<my:Reviewer>
                XmlNodeList reviewersNodes = xmlDoc.SelectNodes("/my:myFields/my:Reviewers/my:Person", nsmgr);
                ReplaceUserInNodes(reviewersNodes, nsmgr, web);

                //<my:cc>
                XmlNodeList ccNodes = xmlDoc.SelectNodes("/my:myFields/my:CC/my:Person", nsmgr);
                ReplaceUserInNodes(ccNodes, nsmgr, web);

                return xmlDoc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ReplaceUserForFeedbackError, e);
                return associationData;
            }
            finally
            {
                if (xmlDoc != null)
                {
                    xmlDoc.RemoveAll();
                    xmlDoc = null;
                }
            }
        }

        private string ReplaceUserForThreeState(IAveWeb web, string associationData)
        {
            XmlDocument xmlDoc = null;
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(associationData);
                string nsPrefix = xmlDoc.DocumentElement.GetNamespaceOfPrefix("my");
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("my", nsPrefix);

                //<my:CustomAssignedTo>
                XmlNode customAssignedToNode = xmlDoc.SelectSingleNode("/my:myFields/my:CustomAssignedTo", nsmgr);
                ReplaceUserInSingleNode(customAssignedToNode, nsmgr, web);
                //<my:ToText>
                XmlNode toTextNode = xmlDoc.SelectSingleNode("/my:myFields/my:ToText", nsmgr);
                ReplaceUserInSingleNode(toTextNode, nsmgr, web);
                //<my:CustomAssignedTo2>
                XmlNode customAssignedTo2Node = xmlDoc.SelectSingleNode("/my:myFields/my:CustomAssignedTo2", nsmgr);
                ReplaceUserInSingleNode(customAssignedTo2Node, nsmgr, web);
                //<my:ToText2>
                XmlNode toText2Node = xmlDoc.SelectSingleNode("/my:myFields/my:ToText2", nsmgr);
                ReplaceUserInSingleNode(toText2Node, nsmgr, web);

                return xmlDoc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ReplaceUserForTreeStateError, e);
                return associationData;
            }
            finally
            {
                if (xmlDoc != null)
                {
                    xmlDoc.RemoveAll();
                    xmlDoc = null;
                }
            }
        }
        private void ReplaceUserInNodesForSP2010(XmlNodeList nodes, XmlNamespaceManager nsmgr, IAveWeb web)
        {
            foreach (XmlNode node in nodes)
            {
                try
                {
                    XmlElement element = node as XmlElement;
                    XmlNode displayNameNode = element.SelectSingleNode("pc:DisplayName", nsmgr);
                    XmlNode accountIdNode = element.SelectSingleNode("pc:AccountId", nsmgr);
                    XmlNode accountTypeNode = element.SelectSingleNode("pc:AccountType", nsmgr);

                    string displayName = displayNameNode.InnerText;
                    string accountId = accountIdNode.InnerText;
                    string accountType = accountTypeNode.InnerText;

                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(accountId);
                    if (user != null)
                    {
                        string newAccountId = user.LoginName;
                        if (!newAccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase))
                        {
                            accountIdNode.InnerText = newAccountId;
                            displayNameNode.InnerText = newAccountId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UserReplaceError, ex);
                }
            }
        }

        private void ReplaceUserInNodes(XmlNodeList nodes, XmlNamespaceManager nsmgr, IAveWeb web)
        {
            foreach (XmlNode node in nodes)
            {
                try
                {
                    XmlElement element = node as XmlElement;
                    XmlNode displayNameNode = element.SelectSingleNode("my:DisplayName", nsmgr);
                    XmlNode accountIdNode = element.SelectSingleNode("my:AccountId", nsmgr);
                    XmlNode accountTypeNode = element.SelectSingleNode("my:AccountType", nsmgr);

                    string displayName = displayNameNode.InnerText;
                    string accountId = accountIdNode.InnerText;
                    string accountType = accountTypeNode.InnerText;

                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(accountId);
                    if (user != null)
                    {
                        string newAccountId = user.LoginName;
                        if (!newAccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase))
                        {
                            accountIdNode.InnerText = newAccountId;
                            displayNameNode.InnerText = newAccountId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UserReplaceError, ex);
                }
            }
        }

        private void ReplaceUserInSingleNode(XmlNode node, XmlNamespaceManager nsmgr, IAveWeb web)
        {
            try
            {
                XmlElement element = node as XmlElement;
                string orgLoginName = element.InnerText;

                IAveUser user = SPPermissionProcessor.GetOrCreateUser(orgLoginName);
                if (user != null)
                {
                    string loginName = user.LoginName;
                    if (!orgLoginName.Equals(loginName, StringComparison.OrdinalIgnoreCase))
                    {
                        element.InnerText = loginName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, ex);
            }
        }

        private enum ConflicStatus
        {
            None,
            BaseId,
            Configuration,
            Equal,
            Orphan,
        }
        #endregion

        #region ************************Common  Region************************

        /// <summary>
        /// SetAssociationUnitPropBySPAssociationForBackup,SetAssociationUnitPropBySPAssociationForRestore公用的逻辑
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="asso"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Assm is part of a name for a property")]
        private void SetAssociationUnitPropBySPAssociationCommon(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {

            assoUnit.mSPAssociation = asso;

            assoUnit.SerializableData.mId = asso.ID;
            assoUnit.SerializableData.mParentAssociationId = asso.ParentAssociationId;
            assoUnit.SerializableData.mBaseId = asso.BaseId;
            SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "BaseId", assoUnit.SerializableData.mBaseId.ToString());
            assoUnit.SerializableData.mAuthor = asso.Author;
            assoUnit.SerializableData.mAuthorLoginName = SPPermissionProcessor.GetUserOrGroupLoginNameFromId(asso.ParentWeb, asso.Author);
            assoUnit.SerializableData.mContentTypeId = asso.ContentTypeId.ToString();
            if (assoUnit.SerializableData.mContentTypeId == "0x")
            {
                assoUnit.SerializableData.mContentTypeId = null;
            }
            if (asso.ParentList != null)
            {
                assoUnit.SerializableData.mParentListId = asso.ParentList.ID;
                SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "ParentList", asso.ParentList.Title);
            }
            assoUnit.SerializableData.mHistoryListId = asso.HistoryListId;
            try
            {
                assoUnit.SerializableData.mHistoryListTitle = asso.HistoryListTitle;
                if (asso.HistoryListId != Guid.Empty) //减少通过抛异常来赋值
                {
                    assoUnit.SerializableData.mHistoryListTitle = asso.ParentWeb.Lists[asso.HistoryListId].Title;
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                logger.Warn("An exception occurred while set association unit propertie mHistoryListTitle. exception:{0}", e);
            }
            assoUnit.SerializableData.mInstanceCount = asso.RunningInstances;
            assoUnit.SerializableData.mInstanceCountDirty = 0;
            assoUnit.SerializableData.mInstantiationParams = asso.AssociationData;
            assoUnit.SerializableData.mName = asso.Name;
            assoUnit.SerializableData.mTaskListId = asso.TaskListId;
            try
            {
                assoUnit.SerializableData.mTaskListTitle = asso.TaskListTitle;
                if (asso.TaskListId != Guid.Empty) //减少通过抛异常来赋值
                {
                    assoUnit.SerializableData.mTaskListTitle = asso.ParentWeb.Lists[asso.TaskListId].Title;
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                logger.Warn("An exception occurred while set association unit properties. exception:{0}", e);
                assoUnit.SerializableData.mTaskListTitle = asso.TaskListTitle;
            }

            assoUnit.SerializableData.mVersion = asso.Version; //(int)LSInvoker.GetProperty(asso, "Version");
            assoUnit.SerializableData.mIsDeclarative = asso.IsDeclarative;
            assoUnit.SerializableData.mInternalName = asso.InternalName;
            string label1;
            string label2;
            GetXomlAndRulesVersionLabel(asso, out label1, out label2);
            assoUnit.XomlVersionLabel = label1;
            assoUnit.RulesVersionLabel = label2;

            if (asso.ParentList != null)
            {
                assoUnit.mListId = asso.ParentList.ID;
                assoUnit.SerializableData.mIsDefaultContentApprovalWorkflow = (asso.ID.Equals(asso.ParentList.DefaultContentApprovalWorkflowId));
            }

            assoUnit.mWebId = asso.ParentWeb.ID;
            assoUnit.mSiteId = asso.ParentWeb.Site.ID;

            IAveWorkflowTemplate template = null;
            try
            {
                template = asso.BaseTemplate;
            }
            catch (Exception _e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, _e.Message);
                logger.Warn("An exception occurred while get association base template. exception:{0}", _e.ToString());
            }
            bool needGetAssemFromInternalName = false;
            string newAssmName = string.Empty;
            try
            {
                if (template != null)
                {
                    var tempplateSetId = template.TemplateIdSet;
                    if (tempplateSetId != null)
                    {
                        newAssmName = tempplateSetId.CodeBesideAssm;
                        if (!string.IsNullOrEmpty(newAssmName))
                        {
                            //if it is a SharePoint2010 SPD Workflow Definition,we can get the SPWorkflowTemplate object, it is different from SharePoint2007.
                            //the GetAssmFullNameFromInternalName function paramater format is 
                            //SPDWorkflowDemo\n<Xoml.4de94745_b540_42f1_a2d7_ed8729d36a59.2.512.-1.0.dll>\n<Cfg.4de94745_b540_42f1_a2d7_ed8729d36a59.3.512.>
                            //so we must add a prefix, temperary is SharePoint2010
                            if ((assoUnit.InternalVersion.Equals(SharePointVersion.SharePoint2010.ToString(), StringComparison.OrdinalIgnoreCase)
                                || assoUnit.InternalVersion.Equals(SharePointVersion.SharePoint2013.ToString(), StringComparison.OrdinalIgnoreCase))
                                && assoUnit.SerializableData.mIsDeclarative)
                                newAssmName = GetAssmFullNameFromInternalName("SharePoint2010\n" + newAssmName);
                        }
                    }
                }
                else if (assoUnit.SerializableData.mIsDeclarative)
                {
                    newAssmName = GetAssmFullNameFromInternalName(assoUnit.SerializableData.mInternalName);
                }
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while getting assemeble name,Error:{0}", e);
                needGetAssemFromInternalName = true;
            }
            if ((string.IsNullOrEmpty(newAssmName) || needGetAssemFromInternalName) && assoUnit.SerializableData.mIsDeclarative)
            {
                logger.Info("Get newAssmName from internal name instead, InternalName:{0}", assoUnit.SerializableData.mInternalName);
                newAssmName = GetAssmFullNameFromInternalName(assoUnit.SerializableData.mInternalName);
            }
            if (!string.IsNullOrEmpty(assoUnit.SerializableData.mCodeBesideAssm))
            {
                logger.Info("Add mCodeBesideAssmMapping.{0} --> {1}", assoUnit.SerializableData.mCodeBesideAssm, newAssmName);
                assoUnit.mCodeBesideAssmMapping = new Dictionary<string, string>(1) { { assoUnit.SerializableData.mCodeBesideAssm, newAssmName } };
            }
            assoUnit.SerializableData.mCodeBesideAssm = newAssmName;
        }

        #endregion
    }

    internal sealed class SPWFAssociationProcNative
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal static string GetStatusFieldSchema(IAveWorkflowAssociation asso, string internalName, bool skipReference, IAveBackupRestoreQueryService queryService)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.GetStatusFieldSchema"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetStatusFieldSchema Native");
                string connString = string.Empty;
                string fieldSchemaXML = string.Empty;
                Guid webId = Guid.Empty;
                Guid listId = asso.ParentList.ID;

                #region Get Fields Schema XML
                using (IAveWeb web = asso.ParentWeb)
                {
                    webId = web.ID;
                    using (IAveSite site = web.Site)
                    {
                        //using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                        //{
                        try
                        {
                            if (Environment.SharePointVersion == SharePointVersion.SharePoint2007)
                            {
                                fieldSchemaXML = queryService.GetFieldsSchemaXML(webId, listId);
                            }
                            else if (Environment.SharePointVersion == SharePointVersion.SharePoint2010)
                            {
                                fieldSchemaXML = asso.ParentList.Fields.GetFieldByInternalName(internalName).SchemaXml;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.GetFieldsSchemaXMLError, ex);
                        }
                        //}
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(fieldSchemaXML))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("<Fields>");
                    builder.Append(fieldSchemaXML.Substring(fieldSchemaXML.IndexOf('<')));
                    builder.Append("</Fields>");

                    XmlDocument doc = null;
                    try
                    {
                        doc = new XmlDocument();
                        doc.LoadXml(builder.ToString());

                        XmlNode node = null;
                        if (skipReference)
                            node = doc.SelectSingleNode("/Fields/Field[@Name='" + internalName + "']");
                        else
                            node = doc.SelectSingleNode("/Fields/FieldRef[@Name=]'" + internalName + "'");
                        if (node != null)
                            fieldSchemaXML = node.OuterXml;
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetStatusFieldSchemaError, e);
                    }
                    finally
                    {
                        if (doc != null)
                            doc.RemoveAll();
                    }
                }
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetStatusFieldSchema Native");
                return fieldSchemaXML;
            }
        }

        internal static void UpdateStatusFieldName(IAveWorkflowAssociation asso)
        {
            using (new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.UpdateStatusFieldName"))
            {
                if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    logger.Debug("Native method [UpdateStatusFieldName] are not supported in client object model.");
                    return;
                }

                if (asso.ParentWeb.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                {
                    logger.Warn("Skip updating workflow status field name because of permission. Name :{0}", asso.InternalNameStatusField);
                    return;
                }

                #region Get Fields Schema XML

                try
                {
                    var site = asso.ParentWeb.Site;
                    using (var queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.UpdateWorkflowStatusFieldName(site.ID, asso.ID, asso.InternalNameStatusField);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateStatusFieldNameError, e);
                }

                #endregion
            }
        }

        internal static void UpdateConfiguration(IAveWorkflowAssociation asso, int configValue)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.UpdateConfiguration"))
            {
                if (asso.ParentWeb.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                {
                    logger.Warn("Skip updating workflow association configuration because of permission. Workflow association configuration value :{0}", configValue);
                    return;
                }
                #region Get Fields Schema XML

                if (WrapperRuntime.CurrentContext.ModelFactory != null && WrapperRuntime.CurrentContext.ModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                {
                    try
                    {
                        var site = asso.ParentWeb.Site;
                        using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                        {
                            queryService.UpdateWorkflowConfiguration(site.ID, asso.ID, configValue);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateConfigrationTimeError, e);
                    }
                }

                #endregion
            }
        }

        internal static void UpdateAssociationName(IAveWorkflowAssociation asso, string name, bool isRenamed)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.UpdateAssociationName"))
            {
                if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    logger.Debug("Native method [UpdateAssociationName] are not supported in client object model.");
                    return;
                }

                if (isRenamed)
                    return;
                if (asso.ParentWeb.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                {
                    logger.Warn("Skip updating workflow association name because of permission. Workflow association name :{0}", name);
                    return;
                }
                #region Update Association Name

                try
                {
                    var site = asso.ParentWeb.Site;
                    using (var queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.UpdateAssociationName(site.ID, asso.ID, name);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateAssociationNameError, e);
                }

                #endregion
            }
        }

        internal static void UpdateCreatedTime(IAveWorkflowAssociation asso, DateTime created)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.UpdateCreatedTime"))
            {
                if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    logger.Debug("Native method [UpdateCreatedTime] are not supported in client object model.");
                    return;
                }
                if (asso.ParentWeb.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                {
                    logger.Warn("Skip updating workflow association created time because of permission. Workflow association created time :{0}", created.ToString());
                    return;
                }
                #region Update Created Property


                try
                {
                    var site = asso.ParentWeb.Site;
                    using (var queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.UpdateWorkflowAssociationCreatedTime(site.ID, asso.ID, created);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateCreatedTime, e);
                }

                #endregion
            }
        }

        internal static void UpdateAuthor(IAveWorkflowAssociation asso, string authorLoginName)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.UpdateAuthor"))
            {
                if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    logger.Debug("Native method [UpdateAuthor] are not supported in client object model.");
                    return;
                }
                if (asso.ParentWeb.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                {
                    logger.Warn("Skip updating workflow association author because of permission. Workflow association author login name :{0}", authorLoginName);
                    return;
                }
                #region Update Author Property
                IAveUser user = SPPermissionProcessor.GetOrCreateUser(authorLoginName);
                if (user != null)
                {
                    try
                    {
                        var site = asso.ParentWeb.Site;
                        using (var queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                        {
                            queryService.UpdateWorkflowAssociationAuthor(site.ID, asso.ID, user.ID);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateModifyTimeError, e);
                    }
                }

                #endregion
            }
        }

        internal static void UpdateModifiedTime(IAveWorkflowAssociation asso, DateTime modified)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.UpdateModifiedTime"))
            {
                if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    logger.Debug("Native method [UpdateModifiedTime] are not supported in client object model.");
                    return;
                }
                if (asso.ParentWeb.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                {
                    logger.Warn("Skip updating workflow association modified time because of permission. Workflow association modified time :{0}", modified.ToString());
                    return;
                }
                #region Update Created Property

                try
                {
                    var site = asso.ParentWeb.Site;
                    using (var queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.UpdateWorkflowAssociationModifiedTime(site.ID, asso.ID, modified);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateModifyTimeError, e);
                }

                #endregion
            }
        }
    }

    internal sealed class SPWFAssociationProc13ModelAPI : SPWFAssociationProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool mForceUpdate = true;

        private const string activationProperties_ParentContentTypeId = "Microsoft.SharePoint.ActivationProperties.ParentContentTypeId";

        public IAveWorkflowServicesManager WFServiceManager
        {
            get
            {
                return Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).WFServiceManager;
            }
        }

        public IAveWorkflowSubscriptionService WFSubscriptionService
        {
            get
            {
                return Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).WFSubscriptionService;
            }
        }

        public IAveWorkflowDeploymentService WFDeploymentService
        {
            get
            {
                return Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).WFDeploymentService;
            }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                IAveWeb web = null;
                switch (mObjectType)
                {
                    case SPWFAssociationParentType.List:
                        web = ((IAveList)mParentObject).ParentWeb;
                        break;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        web = ((IAveContentType)mParentObject).ParentWeb;
                        break;
                    case SPWFAssociationParentType.Web:
                        //web = ((IAveWeb)mParentObject).ParentWeb;
                        web = (IAveWeb)mParentObject;
                        break;
                    default:
                        break;
                }
                return web;
            }
        }

        #region ************************Backup  Region************************

        public override List<byte[]> Backup()
        {
            return BackupAssociationUnit();
        }

        private List<byte[]> BackupAssociationUnit()
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociation13ModelAPI.BackupAssociationUnit"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupAssociationUnit13Model");
                string monitor = mMainMonitorLog = "Association Backup";
                if (mParentObject == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);

                List<IAveWorkflowSubscription> subscriptionCollection = null;
                //Make sure workflow service manager is update.
                Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).UpdateWorkflowServiceManager(ParentWeb);

                subscriptionCollection = GetWorkflowSubscriptionsByType(WFSubscriptionService, mObjectType, mParentObject, this.FilterWorkflowFunction);
                #region old code
                //if (WFSubscriptionService == null)
                //{
                //    return base.Backup();
                //}
                //switch (mObjectType)
                //{
                //    case SPWFAssociationParentType.List:
                //        subscriptionCollection = ExecuteFilterFanction(WFSubscriptionService.EnumerateSubscriptionsByList(((IAveList)mParentObject).ID));
                //        break;
                //    case SPWFAssociationParentType.ListContentType:
                //    case SPWFAssociationParentType.WebContentType:
                //        break;
                //    case SPWFAssociationParentType.Web:
                //        subscriptionCollection = ExecuteFilterFanction(WFSubscriptionService.EnumerateSubscriptionsByEventSource(((IAveWeb)mParentObject).ID));
                //        break;
                //    default:
                //        return base.Backup();
                //}
                #endregion


                List<byte[]> rlt = new List<byte[]>();

                if (subscriptionCollection == null)
                {
                    return rlt;
                }
                foreach (IAveWorkflowSubscription subscription in subscriptionCollection)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.AP_BackupBegin, subscription.Name);
                    mInnerExceptions = new List<SPWFProcessorException>();
                    try
                    {
                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                        mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + subscription.Name);
                        mPerformanceMonitor.StartMonitor(monitor);
                        #endregion
                        logger.Log(AveLogLevel.DEBUG, "Begin to backup workflow association for 13 model, association name: {0}, level: {1}", subscription.Name, mObjectType.ToString());
                        SPWFAssociationUnit assoUnit = BackupOneAssociation(monitor, subscription);

                        byte[] data = SPWFAssociationUnit.Save(assoUnit);
                        rlt.Add(data);

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Serialize Association Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion

                        UnitsOfBackup.AddEx(assoUnit.SerializableData.mId, data);
                    }
                    catch (SPWFProcessorException procException)
                    {
                        logger.Log(AveLogLevel.DEBUG, "An processor error occurred while backing up association unit, error message: {0}", procException);
                        mInnerExceptions.Add(procException);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, "An error occurred while backing up association unit, error message: {0}", e);
                        mInnerExceptions.Add(new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, subscription.Name));
                    }
                    finally
                    {
                        #region Performance Monitor Region
                        mPerformanceMonitor.StopMonitor(monitor);
                        mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                        mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                        #endregion
                    }
                    if (mInnerExceptions.Count > 0)
                        mExceptions.AddEx(subscription.Id, mInnerExceptions);
                    SPWorkflowProcessorRuntime.Log(Logs.AP_BackupFinish, subscription.Name);
                }
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupAssociationUnit13Model");
                return rlt;
            }
        }

        public override List<byte[]> BackupWFReusableTemplates()
        {
            IAveWeb parentWeb = ParentObject as IAveWeb;
            List<byte[]> templateUnits = new List<byte[]>();
            if (parentWeb == null)
            {
                logger.Warn("Invalid parentWeb object while backing up reusable workflow templates.");
                return templateUnits;
            }
#if DEBUG
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Begin to backup 2013 workflow templates:");
#endif
            try
            {
                Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).UpdateWorkflowServiceManager(ParentWeb);
                if (WFDeploymentService == null)
                {
                    return templateUnits;
                }
                foreach (var definition in WFDeploymentService.EnumerateDefinitions(true))
                {
                    if (definition != null
                        && definition.Properties != null
                        && ((definition.Properties.ContainsKey("isReusable") && string.Equals(definition.Properties["isReusable"], Boolean.TrueString, StringComparison.OrdinalIgnoreCase))
                        || !definition.Properties.ContainsKey("isReusable"))
                        && (FilterReusableWorkflowTemplateFunction == null || FilterReusableWorkflowTemplateFunction(new AveReusableWorkflowTemplateInfo
                        {
                            BaseId = definition.Id,
                            Name = definition.DisplayName,
                            Description = definition.Description,
                        })))
                    {
#if DEBUG
                        builder.AppendLine(definition.DisplayName);
#endif
                        byte[] templateUnit = BackupOneWFTemplate(definition, parentWeb);
                        if (templateUnit != null && templateUnit.Length != 0)
                        {
                            templateUnits.Add(templateUnit);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while backup reusable workflow template. Error: {0}", e);
            }
            finally
            {
#if DEBUG
                logger.Debug(builder.ToString());
#endif
            }

            return templateUnits;
        }

        private byte[] BackupOneWFTemplate(IAveWorkflowDefinition definition, IAveWeb web)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneTemplateUnit"))
            {
                logger.Debug("Begin backing up the 13 mode workflow template: {0}", definition.DisplayName);
                SPWFAssociationUnit templateUnit = new SPWFAssociationUnit
                {
                    ParentObject = web,
                    ParentObjectType = SPWFAssociationParentType.Web,
                    mWorkflowDefinition = definition
                };
                try
                {
                    templateUnit.SerializableData.mBaseId = definition.Id;
                    templateUnit.SerializableData.mName = definition.DisplayName;
                    templateUnit.SerializableData.mDescription = definition.Description;
                    Dictionary<string, object> propsFor13ModelWorkflowDef = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, string> kv in definition.Properties)
                    {
                        propsFor13ModelWorkflowDef.AddEx(kv.Key, kv.Value);
                    }
                    templateUnit.SerializableData.Properties.AddEx(SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION, propsFor13ModelWorkflowDef);

                    using (AvePerformanceScope pf3 = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit.BackupOneAssociation.BackupTemplateLibUnit"))
                    {
                        templateUnit.mTemplateLibUnit = SPWorkflowSubListUnit.GenerateSPListUnit(templateUnit, AveListTemplateType.WFSVC);
                    }

                    return SPWFAssociationUnit.Save(templateUnit);
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while backing up one 2013 mode workflow template. TemplateInfo:{0}. Error: {1}", definition.DisplayName, ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取对应类型的WF Subscription
        /// </summary>
        /// <param name="subscriptionService"></param>
        /// <param name="parentType"></param>
        /// <param name="parentObj"></param>
        /// <param name="filterFunction"></param>
        /// <returns></returns>
        private List<IAveWorkflowSubscription> GetWorkflowSubscriptionsByType(IAveWorkflowSubscriptionService subscriptionService, SPWFAssociationParentType parentType, object parentObj, Func<AveWorkflowAssociationInfo, bool> filterFunction)
        {
            if (subscriptionService == null)
            {
                return null;
            }

            List<IAveWorkflowSubscription> subscriptions = null;
            switch (parentType)
            {
                case SPWFAssociationParentType.List:
                    subscriptions = subscriptionService.EnumerateSubscriptionsByList((parentObj as IAveList).ID)
                        .Where(
                        subscription =>
                            (subscription != null
                            && subscription.PropertyDefinitions != null
                            && (
                                    ((subscription.PropertyDefinitions.ContainsKey(activationProperties_ParentContentTypeId) && string.IsNullOrEmpty(subscription.PropertyDefinitions[activationProperties_ParentContentTypeId])))
                                    || !subscription.PropertyDefinitions.ContainsKey(activationProperties_ParentContentTypeId)
                               )
                            )
                            )
                        .ToList<IAveWorkflowSubscription>();
                    break;
                case SPWFAssociationParentType.ListContentType:
                    subscriptions = subscriptionService.EnumerateSubscriptionsByList((parentObj as IAveContentType).ParentList.ID)
                       .Where(
                       subscription =>
                           (subscription != null
                           && subscription.PropertyDefinitions != null
                           && ((subscription.PropertyDefinitions.ContainsKey(activationProperties_ParentContentTypeId)
                                && !string.IsNullOrEmpty(subscription.PropertyDefinitions[activationProperties_ParentContentTypeId])))
                           && string.Equals(subscription.PropertyDefinitions[activationProperties_ParentContentTypeId], (parentObj as IAveContentType).Parent.ID.ToString(), StringComparison.OrdinalIgnoreCase)
                            ))
                        .ToList<IAveWorkflowSubscription>();
                    break;
                case SPWFAssociationParentType.Web:
                    subscriptions = subscriptionService.EnumerateSubscriptionsByEventSource((parentObj as IAveWeb).ID).ToList();
                    break;
                case SPWFAssociationParentType.WebContentType:
                default:
                    break;
            }
            return ExecuteFilterFunction(subscriptions, parentType == SPWFAssociationParentType.ListContentType);
        }

        /// <summary>
        /// 调用filter的方法
        /// </summary>
        /// <param name="aveWorkflowSubscriptionCollection"></param>
        /// <returns></returns>
        private List<IAveWorkflowSubscription> ExecuteFilterFunction(List<IAveWorkflowSubscription> aveWorkflowSubscriptionCollection, bool IsContentTypeWorkflowCollection)
        {
            if (aveWorkflowSubscriptionCollection != null && this.FilterWorkflowFunction != null)
            {
                return aveWorkflowSubscriptionCollection
                    .Where(subScription => (
                        this.FilterWorkflowFunction(
                        new AveWorkflowAssociationInfo() { WorkFlowModle = AveWorkflowModel.Model2013, SubScriptionId = subScription.Id, DefinitionId = subScription.DefinitionId, CTName = null, Name = subScription.Name, IsCTWorkflowAssociation = IsContentTypeWorkflowCollection })))
                    .ToList();
            }
            return aveWorkflowSubscriptionCollection;
        }

        /// <summary>
        /// not used any more, will remove it later
        /// </summary>
        /// <param name="aveWorkflowSubscriptionCollection"></param>
        /// <returns></returns>
        [Obsolete]
        private List<IAveWorkflowSubscription> ExecuteFilterFanction(IAveWorkflowSubscriptionCollection aveWorkflowSubscriptionCollection)
        {
            if (this.FilterWorkflowFunction != null)
            {
                return aveWorkflowSubscriptionCollection.Where(subScription => this.FilterWorkflowFunction(new AveWorkflowAssociationInfo() { WorkFlowModle = AveWorkflowModel.Model2013, SubScriptionId = subScription.Id, DefinitionId = subScription.DefinitionId, CTName = null, Name = subScription.Name, IsCTWorkflowAssociation = false })).ToList();
            }
            return aveWorkflowSubscriptionCollection.ToList();
        }

        public SPWFAssociationUnit BackupOneAssociation(string monitor, IAveWorkflowSubscription subscription)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociation13ModelAPI.BackupAssociationUnit.BackupOneAssociation"))
            {
                SPWFAssociationUnit assoUnit = new SPWFAssociationUnit { ParentObject = mParentObject, ParentObjectType = mObjectType };

                SetAssociationUnitPropBySPAssociation(assoUnit, subscription, true);

                #region Performance Monitor Region
                mPerformanceMonitor.ResetCurrentDuration(monitor);
                #endregion

                if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType || assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
                {
                    using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociation13ModelAPI.BackupAssociationUnit.BackupOneAssociation.BackupTaskListUnit"))
                    {
                        if (mCachedSubListUnits.ContainsKey(assoUnit.SerializableData.mTaskListId))
                        {
                            assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.SerializableData.mTaskListId];
                        }
                        else
                        {
                            assoUnit.mTaskListUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.Tasks);
                            mCachedSubListUnits.AddEx(assoUnit.mTaskListUnit.SerializableData.mId, assoUnit.mTaskListUnit);
                        }
                    }

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Task List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    using (AvePerformanceScope pf2 = new AvePerformanceScope("WFAssociation13ModelAPI.BackupAssociationUnit.BackupOneAssociation.BackupHistListUnit"))
                    {
                        if (mCachedSubListUnits.ContainsKey(assoUnit.SerializableData.mHistoryListId))
                        {
                            assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.SerializableData.mHistoryListId];
                        }
                        else
                        {
                            assoUnit.mHistListUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.WorkflowHistory);
                            mCachedSubListUnits.AddEx(assoUnit.mHistListUnit.SerializableData.mId, assoUnit.mHistListUnit);
                        }
                    }

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup History List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion
                }

                using (AvePerformanceScope pf3 = new AvePerformanceScope("WFAssociation13ModelAPI.BackupAssociationUnit.BackupOneAssociation.BackupTemplateLibUnit"))
                {
                    if (!assoUnit.IsBuiltinBaseId && assoUnit.SerializableData.mIsDeclarative)
                    {
                        assoUnit.mTemplateLibUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.WFSVC);

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Template Library Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion
                    }
                }
                if (IsOnlineNintexWorkflow(assoUnit.WorflowDefinition.Properties))
                {
                    assoUnit.SerializableData.mFormFileUnit = GenerateFormFileCollection(Encoding.UTF8.GetString(assoUnit.mTemplateLibUnit.mTemplateFileUnits[0].SerializableData.mContent), assoUnit.ParentWeb);
                }
                //2013 platform workflow not support custom workflow
                //using (AvePerformanceScope pf4 = new AvePerformanceScope("WFAssociation13ModelAPI.BackupAssociationUnit.BackupOneAssociation.BackupCustomWorkflowData"))
                //{
                //    CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                //    customProc.FireBackupCustomWorkflowDataEvent(assoUnit);
                //}

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Custom Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion
                return assoUnit;
            }
        }

        private List<string> GenerateFormFilePath(string xamlFile, string webServerRelativeUrl)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xamlFile);
            var nfFilePathes = new List<string>();
            foreach (XmlNode child in document.DocumentElement.ChildNodes)
            {
                GenerateFormFilePath(child, webServerRelativeUrl, nfFilePathes);
            }
            return nfFilePathes;
        }

        private void GenerateFormFilePath(XmlNode node, string webServerRelativeUrl, List<string> nfFilePathes)
        {
            if (node.Attributes != null && node.Attributes["NFFileName"] != null)
            {
                nfFilePathes.Add(string.Format("{0}/NintexFormXml/{1}.xml", webServerRelativeUrl, node.Attributes["NFFileName"].Value));
            }
            foreach (XmlNode child in node.ChildNodes)
            {
                GenerateFormFilePath(child, webServerRelativeUrl, nfFilePathes);
            }
        }

        private List<SPWorkflowSubFileSerializableData> GenerateFormFileCollection(string xamlFile, IAveWeb parentweb)
        {
            var filePathes = GenerateFormFilePath(xamlFile, parentweb.ServerRelativeUrl);
            List<SPWorkflowSubFileSerializableData> formFiles = new List<SPWorkflowSubFileSerializableData>();
            foreach (var filePath in filePathes)
            {
                var file = parentweb.GetFile(filePath);
                if (file.Exists)
                {
                    formFiles.Add(SPWorkflowSubFileUnit.GenerateSPFileUnit(file).SerializableData);
                }
            }
            return formFiles;
        }




        private void SetAssociationUnitPropBySPAssociation(SPWFAssociationUnit assoUnit, IAveWorkflowSubscription workflowSubscription, bool isBackup)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociation13ModelAPI.BackupAssociationUnit.BackupOneAssociation.SetAssociationUnitPropBySPAssociation"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetAssociationUnitPropBySPAssociation");
                string monitor = "Set Association Properties";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    CultureInfo currentWebCulture = assoUnit.ParentWeb.LanguageCulture;
                    //PR模块使用，由于PR模块是使用UnattachDatabase方式获取SPSite对象，不能通过site id直接new SPSite对象。所以加重载方法，直接传site对象。
                    IAveWorkflowDefinition workflowDefinition = null;
                    if (SPWorkflowProcessorRuntime.GetSP2013WorkflowDefinitionForPR && isBackup)
                    {
                        workflowDefinition = WFDeploymentService.GetDefinition(workflowSubscription.DefinitionId, ParentWeb.Site);
                    }
                    else
                    {
                        workflowDefinition = WFDeploymentService.GetDefinition(workflowSubscription.DefinitionId);
                    }
                    assoUnit.mWorkflowDefinition = workflowDefinition;
                    assoUnit.mWorkflowSubscription = workflowSubscription;
                    assoUnit.SerializableData.mId = workflowSubscription.Id;
                    if (isBackup)
                        assoUnit.SerializableData.mSourceId = workflowDefinition.Id;
                    assoUnit.SerializableData.mBaseId = Guid.Empty;
                    SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "BaseId", assoUnit.SerializableData.mBaseId.ToString());
                    assoUnit.SerializableData.mAuthor = workflowSubscription.PropertyDefinitions.ContainsKey("SharePointWorkflowContext.Subscription.AuthorId") ? int.Parse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.AuthorId"].Split(new char[] { ';' })[0]) : -1;
                    assoUnit.SerializableData.mAuthorLoginName = workflowSubscription.PropertyDefinitions.ContainsKey("SharePointWorkflowContext.Subscription.AuthorLogin") ? workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.AuthorLogin"] : string.Empty;
                    assoUnit.SerializableData.mAutoCleanupDays = -1;
                    assoUnit.SerializableData.mConfiguration = -1; //(int)LSInvoker.GetProperty(asso, "Configuration");
                    assoUnit.SerializableData.mContentTypeId = workflowSubscription.PropertyDefinitions.ContainsKey("ContentTypeId") ? workflowSubscription.PropertyDefinitions["ContentTypeId"] : "0x";
                    if (assoUnit.SerializableData.mContentTypeId == "0x")
                    {
                        assoUnit.SerializableData.mContentTypeId = null;
                    }

                    if (isBackup)
                    {
                        //SPD workflow preversion Association Name comes from
                        //
                        //SP2007
                        //Microsoft.SharePoint.SoapServer.WebPartPagesWebService.DoWorkflowAssociateWithList
                        //web.RegionalSettings.TimeZone.UTCToLocalTime(associationByBaseID.Created).ToString("G", web.Locale)
                        //
                        //SP2010
                        //Microsoft.SharePoint.Workflow.SPWorkflowNoCodeSupport.GenerateUniqueOldAssociationName
                        //web.RegionalSettings.TimeZone.UTCToLocalTime(oldAssociation.Created).ToString("G", web.Locale)
                        //
                        //SPWorkflowAssociation.Created is a utc time,so for found the pre-restored preversion association, we must convert created to local time
                        //assoUnit.SerializableData.mCreated = ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(DateTime.Parse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.CreatedDate"]));
                        DateTime createdDate = DateTime.MinValue;
                        if (DateTime.TryParse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.CreatedDate"], currentWebCulture, DateTimeStyles.None, out createdDate))
                        {
                            assoUnit.SerializableData.mCreated = ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(createdDate);
                        }
                        else
                        {
                            logger.Warn("Invalid created time format in workflow subscription. TimeString:{0}", workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.CreatedDate"]);
                            assoUnit.SerializableData.mCreated = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        DateTime createdDate = DateTime.MinValue;
                        if (DateTime.TryParse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.CreatedDate"], currentWebCulture, DateTimeStyles.None, out createdDate))
                        {
                            assoUnit.SerializableData.mCreated = createdDate;
                        }
                    }
                    assoUnit.SerializableData.mDescription = workflowDefinition.Description;

                    if (workflowSubscription.PropertyDefinitions.ContainsKey("Microsoft.SharePoint.ActivationProperties.ListId"))
                    {
                        assoUnit.SerializableData.mParentListId = new Guid(workflowSubscription.PropertyDefinitions["Microsoft.SharePoint.ActivationProperties.ListId"]);
                        string tempListName = string.Empty;
                        workflowSubscription.PropertyDefinitions.TryGetValue("Microsoft.SharePoint.ActivationProperties.ListName", out tempListName);
                        SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "ParentList", tempListName);
                    }
                    else if (mObjectType == SPWFAssociationParentType.List)
                    {
                        assoUnit.SerializableData.mParentListId = ((IAveList)mParentObject).ID;
                    }
                    //Change for Platform,because we can't get object from Sharepoint when run platform job if source has been deleted.
                    if (isBackup)
                    {
                        switch (this.ParentObjectType)
                        {
                            case SPWFAssociationParentType.List:
                                IAveList list = (IAveList)assoUnit.ParentObject;
                                assoUnit.SerializableData.mParentId = list.ID.ToString("B");
                                break;
                            case SPWFAssociationParentType.ListContentType:
                            case SPWFAssociationParentType.WebContentType:
                                IAveContentType ct = (IAveContentType)assoUnit.ParentObject;
                                assoUnit.SerializableData.mParentId = ct.ID.ToString();
                                break;
                            case SPWFAssociationParentType.Web:
                                IAveWeb web = (IAveWeb)assoUnit.ParentObject;
                                assoUnit.SerializableData.mParentId = web.ID.ToString("B");
                                break;
                            default:
                                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                        }
                        assoUnit.SerializableData.mOriginalParentId = assoUnit.SerializableData.mParentId;
                        string parentContentTypeId = string.Empty;
                        if (workflowSubscription.PropertyDefinitions.TryGetValue(activationProperties_ParentContentTypeId, out parentContentTypeId))
                        {
                            assoUnit.SerializableData.Properties.AddEx(activationProperties_ParentContentTypeId, parentContentTypeId != null ? parentContentTypeId : string.Empty);
                        }
                    }
                    else
                    {
                        assoUnit.SerializableData.mParentId = assoUnit.ParentId;
                    }
                    assoUnit.SerializableData.mHistoryListId = workflowSubscription.PropertyDefinitions.ContainsKey("HistoryListId") ? new Guid(workflowSubscription.PropertyDefinitions["HistoryListId"]) : Guid.Empty;
                    try
                    {
                        assoUnit.SerializableData.mHistoryListTitle = ParentWeb.Lists.GetById(assoUnit.SerializableData.mHistoryListId).Title;
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                        assoUnit.SerializableData.mHistoryListTitle = string.Empty;
                        logger.Log(AveLogLevel.DEBUG, "An error occurred while getting history list title, error message: {0}", e);
                    }
                    assoUnit.SerializableData.mInstanceCount = -1;
                    assoUnit.SerializableData.mInstanceCountDirty = -1;
                    assoUnit.SerializableData.mInstantiationParams = string.Empty;
                    if (isBackup)
                    {
                        //for nintex statistics
                        DateTime modifiedDate = DateTime.MinValue;
                        if (DateTime.TryParse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.ModifiedDate"], currentWebCulture, DateTimeStyles.None, out modifiedDate))
                        {
                            assoUnit.SerializableData.mModified = ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(modifiedDate);
                        }
                        else
                        {
                            logger.Warn("Invalid modified time format in workflow subscription. TimeString:{0}", workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.ModifiedDate"]);
                            assoUnit.SerializableData.mModified = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        DateTime modifiedDate = DateTime.MinValue;
                        if (DateTime.TryParse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.ModifiedDate"], currentWebCulture, DateTimeStyles.None, out modifiedDate))
                        {
                            assoUnit.SerializableData.mModified = modifiedDate;
                        }
                        else
                        {
                            logger.Warn("Invalid modified time format in workflow subscription. TimeString:{0}", workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.ModifiedDate"]);
                        }
                    }
                    assoUnit.SerializableData.mName = workflowDefinition.DisplayName;
                    if (isBackup)
                    {
                        assoUnit.SerializableData.mOriginalName = workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.DisplayName"];
                    }

                    if (isBackup)
                    {
                        assoUnit.SerializableData.mStatusFieldName = workflowSubscription.PropertyDefinitions.ContainsKey("StatusFieldName") ? workflowSubscription.PropertyDefinitions["StatusFieldName"] : workflowDefinition.Properties["StatusFieldName"];
                        if (!string.IsNullOrEmpty(assoUnit.SerializableData.mStatusFieldName))
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "StatusFieldName", assoUnit.SerializableData.mStatusFieldName);
                        }
                        else
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "StatusFieldName", "NULL");
                        }
                    }
                    assoUnit.SerializableData.mTaskListId = workflowSubscription.PropertyDefinitions.ContainsKey("TaskListId") ? new Guid(workflowSubscription.PropertyDefinitions["TaskListId"]) : Guid.Empty;
                    try
                    {
                        assoUnit.SerializableData.mTaskListTitle = ParentWeb.Lists[assoUnit.SerializableData.mTaskListId].Title;// asso.TaskListTitle;
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                        assoUnit.SerializableData.mTaskListTitle = string.Empty;
                        logger.Log(AveLogLevel.DEBUG, "An error occurred while getting task list title, error message: {0}", e);
                    }

                    assoUnit.SerializableData.mVersion = int.Parse(workflowSubscription.PropertyDefinitions["WorkflowVersion"]);
                    assoUnit.SerializableData.mIsDeclarative = true;
                    assoUnit.SerializableData.mInternalName = workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.DisplayName"];

                    if (workflowSubscription.PropertyDefinitions.ContainsKey("Microsoft.SharePoint.ActivationProperties.ListId") && !string.IsNullOrEmpty(workflowSubscription.PropertyDefinitions["Microsoft.SharePoint.ActivationProperties.ListId"]))
                    {
                        assoUnit.mListId = new Guid(workflowSubscription.PropertyDefinitions["Microsoft.SharePoint.ActivationProperties.ListId"]);
                        assoUnit.SerializableData.mIsDefaultContentApprovalWorkflow = false;
                    }

                    assoUnit.mWebId = ParentWeb.ID;
                    assoUnit.mSiteId = ParentWeb.Site.ID;

                    if (isBackup)
                    {
                        Dictionary<string, object> propsFor13Model = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, string> kv in workflowSubscription.PropertyDefinitions)
                        {
                            propsFor13Model.AddEx(kv.Key, kv.Value);
                        }
                        assoUnit.SerializableData.Properties.AddEx(SPWorkflowCommon.PROPS_13MODEL, propsFor13Model);
                        Dictionary<string, object> propsFor13ModelWorkflowDef = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, string> kv in workflowDefinition.Properties)
                        {
                            propsFor13ModelWorkflowDef.AddEx(kv.Key, kv.Value);
                        }
                        assoUnit.SerializableData.Properties.AddEx(SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION, propsFor13ModelWorkflowDef);

                        //ADO - 167262 为了处理多语言还原时culture不同导致的数据格式不同的情况,为了兼容老数据,还原时需要注意判断属性是否存在
                        if (currentWebCulture != null)
                        {
                            assoUnit.SerializableData.Properties.AddEx(SPWorkflowCommon.PROPS_13MODEL_WebLanguageId, currentWebCulture.LCID);
                        }
                    }
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                    logger.Log(AveLogLevel.DEBUG, "An error occurred while setting workflow association unit property by SP workflow association, error message: {0}", e);
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSetUnitException, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetAssociationUnitPropBySPAssociation");
                }
            }
        }
        #endregion

        #region ************************Restore Region************************
        public override int Restore(SPWFAssociationUnit assoUnit)
        {
            return Restore(assoUnit, true, false);
        }

        public override int Restore(byte[] serializedData)
        {
            return Restore(serializedData, true);
        }

        public override int Restore(SPWFAssociationUnit unit, bool forceUpdate, bool isPostAction)
        {
            mForceUpdate = forceUpdate;
            return RestoreAssociationUnit(unit);
        }
        private bool IsNintexFormAppInstalled(IAveWeb parentWeb)
        {
            Guid NintexFormsAppProductId = new Guid("353e0dc9-57f5-40da-ae3f-380cd5385ab9");
            var nintexFormAppInstance = parentWeb.GetAppInstancesByProductId(NintexFormsAppProductId);
            return nintexFormAppInstance.Count > 0;
        }

        private int RestoreAssociationUnit(SPWFAssociationUnit assoUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreAssociationUnit13Model");
                string monitor = mMainMonitorLog = "Association Restore";
                assoUnit.ParentObject = mParentObject;
                assoUnit.ParentObjectType = mObjectType;
                assoUnit.ParentAveSPWeb = AveSPObjectCache.AveSPWeb;
                assoUnit.WebLevelFieldProcessorCollection = this.WebLevelFieldProcessorCollection;

                byte[] cacheData = SPWFAssociationUnit.Save(assoUnit);

                switch (assoUnit.ParentObjectType)
                {
                    case SPWFAssociationParentType.List:
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                    case SPWFAssociationParentType.Web:
                        break;
                    default:
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                }

                if (assoUnit.ParentObject == null)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);
                }
                //当备份数据中存在nintex form 的数据时，说明这个Workflow是nintex Workflow结合了nintex form的数据，需要判断nintex form 是否安装
                if (assoUnit.SerializableData.mFormFileUnit.Count > 0 && !IsNintexFormAppInstalled(assoUnit.ParentWeb))
                {
                    if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                    {
                        throw new SPWFProcessorException("Can not restore nintex workflow with nintex form, please install nintex form app first.");
                    }
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction);
                }

                //记录event receiver状态
                bool eventFiringDisabled = SPEventManagerWrapper.EventFiringDisabled;
                try
                {
                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + assoUnit.SerializableData.mName);
                    mPerformanceMonitor.StartMonitor(monitor);
                    #endregion

                    IAveWeb parentSPWeb = assoUnit.ParentWeb;
                    {
                        IAveList taskList = null;
                        IAveList histList = null;
                        if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType || assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
                        {
                            bool taskIsNewCreated = !mCachedSubListUnits.ContainsKey(assoUnit.mTaskListUnit.SerializableData.mId);
                            bool histIsNewCreated = !mCachedSubListUnits.ContainsKey(assoUnit.mHistListUnit.SerializableData.mId);
                            bool canUseCache = (!taskIsNewCreated) && (!histIsNewCreated);
                            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.RestoreTaskList"))
                            {
                                #region Get or Create Task List
                                if (canUseCache)
                                {
                                    assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.mTaskListUnit.SerializableData.mId];
                                    assoUnit.mTaskListUnit.mSPList = parentSPWeb.Lists.GetListById(assoUnit.mTaskListUnit.mSPList.ID, true);
                                }
                                else
                                {
                                    SPWorkflowSubListUnit.GetOrCreateSPList(assoUnit, parentSPWeb.Lists, assoUnit.mTaskListUnit, assoUnit.WebLevelFieldProcessorCollection);
                                    mCachedSubListUnits.AddEx(assoUnit.mTaskListUnit.SerializableData.mId, assoUnit.mTaskListUnit);
                                }
                                taskList = assoUnit.mTaskListUnit.mSPList;
                                SPWorkflowProcessorRuntime.Log(Logs.AP_RestoreSubListFinish, taskList.Title);
                                #endregion
                            }

                            #region Performance Monitor Region
                            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Task List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            #endregion

                            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.RestoreHistList"))
                            {
                                #region Get or Create History List
                                if (canUseCache)
                                {
                                    assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.mHistListUnit.SerializableData.mId];
                                    assoUnit.mHistListUnit.mSPList = parentSPWeb.Lists.GetListById(assoUnit.mHistListUnit.mSPList.ID, true);
                                }
                                else
                                {
                                    SPWorkflowSubListUnit.GetOrCreateSPList(assoUnit, parentSPWeb.Lists, assoUnit.mHistListUnit, assoUnit.WebLevelFieldProcessorCollection);
                                    mCachedSubListUnits.AddEx(assoUnit.mHistListUnit.SerializableData.mId, assoUnit.mHistListUnit);
                                }
                                histList = assoUnit.mHistListUnit.mSPList;
                                #endregion
                            }

                            #region Performance Monitor Region
                            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore History List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            #endregion
                        }

                        ConflicStatus13Model cStatus = HandleWorkflowAssociationConflict(assoUnit);

                        SPEventManagerWrapper.EnableEventFiring();

                        if (cStatus == ConflicStatus13Model.None)
                        {
                            #region Performance Monitor Region
                            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Handle Conflict. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            #endregion

                            RemoveExistWorkflowStatusColumn(assoUnit);

                            IAveWorkflowSubscription workflowSubscription = null;
                            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.MarkupWorkflowAssociation"))
                            {
                                #region Markup Workflow Association
                                if (assoUnit.mTemplateLibUnit != null)
                                {
                                    SPWorkflowSubListUnit.GetOrCreateSPList(assoUnit, parentSPWeb.Lists, assoUnit.mTemplateLibUnit, assoUnit.WebLevelFieldProcessorCollection);

                                    #region Performance Monitor Region

                                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Template Library Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));

                                    #endregion

                                    string xamlFileContent;
                                    if (SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(assoUnit, assoUnit.mTemplateLibUnit, out xamlFileContent))
                                    {
                                        // ADO-132265,parentObject的ParentWeb不是创建workflow 相关list的web,save definition时会抛错 所以新建list后需reload下,目的是Reload List.ParentWeb
                                        //365中save definition是在request中做的，save的时候所有API对象都是新取的，不存在上述问题，而且365 AllWebs cache不更新，会导致取不到该list(ADO-141479),因此过滤掉，365不走该重取逻辑
                                        //if (ParentObjectType != null && ParentObjectType == SPWFAssociationParentType.List && SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                                        //{
                                        //    ParentObject = tempSPWeb.Lists[((IAveList)ParentObject).ID];
                                        //}
                                        IAveWorkflowDefinition workflowDefinition = PublishWorkflowDefinition(assoUnit, xamlFileContent);
                                        if (!UseNintexAPIPublish(workflowDefinition.Properties))
                                        {
                                            workflowSubscription = PublishWorkflowSubscription(assoUnit, workflowDefinition, true);
                                        }
                                    }

                                    #region Performance Monitor Region

                                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Associate Markup Workflow. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));

                                    #endregion
                                }

                                #endregion
                            }
                            if (workflowSubscription != null)
                            {
                                AddAssociationToRestoredCollection(assoUnit, workflowSubscription);

                                #region Performance Monitor Region
                                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Update Association Unit Properties. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                                #endregion
                            }
                        }
                        else if (cStatus == ConflicStatus13Model.Configuration)
                        {
                            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.ConfigurationWorkflowAssociation"))
                            {
                                string xamlFileContent;
                                SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(assoUnit, assoUnit.mTemplateLibUnit, out xamlFileContent);

                                IAveWorkflowDefinition workflowDefinition = PublishWorkflowDefinition(assoUnit, xamlFileContent);

                                if (!UseNintexAPIPublish(workflowDefinition.Properties))
                                {
                                    PublishWorkflowSubscription(assoUnit, workflowDefinition, false);
                                }
                                AddAssociationToRestoredCollection(assoUnit, assoUnit.mWorkflowSubscription);
                            }
                        }

                        assoUnit.DisposeSubListUnits();

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Custom Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion

                        SPWorkflowProcessorRuntime.Log(Logs.AP_RestoreFinish, assoUnit.SerializableData.mOriginalName);
                    }

                }
                catch (SPWFProcessorException procException)
                {
                    ParseExceptionToCacheData(procException, assoUnit, cacheData);
                    throw;
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, assoUnit.SerializableData.mId);
                }
                finally
                {
                    #region Performance Monitor Region
                    mPerformanceMonitor.StopMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    #endregion

                    if (eventFiringDisabled)
                    {
                        SPEventManagerWrapper.DisableEventFiring();
                    }
                    else
                    {
                        SPEventManagerWrapper.EnableEventFiring();
                    }
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreAssociationUnit13Model");
                }


                return 0;
            }
        }

        public override void RestoreReusableWFTemplate(SPWFAssociationUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreReusableWFTemplate"))
            {
                unit.ParentObject = mParentObject;
                unit.ParentObjectType = mObjectType;
                unit.WebLevelFieldProcessorCollection = this.WebLevelFieldProcessorCollection;
                //记录event receiver状态
                bool eventFiringDisabled = SPEventManagerWrapper.EventFiringDisabled;

                if (unit.ParentObject == null)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);
                }

                byte[] cacheData = SPWFAssociationUnit.Save(unit);

                try
                {
                    IAveWeb parentSPWeb = unit.ParentWeb;
                    {

                        SPEventManagerWrapper.EnableEventFiring();

                        if (unit.mTemplateLibUnit != null)
                        {
                            IAveList tempList = SPWorkflowSubListUnit.GetOrCreateSPList(unit, parentSPWeb.Lists, unit.mTemplateLibUnit, unit.WebLevelFieldProcessorCollection);


                            string xamlFileContent = string.Empty;
                            SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(unit, unit.mTemplateLibUnit, out xamlFileContent);

                            IAveWorkflowDefinition workflowDefinition = PublishWorkflowDefinition(unit, xamlFileContent);

                            AddRestoredWorkflowTemplateToCache(unit, workflowDefinition);
                        }
                        else
                        {
                            throw new AveWrapperSkipException("Skip restoring the workflow template as mTemplateLibUnit is null.");
                        }

                        using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.RestoreCustomWorkflowData"))
                        {
                            CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                            customProc.FireRestoreCustomWorkflowDataEvent(unit);
                        }
                    }

                    unit.DisposeSubListUnits();
                }
                catch (SPWFProcessorException procException)
                {
                    ParseExceptionToCacheData(procException, unit, cacheData);
                    throw;
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, unit.SerializableData.mId);
                }
                finally
                {
                    if (eventFiringDisabled != SPEventManagerWrapper.EventFiringDisabled)
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
            }
        }

        private void AddRestoredWorkflowTemplateToCache(SPWFAssociationUnit assoUnit, IAveWorkflowDefinition definition)
        {
            IAveWeb parentWeb = assoUnit.ParentWeb;

            if (!SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(parentWeb.Site.ID, parentWeb.ID, definition.Id))
            {
                // Reusable Workflow Template ,而且不在cache中
                SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Add(parentWeb.Site.ID, parentWeb.ID, definition.Id, definition.Id);
            }
            //else
            //{
            //    //already in cache, not need to add again
            //}

        }

        public override int Restore(byte[] serializedData, bool forceUpdate)
        {
            try
            {
                SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(serializedData);
                return Restore(assoUnit, forceUpdate, false);
            }
            catch (SPWFProcessorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e);
            }
        }

        private void SetOnlineNintexWorkflowDefinitionProperties(IAveWorkflowDefinition workflowDefinition, Dictionary<string, object> workflowDefinitionProps, SPWFAssociationUnit assoUnit)
        {
            string[] nintexNeedUpdateProperties = new string[] { "SPDConfig.TaskListID", "SPDConfig.HistoryListID", "NWConfig.Designer", "SPDConfig.LastEditMode", "VariableInfo", "NWConfig.Region", "IsProjectMode" };
            /* AppAuthor、AppEditor 这两个属性还没有找到如何获取,并且不影响Nintex 的还原
               SubscriptionId、SubscriptionName 这两个属性不影响nintex还原 暂时先不更新*/
            foreach (var nintexPropertry in nintexNeedUpdateProperties)
            {
                object propertyValue;
                if (workflowDefinitionProps.TryGetValue(nintexPropertry, out propertyValue))
                {
                    if (string.Equals("SPDConfig.TaskListID", nintexPropertry, StringComparison.OrdinalIgnoreCase))
                    {
                        workflowDefinition.SetProperty(nintexPropertry, GetTaskListId(assoUnit).ToString());
                    }
                    else if (string.Equals("SPDConfig.HistoryListID", nintexPropertry, StringComparison.OrdinalIgnoreCase))
                    {
                        workflowDefinition.SetProperty(nintexPropertry, GetHistoryListId(assoUnit).ToString());
                    }
                    else
                    {
                        workflowDefinition.SetProperty(nintexPropertry, propertyValue.ToString());
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are workflow properties")]
        private IAveWorkflowDefinition PublishWorkflowDefinition(SPWFAssociationUnit assoUnit, string xamlFileContent)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.PublishWorkflowDefinition"))
            {
                IAveWorkflowDefinition workflowDefinition = null;
                Guid azureWorkflowDefintionId = Guid.Empty;
                //For Reusable workflow definition
                bool isReuableWorkflow = false;
                bool isBuiltInWorkflow = false;
                string restrictToType = string.Empty;
                Dictionary<string, object> workflowDefinitionProps = null;
                if (assoUnit.SerializableData.Properties.ContainsKey(SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION))
                {
                    workflowDefinitionProps = (Dictionary<string, object>)assoUnit.SerializableData.Properties[SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION];
                    if (workflowDefinitionProps.ContainsKey("isReusable"))
                    {
                        isReuableWorkflow = Boolean.Parse(workflowDefinitionProps["isReusable"].ToString());
                    }
                    else
                    {
                        isBuiltInWorkflow = true;
                    }
                    if (workflowDefinitionProps.ContainsKey("RestrictToType"))
                    {
                        restrictToType = workflowDefinitionProps["RestrictToType"].ToString();
                    }
                }
                //find exist
                workflowDefinition = FindWorkflowDefinitionByName(assoUnit, assoUnit.SerializableData.mName, isReuableWorkflow | isBuiltInWorkflow, restrictToType);
                if (isReuableWorkflow && workflowDefinition != null && SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(assoUnit.ParentWeb.Site.ID, assoUnit.ParentWeb.ID, workflowDefinition.Id))
                {
                    //找到了reusable definition，并且还原过了，直接返回即可，只还原一次reusable definition
                    return workflowDefinition;
                }
                if (workflowDefinition == null)
                {
                    workflowDefinition = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowDefinition();
                }
                workflowDefinition.DisplayName = assoUnit.SerializableData.mName;
                workflowDefinition.Xaml = xamlFileContent;
                workflowDefinition.Description = assoUnit.SerializableData.mDescription;
                if (isBuiltInWorkflow)
                {
                    workflowDefinition.Id = assoUnit.SerializableData.mBaseId;
                }
                string[] needUpdateproperties = new string[] { "SPDConfig.StartOnCreate", "SPDConfig.StartManually", "SPDConfig.StartOnChange", "AutosetStatusToStageName", "FormField", "RequiresInitiationForm", "Definition.CreatedDateUTC", "Definition.ModifiedDateUTC" };
                //ADO-167262 处理原端目的端站点culture info不同导致数据格式不同的情况使用
                var culture = GetLanguageCulture(assoUnit);

                foreach (var prop in needUpdateproperties)
                {
                    object value;
                    if (workflowDefinitionProps != null && workflowDefinitionProps.TryGetValue(prop, out value))
                    {

                        if (string.Equals(prop, "Definition.CreatedDateUTC", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prop, "Definition.ModifiedDateUTC", StringComparison.OrdinalIgnoreCase))
                        {
                            DateTime dateTime;
                            if (IsDateTimeAvaliable(value.ToString(), culture, out dateTime))
                            {
                                value = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToString(assoUnit.ParentWeb.LanguageCulture);
                            }
                        }
                        workflowDefinition.SetProperty(prop, value.ToString());
                    }
                }

                if (IsOnlineNintexWorkflow(workflowDefinitionProps))
                {
                    SetOnlineNintexWorkflowDefinitionProperties(workflowDefinition, workflowDefinitionProps, assoUnit);
                    //使用NintexAPI publish workflow时 需要有该属性
                    workflowDefinition.SetProperty("SubscriptionId", Guid.NewGuid().ToString("B"));
                }

                if (!isBuiltInWorkflow)
                {
                    if (isReuableWorkflow)
                    {
                        workflowDefinition.SetProperty("RestrictToType", restrictToType);
                        //reusable workflow 只有RestrictToType属性，没有RestrictToScope
                    }
                    else if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web && IsOnlineNintexWorkflow(workflowDefinitionProps))
                    {
                        //ADO-191305 Nintex 更新后site level workflow RestrictToScope property的value是null，如果设置为assoUnit.ParentWeb.ID，
                        //则会导致nintex workflow gallery 上不显示
                        workflowDefinition.SetProperty("RestrictToType", "Site");
                    }
                    else if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
                    {
                        workflowDefinition.SetProperty("RestrictToType", "Site");
                        workflowDefinition.SetProperty("RestrictToScope", assoUnit.ParentWeb.ID.ToString());
                    }
                    else if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
                    {
                        workflowDefinition.SetProperty("RestrictToType", "List");
                        workflowDefinition.SetProperty("RestrictToScope", assoUnit.ParentList.ID.ToString());
                    }
                    else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                    {
                        workflowDefinition.SetProperty("RestrictToType", "List");
                        workflowDefinition.SetProperty("RestrictToScope", assoUnit.ParentContentType.ParentList.ID.ToString());
                    }
                    workflowDefinition.SetProperty("isReusable", isReuableWorkflow.ToString());
                }
                //此处的try catch是为了避免web对象不一致时导致SaveDefinition出错
                //如果以后找到重现规律并解决此类问题或者确认此类问题不存在时，去掉此处try catch reload逻辑


                try
                {
                    azureWorkflowDefintionId = this.WFDeploymentService.SaveDefinition(workflowDefinition);
                }
                catch (Exception ex)
                {
                    logger.Info("cannot save workflow definition, try reload workflow service manager and save it. exception:{0}", ex.ToString());

                    //assoUnit的ParentObject和当前类中的mParentObject应该是同一个对象，ReloadWeb需要保证Child都reload，所以用AssoUnit上的方法，reload后再给当前对象重新赋值
                    assoUnit.ReloadParentWeb();
                    this.mParentObject = assoUnit.ParentObject;

                    Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).UpdateWorkflowServiceManager(ParentWeb, true);
                    azureWorkflowDefintionId = this.WFDeploymentService.SaveDefinition(workflowDefinition);
                }
                if (assoUnit.SerializableData.mFormFileUnit.Count > 0)
                {
                    AddNintexFormFile(assoUnit.SerializableData.mFormFileUnit, assoUnit);
                }


                //ADO-213866 如果目的端没有publish 过nintex workflow 那么publish workflow 会失败，测试使用的action 是send email 没有测试过其他action是否存在同样问题， 现逻辑改成 如果是nintex workflow 那么直接使用nintex api 来publish
                if (UseNintexAPIPublish(workflowDefinition.Properties) && AveSPObjectCache.AveSPWeb.ParentSite.SPSite.UserAccountInfo.ConnectionType != BposConnectionType.AppToken)
                {
                    try
                    {
                        //测试时发现如果不使用API publish 结合了nintex form的nintex workflow的话，转移到目的端的nintex workflow打开时会产生空引用异常
                        ParentWeb.PublishNintexWorkflow(azureWorkflowDefintionId);
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while publish nintex workflow with nintex api, try to use SharePoint api to publish workflow. Error:{0}", e);
                        //Nintex api publish 失败时 使用SharePoint API 再次尝试publish
                        WFDeploymentService.PublishDefinition(azureWorkflowDefintionId);
                    }
                }
                else
                {
                    WFDeploymentService.PublishDefinition(azureWorkflowDefintionId);
                }

                //return workflowDefinition;
                return WFDeploymentService.GetDefinition(azureWorkflowDefintionId);
            }
        }
        private bool IsNintexWorkflow(IDictionary<string, string> workflowProperties)
        {
            const string nintexWorkflowPropertyKey = "NWConfig.Designer";
            if (workflowProperties != null)
            {
                return workflowProperties.ContainsKey(nintexWorkflowPropertyKey);
            }
            return false;
        }
        private bool UseNintexAPIPublish(IDictionary<string, string> workflowProperties)
        {
            return IsNintexWorkflow(workflowProperties);
        }

        private void AddNintexFormFile(List<SPWorkflowSubFileSerializableData> formFiles, SPWFAssociationUnit assoUnit)
        {
            try
            {
                var parentWeb = (IAveWeb)assoUnit.ParentWeb;
                foreach (var formFile in formFiles)
                {
                    var fileContent = Encoding.UTF8.GetString(formFile.mContent);
                    var nintexFormServie = new AvePoint.Wrapper.Restore.NintexForm.NintexFormContentProcessorOnline(assoUnit.ParentAveSPWeb, assoUnit.ParentList);
                    fileContent = nintexFormServie.ReplaceFormContent(fileContent, string.Empty, true);

                    string fileServerRelativeUrl = string.Format(@"{0}/NintexFormXml/{1}", parentWeb.ServerRelativeUrl, formFile.mName);
                    var file = parentWeb.GetFile(fileServerRelativeUrl);
                    if (file.Exists)
                    {
                        logger.Info("The file {0} exist in web, skip to restore this nintex form file.", fileServerRelativeUrl);
                        continue;
                    }
                    var nintexFormList = parentWeb.Lists.GetListByName("NintexFormXml", false);
                    if (nintexFormList == null)
                    {
                        var nintexFormListId = parentWeb.Lists.Add("NintexFormXml", "", "NintexFormXml", Guid.Empty.ToString(), (int)AveListTemplateType.DocumentLibrary, "", AveQuickLaunchOptions.Off);
                        nintexFormList = parentWeb.GetList(nintexFormListId);
                        nintexFormList.Hidden = true;
                        nintexFormList.Update();
                    }
                    nintexFormList.RootFolder.Files.Add(new AveFileCreationInformation { Url = string.Format("{0}/{1}", nintexFormList.RootFolder.ServerRelativeUrl, formFile.mName), Content = formFile.mContent });
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while add nintex form file to NintexFormXml list, error: {0}", e);
            }
        }

        private CultureInfo GetLanguageCulture(SPWFAssociationUnit associationUnit)
        {
            if (associationUnit == null)
            {
                throw new ArgumentNullException("associationUnit");
            }
            var languageId = associationUnit.SerializableData.Properties.Contains(SPWorkflowCommon.PROPS_13MODEL_WebLanguageId)
                              ? (int)associationUnit.SerializableData.Properties[SPWorkflowCommon.PROPS_13MODEL_WebLanguageId]
                              : (int)associationUnit.ParentWeb.Language;
            return new CultureInfo(languageId);
        }

        private static bool IsDateTimeAvaliable(string propertyValue, IFormatProvider cultureInfo, out DateTime dateTime)
        {
            var isAvaliable = false;

            if (DateTime.TryParse(propertyValue, cultureInfo, DateTimeStyles.None, out dateTime))
            {
                isAvaliable = true;
            }
            else
            {
                dateTime = DateTime.UtcNow;
            }

            return isAvaliable;
        }

        private Guid GetHistoryListId(SPWFAssociationUnit assoUnit)
        {
            if (assoUnit.mHistListUnit.mSPList == null)
            {
                logger.Warn("History list is nul, can not get history list id.");
                return Guid.Empty;
            }
            return assoUnit.mHistListUnit.mSPList.ID;
        }

        private Guid GetTaskListId(SPWFAssociationUnit assoUnit)
        {
            if (assoUnit.mTaskListUnit.mSPList == null)
            {
                logger.Warn("Task list is nul, can not get task list id.");
                return Guid.Empty;
            }
            return assoUnit.mTaskListUnit.mSPList.ID;
        }

        private IAveWorkflowSubscription PublishWorkflowSubscription(SPWFAssociationUnit assoUnit, IAveWorkflowDefinition workflowDefinition, bool isNewCreate)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.PublishWorkflowSubscription"))
            {
                Guid subscriptionId = Guid.NewGuid();
                IAveWorkflowSubscription definitionSubscription = null;
                if (isNewCreate)
                {
                    definitionSubscription = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowSubscription();
                    definitionSubscription.Id = subscriptionId;
                }
                else
                {
                    definitionSubscription = assoUnit.mWorkflowSubscription;
                }
                definitionSubscription.Name = assoUnit.SerializableData.mOriginalName;
                if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
                {
                    definitionSubscription.EventSourceId = assoUnit.ParentList.ID;
                }
                else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                {
                    definitionSubscription.EventSourceId = assoUnit.ParentContentType.ParentList.ID;
                    IAveContentType ct = assoUnit.ParentObject as IAveContentType;
                    if (ct != null && ct.Parent != null && ct.Parent.ID != null)
                    {
                        definitionSubscription.SetProperty(activationProperties_ParentContentTypeId, ct.Parent.ID.ToString());
                    }
                }
                else if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
                {
                    definitionSubscription.EventSourceId = assoUnit.ParentWeb.ID;
                }
                definitionSubscription.EventTypes = new List<string>();
                definitionSubscription.DefinitionId = workflowDefinition.Id;

                definitionSubscription.SetProperty("HistoryListId", GetHistoryListId(assoUnit).ToString().ToUpper(CultureInfo.InvariantCulture));
                definitionSubscription.SetProperty("TaskListId", GetTaskListId(assoUnit).ToString().ToUpper(CultureInfo.InvariantCulture));
                if (assoUnit.SerializableData.Properties.Contains("Props.13Model"))
                {
                    string[] needUpdateProperties = new string[] { "SharePointWorkflowContext.Subscription.CreatedDate", "SharePointWorkflowContext.Subscription.ModifiedDate", "CreatedBySPD" };
                    var properties = assoUnit.SerializableData.Properties["Props.13Model"] as Dictionary<string, object>;
                    var culture = GetLanguageCulture(assoUnit);
                    object value = null;
                    foreach (var property in needUpdateProperties)
                    {
                        if (properties != null && properties.TryGetValue(property, out value))
                        {
                            if (string.Equals(property, "SharePointWorkflowContext.Subscription.CreatedDate", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(property, "SharePointWorkflowContext.Subscription.ModifiedDate", StringComparison.OrdinalIgnoreCase))
                            {
                                //使用目的端对应culture的time format
                                DateTime dateTime;
                                if (IsDateTimeAvaliable(value.ToString(), culture, out dateTime))
                                {
                                    value = dateTime.ToString(assoUnit.ParentWeb.LanguageCulture);
                                }
                            }
                            definitionSubscription.SetProperty(property, value.ToString());
                        }
                    }
                    if (properties.TryGetValue("SharePointWorkflowContext.Subscription.EventType", out value))
                    {
                        string[] eventTypes = value.ToString().Split(new string[] { "#;" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string eventType in eventTypes)
                        {
                            if (!definitionSubscription.EventTypes.Contains(eventType))
                            {
                                definitionSubscription.EventTypes.Add(eventType);
                            }
                        }
                        //WorkflowBusinessBehaviorController.GetStartOptionPreBehavior(assoUnit, definitionSubscription).Run();
                        //if (definitionSubscription.EventTypes.Contains("ItemAdded"))
                        //{
                        //    definitionSubscription.EventTypes.Remove("ItemAdded");
                        //}
                        //if (definitionSubscription.EventTypes.Contains("ItemUpdated"))
                        //{
                        //    definitionSubscription.EventTypes.Remove("ItemUpdated");
                        //}
                    }
                }
                Guid workflowSubscriptionId;
                //此处的try catch是为了避免对象不一致时导致publish出错，针对reusable wf，reload后重新publish
                //如果以后找到重现规律并解决ADO-162423中的问题或者确认ADO-162423此类问题不存在时，去掉此处try catch reload逻辑
                try
                {
                    workflowSubscriptionId = PublishWorkflowSubscription(assoUnit, definitionSubscription);
                }
                catch (Exception ex)
                {
                    logger.Info("cannot publish workflow subscription, try reload workflow service manager and publish it. exception:{0}", ex);

                    //assoUnit的ParentObject和当前类中的mParentObject应该是同一个对象，ReloadWeb需要保证Child都reload，所以用AssoUnit上的方法，reload后再给当前对象重新赋值
                    assoUnit.ReloadParentWeb();
                    mParentObject = assoUnit.ParentObject;

                    Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).UpdateWorkflowServiceManager(ParentWeb, true);
                    workflowSubscriptionId = PublishWorkflowSubscription(assoUnit, definitionSubscription);
                }
                return WFSubscriptionService.GetSubscription(workflowSubscriptionId);
            }
        }

        private Guid PublishWorkflowSubscription(SPWFAssociationUnit assoUnit, IAveWorkflowSubscription definitionSubscription)
        {
            var workflowSubscriptionId = Guid.Empty;
            switch (assoUnit.ParentObjectType)
            {
                case SPWFAssociationParentType.List:
                    workflowSubscriptionId = WFSubscriptionService.PublishSubscriptionForList(definitionSubscription, assoUnit.ParentList.ID);
                    break;
                case SPWFAssociationParentType.ListContentType:
                    workflowSubscriptionId = WFSubscriptionService.PublishSubscriptionForList(definitionSubscription, assoUnit.ParentContentType.ParentList.ID);
                    break;
                case SPWFAssociationParentType.Web:
                    workflowSubscriptionId = WFSubscriptionService.PublishSubscription(definitionSubscription);
                    break;
            }
            return workflowSubscriptionId;
        }

        public void SetRestoredUnit(SPWFAssociationUnit assoUnit, object asso)
        {
            assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.mTaskListUnit.SerializableData.mId];
            assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.mHistListUnit.SerializableData.mId];
            AddAssociationToRestoredCollection(assoUnit, (IAveWorkflowSubscription)asso);
        }

        protected void AddAssociationToRestoredCollection(SPWFAssociationUnit assoUnit, IAveWorkflowSubscription workflowSubscription)
        {
            IAveWorkflowDefinition workflowDefinition = WFDeploymentService.GetDefinition(workflowSubscription.DefinitionId);
            if (NeedPostActionAssociations.Contains(assoUnit.SerializableData.mId))
            {
                NeedPostActionAssociations.Remove(assoUnit.SerializableData.mId);
            }
            if (SPWorkflowProcessorRuntime.CachedAssociationCount > 0)
            {
                if (mUnitsOfRestored != null && mUnitsOfRestored.Count >= SPWorkflowProcessorRuntime.CachedAssociationCount)
                {
                    logger.Log(AveLogLevel.INFO, "Restored workflow definitions count: {0}", mUnitsOfRestored.Count);
                    foreach (KeyValuePair<Guid, SPWFAssociationUnit> kv in mUnitsOfRestored)
                    {
                        logger.Log(AveLogLevel.INFO, "{0}  {1}", kv.Key, kv.Value.SerializableData.mName);
                    }
                    mUnitsOfRestored.Clear();
                }
                mUnitsOfRestored.AddEx(assoUnit.SerializableData.mSourceId, assoUnit);
                //var newAsso = assoUnit.SPAssoicationCollection[asso.Id];
                UnitsOfRestoredNameMapping.AddEx(assoUnit.SerializableData.mOriginalName.ToLowerInvariant(), workflowSubscription.Name);
            }
            else//use mId for supporting old backed up data
            {
                mUnitsOfRestored.AddEx(assoUnit.SerializableData.mId, assoUnit);
            }
            SetAssociationUnitPropBySPAssociation(assoUnit, workflowSubscription, false);

            if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
            {
                //fixup GUID Mapping in template files
                try
                {
                    if (!assoUnit.IsBuiltinBaseId &&
                        assoUnit.SerializableData.mIsDeclarative &&
                        assoUnit.mTemplateLibUnit != null
                        && assoUnit.mTemplateLibUnit.mTemplateFileUnits != null
                        && assoUnit.mTemplateLibUnit.mTemplateFileUnits.Count > 0)
                    {
                        Dictionary<string, object> temp = new Dictionary<string, object>();
                        foreach (SPWorkflowSubFileUnit fileUnit in assoUnit.mTemplateLibUnit.mTemplateFileUnits)
                        {
                            SPWorkflowSubFileUnit.FixupDictionary(ParentWeb, temp, fileUnit.SerializableData.mGUIDDictionary);
                        }

                        foreach (KeyValuePair<string, object> pair in temp)
                        {
                            Guid key = new Guid(pair.Key);
                            Guid value = new Guid(pair.Value as string);
                            assoUnit.AllGUIDInTemplate.AddEx(key, value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.GuidMappingFixingUpError, ex);
                }
            }
        }

        private enum ConflicStatus13Model
        {
            None,
            BaseId,
            Configuration,
            Equal,
            Orphan,
        }

        private ConflicStatus13Model HandleWorkflowAssociationConflict(SPWFAssociationUnit assoUnit)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.HandleWorkflowAssociationConflict"))
            {
                try
                {
                    IAveWorkflowSubscription equalAssociation = null;
                    ConflicStatus13Model conflict = CheckWorkflowAssociationConflict(assoUnit, out equalAssociation);
                    switch (conflict)
                    {
                        case ConflicStatus13Model.None:
                            return conflict;
                        case ConflicStatus13Model.Configuration:
                        case ConflicStatus13Model.Equal:
                        case ConflicStatus13Model.Orphan:
                            if (mForceUpdate && (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType || assoUnit.ParentObjectType == SPWFAssociationParentType.Web))
                            {
                                equalAssociation.SetProperty("HistoryListId", assoUnit.mHistListUnit.mSPList.ID.ToString());
                                equalAssociation.SetProperty("TaskListId", assoUnit.mTaskListUnit.mSPList.ID.ToString());
                                if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
                                {
                                    equalAssociation.SetProperty("RestrictToType", "List");
                                    equalAssociation.SetProperty("RestrictToScope", assoUnit.ParentList.ID.ToString());
                                }
                            }
                            assoUnit.mWorkflowSubscription = equalAssociation;
                            //Note: 这里吧这个调用放到此函数调用层，Configuration逻辑分支里面.
                            //AddAssociationToRestoredCollection(assoUnit, equalAssociation);
                            return conflict;
                        case ConflicStatus13Model.BaseId:
                            return conflict;
                        default:
                            return conflict;

                    }
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationHandleConflictError, e);
                }
            }
        }

        private ConflicStatus13Model CheckWorkflowAssociationConflict(SPWFAssociationUnit assoUnit, out IAveWorkflowSubscription outAsso)
        {
            outAsso = null;
            IAveWorkflowSubscriptionCollection subscriptionCollection = null;
            if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
            {
                subscriptionCollection = this.WFSubscriptionService.EnumerateSubscriptionsByList(assoUnit.ParentList.ID);
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
            {
                subscriptionCollection = this.WFSubscriptionService.EnumerateSubscriptionsByList(assoUnit.ParentContentType.ParentList.ID);
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
            {
                subscriptionCollection = this.WFSubscriptionService.EnumerateSubscriptionsByEventSource(assoUnit.ParentWeb.ID);
            }
            else
            {
                logger.Warn("Invalid sharepoint 2010 mode workflow. type: {0}", assoUnit.ParentObjectType);
            }
            IAveWorkflowSubscription subscription = subscriptionCollection.GetSubscriptionByName(assoUnit.SerializableData.mOriginalName);

            if (subscription == null)
            {
                return ConflicStatus13Model.None;
            }
            //else if (asso.BaseId != assoUnit.SerializableData.mBaseId)
            //{
            //    return ConflicStatus.BaseId;
            //}
            //else if (CheckAssociationConfigConflict(assoUnit, asso))
            //{
            //    return ConflicStatus.Configuration;
            //}
            //else if(string.IsNullOrEmpty(asso.TaskListTitle) || string.IsNullOrEmpty(asso.HistoryListTitle))
            //{
            //    outAsso = asso;
            //    return ConflicStatus.Orphan;
            //}
            else
            {
                outAsso = subscription;
                return ConflicStatus13Model.Configuration;
            }
        }

        private IAveWorkflowDefinition FindWorkflowDefinitionByName(SPWFAssociationUnit assoUnit, string workflowDefinitionName, bool isReusable, string restrictToType)
        {
            IAveWorkflowSubscriptionCollection subscriptionCollection = null;
            if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
            {
                subscriptionCollection = this.WFSubscriptionService.EnumerateSubscriptionsByList(assoUnit.ParentList.ID);
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
            {
                subscriptionCollection = this.WFSubscriptionService.EnumerateSubscriptionsByList(assoUnit.ParentContentType.ParentList.ID);
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
            {
                subscriptionCollection = this.WFSubscriptionService.EnumerateSubscriptionsByEventSource(assoUnit.ParentWeb.ID);
            }
            else
            {
                logger.Warn("Invalid sharepoint 2010 mode workflow. type: {0}", assoUnit.ParentObjectType);
            }
            foreach (IAveWorkflowSubscription subscription in subscriptionCollection)
            {
                IAveWorkflowDefinition definition = WFDeploymentService.GetDefinition(subscription.DefinitionId);
                if (definition.DisplayName.Equals(workflowDefinitionName))
                {
                    return definition;
                }
            }
            if (!isReusable)
            {
                return null;
            }
            foreach (IAveWorkflowDefinition definition in WFDeploymentService.EnumerateDefinitions(true))
            {
                if (definition.DisplayName.Equals(workflowDefinitionName) &&
                    restrictToType.Equals(definition.RestrictToType))
                {
                    return definition;
                }
            }
            return null;
        }

        private int RemoveExistWorkflowStatusColumn(SPWFAssociationUnit assoUnit)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.RemoveExistWorkflowStatusColumn"))
            {
                try
                {
                    IAveList list = null;
                    switch (assoUnit.ParentObjectType)
                    {
                        case SPWFAssociationParentType.List:
                            list = assoUnit.ParentList;
                            break;
                        case SPWFAssociationParentType.ListContentType:
                            list = (assoUnit.ParentObject as IAveContentType).ParentList;
                            break;
                        case SPWFAssociationParentType.Web:
                        case SPWFAssociationParentType.WebContentType:
                        default:
                            break;
                    }

                    if (list != null)
                    {
                        object statusFieldObj = list.Fields.GetField(assoUnit.SerializableData.mOriginalName);
                        IAveField statusField = null;
                        if (statusFieldObj != null)
                        {
                            statusField = statusFieldObj as IAveFieldUrl;
                            statusField.ReadOnlyField = false;
                            statusField.Update();
                            statusField.Delete();
                            return 0;
                        }

                        if (statusField == null)
                        {
                            //可以建与builtIn Column 同名的wf，这里会找到builtIn column，导致删不掉
                            statusFieldObj = list.Fields.GetFieldByInternalName(assoUnit.SerializableData.mStatusFieldName, false);
                            if (statusFieldObj != null)
                            {
                                statusField = statusFieldObj as IAveFieldUrl;
                                statusField.ReadOnlyField = false;
                                statusField.Update();
                                statusField.Delete();
                                return 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, "An error occurred while delete existing field. Detail:{0}", ex);
                }
                return 1;
            }
        }
        #endregion
    }

    public class SPWFExportableWorkflowAssociation : SPWFAssociationProc
    {
    }

    public class WFAveSPObjectCache
    {
        public WFAveSPObjectCache()
        { }
        public WFAveSPObjectCache(IAveSPWeb web, IAveSPList list)
        {
            AveSPWeb = web;
            AveSPList = list;
        }

        public IAveSPWeb AveSPWeb { get; set; }
        public IAveSPList AveSPList { get; set; }
    }

    public sealed class SPExportedNintexWorkflowAssociation : SPWFExportableWorkflowAssociation
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        bool serviceInited = false;

        private string serviceUrl;
        private string workflowType;
        private string listName;
        private IAveList parentList;
        private IAveWeb parentWeb;
        private Wrapper.Workflow.Nintex.NintexWFService.NintexWorkflowWS ns;
        private void InitNintexService()
        {
            StringBuilder serviceLocation = new StringBuilder();
            if (mObjectType == SPWFAssociationParentType.List)
            {
                parentList = (IAveList)mParentObject;
                parentWeb = parentList.ParentWeb;
                listName = parentList.ID.ToString();
                workflowType = "List";
                serviceLocation.Append(parentList.ParentWeb.Url);
            }
            else if (mObjectType == SPWFAssociationParentType.Web)
            {
                parentWeb = (IAveWeb)mParentObject;
                listName = Guid.Empty.ToString();
                workflowType = "Site";
                serviceLocation.Append(parentWeb.Url);
            }
            else
            {
                logger.Warn("Nintex content type workflow can not be exported now");
                var contentType = (IAveContentType)mParentObject;
                serviceLocation.Append(contentType.Web.Url);
            }
            serviceLocation.Append("/_vti_bin/NintexWorkflow/Workflow.asmx");
            serviceUrl = serviceLocation.ToString();

            ns = new Wrapper.Workflow.Nintex.NintexWFService.NintexWorkflowWS { Url = serviceUrl };
            if (parentWeb != null && parentWeb.Site.UserAccountInfo != null)
            {
                ns.Credentials = new NetworkCredential(parentWeb.Site.UserAccountInfo.UserName, parentWeb.Site.UserAccountInfo.Password);
            }
            else
            {
                ns.UseDefaultCredentials = true;
            }
        }

        public override List<byte[]> Backup()
        {
            return BackupAssociationUnit();
        }

        private List<byte[]> BackupAssociationUnit()
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.BackupAssociationUnit"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupAssociationUnit");
                string monitor = mMainMonitorLog = "Association Backup";
                if (mParentObject == null)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);
                }

                IAveWorkflowAssociationCollection assoCollection = null;
                switch (mObjectType)
                {
                    case SPWFAssociationParentType.List:
                        assoCollection = ((IAveList)mParentObject).WorkflowAssociations;
                        break;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        break;
                    case SPWFAssociationParentType.Web:
                        assoCollection = ((IAveWeb)mParentObject).WorkflowAssociations;
                        break;
                    default:
                        return base.Backup();
                }
                List<byte[]> rlt = new List<byte[]>();

                if (assoCollection == null || assoCollection.Count == 0)
                {
                    return rlt;
                }

                InitNintexService();



                foreach (IAveWorkflowAssociation asso in assoCollection.Where(ShouldWorkflowBackup))
                {
                    SPWorkflowProcessorRuntime.Log(Logs.AP_BackupBegin, asso.Name);
                    mInnerExceptions = new List<SPWFProcessorException>();
                    try
                    {
                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                        mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + asso.Name);
                        mPerformanceMonitor.StartMonitor(monitor);
                        #endregion
                        logger.Log(AveLogLevel.INFO, "Begin to export workflow association , association name: {0}, level: {1}", asso.Name, mObjectType.ToString());

                        SPWFAssociationUnit assoUnit = BackupOneAssociation(monitor, asso);

                        byte[] data = SPWFAssociationUnit.Save(assoUnit);
                        rlt.Add(data);

#if DEBUG
                        //Export backup assocation to local
                        try
                        {
                            var currentLocation = AppDomain.CurrentDomain.BaseDirectory;
                            var workflowExportLocation = string.Format("{0}\\{1}", currentLocation, "WFExport");
                            if (!Directory.Exists(workflowExportLocation))
                            {
                                Directory.CreateDirectory(workflowExportLocation);
                            }

                            var files = Directory.GetFiles(workflowExportLocation).ToList();

                            bool needCreate = true;

                            foreach (var curFile in files)
                            {
                                var curFileInfo = new FileInfo(curFile);
                                if (curFileInfo.Name == asso.Name && curFileInfo.Length == data.Length)
                                {
                                    needCreate = false;
                                    break;
                                }
                            }

                            if (needCreate)
                            {
                                File.WriteAllBytes(string.Format("{0}\\{1}", workflowExportLocation, asso.Name), data);
                            }
                        }
                        catch (Exception)
                        {

                        }

#endif

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Serialize Association Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion

                        //UnitsOfBackup.AddEx(assoUnit.SerializableData.mId, data);
                    }
                    catch (SPWFProcessorException procException)
                    {
                        logger.Log(AveLogLevel.WARN, "An error occurred while backup workflow association.Name:{0}, defined Exception:{1}.", asso.Name, procException.ToString());
                        mInnerExceptions.Add(procException);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, "An error occurred while backup workflow association.Name:{0}, Exception:{1}.", asso.Name, e);
                        mInnerExceptions.Add(new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, asso.ID));
                    }
                    finally
                    {
                        #region Performance Monitor Region
                        mPerformanceMonitor.StopMonitor(monitor);
                        mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                        mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                        #endregion
                    }
                    if (mInnerExceptions.Count > 0)
                        mExceptions.AddEx(asso.ID, mInnerExceptions);
                    SPWorkflowProcessorRuntime.Log(Logs.AP_BackupFinish, asso.Name);
                }
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupAssociationUnit");
                return rlt;
            }
        }

        private bool IsNintexWorkflow(IAveWorkflowAssociation association)
        {
            try
            {
                switch (ParentObjectType)
                {
                    case SPWFAssociationParentType.List:
                        return ns.WorkflowExists(association.Name, (ParentObject as IAveList).ID, "List") == Wrapper.Workflow.Nintex.NintexWFService.NameInUseStatus.NameUsedInThisList;
                    case SPWFAssociationParentType.Web:
                        return ns.WorkflowExists(association.Name, Guid.Empty, "Site") != Wrapper.Workflow.Nintex.NintexWFService.NameInUseStatus.NameNotUsed;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Exception was thrown while check nintex workflow , workflow name {0}, parent object type {1}. Error {2}", association.Name, ParentObjectType, e);
                return false;
            }
            return true;
        }

        private bool ShouldWorkflowBackup(IAveWorkflowAssociation association)
        {
            return ExecuteFilterFanction(association) && IsNintexWorkflow(association);
        }

        private byte[] ExportWorkflow(string workflowName)
        {
            var exportedWorkflow = ns.ExportWorkflow(workflowName, listName, workflowType);

            if (exportedWorkflow != null)
            {
                exportedWorkflow = exportedWorkflow.TrimStart((char)65279);
            }
            return Encoding.UTF8.GetBytes(exportedWorkflow);
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private void ImportWorkflow(SPWFAssociationUnit assoUnite)
        {
            string workflowName = assoUnite.SerializableData.mName;
            var namePos = workflowName.IndexOf('(');

            if (namePos > 0)
            {
                workflowName = workflowName.Substring(0, namePos).TrimEnd();
            }
            //var nintextWF = new Wrapper.Workflow.Nintex.NintexWFService.NintexWorkflowWS();
            object nintextWF = AveAssemblyUtility.CreateInstance("Wrapper.Workflow.Nintex, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = fffb45e56dd478e3", "Wrapper.Workflow.Nintex.NintexWFService.NintexWorkflowWS");

            AveAssemblyUtility.SetPropertyValue(nintextWF, "Url", serviceUrl);
            AveAssemblyUtility.SetPropertyValue(nintextWF, "UseDefaultCredentials", true);
            //nintextWF.Url = serviceUrl;
            //nintextWF.UseDefaultCredentials = true;

            //nintextWF.SaveFromNWF(assoUnite.ExportFile, listName, workflowName);
            AveAssemblyUtility.InvokeMethod(nintextWF, "SaveFromNWF", workflowName);

        }

        private bool ExecuteFilterFanction(IAveWorkflowAssociation association)
        {
            if (this.FilterWorkflowFunction != null)
            {
                return FilterWorkflowFunction(new AveWorkflowAssociationInfo() { WorkFlowModle = AveWorkflowModel.ModelNintex, AssociationId = association.ID, AssociationBaseId = association.BaseId, CTName = null, Name = association.Name, IsCTWorkflowAssociation = false });
            }
            return true;
        }

        public override SPWFAssociationUnit BackupOneAssociation(string monitor, IAveWorkflowAssociation asso)
        {
            SPWFAssociationUnit assoUnit = new SPWFAssociationUnit();
            assoUnit.ParentObject = mParentObject;
            assoUnit.ParentObjectType = mObjectType;
            assoUnit.ExportFile = ExportWorkflow(asso.Name);
            assoUnit.SerializableData.mName = asso.Name;
            assoUnit.SerializableData.mOriginalName = asso.Name;
            assoUnit.SerializableData.mId = asso.ID;
            assoUnit.SerializableData.mParentAssociationId = asso.ParentAssociationId;
            assoUnit.SerializableData.mBaseId = asso.BaseId;
            assoUnit.SerializableData.Properties.Add(SPWorkflowCommon.PROPS_ExportedNintex, true);
            return assoUnit;
        }

        public override int Restore(SPWFAssociationUnit unit)
        {
            return Restore(unit, true, false);
        }

        public override int Restore(byte[] serializedData)
        {
            return Restore(serializedData, true);
        }

        public override int Restore(SPWFAssociationUnit unit, bool forceUpdate, bool isPostAction)
        {
            try
            {
                if (AveSPObjectCache.AveSPWeb.ParentSite.SPSite.UserAccountInfo.ConnectionType == BposConnectionType.AppToken)
                {
                    throw new SPWFProcessorException("Can not restore nintex workflow with apptoken connection type");
                }

                InitNintexService();

                //var listReferences = isPostAction ? SerializerHelper.DeserializeListReferenceCollection(unit.ExportFile) : null; //这段逻辑是为了尽量保证后面逻辑中get目的端list，content type等object的时候，优先使用id，不行再通过name获取
                var listReferences = SerializerHelper.DeserializeListReferenceCollection(unit.ExportFile); //没有必要放post 里处理list title 的查找，极个别case 会找错，待有CI 再处理。Post 里会多还一遍wf，性价比不高。
                var srcWebTimeZone = AveSPObjectCache.AveSPWeb.WebSettingInfo != null ?
                    AveSPObjectCache.AveSPWeb.WebSettingInfo.TimeZone : new AveRestorableProperty<short>();

                NWDataMappingManager dataMappingManager = new NWDataMappingManager(
                    AveSPObjectCache.AveSPWeb.ParentSite.MappingManager,
                    AveSPObjectCache.AveSPWeb.ParentSite.SPMembers,
                    parentWeb,
                    parentList,
                    listReferences,
                    SPWorkflowProcessorRuntime.ForceEnsureUsersInWorkflow,
                    srcWebTimeZone.IsAvailable ? srcWebTimeZone.Value : (short)-1);

                return RestoreAssociationUnit(unit, isPostAction, dataMappingManager);
            }
            catch (NWListNotFoundException ex)
            {
                logger.Info("List {0} can not be found, put {1} in post action", ex.ListId, unit.SerializableData.mName);
                CacheDataForPostAction(unit);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction, ex);
            }
            catch (NWNeedPostActionException e)
            {
                CacheDataForPostAction(unit);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction, e);
            }
        }

        private void CacheDataForPostAction(SPWFAssociationUnit assoUnit)
        {
            if (parentList == null)
            {
                SPWorkflowProcessorRuntime.OnCacheData(parentWeb.Site.Url, parentWeb.Site.ID.ToString(), parentWeb.ID.ToString(), string.Empty, string.Empty, 0, assoUnit.SerializableData.mId.ToString(), SPWFAssociationUnit.Save(assoUnit));
            }
            else
            {
                SPWorkflowProcessorRuntime.OnCacheData(parentWeb.Site.Url, parentWeb.Site.ID.ToString(), parentWeb.ID.ToString(), parentList.ID.ToString(), parentList.ID.ToString(), 0, assoUnit.SerializableData.mId.ToString(), SPWFAssociationUnit.Save(assoUnit));
            }
            if (!NeedPostActionAssociations.Contains(assoUnit.SerializableData.mId))
            {
                NeedPostActionAssociations.Add(assoUnit.SerializableData.mId);
            }
            if (!NoRestoredWFCache.Contains(assoUnit.SerializableData.mBaseId))
            {
                NoRestoredWFCache.Add(assoUnit.SerializableData.mBaseId);
            }
        }


        /// <summary>
        /// 该方法为Nintex Workflow Test专用方法，请不要随意使用
        /// </summary>
        /// <param name="serializedData"></param>
        /// <param name="mappingManager"></param>
        /// <returns></returns>
        internal int RestoreAssociationUnitForTest(byte[] serializedData, INintexDataMappingManager dataMappingManager)
        {
            SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(serializedData);
            return RestoreAssociationUnitForTest(assoUnit, dataMappingManager);
        }

        /// <summary>
        /// 该方法为Nintex Workflow Test专用方法，请不要随意使用
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="mappingManager"></param>
        /// <returns></returns>
        internal int RestoreAssociationUnitForTest(SPWFAssociationUnit assoUnit, INintexDataMappingManager dataMappingManager)
        {
            InitNintexService();
            return RestoreAssociationUnit(assoUnit, true, dataMappingManager);
        }

        private byte[] BuildWorkflowMetadataContent(string nintexWFType, Guid scopeId, IAveWeb parentWeb, string customerId)
        {
            //生成metadata文件
            var metadataProcessor = new NintexWFMetadataProcessor();
            return metadataProcessor.GetWorkflowMetadataContent(nintexWFType, scopeId, parentWeb.ID.ToString(), customerId);
        }

        private byte[] BuildWorkflowManifestContent(bool needVariables, bool needLists)
        {
            //生成manifest文件
            var manIfEstProcessor = new NintexWFManifestProcessor();
            return manIfEstProcessor.GetWorkflowManifestContent(DateTime.Now, needVariables, needLists);
        }

        private byte[] BuildListsContent(IAveWeb web, IAveList list, Dictionary<Guid, List<string>> listLookup)
        {
            //生成lists文件
            var listsProcessor = new NintexWFListsProcessor(parentWeb, parentList);
            return listsProcessor.GetListsContent(listLookup);
        }

        private WorkflowSettings BuildWorkflowSettingContent(NWActionConfig rootAction, bool isPostAction)
        {
            //生成settings文件
            var settingsProcessor = new NintexWFSettingsProcessor(parentWeb, isPostAction);
            return settingsProcessor.GetWorkflowSettingContent(rootAction); ;
        }

        private bool IsSameWorkflow(IAveWorkflowDefinition workflowDefinition, IAveWeb parentWeb, IAveList parentList)
        {
            if (string.Equals("List", workflowDefinition.RestrictToType, StringComparison.OrdinalIgnoreCase))
            {
                if (this.mObjectType == SPWFAssociationParentType.List)
                {
                    if (parentList.ID.Equals(new Guid(workflowDefinition.RestrictToScope)))
                    {
                        return true;
                    }
                }
            }
            else if (string.Equals("Site", workflowDefinition.RestrictToType, StringComparison.OrdinalIgnoreCase))
            {
                if (this.mObjectType == SPWFAssociationParentType.Web)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 对Action中的list Id，ContentTypeId，UserLoginName等进行mapping处理
        /// </summary>
        /// <param name="workflowAction"></param>
        /// <param name="dataMappingManager"></param>
        /// <param name="isListLevel"></param>
        /// <returns></returns>
        private NWListLookupCacheManager MappingActionData(WorkflowAction workflowAction, INintexDataMappingManager dataMappingManager, string taskListId, string historyListId, bool isListLevel, bool isPostAction)
        {
            var actionConvertermappingManager = new NWActionMappingManager(dataMappingManager, taskListId, historyListId, parentList != null);
            return actionConvertermappingManager.MappingWorkflowActionData(workflowAction, isPostAction);
        }

        private Stream CompressNintexWFData(SPWFAssociationUnit assoUnit, WorkflowSettings workflowSettings, WorkflowAction workflowAction, ArrayOfVariable variables, byte[] listsContent, byte[] manifestContent, byte[] metadataContent, List<byte[]> formFilesContent)
        {
            var actionContent = SerializerHelper.SerializeObjectToBytes(workflowAction);
            var variableContent = variables.Variable.Length > 0 ? SerializerHelper.SerializeObjectToBytes(variables) : null;
            var settingsContent = SerializerHelper.SerializeObjectToBytes(workflowSettings);

            Record(assoUnit, manifestContent, settingsContent, metadataContent, listsContent, actionContent, variableContent, formFilesContent);

            return NintexWFCompressor.CompressNintexWFData(manifestContent,
               actionContent,
                metadataContent,
                settingsContent,
                listsContent,
                variableContent,
                formFilesContent
                );
        }
        private string GetNintexWorkflowCustomerID()
        {
            NintexAppProcessor nintexAppProcessor = new NintexAppProcessor(parentWeb);
            var nintexWorkflowAppInstance = nintexAppProcessor.GetOrCreateNintexWorkflowApp();
            return GetNintexWorkflowCustomerID(nintexWorkflowAppInstance);
        }

        private string GetNintexWorkflowCustomerID(IAveAppInstance nintexWorkflowAppInstance)
        {
            string nintexWorkflowAppPrincipalId = nintexWorkflowAppInstance.AppPrincipalId; //5d3d5c89-3c4c-4b46-ac2c-86095ea300c7 is nintex workflow app product id
            return nintexWorkflowAppPrincipalId.Substring(nintexWorkflowAppPrincipalId.IndexOf("@", StringComparison.OrdinalIgnoreCase) + 1);
        }

        private List<byte[]> BuildFormContent(ExportedWorkflow exportedWorkflow, Dictionary<string, string> actionIdAndFormUrlMapping, INintexDataMappingManager mappingManager, string customerId, bool isPostAction)
        {
            try
            {
                if (actionIdAndFormUrlMapping.Count > 0)
                {
                    new NintexAppProcessor(parentWeb).GetOrCreateNintexFormsApp();
                }
                var formFileProcessor = new NWFormFileProcessor(AveSPObjectCache.AveSPWeb, exportedWorkflow, actionIdAndFormUrlMapping, mappingManager, isPostAction);
                var formFilesBytes = formFileProcessor.GenerateFormFiles(parentList, customerId);
                return formFilesBytes;
            }
            catch (AveNintexFormPostException ex)
            {
                throw new NWNeedPostActionException("Need handle nintex form in nintex workflow in post action.", ex);
            }
        }



        private int RestoreAssociationUnit(SPWFAssociationUnit assoUnit, bool isPostAction, INintexDataMappingManager dataMappingManager)
        {
            logger.Info("Import workflow {0}, parent web url: {1}, parent list Title: {2}.", assoUnit.SerializableData.mName, parentWeb.ServerRelativeUrl, parentList == null ? string.Empty : parentList.Title);

            var exportedWorkflow = SerializerHelper.DeserializeExportedWorkflow(assoUnit.ExportFile);
            if (!isPostAction)
            {
                NintexWFSettingsProcessor.CheckAutoStartOption(exportedWorkflow.Configurations.ActionConfigs[0]);
            }

            var scopeId = parentList == null ? Guid.Empty : parentList.ID;

            var customerId = GetNintexWorkflowCustomerID();

            var workflowSettings = BuildWorkflowSettingContent(exportedWorkflow.Configurations.ActionConfigs[0], isPostAction);

            var variables = ProcessVariables(exportedWorkflow);


            var actionProcessor = new NintexWFActionProcessor(parentWeb, parentList, variables.Variable, dataMappingManager);
            var workflowAction = actionProcessor.BuildWorkflowAction(exportedWorkflow);

            var formFilesContent = BuildFormContent(exportedWorkflow, actionProcessor.ActionIdAndFormMapping, dataMappingManager, customerId, isPostAction);


            var listLookupCacheManager = MappingActionData(workflowAction, dataMappingManager, workflowSettings.TaskListId, workflowSettings.HistoryListId, parentList != null, isPostAction);

            var listsContent = BuildListsContent(parentWeb, parentList, listLookupCacheManager.ListLookupCache);
            var manifestContent = BuildWorkflowManifestContent(variables.Variable.Length > 0, parentList != null);
            var metadataContent = BuildWorkflowMetadataContent(workflowType, scopeId, parentWeb, customerId);


            var stream = CompressNintexWFData(assoUnit, workflowSettings, workflowAction, variables, listsContent, manifestContent, metadataContent, formFilesContent);

            if (actionProcessor.WorkflowActionAdapter.HasPlaceHolderAction)
            {
                //Save Workflow
                parentWeb.ImportNintexWorkflow(stream, assoUnit.SerializableData.mName, parentList == null ? string.Empty : parentList.Title, parentList == null ? Guid.Empty : parentList.ID, true);
            }
            else
            {
                //Publish Workflow
                parentWeb.PublishNintexWorkflow(stream, assoUnit.SerializableData.mName, parentList == null ? string.Empty : parentList.Title, parentList == null ? Guid.Empty : parentList.ID);
            }

            return 0;
        }

        private void Record(SPWFAssociationUnit assoUnit, byte[] manifest, byte[] settings, byte[] metadata, byte[] lists, byte[] actionbytes, byte[] vairsbytes, List<byte[]> formFilesBytes)
        {
            try
            {
                if (WrapperConfiguration.DebugNintexWorkflowMigration)
                {
                    var parentFolderName = string.Format("{0}_{1}", assoUnit.SerializableData.mName, Guid.NewGuid());
                    var parentDirecotry = string.Format("{0}\\{1}\\{2}", AveEnv.AgentLogFolder, "PublishWFDebug", parentFolderName);

                    if (Directory.Exists(parentDirecotry))
                    {
                        Directory.Delete(parentDirecotry, true);
                    }

                    Directory.CreateDirectory(parentDirecotry);
                    logger.Info("Export nintex workflow file location is {0}", parentDirecotry);
                    var workflowDir = string.Format("{0}\\{1}", parentDirecotry, "Workflow");

                    Directory.CreateDirectory(workflowDir);

                    if (actionbytes != null)
                    {
                        File.WriteAllBytes(string.Format("{0}\\{1}", parentDirecotry, @"Workflow\Actions.xml"), actionbytes);
                    }

                    if (metadata != null)
                    {
                        File.WriteAllBytes(string.Format("{0}\\{1}", parentDirecotry, @"Workflow\Metadata.xml"), metadata);
                    }
                    if (settings != null)
                    {
                        File.WriteAllBytes(string.Format("{0}\\{1}", parentDirecotry, @"Workflow\Settings.xml"), settings);
                    }

                    if (lists != null)
                    {
                        File.WriteAllBytes(string.Format("{0}\\{1}", parentDirecotry, "Lists.xml"), lists);
                    }
                    if (manifest != null)
                    {
                        File.WriteAllBytes(string.Format("{0}\\{1}", parentDirecotry, @"Manifest.xml"), manifest);
                    }

                    if (assoUnit.ExportFile != null)
                    {
                        File.WriteAllBytes(string.Format("{0}\\{1}", parentDirecotry, @"exportedWorkflowData.xml"), assoUnit.ExportFile);
                    }

                    if (vairsbytes != null)
                    {
                        File.WriteAllBytes(string.Format("{0}\\{1}", parentDirecotry, @"Workflow\Variables.xml"), vairsbytes);
                    }

                    if (formFilesBytes.Count > 0)
                    {
                        var formsDir = string.Format("{0}\\{1}", parentDirecotry, "Forms");
                        Directory.CreateDirectory(formsDir);
                        for (int i = 1; i <= formFilesBytes.Count; i++)
                        {
                            File.WriteAllBytes(string.Format("{0}\\{1}", parentDirecotry, string.Format(@"Forms\Form{0}.xml", i.ToString())), formFilesBytes[i - 1]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Exception was thrown while set logs {0}", e);
            }
        }

        public ArrayOfVariable ProcessVariables(ExportedWorkflow exportedWorkflow)
        {
            //13 Mode 下 第一第一个Action 一般是NWWorkflowVariablesAdapter
            //Nintex.Workflow.Activities.Adapters.NWWorkflowVariablesAdapter
            var variables = exportedWorkflow.Configurations.ActionConfigs[0].WorkflowVariables;
            var nintexVariablesProcessor = new NintexVariablesProcessor(parentWeb);
            return nintexVariablesProcessor.GetArrayOfVariable(variables);
        }

        public override int Restore(byte[] serializedData, bool forceUpdate)
        {
            try
            {
                SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(serializedData);
                return Restore(assoUnit, forceUpdate, false);
            }
            catch (SPWFProcessorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e);
            }
        }
    }



    internal class SPWorkflowAssociationComparer : IComparer<IAveWorkflowAssociation>
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal bool GetInfoFromInternalName(string internalName, out Guid noCodeWorkflowLibId, out int cfgFileItemId, out int cfgFileVersion)
        {
            noCodeWorkflowLibId = Guid.Empty;
            cfgFileItemId = -1;
            cfgFileVersion = -1;
            try
            {
                int startIndex = internalName.LastIndexOf("<cfg.", StringComparison.OrdinalIgnoreCase);
                if (startIndex > 0)
                {
                    internalName = internalName.Substring(startIndex);
                    if (internalName.ToLower(CultureInfo.CurrentCulture).StartsWith("<cfg.", StringComparison.OrdinalIgnoreCase)
                        && internalName.EndsWith(">", StringComparison.OrdinalIgnoreCase))
                    {
                        internalName = internalName.Substring(1, internalName.Length - 2);
                        string[] splitedCfgName = internalName.Split('.');
                        noCodeWorkflowLibId = new Guid(splitedCfgName[1].Replace('_', '-'));
                        cfgFileItemId = int.Parse(splitedCfgName[2]);
                        cfgFileVersion = int.Parse(splitedCfgName[3]);
                        return true;
                    }
                    else
                        logger.Warn("Invalid workflow definition internal name v1.Name:{0}", internalName);
                    return false;
                }
                else
                {
                    logger.Warn("Invalid workflow definition internal name v2.Name:{0}", internalName);
                    return false;
                }


            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while GetInfoFromInternalName.Name:{0},Error:{1}", internalName, e);
                return false;
            }
        }

        public int Compare(IAveWorkflowAssociation x, IAveWorkflowAssociation y)
        {
            if (x != null && y != null)
            {

                //internal name format   <Cfg.360f6279_595b_486f_a971_48a6f3189720.4.1024.>
                Guid xLibId = Guid.Empty;
                int xItemId = -1;
                int xVersionId = -1;
                GetInfoFromInternalName(x.InternalName, out xLibId, out xItemId, out xVersionId);

                Guid yLibId = Guid.Empty;
                int yItemId = -1;
                int yVersionId = -1;
                GetInfoFromInternalName(y.InternalName, out yLibId, out yItemId, out yVersionId);

                if (xLibId != yLibId)
                {
                    return xLibId.CompareTo(yLibId);
                }
                else
                {
                    if (xItemId > yItemId)
                    {
                        return 1;
                    }
                    else if (xItemId < yItemId)
                    {
                        return -1;
                    }
                    else
                    {
                        if (x.Enabled != y.Enabled)
                        {
                            if (x.Enabled)
                            {
                                return 1;
                            }
                            else
                            {
                                return -1;
                            }
                        }

                        if (xVersionId > yVersionId)
                        {
                            return 1;
                        }
                        else if (xVersionId < yVersionId)
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }
    }

    internal class SPWFAssociationUnitComparer : IComparer<SPWFAssociationUnit>
    {
        public int Compare(SPWFAssociationUnit x, SPWFAssociationUnit y)
        {
            if (x != null && y != null)
            {
                if (x.SerializableData.mOriginalName != null && y.SerializableData.mOriginalName != null)
                {
                    if (x.SerializableData.mBaseId == y.SerializableData.mBaseId)
                    {
                        bool isCurrentX = y.SerializableData.mOriginalName.IndexOf(x.SerializableData.mOriginalName, StringComparison.OrdinalIgnoreCase) >= 0;//if x is current version, then y must be a preversion. y's name must include the x'name
                        bool isCurrentY = x.SerializableData.mOriginalName.IndexOf(y.SerializableData.mOriginalName, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (isCurrentX && isCurrentY)
                        {
                            return 0;
                        }
                        else if (isCurrentX)
                        {
                            return 1;//x>y,move current version to next 
                        }
                        else if (isCurrentY)
                        {
                            return -1;//x<y
                        }
                        else
                        {
                            return string.Compare(x.SerializableData.mOriginalName, y.SerializableData.mOriginalName, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    else
                    {
                        return string.Compare(x.SerializableData.mOriginalName, y.SerializableData.mOriginalName, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            return 0;
        }
    }

    internal class SPWFAssociationUnitComparerCreatedTime : IComparer<SPWFAssociationUnit>
    {
        public int Compare(SPWFAssociationUnit x, SPWFAssociationUnit y)
        {
            if (x != null && y != null && x.SerializableData != null && y.SerializableData != null && x.SerializableData.mCreated != null && y.SerializableData.mCreated != null)
            {
                long tickX = x.SerializableData.mCreated.Ticks;
                long tickY = y.SerializableData.mCreated.Ticks;

                if (tickX == tickY)
                    return 0;
                else if (tickX > tickY)
                    return 1;
                else
                    return -1;
            }
            else
            {
                return 0;
            }
        }
    }

}
