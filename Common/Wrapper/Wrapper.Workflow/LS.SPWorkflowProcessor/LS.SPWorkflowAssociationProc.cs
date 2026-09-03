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
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Common;
using System.Globalization;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Common;
using System.Linq;
using LS.SPWorkflowProcessor.SerializableObjects;

namespace LS.SPWorkflowProcessor
{
    public class SPWFAssociationProc : IDisposable
    {
        private IAveSite mSite = null;
        public IAveSite Site
        {
            get
            {
                return mSite;
            }
            set { mSite = value; }
        }

        private AveMappingManager mMappingManager = null;
        public AveMappingManager MappingManager
        {
            get
            {
                return mMappingManager;
            }
            set { mMappingManager = value; }
        }

        protected const string BesideAssmProfix = ", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        protected SPWFAssociationParentType mObjectType = SPWFAssociationParentType.Invalid;
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
        protected SPWFProcessorType mProcType;
        protected Dictionary<Guid, SPWorkflowSubListUnit> mCachedSubListUnits;
        public Dictionary<Guid, SPWorkflowSubListUnit> CachedSubListUnits
        {
            get { return mCachedSubListUnits; }
        }
        protected LSPerformanceMonitor mPerformanceMonitor;
        protected string mMainMonitorLog = string.Empty;

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


        protected Dictionary<string, string> mUnitsOfRestoredNameMapping;
        public Dictionary<string, string> UnitsOfRestoredNameMapping
        {
            get { return mUnitsOfRestoredNameMapping; }
        }

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
            mUnitsOfRestoredNameMapping = new Dictionary<string, string>();
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
            mUnitsOfRestored.Clear();
            mUnitsOfRestoredNameMapping.Clear();
            if (mPerformanceMonitor != null)
                mPerformanceMonitor.Dispose();

            //CustomProcessors.Clear();
        }

        public event RestoreWFDefinitionEventHandler RestoreWFDefinitionEvent;
        public virtual void RestoreReusableWFTemplate(SPWFAssociationUnit unit)
        {
        }
        public void OnRestoreWFAssociation(object sender, RestoreWFDefinitionEventArgs e)
        {
            RestoreWFDefinitionEvent(sender, e);
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
                case SPWFProcessorType.Project:
                    proc = new SPWFAssociationProcProjectAPI();
                    break;
                case SPWFProcessorType.Native:
                default:
                    proc = new SPWFAssociationProcNative();
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

        public virtual int Restore(SPWFAssociationUnit unit, bool forceUpdate)
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
                string xomlName = splitedName[1].ToLower();
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
                            node.InnerText = aveField.SPFieldInternal.ID.ToString("B").ToUpper();
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
                mInnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.CreateStatusFieldException, e));
            }
            finally
            {
            }
        }

        internal static string GetStatusFieldSchema(IAveWorkflowAssociation asso, string internalName, bool skipReference)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetStatusFieldSchema");
            SPWorkflowProcessorRuntime.Log(Logs.AP_StatusFieldName, internalName);
            string schema = string.Empty;
            try
            {
                schema = SPWFAssociationProcNative.GetStatusFieldSchema(asso, internalName, skipReference);
                if (string.IsNullOrEmpty(schema))
                {
                    schema = asso.ParentList.Fields.GetFieldByInternalName(internalName).SchemaXml;
                }
                SPWorkflowProcessorRuntime.Log(Logs.AP_StatusFieldName, schema);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_GetStatusFieldSchemaException, e.Message);
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetStatusFieldSchema");
            return schema;
        }
        public Func<AveWorkflowAssociationInfo, bool> FilterWorkflowFunction { get; set; }
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
            return Restore(assoUnit, true);
        }

        public override int Restore(byte[] serializedData)
        {
            return Restore(serializedData, true);
        }

        public override int Restore(SPWFAssociationUnit unit, bool forceUpdate)
        {
            mForceUpdate = forceUpdate;
            return RestoreAssociationUnit(unit);
        }

        public override int Restore(byte[] serializedData, bool forceUpdate)
        {
            try
            {
                SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(serializedData);
                return Restore(assoUnit, forceUpdate);
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

        public override void SetRestoredUnit(SPWFAssociationUnit assoUnit, object asso)
        {
            assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.mTaskListUnit.SerializableData.mId];
            assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.mHistListUnit.SerializableData.mId];
            AddAssociationToRestoredCollection(assoUnit, (IAveWorkflowAssociation)asso);
            base.SetRestoredUnit(assoUnit, (IAveWorkflowAssociation)asso);
        }

        protected override void AddAssociationToRestoredCollection(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso)
        {
            if (mForceUpdate)
            {
                asso.AssociationData = assoUnit.SerializableData.mInstantiationParams;
                if (assoUnit.IsBuiltinBaseIdForSP2010)
                {
                    logger.Info("Replace user in workflow associationdata for SP2010. Association baseId:{0}", asso.BaseId.ToString());
                    asso.AssociationData = ReplaceUserInAssociationDataForSP2010(asso.ParentWeb, asso.AssociationData, assoUnit.WorkflowType);
                }
                try
                {
                    if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                    {
                        SPWFAssociationProcNative.UpdateAssociationName(asso, assoUnit.SerializableData.mOriginalName, assoUnit.IsRenamed);
                        SPWFAssociationProcNative.UpdateCreatedTime(asso, asso.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(assoUnit.SerializableData.mCreated));
                        //if only restore association,but not restore instance
                        ModifyDefaultConfiguration(ref assoUnit.SerializableData.mConfiguration);
                        asso.ParentWeb.Dispose();
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
                    SPWFAssociationProcNative.UpdateConfiguration(asso, assoUnit.SerializableData.mConfiguration);
                }
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
                    //mUnitsOfRestored.Clear();  //mUnitsOfRestored.Clear
                }
                mUnitsOfRestored.AddEx(assoUnit.SerializableData.mSourceId, assoUnit);
                var newAsso = assoUnit.SPAssoicationCollection[asso.ID];
                mUnitsOfRestoredNameMapping.AddEx(assoUnit.SerializableData.mOriginalName.ToLower(), null == newAsso ? asso.Name : newAsso.Name);
            }
            else//use mId for supporting old backed up data
            {
                mUnitsOfRestored.AddEx(assoUnit.SerializableData.mId, assoUnit);
            }
            SetAssociationUnitPropBySPAssociation(assoUnit, asso, false);

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
                        IAveListCollection lists = asso.ParentWeb.Lists;
                        foreach (SPWorkflowSubFileUnit fileUnit in assoUnit.mTemplateLibUnit.mTemplateFileUnits)
                        {
                            SPWorkflowSubFileUnit.FixupDictionary(lists, temp, fileUnit.SerializableData.mGUIDDictionary);
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
                    IAveList list = asso.ParentWeb.Lists[libId];
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
                }
            }
        }

        #region ************************Backup  Region************************
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Method name to invoke.")]
        private void SetAssociationUnitPropBySPAssociation(SPWFAssociationUnit assoUnit, IAveWorkflowAssociation asso, bool isBackup)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetAssociationUnitPropBySPAssociation");
            string monitor = "Set Association Properties";
            try
            {
                mPerformanceMonitor.StartMonitor(monitor);

                assoUnit.mSPAssociation = asso;

                assoUnit.SerializableData.mId = asso.ID;
                if (isBackup)
                    assoUnit.SerializableData.mSourceId = asso.ID;
                assoUnit.SerializableData.mBaseId = asso.BaseId;
                SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "BaseId", assoUnit.SerializableData.mBaseId.ToString());
                assoUnit.SerializableData.mAuthor = asso.Author;
                assoUnit.SerializableData.mAuthorLoginName = SPPermissionProcessor.GetUserLoginNameFromId(asso.ParentWeb, asso.Author);
                assoUnit.SerializableData.mAutoCleanupDays = asso.AutoCleanupDays;
                assoUnit.SerializableData.mConfiguration = (int)asso.Configuration; //(int)LSInvoker.GetProperty(asso, "Configuration");
                assoUnit.SerializableData.mContentTypeId = asso.ContentTypeId.ToString();// ((IAveContentTypeId)LSInvoker.GetProperty(asso, "ContentTypeId")).ToString();
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

                    assoUnit.SerializableData.mCreated = asso.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(asso.Created);

                }
                else
                {
                    assoUnit.SerializableData.mCreated = asso.Created;
                }
                assoUnit.SerializableData.mDescription = asso.Description;
                if (asso.ParentList != null)
                {
                    assoUnit.SerializableData.mParentListId = asso.ParentList.ID;
                    SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "ParentList", asso.ParentList.Title);
                }
                //Change for Platform,because we can't get object from Sharepoint when run platform job if source has been deleted.
                if (isBackup)
                {
                    //assoUnit.SerializableData.mOriginalParentId = assoUnit.ParentId;
                    switch (this.ParentObjectType)
                    {
                        case SPWFAssociationParentType.List:
                            IAveList list = (IAveList)assoUnit.ParentObject;
                            assoUnit.SerializableData.mParentId = list.ID.ToString("B");
                            assoUnit.SerializableData.mIsDefaultContentApprovalWorkflow = assoUnit.Id.ToString().Equals(list.DefaultContentApprovalWorkflowId.ToString(), StringComparison.OrdinalIgnoreCase);
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
                }
                else
                {
                    assoUnit.SerializableData.mParentId = assoUnit.ParentId;
                }
                assoUnit.SerializableData.mHistoryListId = asso.HistoryListId;
                assoUnit.SerializableData.mHistoryListTitle = asso.HistoryListTitle;
                assoUnit.SerializableData.mInstanceCount = asso.RunningInstances;
                assoUnit.SerializableData.mInstanceCountDirty = 0;
                assoUnit.SerializableData.mInstantiationParams = asso.AssociationData;
                if (isBackup)
                {
                    //for nintex statistics
                    assoUnit.SerializableData.mModified = asso.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(asso.Modified);
                }
                else
                {
                    assoUnit.SerializableData.mModified = asso.Modified;
                }
                assoUnit.SerializableData.mName = asso.Name;
                if (isBackup)
                {
                    assoUnit.SerializableData.mOriginalName = asso.Name;
                }
                assoUnit.SerializableData.mPermissionsManual = (int)asso.PermissionsManual;

                if (isBackup)
                {
                    assoUnit.SerializableData.mStatusFieldName = (string)LSInvoker.GetProperty(asso, "InternalNameStatusField");
                    if (!string.IsNullOrEmpty(assoUnit.SerializableData.mStatusFieldName))
                    {
                        assoUnit.SerializableData.mStatusFieldSchema = SPWFAssociationProc.GetStatusFieldSchema(asso, assoUnit.SerializableData.mStatusFieldName, true);
                        SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "StatusFieldName", assoUnit.SerializableData.mStatusFieldName);
                    }
                    else
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.AP_AssociationProperty, "StatusFieldName", "NULL");
                    }
                }
                assoUnit.SerializableData.mTaskListId = asso.TaskListId;
                try
                {
                    assoUnit.SerializableData.mTaskListTitle = asso.ParentWeb.Lists[asso.TaskListId].Title;// asso.TaskListTitle;
                    asso.ParentWeb.Dispose();
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                    assoUnit.SerializableData.mTaskListTitle = asso.TaskListTitle;
                }

                assoUnit.SerializableData.mVersion = asso.Version; //(int)LSInvoker.GetProperty(asso, "Version");
                assoUnit.SerializableData.mIsDeclarative = asso.IsDeclarative;
                assoUnit.SerializableData.mEnable = asso.Enabled;
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

                IAveWorkflowTemplate template = asso.BaseTemplate;
                string newAssmName = string.Empty;
                if (template != null)
                {
                    object templateIdSet = LSInvoker.GetProperty(template, "TemplateIdSet");
                    newAssmName = (string)LSInvoker.GetProperty(templateIdSet, "CodeBesideAssm");
                    //if it is a SharePoint2010 SPD Workflow Definition,we can get the SPWorkflowTemplate object, it is different from SharePoint2007.
                    //the GetAssmFullNameFromInternalName function paramater format is 
                    //SPDWorkflowDemo\n<Xoml.4de94745_b540_42f1_a2d7_ed8729d36a59.2.512.-1.0.dll>\n<Cfg.4de94745_b540_42f1_a2d7_ed8729d36a59.3.512.>
                    //so we must add a prefix, temperary is SharePoint2010
                    if (assoUnit.InternalVersion.Equals(SharePointVersion.SharePoint2010.ToString(), StringComparison.OrdinalIgnoreCase) && assoUnit.SerializableData.mIsDeclarative)
                        newAssmName = GetAssmFullNameFromInternalName("SharePoint2010\n" + newAssmName);
                }
                else if (assoUnit.SerializableData.mIsDeclarative)
                {
                    newAssmName = GetAssmFullNameFromInternalName(assoUnit.SerializableData.mInternalName);
                }
                if (!string.IsNullOrEmpty(assoUnit.SerializableData.mCodeBesideAssm))
                {
                    assoUnit.mCodeBesideAssmMapping = new Dictionary<string, string>(1);
                    assoUnit.mCodeBesideAssmMapping.Add(assoUnit.SerializableData.mCodeBesideAssm, newAssmName);
                }
                assoUnit.SerializableData.mCodeBesideAssm = newAssmName;

                //IssueTracking(Three-state) Workflow
                if (asso.BaseId.Equals(new Guid("C6964BFF-BF8D-41AC-AD5E-B61EC111731A")))
                {
                    if (isBackup)
                    {
                        GetOrSetIssueTrackingFields(assoUnit, true);
                    }
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSetUnitException, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetAssociationUnitPropBySPAssociation");
            }
        }

        private List<byte[]> BackupAssociationUnit()
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupAssociationUnit");
            string monitor = mMainMonitorLog = "Association Backup";
            if (mParentObject == null)
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);
            List<IAveWorkflowAssociation> assoCollection;
            string parentInfo = "";
            switch (mObjectType)
            {
                case SPWFAssociationParentType.List:
                    var tempList = (IAveList)mParentObject;
                    parentInfo = string.Format("parentObjectType:List,WebUrl:{0},ListTitle:{1},ListId:{2}.", tempList.ParentWeb.Url, tempList.Title, tempList.ID);
                    assoCollection = ExecuteFilterFanction(tempList.WorkflowAssociations);
                    break;
                case SPWFAssociationParentType.ListContentType:
                case SPWFAssociationParentType.WebContentType:
                    var tempCT = (IAveContentType)mParentObject;

                    if (tempCT.ParentList != null)
                    {

                    }
                    parentInfo = string.Format("CTWF parentObjectType{0}:ContentType,WebUrl:{1},ContentTypeName:{2}.", mObjectType, tempCT.ParentWeb.Url, tempCT.Name);
                    assoCollection = ExecuteFilterFanction(tempCT.WorkflowAssociations,tempCT.Name);
                    break;
                case SPWFAssociationParentType.Web:
                    var tempWeb = (IAveWeb)mParentObject;
                    parentInfo = string.Format("parentObjectType:Web,WebUrl:{0}.", tempWeb.Url);
                    assoCollection = ExecuteFilterFanction(((IAveWeb)mParentObject).WorkflowAssociations);
                    break;
                default:
                    return base.Backup();
            }


            List<byte[]> rlt = new List<byte[]>();
            if (assoCollection == null || assoCollection.Count == 0)
            {
                return rlt;
            }

            List<IAveWorkflowAssociation> assoCollectionSorted = new List<IAveWorkflowAssociation>();
            foreach (IAveWorkflowAssociation asso in assoCollection)
            {
                assoCollectionSorted.Add(asso);
            }

            assoCollectionSorted.Sort(new SPWorkflowAssociationInternalNameComparer());
            StringBuilder backupWorkflowList = new StringBuilder();
            backupWorkflowList.AppendLine(string.Format("ParentInfo:{0}", parentInfo));
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

                    SPWFAssociationUnit assoUnit = BackupOneAssociation(monitor, asso);

                    byte[] data = SPWFAssociationUnit.Save(assoUnit);
                    rlt.Add(data);

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Serialize Association Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    UnitsOfBackup.AddEx(assoUnit.SerializableData.mId, data);
                    backupWorkflowList.AppendLine(string.Format("Name:{0},Id:{1},BaseId:{2},InternalName:{3}",
                        assoUnit.SerializableData.mName,
                        assoUnit.SerializableData.mId,
                        assoUnit.SerializableData.mBaseId,
                        assoUnit.SerializableData.mInternalName.Replace("\n\n", "")));
                }
                catch (SPWFProcessorException procException)
                {
                    mInnerExceptions.Add(procException);
                }
                catch (Exception e)
                {
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
            logger.Info(backupWorkflowList.ToString());
            mPerformanceMonitor.RemoveMonitor(monitor);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupAssociationUnit");
            return rlt;
        }

        private List<IAveWorkflowAssociation> ExecuteFilterFanction(IAveWorkflowAssociationCollection assoCollection, string ctName = null)
        {
            if (this.FilterWorkflowFunction != null)
            {
                return assoCollection.Where(ass => this.FilterWorkflowFunction(new AveWorkflowAssociationInfo() { WorkFlowModle = AveWorkflowModel.Model2010, AssociationId = ass.ID, AssociationBaseId = ass.BaseId, CTName = ctName, Name = ass.Name, IsCTWorkflowAssociation = ctName != null })).ToList();
            }
            return assoCollection.ToList();
        }

        public override SPWFAssociationUnit BackupOneAssociation(string monitor, IAveWorkflowAssociation asso)
        {
            SPWFAssociationUnit assoUnit = new SPWFAssociationUnit();
            assoUnit.ParentObject = mParentObject;
            assoUnit.ParentObjectType = mObjectType;
            SetAssociationUnitPropBySPAssociation(assoUnit, asso, true);

            #region Performance Monitor Region
            mPerformanceMonitor.ResetCurrentDuration(monitor);
            #endregion

            if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType || assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
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

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Task List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion

                if (mCachedSubListUnits.ContainsKey(assoUnit.SerializableData.mHistoryListId))
                {
                    assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.SerializableData.mHistoryListId];
                }
                else
                {
                    assoUnit.mHistListUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.WorkflowHistory);
                    mCachedSubListUnits.AddEx(assoUnit.mHistListUnit.SerializableData.mId, assoUnit.mHistListUnit);
                }

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup History List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion
            }

            if (!assoUnit.IsBuiltinBaseId && assoUnit.SerializableData.mIsDeclarative)
            {
                assoUnit.mTemplateLibUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.NoCodeWorkflows);

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Template Library Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion
            }

            CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
            customProc.FireBackupCustomWorkflowDataEvent(assoUnit);

            #region Performance Monitor Region
            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Custom Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
            #endregion
            return assoUnit;
        }
        #endregion

        #region ************************Restore Region************************
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        private int RestoreAssociationUnit(SPWFAssociationUnit assoUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreAssociationUnit");
            string monitor = mMainMonitorLog = "Association Restore";
            assoUnit.ParentObject = mParentObject;
            assoUnit.ParentObjectType = mObjectType;
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

            string log = string.Empty;
            try
            {

                if (assoUnit.ParentObject == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);

                Guid parentSiteId = assoUnit.SPSiteId;
                Guid parentWebId = assoUnit.SPWebId;

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + assoUnit.SerializableData.mName);
                mPerformanceMonitor.StartMonitor(monitor);
                #endregion

                IAveWeb parentSPWeb = assoUnit.ParentWeb;
                IAveList taskList = null;
                IAveList histList = null;
                if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType || assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
                {
                    bool taskIsNewCreated = !mCachedSubListUnits.ContainsKey(assoUnit.mTaskListUnit.SerializableData.mId);
                    bool histIsNewCreated = !mCachedSubListUnits.ContainsKey(assoUnit.mHistListUnit.SerializableData.mId);
                    bool canUseCache = (!taskIsNewCreated) && (!histIsNewCreated);
                    #region Get or Create Task List
                    if (canUseCache)
                    {
                        assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.mTaskListUnit.SerializableData.mId];
                    }
                    else
                    {
                        SPWorkflowSubListUnit.GetOrCreateSPList(parentSPWeb.Lists, assoUnit.mTaskListUnit, assoUnit.WebLevelFieldProcessorCollection);
                        mCachedSubListUnits.AddEx(assoUnit.mTaskListUnit.SerializableData.mId, assoUnit.mTaskListUnit);
                    }
                    taskList = assoUnit.mTaskListUnit.mSPList;
                    SPWorkflowProcessorRuntime.Log(Logs.AP_RestoreSubListFinish, taskList.Title);
                    #endregion

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Task List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    #region Get or Create History List
                    if (canUseCache)
                    {
                        assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.mHistListUnit.SerializableData.mId];
                    }
                    else
                    {
                        SPWorkflowSubListUnit.GetOrCreateSPList(parentSPWeb.Lists, assoUnit.mHistListUnit, assoUnit.WebLevelFieldProcessorCollection);
                        mCachedSubListUnits.AddEx(assoUnit.mHistListUnit.SerializableData.mId, assoUnit.mHistListUnit);
                    }
                    histList = assoUnit.mHistListUnit.mSPList;
                    #endregion

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore History List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion
                }

                ConflicStatus cStatus = HandleWorkflowAssociationConflict(assoUnit);
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
                        #region Markup Workflow Association
                        if (assoUnit.mTemplateLibUnit != null)
                        {
                            IAveList tempList = SPWorkflowSubListUnit.GetOrCreateSPList(parentSPWeb.Lists, assoUnit.mTemplateLibUnit, assoUnit.WebLevelFieldProcessorCollection);

                            #region Performance Monitor Region
                            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Template Library Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            #endregion

                            bool HasConfigFile = false;
                            foreach (SPWorkflowSubFileUnit subFile in assoUnit.mTemplateLibUnit.mTemplateFileUnits)
                            {
                                if (subFile.SerializableData.mName.ToLower().EndsWith(".xoml.wfconfig.xml"))
                                {
                                    HasConfigFile = true;
                                }
                            }

                            using (IAveWeb tempSPWeb = parentSPWeb.Site.OpenWeb(parentSPWeb.ID))
                            {
                                if (tempSPWeb.WorkflowTemplates.GetTemplateByBaseID(assoUnit.SerializableData.mBaseId) == null && HasConfigFile && SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(assoUnit, assoUnit.mTemplateLibUnit, true, out newAsso)) { }
                            }

                            using (IAveWeb tempSPWeb = parentSPWeb.Site.OpenWeb(parentSPWeb.ID))
                            {
                                if (newAsso == null && tempSPWeb.WorkflowTemplates[assoUnit.SerializableData.mBaseId] != null)
                                {
                                    IAveWorkflowTemplate wfTemplate = tempSPWeb.WorkflowTemplates[assoUnit.SerializableData.mBaseId];
                                    if (wfTemplate == null)
                                    {
                                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationCannotGetWorkflowTemplate);
                                    }
                                    IAveWorkflowAssociation temp = assoUnit.SPAssoicationCollection.GetAssociationByName(assoUnit.SerializableData.mName, CultureInfo.CurrentUICulture);
                                    bool needAddToParentObject = false;
                                    if (temp == null)
                                    {
                                        if (assoUnit.ParentObjectType == SPWFAssociationParentType.WebContentType)
                                        {
                                            temp = assoUnit.CreateSPWorkflowAssociation(wfTemplate, assoUnit.SerializableData.mName, assoUnit.SerializableData.mTaskListTitle, assoUnit.SerializableData.mHistoryListTitle);
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
                                    temp.AutoStartChange = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartChange);
                                    temp.AutoStartCreate = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartAdd);
                                    temp.Description = assoUnit.SerializableData.mDescription;
                                    temp.MarkedForDelete = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.MarkedForDelete);
                                    temp.PermissionsManual = (AveBasePermissions)assoUnit.SerializableData.mPermissionsManual;
                                    //temp.InternalNameStatusField = assoUnit.SerializableData.mStatusFieldName;
                                    //LSInvoker.SetProperty(temp, "InternalNameStatusField", assoUnit.SerializableData.mStatusFieldName);
                                    newAsso = needAddToParentObject ? assoUnit.AddSPAssociationToParentObject(temp) : temp;
                                    newAsso.Enabled = assoUnit.SerializableData.mEnable;
                                    //assoUnit.UpdateWorkflowAssociation(newAsso);//DOC-68808
                                }
                            }

                            #region Performance Monitor Region
                            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Associate Markup Workflow. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        #region Code Workflow Association
                        //IssueTracking(Three-state) Workflow

                        IAveWorkflowTemplate wfTemplate = GetWorkflowTemplate(assoUnit, parentSPWeb);
                        IAveWorkflowAssociation temp = assoUnit.SPAssoicationCollection.GetAssociationByName(assoUnit.SerializableData.mName, CultureInfo.CurrentUICulture);
                        bool needAddToParentObject = false;
                        if (temp == null)
                        {
                            if (assoUnit.ParentObjectType == SPWFAssociationParentType.WebContentType)
                            {
                                temp = assoUnit.CreateSPWorkflowAssociation(wfTemplate, assoUnit.SerializableData.mName, assoUnit.SerializableData.mTaskListTitle, assoUnit.SerializableData.mHistoryListTitle);
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
                        temp.AutoStartChange = false;// CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartChange);
                        temp.AutoStartCreate = false;// CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartAdd);
                        temp.Description = assoUnit.SerializableData.mDescription;
                        temp.MarkedForDelete = CheckConfiguration(assoUnit.SerializableData.mConfiguration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.MarkedForDelete);
                        temp.PermissionsManual = (AveBasePermissions)assoUnit.SerializableData.mPermissionsManual;
                        //temp.InternalNameStatusField = assoUnit.SerializableData.mStatusFieldName;
                        //LSInvoker.SetProperty(temp, "InternalNameStatusField", assoUnit.SerializableData.mStatusFieldName);
                        newAsso = needAddToParentObject ? assoUnit.AddSPAssociationToParentObject(temp) : temp;
                        newAsso.Enabled = assoUnit.SerializableData.mEnable;
                        //assoUnit.UpdateWorkflowAssociation(newAsso);//DOC-68808
                        #endregion

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Associate Code Workflow. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion
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
                    if (!assoUnit.IsBuiltinBaseId && assoUnit.SerializableData.mIsDeclarative)
                    {
                        IAveWorkflowAssociation newAsso = null;
                        bool bolCreate = false;
                        bool tempRenamed = assoUnit.IsRenamed;
                        if (SPWorkflowProcessorRuntime.UpdateCurrentVersion)
                        {
                            if (CheckPublicNewVersionCondition(assoUnit))
                            {
                                bolCreate = assoUnit.IsCurrentVersion;
                                if (bolCreate)
                                {
                                    assoUnit.IsRenamed = true;
                                }
                            }
                        }
                        using (IAveWeb tempSPWeb = parentSPWeb.Site.AllWebs[parentSPWeb.ID])
                        {
                            if (tempSPWeb.WorkflowTemplates.GetTemplateByBaseID(assoUnit.SerializableData.mBaseId) == null && SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(assoUnit, assoUnit.mTemplateLibUnit, bolCreate, out newAsso)) { }
                        }
                        assoUnit.IsRenamed = tempRenamed;
                        if (newAsso != null)
                        {
                            AddAssociationToRestoredCollection(assoUnit, newAsso);
                        }
                    }
                }
                CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                customProc.FireRestoreCustomWorkflowDataEvent(assoUnit);

                assoUnit.DisposeSubListUnits();

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Custom Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion

                SPWorkflowProcessorRuntime.Log(Logs.AP_RestoreFinish, assoUnit.SerializableData.mOriginalName);
            }
            catch (SPWFProcessorException procException)
            {
                try
                {
                    if (procException.ErrorCode == 9999)
                    {
                        switch (assoUnit.ParentObjectType)
                        {
                            case SPWFAssociationParentType.Web:
                                SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), string.Empty, string.Empty, 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                                break;
                            case SPWFAssociationParentType.List:
                                SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentList.ParentWeb.Site.ID.ToString(), assoUnit.ParentList.ParentWeb.ID.ToString(), assoUnit.ParentList.ID.ToString(), assoUnit.ParentList.ID.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                                break;
                            case SPWFAssociationParentType.ListContentType:
                                SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), assoUnit.ParentContentType.ParentList.ID.ToString(), assoUnit.ParentId.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                                break;
                            case SPWFAssociationParentType.WebContentType:
                                SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), string.Empty, assoUnit.ParentId.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                                break;
                            default:
                                break;
                        }
                        if (!NeedPostActionAssociations.Contains(assoUnit.SerializableData.mId))
                        {
                            NeedPostActionAssociations.Add(assoUnit.SerializableData.mId);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.CacheDataError, e.ToString());
                }//need not log
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, assoUnit.SerializableData.mId);
            }
            finally
            {
                if (assoUnit.SerializableData.mId != Guid.Empty)
                {
                    //SPWorkflowProcessorRuntime.mapp.SiteMappingManager.AddWorkflowIdMapping(assoUnit.SerializableData.mSourceId, assoUnit.SerializableData.mId);
                }
                #region Performance Monitor Region
                mPerformanceMonitor.StopMonitor(monitor);
                mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                #endregion
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreAssociationUnit");
            }


            return 0;
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
                wfTemplate = web.WorkflowTemplates[mappingBaseId];
                if (wfTemplate == null)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationCannotGetWorkflowTemplate, null,
                        "Cannot find workflow template associated with the workflow definition. Make sure this workflow template is deployed and the feature is active.");
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
        private ConflicStatus HandleWorkflowAssociationConflict(SPWFAssociationUnit assoUnit)
        {
            try
            {
                IAveWorkflowAssociation equalAssociation = null;
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
                            equalAssociation.SetTaskList(assoUnit.mTaskListUnit.mSPList);
                            equalAssociation.SetHistoryList(assoUnit.mHistListUnit.mSPList);
                        }
                        AddAssociationToRestoredCollection(assoUnit, equalAssociation);
                        return conflict;
                    case ConflicStatus.BaseId:
                        InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.AssociationBaseIdConflict));
                        assoUnit.SerializableData.mName = assoUnit.SerializableData.mOriginalName + assoConflictProfix + assoConfilictIndex.ToString();
                        assoConfilictIndex++;
                        conflict = HandleWorkflowAssociationConflict(assoUnit);
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


        public string ReplaceUserInAssociationData(IAveWeb web, string orgAssociationData, Guid assoBaseId)
        {
            if (string.IsNullOrEmpty(orgAssociationData))
            {
                return orgAssociationData;
            }
            string associationData = orgAssociationData;
            switch (assoBaseId.ToString().ToUpper())
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
        public string ReplaceUserInAssociationDataForSP2010(IAveWeb web, string orgAssociationData, WorkflowType wftype)
        {
            if (string.IsNullOrEmpty(orgAssociationData))
            {
                return orgAssociationData;
            }
            string associationData = orgAssociationData;
            if (WorkflowType.None != wftype)
            {

                if (WorkflowType.ThreeState == wftype)
                {
                    associationData = ReplaceUserForThreeState(web, associationData);
                }
                else
                {
                    associationData = ReplaceUserForSP2010(web, associationData, wftype);
                }
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
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("dfs", nsPrefixdfs);
                nsmgr.AddNamespace("pc", nsPrefixpc);
                nsmgr.AddNamespace("d", nsPrefixd);
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
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ReplaceUserForSP2010Error, e.ToString());
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
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ReplaceUserForFeedbackError, e.ToString());
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
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ReplaceUserForTreeStateError, e.ToString());
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

                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(accountIdNode.InnerText);
                    if (user != null)
                    {
                        if (!user.LoginName.Equals(accountIdNode.InnerText, StringComparison.OrdinalIgnoreCase))
                        {
                            accountIdNode.InnerText = user.LoginName;
                            displayNameNode.InnerText = user.LoginName;
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


                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(accountIdNode.InnerText);
                    if (user != null)
                    {
                        if (!user.LoginName.Equals(accountIdNode.InnerText, StringComparison.OrdinalIgnoreCase))
                        {
                            accountIdNode.InnerText = user.LoginName;
                            displayNameNode.InnerText = user.LoginName;
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
    }

    internal sealed class SPWFAssociationProcNative : SPWFAssociationProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public override List<byte[]> Backup()
        {
            return base.Backup();
        }

        public override int Restore(SPWFAssociationUnit unit)
        {
            return base.Restore(unit);
        }

        internal static string GetStatusFieldSchema(IAveWorkflowAssociation asso, string internalName, bool skipReference)
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
                    using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
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
                    }
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
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetStatusFieldSchemaError, e.ToString());
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

        internal static void UpdateStatusFieldName(IAveWorkflowAssociation asso)
        {
            //SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetStatusFieldSchema Native");
            string temp = (string)LSInvoker.GetProperty(asso, "InternalNameStatusField");

            #region Get Fields Schema XML

            using (IAveSite site = asso.ParentWeb.Site)
            {
                try
                {
                    using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.UpdateStatusFieldName(asso.ID, temp);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateStatusFieldNameError, e.ToString());
                }
            }

            #endregion
        }

        internal static void UpdateConfiguration(IAveWorkflowAssociation asso, int configValue)
        {
            #region Get Fields Schema XML

            if (WrapperRuntime.CurrentContext.ModelFactory != null && WrapperRuntime.CurrentContext.ModelFactory.ContextKind != AveContextKind.ClientObjectModel)
            {
                using (IAveSite site = asso.ParentWeb.Site)
                {
                    try
                    {
                        using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                        {
                            queryService.UpdateConfiguration(asso.ID, configValue);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateConfigrationTimeError, e.ToString());
                    }
                }
            }

            #endregion
        }

        internal static void UpdateAssociationName(IAveWorkflowAssociation asso, string name, bool isRenamed)
        {
            if (isRenamed)
                return;

            #region Update Association Name

            using (IAveSite site = asso.ParentWeb.Site)
            {
                try
                {
                    using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.UpdateAssociationName(asso.ID, name);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateAssociationNameError, e.ToString());
                }
            }

            #endregion
        }

        internal static void UpdateCreatedTime(IAveWorkflowAssociation asso, DateTime created)
        {
            #region Update Created Property

            using (IAveSite site = asso.ParentWeb.Site)
            {
                try
                {
                    using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.UpdateCreatedTime(asso.ID, created);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateCreatedTime, e.ToString());
                }
            }

            #endregion
        }

        internal static void UpdateModifiedTime(IAveWorkflowAssociation asso, DateTime modified)
        {
            #region Update Created Property

            using (IAveSite site = asso.ParentWeb.Site)
            {
                try
                {
                    using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.UpdateModifiedTime(asso.ID, modified);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateModifyTimeError, e.ToString());
                }
            }

            #endregion
        }

        internal static void RecalculateRunningInstanceCount(Guid siteId, Guid webId, Guid listId, IAveWorkflowAssociation asso)
        {
            #region Update Created Property

            using (IAveSite site = asso.ParentWeb.Site)
            {
                try
                {
                    using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(site))
                    {
                        queryService.RecalculateRunningInstanceCount(siteId, webId, listId, asso.ID);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.RecalculateRunningInstanceCountError, e.ToString());
                }
            }

            #endregion
        }
    }

    internal sealed class SPWFAssociationProcProjectAPI : SPWFAssociationProc13ModelAPI
    {
        public override IAveWeb ParentWeb
        {
            get { return (IAveWeb)mParentObject; }
        }
        protected override List<IAveWorkflowSubscription> GetWorkflowSubscriptions()
        {
            var subscriptionCollection = WFSubscriptionService.EnumerateSubscriptionsByEventSource(AveProjectConstants.ProjectWorkflow_EventSourceId).ToList();
            return subscriptionCollection;
        }

        protected override void SetWorklfowRestrictToProperty(SPWFAssociationUnit assoUnit, IAveWorkflowDefinition workflowDefinition, bool isReuableWorkflow, Dictionary<string, object> workflowDefinitionProps)
        {
            workflowDefinition.SetProperty("RestrictToType", "Site");
            workflowDefinition.SetProperty("RestrictToScope", assoUnit.ParentWeb.ID.ToString());
        }

        protected override void SetWorkflowSubscriptionEventSourceProperty(SPWFAssociationUnit assoUnit, IAveWorkflowSubscription definitionSubscription)
        {
            definitionSubscription.EventSourceId = AveProjectConstants.ProjectWorkflow_EventSourceId;
            definitionSubscription.SetProperty("Microsoft.ProjectServer.ActivationProperties.CurrentStageId", "");
            definitionSubscription.SetProperty("Microsoft.SharePoint.ActivationProperties.ParentContentTypeId", "");
            definitionSubscription.SetProperty("Microsoft.ProjectServer.ActivationProperties.ProjectId", "");
            definitionSubscription.SetProperty("Microsoft.ProjectServer.ActivationProperties.RequestedStageId", "");
        }

        protected override IAveWorkflowSubscriptionCollection LoadWorkflowSubScriptionsBySource(SPWFAssociationUnit assoUnit)
        {
            var subscriptionCollection = this.WFSubscriptionService.EnumerateSubscriptionsByEventSource(AveProjectConstants.ProjectWorkflow_EventSourceId);
            return subscriptionCollection;
        }
    }

    internal class SPWFAssociationProc13ModelAPI : SPWFAssociationProc
    {
        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected bool mForceUpdate = true;
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

        public virtual IAveWeb ParentWeb
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

        protected virtual List<IAveWorkflowSubscription> GetWorkflowSubscriptions()
        {
            List<IAveWorkflowSubscription> subscriptionCollection = null;
            switch (mObjectType)
            {
                case SPWFAssociationParentType.List:
                    subscriptionCollection = WFSubscriptionService.EnumerateSubscriptionsByList(((IAveList)mParentObject).ID)
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
                        .ToList();
                    break;
                case SPWFAssociationParentType.ListContentType:
                    subscriptionCollection = WFSubscriptionService.EnumerateSubscriptionsByList(((IAveContentType)mParentObject).ParentList.ID)
                       .Where(
                       subscription =>
                           (subscription != null
                           && subscription.PropertyDefinitions != null
                           && ((subscription.PropertyDefinitions.ContainsKey(activationProperties_ParentContentTypeId)
                                && !string.IsNullOrEmpty(subscription.PropertyDefinitions[activationProperties_ParentContentTypeId])))
                           && string.Equals(subscription.PropertyDefinitions[activationProperties_ParentContentTypeId], ((IAveContentType)mParentObject).Parent.ID.ToString(), StringComparison.OrdinalIgnoreCase)
                            ))
                        .ToList();
                    break;
                case SPWFAssociationParentType.WebContentType:
                    break;
                case SPWFAssociationParentType.Web:
                    subscriptionCollection = WFSubscriptionService.EnumerateSubscriptionsByEventSource(((IAveWeb)mParentObject).ID).ToList();
                    break;
            }
            return subscriptionCollection;
        }

        protected virtual List<byte[]> BackupAssociationUnit()
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupAssociationUnit13Model");
            string monitor = mMainMonitorLog = "Association Backup";
            if (mParentObject == null)
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);

            List<IAveWorkflowSubscription> subscriptionCollection = null;
            //Make sure workflow service manager is update.
            Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).UpdateWorkflowServiceManager(ParentWeb);
            if (WFSubscriptionService == null)
            {
                return base.Backup();
            }
            subscriptionCollection = GetWorkflowSubscriptions();
            List<byte[]> rlt = new List<byte[]>();
            if (subscriptionCollection == null)
            {
                return rlt;
            }

            List<IAveWorkflowSubscription> subscriptionCollectionList = ExecuteFilterFunction(subscriptionCollection, mObjectType == SPWFAssociationParentType.ListContentType);

            foreach (IAveWorkflowSubscription subscription in subscriptionCollectionList)
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
                    logger.Warn("backup workflow {0} failed. error message:{1}", subscription.Name, procException.ToString());
                    mInnerExceptions.Add(procException);
                }
                catch (Exception e)
                {
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
                List<byte[]> othertemplate = new List<byte[]>();
                foreach (var definition in WFDeploymentService.EnumerateDefinitions(true))
                {
                    if (definition != null && definition.Properties != null)
                    {
#if DEBUG
                        builder.AppendLine(definition.DisplayName);
#endif
                        byte[] templateUnit = BackupOneWFTemplate(definition, parentWeb);
                        if (templateUnit != null && templateUnit.Length != 0)
                        {
                            //判断是否是reusable workflow
                            if (string.IsNullOrEmpty(definition.RestrictToScope) && string.IsNullOrEmpty(definition.RestrictToType))
                            {
                                templateUnits.Add(templateUnit);
                            }
                            else
                            {
                                othertemplate.Add(templateUnit);
                            }
                        }
                    }
                }
                //templateUnits.AddRange(othertemplate);
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
                    if (string.IsNullOrEmpty(definition.RestrictToScope) && string.IsNullOrEmpty(definition.RestrictToType))
                    {
                        templateUnit.SerializableData.mIsNintexReusableWorkflow = true;
                    }
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
        /// 调用filter的方法
        /// </summary>
        /// <param name="aveWorkflowSubscriptionCollection"></param>
        /// <returns></returns>
        private List<IAveWorkflowSubscription> ExecuteFilterFunction(List<IAveWorkflowSubscription> aveWorkflowSubscriptionCollection, bool IsCTWorkflow)
        {
            if (aveWorkflowSubscriptionCollection != null && this.FilterWorkflowFunction != null)
            {
                return aveWorkflowSubscriptionCollection
                    .Where(subScription => (
                        this.FilterWorkflowFunction(
                        new AveWorkflowAssociationInfo() { WorkFlowModle = AveWorkflowModel.Model2013, SubScriptionId = subScription.Id, DefinitionId = subScription.DefinitionId, CTName = null, Name = subScription.Name, IsCTWorkflowAssociation = IsCTWorkflow })))
                    .ToList();
            }
            return aveWorkflowSubscriptionCollection.ToList();
        }


        public SPWFAssociationUnit BackupOneAssociation(string monitor, IAveWorkflowSubscription subscription)
        {
            SPWFAssociationUnit assoUnit = new SPWFAssociationUnit();
            assoUnit.ParentObject = mParentObject;
            assoUnit.ParentObjectType = mObjectType;

            SetAssociationUnitPropBySPAssociation(assoUnit, subscription, true);

            #region Performance Monitor Region
            mPerformanceMonitor.ResetCurrentDuration(monitor);
            #endregion

            if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType
                || assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
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

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Task List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion

                if (mCachedSubListUnits.ContainsKey(assoUnit.SerializableData.mHistoryListId))
                {
                    assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.SerializableData.mHistoryListId];
                }
                else
                {
                    assoUnit.mHistListUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.WorkflowHistory);
                    mCachedSubListUnits.AddEx(assoUnit.mHistListUnit.SerializableData.mId, assoUnit.mHistListUnit);
                }

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup History List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion
            }

            if (!assoUnit.IsBuiltinBaseId && assoUnit.SerializableData.mIsDeclarative)
            {
                assoUnit.mTemplateLibUnit = SPWorkflowSubListUnit.GenerateSPListUnit(assoUnit, AveListTemplateType.WFSVC);

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Template Library Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion
            }

            if (IsNintexWorkflow(assoUnit.WorflowDefinition.Properties))
            {
                assoUnit.SerializableData.mFormFileUnit = GenerateFormFileCollection(Encoding.UTF8.GetString(assoUnit.mTemplateLibUnit.mTemplateFileUnits[0].SerializableData.mContent), assoUnit.ParentWeb);
            }

            CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
            customProc.FireBackupCustomWorkflowDataEvent(assoUnit);

            #region Performance Monitor Region
            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Backup Custom Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
            #endregion
            return assoUnit;
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
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetAssociationUnitPropBySPAssociation");
            string monitor = "Set Association Properties";
            try
            {
                mPerformanceMonitor.StartMonitor(monitor);

                IAveWorkflowDefinition workflowDefinition = WFDeploymentService.GetDefinition(workflowSubscription.DefinitionId);
                assoUnit.mWorkflowDefinition = workflowDefinition;
                assoUnit.mWorkflowSubscription = workflowSubscription;
                assoUnit.SerializableData.mId = workflowSubscription.Id;
                assoUnit.SerializableData.mEnable = workflowSubscription.Enabled;
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
                try
                {
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

                        assoUnit.SerializableData.mCreated = ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(DateTime.Parse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.CreatedDate"]));
                    }
                    else
                    {
                        assoUnit.SerializableData.mCreated = DateTime.Parse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.CreatedDate"]);
                    }
                }
                catch (Exception ex)
                {
                    assoUnit.SerializableData.mCreated = DateTime.Now;
                    logger.Warn("covert backup workflow created time failed.created time:{0} , time zone description:{1} ,workflow displayName:{2} ,error message:{3}", workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.CreatedDate"].ToString(), ParentWeb.RegionalSettings.TimeZone.Description, workflowSubscription.Name, ex.Message);
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
                    //assoUnit.SerializableData.mOriginalParentId = assoUnit.ParentId;
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
                assoUnit.SerializableData.mHistoryListTitle = ParentWeb.Lists.GetById(assoUnit.SerializableData.mHistoryListId).Title;
                assoUnit.SerializableData.mInstanceCount = -1;
                assoUnit.SerializableData.mInstanceCountDirty = -1;
                assoUnit.SerializableData.mInstantiationParams = string.Empty;
                try
                {
                    if (isBackup)
                    {
                        //for nintex statistics
                        assoUnit.SerializableData.mModified = ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(DateTime.Parse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.ModifiedDate"]));
                    }
                    else
                    {
                        assoUnit.SerializableData.mModified = DateTime.Parse(workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.ModifiedDate"]);
                    }
                }
                catch (Exception ex)
                {
                    assoUnit.SerializableData.mModified = DateTime.Now;
                    logger.Warn("covert backup workflow modified time failed.modified time:{0} ,time zone description:{1} ,workfolw displayname:{2} ,error message:{3}", workflowSubscription.PropertyDefinitions["SharePointWorkflowContext.Subscription.CreatedDate"].ToString(), ParentWeb.RegionalSettings.TimeZone.Description, workflowSubscription.Name, ex.Message);
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
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_SetAssociationUintPropertiesException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSetUnitException, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetAssociationUnitPropBySPAssociation");
            }
        }
        #endregion

        #region ************************Restore Region************************
        public override int Restore(SPWFAssociationUnit assoUnit)
        {
            return Restore(assoUnit, true);
        }

        public override int Restore(byte[] serializedData)
        {
            return Restore(serializedData, true);
        }

        public override int Restore(SPWFAssociationUnit unit, bool forceUpdate)
        {
            mForceUpdate = forceUpdate;
            return RestoreAssociationUnit(unit);
        }

        private int RestoreAssociationUnit(SPWFAssociationUnit assoUnit)
        {
            //SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreAssociationUnit13Model");
            logger.Log(AveLogLevel.INFO, "RestoreAssociationUnit13Model Started...");
            string monitor = mMainMonitorLog = "Association Restore";
            assoUnit.ParentObject = mParentObject;
            assoUnit.ParentObjectType = mObjectType;
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

            string log = string.Empty;
            try
            {

                if (assoUnit.ParentObject == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);
                var formUnits = assoUnit.SerializableData.mFormFileUnit;
                //当备份数据中存在nintex form 的数据时，说明这个Workflow是nintex Workflow结合了nintex form的数据，需要判断nintex form 是否安装
                if (formUnits!=null&& formUnits.Count > 0 && !IsNintexFormAppInstalled(assoUnit.ParentWeb))
                {
                    if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                    {
                        throw new SPWFProcessorException("Can not restore nintex workflow with nintex form, please install nintex form app first.");
                    }
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction);
                }
                Guid parentSiteId = assoUnit.SPSiteId;
                Guid parentWebId = assoUnit.SPWebId;

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + assoUnit.SerializableData.mName);
                mPerformanceMonitor.StartMonitor(monitor);
                #endregion

                IAveWeb parentSPWeb = assoUnit.ParentWeb;
                IAveList taskList = null;
                IAveList histList = null;
                if (assoUnit.ParentObjectType == SPWFAssociationParentType.List || assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType
                    || assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
                {
                    bool taskIsNewCreated = !mCachedSubListUnits.ContainsKey(assoUnit.mTaskListUnit.SerializableData.mId);
                    bool histIsNewCreated = !mCachedSubListUnits.ContainsKey(assoUnit.mHistListUnit.SerializableData.mId);
                    bool canUseCache = (!taskIsNewCreated) && (!histIsNewCreated);
                    #region Get or Create Task List
                    if (canUseCache)
                    {
                        assoUnit.mTaskListUnit = mCachedSubListUnits[assoUnit.mTaskListUnit.SerializableData.mId];
                    }
                    else
                    {
                        SPWorkflowSubListUnit.GetOrCreateSPList(parentSPWeb.Lists, assoUnit.mTaskListUnit, assoUnit.WebLevelFieldProcessorCollection);
                        mCachedSubListUnits.AddEx(assoUnit.mTaskListUnit.SerializableData.mId, assoUnit.mTaskListUnit);
                    }
                    taskList = assoUnit.mTaskListUnit.mSPList;
                    SPWorkflowProcessorRuntime.Log(Logs.AP_RestoreSubListFinish, taskList.Title);
                    #endregion

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Task List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    #region Get or Create History List
                    if (canUseCache)
                    {
                        assoUnit.mHistListUnit = mCachedSubListUnits[assoUnit.mHistListUnit.SerializableData.mId];
                    }
                    else
                    {
                        SPWorkflowSubListUnit.GetOrCreateSPList(parentSPWeb.Lists, assoUnit.mHistListUnit, assoUnit.WebLevelFieldProcessorCollection);
                        mCachedSubListUnits.AddEx(assoUnit.mHistListUnit.SerializableData.mId, assoUnit.mHistListUnit);
                    }
                    histList = assoUnit.mHistListUnit.mSPList;
                    #endregion

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore History List Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion
                }

                ConflicStatus13Model cStatus = HandleWorkflowAssociationConflict(assoUnit);
                //SPEventManagerWrapper.EnableEventFiring();
                if (cStatus == ConflicStatus13Model.None)
                {
                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Handle Conflict. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    //RemoveExistWorkflowStatusColumn(assoUnit);
                    IAveWorkflowSubscription workflowSubscription = null;
                    string xamlFileContent = string.Empty;

                    #region Markup Workflow Association
                    if (assoUnit.mTemplateLibUnit != null)
                    {
                        IAveList tempList = SPWorkflowSubListUnit.GetOrCreateSPList(parentSPWeb.Lists, assoUnit.mTemplateLibUnit, assoUnit.WebLevelFieldProcessorCollection);

                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Template Library Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion

                        using (IAveWeb tempSPWeb = parentSPWeb.Site.AllWebs[parentSPWeb.ID])
                        {
                            if (SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(assoUnit, assoUnit.mTemplateLibUnit, out xamlFileContent))
                            {
                                string fieldSchema=RemoveExistWorkflowStatusColumn(assoUnit);
                                logger.Info($"SP2013 Platform Workflow Status Field Schema is {fieldSchema}");
                                try
                                {
                                    Guid subscriptionId = Guid.NewGuid();
                                    IAveWorkflowDefinition workflowDefinition = PublishWorkflowDefinition(assoUnit, xamlFileContent, subscriptionId);
                                    if (!workflowDefinition.Properties.ContainsKey("NWConfig.Designer") || assoUnit.ParentObjectType.Equals(SPWFAssociationParentType.ListContentType))
                                    {
                                        workflowSubscription = PublishWorkflowSubscription(assoUnit, workflowDefinition, true, subscriptionId);
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Error("Publish workflow definition failed.ReAdd the workflow status field back.Error:{0}",e);
                                    if (!string.IsNullOrEmpty(fieldSchema))
                                    {
                                        try
                                        {
                                            var field = assoUnit.ParentList.Fields.AddFieldAsXml(fieldSchema, false, AveAddFieldOptions.AddFieldInternalNameHint);
                                            logger.Info($"ReAdd field success.Field:[{field.Title}],[{field.InternalName}][{field.ID}]");
                                        }
                                        catch (Exception ex2)
                                        {
                                            logger.Warn("Re add workflow status field back failed.Error:{0}", ex2);
                                        }
                                    }
                                    throw;
                                }
                            }
                        }


                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Associate Markup Workflow. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        #endregion
                    }
                    #endregion
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
                    string xamlFileContent = string.Empty;
                    SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(assoUnit, assoUnit.mTemplateLibUnit, out xamlFileContent);
                    Guid subscriptionId = assoUnit.mWorkflowSubscription.Id;
                    IAveWorkflowDefinition workflowDefinition = PublishWorkflowDefinition(assoUnit, xamlFileContent, subscriptionId);
                    if (!workflowDefinition.Properties.ContainsKey("NWConfig.Designer") || assoUnit.ParentObjectType.Equals(SPWFAssociationParentType.ListContentType))
                    {
                        PublishWorkflowSubscription(assoUnit, workflowDefinition, false, subscriptionId);
                    }
                    AddAssociationToRestoredCollection(assoUnit, assoUnit.mWorkflowSubscription);
                }
                CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                customProc.FireRestoreCustomWorkflowDataEvent(assoUnit);

                assoUnit.DisposeSubListUnits();

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Restore Custom Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                #endregion
                logger.Log(AveLogLevel.INFO, "Restore Association Unit finish, assUnit name:{0}.", assoUnit.SerializableData.mOriginalName);
                //SPWorkflowProcessorRuntime.Log(Logs.AP_RestoreFinish, assoUnit.SerializableData.mOriginalName);
            }
            catch (SPWFProcessorException procException)
            {
                try
                {
                    if (procException.ErrorCode == 9999)
                    {
                        switch (assoUnit.ParentObjectType)
                        {
                            case SPWFAssociationParentType.Web:
                                SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), string.Empty, string.Empty, 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                                break;
                            case SPWFAssociationParentType.List:
                                //SAAS-29423 诊断log
                                logger.Warn("Restore list association unit failed,assoUnit name:{0}, list name:{1}", assoUnit.SerializableData.mOriginalName, assoUnit.ParentList.Title);
                                SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentList.ParentWeb.Site.ID.ToString(), assoUnit.ParentList.ParentWeb.ID.ToString(), assoUnit.ParentList.ID.ToString(), assoUnit.ParentList.ID.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                                break;
                            case SPWFAssociationParentType.ListContentType:
                                SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), assoUnit.ParentContentType.ParentList.ID.ToString(), assoUnit.ParentId.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                                break;
                            case SPWFAssociationParentType.WebContentType:
                                SPWorkflowProcessorRuntime.OnCacheData(assoUnit.ParentWeb.Site.ID.ToString(), assoUnit.ParentWeb.ID.ToString(), string.Empty, assoUnit.ParentId.ToString(), 0, assoUnit.SerializableData.mId.ToString(), cacheData);
                                break;
                            default:
                                break;
                        }
                        if (!NeedPostActionAssociations.Contains(assoUnit.SerializableData.mId))
                        {
                            //SAAS-29423 诊断log
                            logger.Warn("The assoUnit:{0} has been put into post action.", assoUnit.SerializableData.mOriginalName);
                            NeedPostActionAssociations.Add(assoUnit.SerializableData.mId);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.CacheDataError, e.ToString());
                }
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

                //if (!SPEventManagerWrapper.EventFiringDisabled)
                //{
                //    SPEventManagerWrapper.DisableEventFiring();
                //}
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreAssociationUnit13Model");
            }


            return 0;
        }
        private bool IsNintexFormAppInstalled(IAveWeb parentWeb)
        {
            Guid NintexFormsAppProductId = new Guid("353e0dc9-57f5-40da-ae3f-380cd5385ab9");
            var nintexFormAppInstance = parentWeb.GetAppInstancesByProductId(NintexFormsAppProductId);
            return nintexFormAppInstance.Count > 0;
        }

        public override void RestoreReusableWFTemplate(SPWFAssociationUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreReusableWFTemplate"))
            {
                unit.ParentObject = mParentObject;
                unit.ParentObjectType = mObjectType;
                unit.WebLevelFieldProcessorCollection = this.WebLevelFieldProcessorCollection;
                //记录event receiver状态
                //bool eventFiringDisabled = SPEventManagerWrapper.EventFiringDisabled;

                if (unit.ParentObject == null)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentIsNull);
                }

                try
                {
                    IAveWeb parentSPWeb = unit.ParentWeb;
                    {

                        //SPEventManagerWrapper.EnableEventFiring();

                        if (unit.mTemplateLibUnit != null)
                        {
                            IAveList tempList = SPWorkflowSubListUnit.GetOrCreateSPList(parentSPWeb.Lists, unit.mTemplateLibUnit, unit.WebLevelFieldProcessorCollection);


                            string xamlFileContent = string.Empty;
                            SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(unit, unit.mTemplateLibUnit, out xamlFileContent);
                            Guid subscriptionId = Guid.Empty;
                            if (unit.mWorkflowSubscription != null)
                            {
                                subscriptionId = unit.mWorkflowSubscription.Id;
                            }
                            IAveWorkflowDefinition workflowDefinition = PublishWorkflowDefinition(unit, xamlFileContent, subscriptionId);
                            AddRestoredWorkflowTemplateToCache(unit, workflowDefinition);
                        }
                        else
                        {
                            throw new AveWrapperSkipException("Skip restoring the workflow template as mTemplateLibUnit is null.");
                        }

                        using (AvePerformanceScope pf1 = new AvePerformanceScope("WFAssociationProc13ModeAPI.RestoreAssociationUnit.RestoreCustomWorkflowData"))
                        {
                            CustomWorkflowAssociationProc customProc = new CustomWorkflowAssociationProc(CustomProcessors);
                            //customProc.FireRestoreCustomWorkflowDataEvent(unit);
                        }
                    }

                    unit.DisposeSubListUnits();
                }
                catch (SPWFProcessorException procException)
                {
                    try
                    {
                        if (procException.ErrorCode == 9999)
                        {
                            //need to cache data later
                            logger.Info("Skip restoring the workflow template at this moment, we will restore it later.");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.CacheDataError, e);
                    }
                    throw;
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationUnknownError, e, unit.SerializableData.mId);
                }
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
        private Guid GetHistoryListId(SPWFAssociationUnit assoUnit)
        {
            if (assoUnit.mHistListUnit != null && assoUnit.mHistListUnit.mSPList != null)
            {
                return assoUnit.mHistListUnit.mSPList.ID;
            }
            logger.Warn("History list is nul, can not get history list id.");
            return Guid.Empty;
        }

        private Guid GetTaskListId(SPWFAssociationUnit assoUnit)
        {
            if (assoUnit.mTaskListUnit != null && assoUnit.mTaskListUnit.mSPList != null)
            {
                return assoUnit.mTaskListUnit.mSPList.ID;
            }
            logger.Warn("Task list is nul, can not get task list id.");
            return Guid.Empty;
        }
        private void AddRestoredWorkflowTemplateToCache(SPWFAssociationUnit assoUnit, IAveWorkflowDefinition definition)
        {
            IAveWeb parentWeb = assoUnit.ParentWeb;
            if (!SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(parentWeb.Site.ID, parentWeb.ID, definition.Id))
            {
                // Reusable Workflow Template ,而且不在cache中
                SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Add(parentWeb.Site.ID, parentWeb.ID, definition.Id);
            }
        }
        public override int Restore(byte[] serializedData, bool forceUpdate)
        {
            try
            {
                SPWFAssociationUnit assoUnit = SPWFAssociationUnit.Load(serializedData);
                return Restore(assoUnit, forceUpdate);
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

        private IAveWorkflowDefinition PublishWorkflowDefinition(SPWFAssociationUnit assoUnit, string xamlFileContent, Guid subscriptionId)
        {
            IAveWorkflowDefinition workflowDefinition = null;
            Guid azureWorkflowDefintionId = Guid.Empty;

            //find exist
            bool isReuableWorkflow = assoUnit.SerializableData.mIsNintexReusableWorkflow;
            string restrictToType = string.Empty;
            Dictionary<string, object> workflowDefinitionProps = null;
            if (assoUnit.SerializableData.Properties.ContainsKey(SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION))
            {
                workflowDefinitionProps = (Dictionary<string, object>)assoUnit.SerializableData.Properties[SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION];
                ArgumentCheck.CheckNotNull(workflowDefinitionProps);
                if (workflowDefinitionProps.ContainsKey("RestrictToType"))
                {
                    restrictToType = workflowDefinitionProps["RestrictToType"].ToString();
                }
            }
            //find exist
            workflowDefinition = FindWorkflowDefinitionByName(assoUnit, assoUnit.SerializableData.mName, isReuableWorkflow, restrictToType);
            if (workflowDefinition == null)
            {
                workflowDefinition = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowDefinition();
                //new create resuable workflow keep id
                if (isReuableWorkflow)
                {
                    workflowDefinition.Id = new Guid(assoUnit.SerializableData.mBaseId.ToString());
                }
            }
            else if (isReuableWorkflow && SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Contains(assoUnit.ParentWeb.Site.ID, assoUnit.ParentWeb.ID, workflowDefinition.Id))
            {
                //找到了reusable definition，并且还原过了，直接返回即可，只还原一次reusable definition
                return workflowDefinition;
            }
            workflowDefinition.DisplayName = assoUnit.SerializableData.mName;
            workflowDefinition.Xaml = xamlFileContent;

            string[] needUpdateproperties = new string[] { "SPDConfig.StartOnCreate", "SPDConfig.StartManually", "SPDConfig.StartOnChange", "AutosetStatusToStageName", "FormField", "RequiresInitiationForm", "Definition.CreatedDateUTC", "Definition.ModifiedDateUTC", "Definition.Description" };
            bool requiresInitiationForm = false;
            foreach (string prop in needUpdateproperties)
            {
                if ((bool?)(workflowDefinitionProps?.ContainsKey(prop))??false)
                {
                    if (prop == "Definition.CreatedDateUTC" || prop == "Definition.ModifiedDateUTC")
                    {
                        DateTime tempTime = DateTime.Now;
                        try
                        {
                            tempTime = DateTime.Parse(workflowDefinitionProps[prop].ToString());
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("convert DateTime failed ,workflow display name:{0} ,DateTime vale:{1} ,due to {2}", assoUnit.SerializableData.mName, workflowDefinitionProps[prop].ToString(), ex.Message);
                        }
                        var time = DateTime.SpecifyKind(tempTime, DateTimeKind.Utc);
                        workflowDefinition.SetProperty(prop, time.ToLocalTime().ToString());
                        continue;
                    }
                    if (prop.Equals("RequiresInitiationForm", StringComparison.OrdinalIgnoreCase))
                    {
                        requiresInitiationForm = bool.Parse(workflowDefinitionProps[prop].ToString());
                    }
                    workflowDefinition.SetProperty(prop, workflowDefinitionProps[prop].ToString());
                }
            }
            workflowDefinition.SetProperty("SubscriptionId", subscriptionId.ToString("B"));
            workflowDefinition.RequiresInitiationForm = requiresInitiationForm;
            //web level this three nintex workflow template is build in，RestrictToType and RestrictToScope can't display.
            SetWorklfowRestrictToProperty(assoUnit, workflowDefinition, isReuableWorkflow, workflowDefinitionProps);
            workflowDefinition.SetProperty("isReusable", isReuableWorkflow.ToString());
            SetOnlineNintexWorkflowDefinitionProperties(workflowDefinition, (Dictionary<string, object>)assoUnit.SerializableData.Properties[SPWorkflowCommon.PROPS_13MODEL_WFDEFINITION], assoUnit);
            try
            {
                //workflowDefinition.Id = new Guid(assoUnit.SerializableData.mBaseId.ToString());
                azureWorkflowDefintionId = this.WFDeploymentService.SaveDefinition(workflowDefinition);
            }
            catch (Exception ex)
            {
                logger.Info("cannot save workflow definition, try reload workflow service manager and save it. exception:{0}", ex.ToString());
                Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).UpdateWorkflowServiceManager(ParentWeb);
                azureWorkflowDefintionId = this.WFDeploymentService.SaveDefinition(workflowDefinition);
            }
            var formUnits = assoUnit.SerializableData.mFormFileUnit;
            if (formUnits!=null&& formUnits.Count > 0)
            {
                AddNintexFormFile(formUnits, ParentWeb);
            }

            if (IsNintexWorkflow(workflowDefinition.Properties))
            {
                //logger.Info("parentWeb.url :{0}", ParentWeb.Url);
                ParentWeb.PublishNintexWorkflow(azureWorkflowDefintionId.ToString(), workflowDefinition.RestrictToScope);
            }
            else
            {
                //azureWorkflowDefintionId = this.WFDeploymentService.SaveDefinition(workflowDefinition);
                WFDeploymentService.PublishDefinition(azureWorkflowDefintionId);
            }
            return WFDeploymentService.GetDefinition(azureWorkflowDefintionId);
        }
        private void AddNintexFormFile(List<SPWorkflowSubFileSerializableData> formFiles, IAveWeb parentWeb)
        {
            foreach (var formFile in formFiles)
            {
                var fileContent = Encoding.UTF8.GetString(formFile.mContent);
                string fileServerRelativeUrl = string.Format(@"{0}/NintexFormXml/{1}", parentWeb.ServerRelativeUrl, formFile.mName);
                var file = parentWeb.GetFile(fileServerRelativeUrl);
                if (file.Exists)
                {
                    logger.Info("The file {0} exist in web, skip to restore this nintex form file.", fileServerRelativeUrl);
                    continue;
                }
                var nintexFormList = parentWeb.GetListByTitle("NintexFormXml");
                if (nintexFormList == null)
                {
                    var nintexFormListId = parentWeb.Lists.Add("NintexFormXml", "", "NintexFormXml", Guid.Empty.ToString(), (int)AveListTemplateType.DocumentLibrary, "", AveQuickLaunchOptions.Off);
                    nintexFormList = parentWeb.GetList(nintexFormListId);
                    nintexFormList.Hidden = true;
                    nintexFormList.Update();
                }
                nintexFormList.RootFolder.Files.Add(new AveFileCreationInformation { Url = formFile.mName, Content = formFile.mContent });
            }
        }
        protected virtual void SetWorklfowRestrictToProperty(SPWFAssociationUnit assoUnit, IAveWorkflowDefinition workflowDefinition, bool isReuableWorkflow, Dictionary<string, object> workflowDefinitionProps)
        {
            string[] nintexWorkflowproperties = new string[] { "NintexHawkeyeWorkflowInstance", "NintexSendCloudActivity", "NintexWorkflowInitiation" };
            if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
            {
                if (!nintexWorkflowproperties.Contains(workflowDefinition.DisplayName))
                {
                    if (workflowDefinitionProps.ContainsKey("NWConfig.Designer"))
                    {
                        workflowDefinition.SetProperty("RestrictToType", "Site");
                    }
                    else
                    {
                        workflowDefinition.SetProperty("RestrictToType", "Site");
                        workflowDefinition.SetProperty("RestrictToScope", assoUnit.ParentWeb.ID.ToString());
                    }
                }
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
            {
                workflowDefinition.SetProperty("RestrictToType", "List");
                if (!isReuableWorkflow)
                {
                    workflowDefinition.SetProperty("RestrictToScope", assoUnit.ParentList.ID.ToString());
                }
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
            {
                workflowDefinition.SetProperty("RestrictToType", "List");
                if (!isReuableWorkflow)
                {
                    workflowDefinition.SetProperty("RestrictToScope", assoUnit.ParentContentType.ParentList.ID.ToString());
                }
            }
        }

        protected IAveWorkflowDefinition FindWorkflowDefinitionByName(SPWFAssociationUnit assoUnit, string workflowDefinitionName, bool isReusable, string restrictToType)
        {
            IAveWorkflowSubscriptionCollection subscriptionCollection = LoadWorkflowSubScriptionsBySource(assoUnit);
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

        protected virtual IAveWorkflowSubscriptionCollection LoadWorkflowSubScriptionsBySource(SPWFAssociationUnit assoUnit)
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

            return subscriptionCollection;
        }

        private IAveWorkflowSubscription PublishWorkflowSubscription(SPWFAssociationUnit assoUnit, IAveWorkflowDefinition workflowDefinition, bool isNewCreate, Guid subscriptionId)
        {
            //Guid subscriptionId = Guid.NewGuid();
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
            definitionSubscription.EventTypes = new List<string>();
            definitionSubscription.DefinitionId = workflowDefinition.Id;
            definitionSubscription.Enabled = assoUnit.SerializableData.mEnable;
            definitionSubscription.SetProperty("HistoryListId", assoUnit.mHistListUnit.mSPList.ID.ToString());
            definitionSubscription.SetProperty("TaskListId", assoUnit.mTaskListUnit.mSPList.ID.ToString());
            SetWorkflowSubscriptionEventSourceProperty(assoUnit, definitionSubscription);

            #region RestoreWorkflowSubscriptionProperties

            Dictionary<string, object> propertyDefinitions = null;
            if (assoUnit.SerializableData.Properties.Contains("Props.13Model") && ((Dictionary<string, object>)assoUnit.SerializableData.Properties["Props.13Model"]).ContainsKey("SharePointWorkflowContext.Subscription.EventType"))
            {
                propertyDefinitions = assoUnit.SerializableData.Properties["Props.13Model"] as Dictionary<string, object>;
                string[] eventTypes = ((Dictionary<string, object>)assoUnit.SerializableData.Properties["Props.13Model"])["SharePointWorkflowContext.Subscription.EventType"].ToString().Split(new string[] { "#;" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string eventType in eventTypes)
                {
                    if (!string.Equals(eventType, "ItemAdded") && !string.Equals(eventType, "ItemUpdated") && !definitionSubscription.EventTypes.Contains(eventType))
                    {
                        definitionSubscription.EventTypes.Add(eventType);
                    }
                }

                foreach (KeyValuePair<string, object> subscriptionProperty in propertyDefinitions)
                {
                    if (!string.IsNullOrEmpty(subscriptionProperty.Key)
                           && subscriptionProperty.Key.StartsWith("Microsoft.SharePoint.ExternalVariable."))
                    {
                        definitionSubscription.SetProperty(subscriptionProperty.Key, subscriptionProperty.Value as string);
                    }
                }
            }

            #endregion
            Guid workflowSubscriptionId = Guid.Empty;
            if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
            {
                workflowSubscriptionId = this.WFSubscriptionService.PublishSubscriptionForList(definitionSubscription, assoUnit.ParentList.ID);
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
            {
                workflowSubscriptionId = this.WFSubscriptionService.PublishSubscriptionForList(definitionSubscription, assoUnit.ParentContentType.ParentList.ID);
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
            {
                workflowSubscriptionId = this.WFSubscriptionService.PublishSubscription(definitionSubscription);
            }
            return WFSubscriptionService.GetSubscription(workflowSubscriptionId);
        }

        protected virtual void SetWorkflowSubscriptionEventSourceProperty(SPWFAssociationUnit assoUnit, IAveWorkflowSubscription definitionSubscription)
        {
            if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
            {
                definitionSubscription.EventSourceId = assoUnit.ParentList.ID;
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
            {
                definitionSubscription.EventSourceId = assoUnit.ParentContentType.ParentList.ID;
                if (assoUnit.SerializableData.Properties.ContainsKey(activationProperties_ParentContentTypeId))
                {
                    definitionSubscription.SetProperty(activationProperties_ParentContentTypeId, assoUnit.SerializableData.Properties[activationProperties_ParentContentTypeId].ToString());
                }
            }
            else if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web)
            {
                definitionSubscription.EventSourceId = assoUnit.ParentWeb.ID;
            }
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
                    //mUnitsOfRestored.Clear();  //postaction里还原workflow startoption时会用到此缓存 
                }
                mUnitsOfRestored.AddEx(assoUnit.SerializableData.mSourceId, assoUnit);
                //var newAsso = assoUnit.SPAssoicationCollection[asso.Id];
                mUnitsOfRestoredNameMapping.AddEx(assoUnit.SerializableData.mOriginalName.ToLower(), workflowSubscription.Name);
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
                        IAveListCollection lists = ParentWeb.Lists;
                        foreach (SPWorkflowSubFileUnit fileUnit in assoUnit.mTemplateLibUnit.mTemplateFileUnits)
                        {
                            SPWorkflowSubFileUnit.FixupDictionary(lists, temp, fileUnit.SerializableData.mGUIDDictionary);
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

        private ConflicStatus13Model CheckWorkflowAssociationConflict(SPWFAssociationUnit assoUnit, out IAveWorkflowSubscription outAsso)
        {
            outAsso = null;
            IAveWorkflowSubscriptionCollection subscriptionCollection = LoadWorkflowSubScriptionsBySource(assoUnit);

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

     

        private string RemoveExistWorkflowStatusColumn(SPWFAssociationUnit assoUnit)
        {
            string schema = string.Empty;
            try
            {
                if (assoUnit.ParentList != null)
                {
                    object statusFieldObj = assoUnit.ParentList.Fields.GetField(assoUnit.SerializableData.mStatusFieldName);
                    if (statusFieldObj != null)
                    {
                        IAveField statusField = statusFieldObj as IAveFieldUrl;
                        schema = statusField.SchemaXml;
                        statusField.ReadOnlyField = false;
                        statusField.Update();
                        statusField.Delete();
                        return schema;
                    }
                    else
                    {
                        statusFieldObj = assoUnit.ParentList.Fields.GetFieldByInternalName(assoUnit.SerializableData.mStatusFieldName, false);
                        if (statusFieldObj != null)
                        {
                            IAveField statusField = statusFieldObj as IAveFieldUrl;
                            schema = statusField.SchemaXml;
                            statusField.ReadOnlyField = false;
                            statusField.Update();
                            statusField.Delete();
                            return schema;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, "An error occurred while delete existing field. Detail:{0}", ex.Message);
            }
            return string.Empty;
        }
        #endregion
    }

    internal class SPWorkflowAssociationInternalNameComparer : IComparer<IAveWorkflowAssociation>
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

    internal class SPWorkflowAssociationComparer : IComparer<IAveWorkflowAssociation>
    {
        public int Compare(IAveWorkflowAssociation x, IAveWorkflowAssociation y)
        {
            if (x != null && y != null)
            {
                if (x.Created != null && y.Created != null)
                {
                    long tickX = x.Created.Ticks;
                    long tickY = y.Created.Ticks;

                    if (tickX == tickY)
                        return 0;
                    else if (tickX > tickY)
                        return 1;
                    else
                        return -1;
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
