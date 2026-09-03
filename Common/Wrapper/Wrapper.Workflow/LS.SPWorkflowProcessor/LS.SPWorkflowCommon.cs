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
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using LS.SPWorkflowProcessor.SerializableObjects;
using LS.SPWorkflowProcessor.Services;
using LS.Converters;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.Utility;

namespace LS.SPWorkflowProcessor
{
    public delegate void RestoreWFInstanceEventHandler(object sender, RestoreWFInstanceEventArgs e);

    public delegate void RestoreWFDefinitionEventHandler(object sender, RestoreWFDefinitionEventArgs e);

    public class RestoreWFInstanceEventArgs 
    {
        public object ParentObject { get; set; }
        public SPWFAssociationParentType ParentObjectType { get; set; }
    }

    public class RestoreWFDefinitionEventArgs
    {
        public AveMappingManager Mapping { get; set; }
        public object ParentObject { get; set; }
        public SPWFAssociationParentType ParentObjectType { get; set; }
    }

    public class SPWFAssociationUnit
    {

        #region Serializable Data
        private SPWFAssociationSerializableData mSerializableData = null;
        public SPWFAssociationSerializableData SerializableData
        {
            get
            {
                if (mSerializableData == null)
                    mSerializableData = new SPWFAssociationSerializableData();
                return mSerializableData;
            }
        }
        #endregion

        #region From Serializable Data
        internal SPWorkflowSubListUnit mTaskListUnit;
        internal SPWorkflowSubListUnit mHistListUnit;
        internal SPWorkflowSubListUnit mTemplateLibUnit;
        private Dictionary<string, AveSPField> mIssueTrackingRefFields;
        #endregion

        internal string mNonSerializedCustomDataKey;
        internal object mNonSerializedCustomData;
        private object mParentObject;
        private SPWFAssociationParentType mParentObjectType;
        internal IAveWorkflowAssociationCollection mSPAssoicationCollection;
        internal IAveWorkflowAssociation mSPAssociation;
        internal IAveWorkflowDefinition mWorkflowDefinition;
        internal IAveWorkflowDefinitionCollection mWorkflowDefinitionCollection;
        internal IAveWorkflowSubscription mWorkflowSubscription;
        private IAveWorkflowServicesManager mWorkflowServiceManager = null;
        internal Guid mListId;
        internal Guid mWebId;
        internal Guid mSiteId;
        internal Dictionary<string, string> mCodeBesideAssmMapping;
        internal Dictionary<Guid, Guid> mAllGUIDInTemplate;
        private WorkflowType mWorkflowType = WorkflowType.None;
        public bool isCreateField = false;
        public string reusableWFContentTypeName;

        internal Dictionary<string, AveSPField> IssueTrackingRefFields
        {
            get
            {
                if (mIssueTrackingRefFields == null)
                    mIssueTrackingRefFields = new Dictionary<string, AveSPField>();
                return mIssueTrackingRefFields;
            }
        }
        public Guid Id
        {
            get { return SerializableData.mId; }
        }

        public Guid SourceId
        {
            get { return SerializableData.mSourceId; }
        }

        public object ParentObject
        {
            get { return mParentObject; }
            set
            {
                mParentObject = value;

            }
        }
        public SPWFAssociationParentType ParentObjectType
        {
            get { return mParentObjectType; }
            set { mParentObjectType = value; }
        }
        public IAveWorkflowAssociationCollection SPAssoicationCollection
        {
            get
            {
                switch (this.ParentObjectType)
                {

                    case SPWFAssociationParentType.List:
                        mSPAssoicationCollection = ParentList.WorkflowAssociations;
                        break;
                    case SPWFAssociationParentType.Web:
                        mSPAssoicationCollection = ParentWeb.WorkflowAssociations;
                        break;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        mSPAssoicationCollection = ParentContentType.WorkflowAssociations;
                        break;
                    default:
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                }
                return mSPAssoicationCollection;
            }
        }
        public IAveWorkflowAssociation SPAssociation
        {
            get { return mSPAssociation; }
        }

        public IAveWorkflowDefinitionCollection WorkflowDefinitionCollection
        {
            get
            {
                switch (this.ParentObjectType)
                {

                    case SPWFAssociationParentType.List:
                        //mWorkflowDefinitionCollection
                        //IAveWorkflowSubscriptionCollection subscriptionCollection = WFServiceManager.GetWorkflowSubscriptionService().EnumerateSubscriptionsByList(((IAveList)mParentObject).ID);
                        //foreach (IAveWorkflowSubscription subscription in subscriptionCollection) 
                        //{

                        //}
                        break;
                    case SPWFAssociationParentType.Web:
                        break;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        break;
                    default:
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                }
                return mWorkflowDefinitionCollection;
            }
        }

        public IAveWorkflowDefinition WorflowDefinition
        {
            get { return mWorkflowDefinition; }
        }

        public IAveWorkflowSubscription WorkflowSubscription
        {
            get { return mWorkflowSubscription; }
        }

        public IAveWorkflowServicesManager WFServiceManager
        {
            get
            {
                if (mWorkflowServiceManager == null)
                {
                    mWorkflowServiceManager = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowServicesManager(ParentWeb);
                }
                return mWorkflowServiceManager;
            }
            set { mWorkflowServiceManager = value; }
        }

        private Dictionary<Guid, SPFieldProcessor> mWebLevelFieldProcessorCollection;

        internal Dictionary<Guid, Guid> AllGUIDInTemplate
        {
            get
            {
                if (mAllGUIDInTemplate == null)
                    mAllGUIDInTemplate = new Dictionary<Guid, Guid>();
                return mAllGUIDInTemplate;
            }
        }

        internal string XomlVersionLabel
        { get; set; }
        internal string RulesVersionLabel
        { get; set; }

        internal Guid SPWebId
        {
            get
            {
                if (mWebId == null || mWebId == Guid.Empty)
                    mWebId = ParentWeb.ID;
                return mWebId;
            }
        }
        internal Guid SPSiteId
        {
            get
            {
                if (mSiteId == null || mSiteId == Guid.Empty)
                {
                    switch (ParentObjectType)
                    {
                        case SPWFAssociationParentType.List:
                        case SPWFAssociationParentType.ListContentType:
                        case SPWFAssociationParentType.WebContentType:
                        case SPWFAssociationParentType.Web:
                            using (IAveSite site = ParentWeb.Site)
                            {
                                mSiteId = site.ID;
                            }
                            break;
                        case SPWFAssociationParentType.Invalid:
                        default:
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                    }
                }
                return mSiteId;
            }
        }
        private IAveWeb mParentWeb;
        public IAveWeb ParentWeb
        {
            get
            {
                if (mParentWeb == null)
                {
                    switch (ParentObjectType)
                    {

                        case SPWFAssociationParentType.List:
                            mParentWeb = ((IAveList)ParentObject).ParentWeb;
                            break;
                        case SPWFAssociationParentType.ListContentType:
                        case SPWFAssociationParentType.WebContentType:
                            mParentWeb = ((IAveContentType)ParentObject).ParentWeb;
                            break;
                        case SPWFAssociationParentType.Web:
                            mParentWeb = (IAveWeb)ParentObject;
                            break;
                        case SPWFAssociationParentType.Invalid:
                        default:
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                    }
                }
                return mParentWeb;
            }
        }
        public IAveList ParentList
        {
            get
            {
                if (ParentObjectType == SPWFAssociationParentType.List)
                {
                    Guid listId = ((IAveList)ParentObject).ID;

                    using (IAveSite site = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite(((IAveList)ParentObject).ParentWeb.Site.Url))
                    {
                        using (IAveWeb web = site.OpenWeb(SPWebId))
                        {
                            return web.Lists[listId];
                        }
                    }

                }
                else
                    return null;
            }
        }
        public IAveContentType ParentContentType
        {
            get
            {
                IAveContentType ct = (IAveContentType)ParentObject;
                switch (ParentObjectType)
                {
                    case SPWFAssociationParentType.ListContentType:

                        Guid listId = ct.ParentList.ID;
                        Guid webId = ct.ParentWeb.ID;
                        Guid siteId = ct.ParentWeb.Site.ID;
                        IAveContentTypeId ctId = ct.ID;
                        using (IAveSite site = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite(ct.ParentWeb.Site.Url))
                        {
                            using (IAveWeb web = site.OpenWeb(webId))
                            {
                                IAveList list = web.Lists[listId];
                                ct = list.ContentTypes[ctId];
                            }
                        }
                        return ct;
                    case SPWFAssociationParentType.WebContentType:
                        using (IAveWeb web = ct.ParentWeb.Site.OpenWeb(ct.ParentWeb.ID))
                        {
                            ct = web.ContentTypes[ct.ID];
                        }
                        return ct;
                    default:
                        return null;
                }
            }
        }
        public string ParentId
        {
            get
            {
                switch (this.ParentObjectType)
                {
                    case SPWFAssociationParentType.List:
                        SerializableData.mParentId = ParentList.ID.ToString("B");
                        break;
                    case SPWFAssociationParentType.ListContentType:
                    case SPWFAssociationParentType.WebContentType:
                        SerializableData.mParentId = ParentContentType.ID.ToString();
                        break;
                    case SPWFAssociationParentType.Web:
                        SerializableData.mParentId = ParentWeb.ID.ToString("B");
                        break;
                    default:
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                }
                return SerializableData.mParentId;
            }
        }
        public string OriginalParentId
        {
            get { return SerializableData.mOriginalParentId; }
        }
        public string InternalVersion
        {
            get { return Environment.SharePointVersion.ToString(); }
        }
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
        public bool IsBuiltinBaseId
        {
            get
            {
                //if (m2010BuildinBaseIds == null)
                //{
                //    m2010BuildinBaseIds = new List<Guid>();
                //    m2010BuildinBaseIds.Add(new Guid("E43856D2-1BB4-40ef-B08B-016D89A00409"));//Publishing Approval
                //    m2010BuildinBaseIds.Add(new Guid("C6964BFF-BF8D-41AC-AD5E-B61EC111731A"));//Three-state
                //    m2010BuildinBaseIds.Add(new Guid("DD19A800-37C1-43C0-816D-F8EB5F4A4145"));//Disposition Approval
                //    m2010BuildinBaseIds.Add(new Guid("8AD4D8F0-93A7-4941-9657-CF3706F00409"));//Approval
                //    m2010BuildinBaseIds.Add(new Guid("3BFB07CB-5C6A-4266-849B-8D6711700409"));//Collect Feedback  
                //    m2010BuildinBaseIds.Add(new Guid("77C71F43-F403-484B-BCB2-303710E00409"));//Collect signature
                //}
                //if (m2010BuildinBaseIds.Contains(this.SerializableData.mBaseId))
                //    return true;
                //else
                //    return false;
                return (IsBuiltinBaseIdForSP2007 || IsBuiltinBaseIdForSP2010);
            }
        }
        public bool IsBuiltinBaseIdForSP2007
        {
            get
            {
                //if (m2007BuildinBaseIds == null)
                //{
                //    m2007BuildinBaseIds = new List<Guid>();
                //    m2007BuildinBaseIds.Add(new Guid("B4154DF4-CC53-4C4F-ADEF-1ECF0B7417F6"));//Translation Management
                //    m2007BuildinBaseIds.Add(new Guid("C6964BFF-BF8D-41AC-AD5E-B61EC111731A"));//Three-state
                //    m2007BuildinBaseIds.Add(new Guid("DD19A800-37C1-43C0-816D-F8EB5F4A4145"));//Disposition Approval
                //    m2007BuildinBaseIds.Add(new Guid("C6964BFF-BF8D-41AC-AD5E-B61EC111731C"));//Approval
                //    m2007BuildinBaseIds.Add(new Guid("46C389A4-6E18-476C-AA17-289B0C79FB8F"));//Collect Feedback  
                //    m2007BuildinBaseIds.Add(new Guid("2F213931-3B93-4F81-B021-3022434A3114"));//Collect signature
                //}
                //if (m2007BuildinBaseIds.Contains(this.SerializableData.mBaseId))
                //    return true;
                //else
                return false;
            }
        }
        public bool IsBuiltinBaseIdForSP2010
        {
            get
            {
                string newBaseId = ToEnglishBaseIdForSP2010(this.SerializableData.mBaseId);
                switch (newBaseId)
                {
                    case AveConstants.COLLECT_FEEDBACK_BASEID:
                        this.mWorkflowType = WorkflowType.Feedback2010;
                        break;
                    case AveConstants.COLLECT_SIGNATURE_BASEID:
                        this.mWorkflowType = WorkflowType.Signatures2010;
                        break;
                    case AveConstants.APPROVAL_BASEID:
                    case AveConstants.PUBLISHING_APPROVAL_BASEID:
                    case AveConstants.Disposition_Approval:
                        this.mWorkflowType = WorkflowType.Approval2010;
                        break;
                    case AveConstants.Three_State:
                        this.mWorkflowType = WorkflowType.ThreeState;
                        break;
                    default:
                        return false;
                }
                return true;
            }
        }
        /// <summary>
        /// 根据BaseId判断类型开启对应Feature
        /// </summary>
        public string BuiltinEnglishBaseIdForSP2010
        {
            get
            {
                return ToEnglishBaseIdForSP2010(this.SerializableData.mBaseId);
            }
        }
        public WorkflowType WorkflowType
        {
            get { return mWorkflowType; }
        }
        public SPWorkflowSubListUnit TaskListUnit
        {
            get { return mTaskListUnit; }
        }
        public SPWorkflowSubListUnit HistListUnit
        {
            get { return mHistListUnit; }
        }
        public bool IsRenamed
        {
            get;
            set;
        }

        private int mIsCurrentVersion = -1;//true 0, false 1, null -1
        public bool IsCurrentVersion
        {
            get 
            {
                if (mIsCurrentVersion == -1)
                {
                    long _max = long.MinValue;
                    IAveWorkflowAssociationCollection parentAssociationColl = null;
                    IAveWorkflowAssociation tempAsso = null;
                    try
                    {
                        switch (ParentObjectType)
                        {
                            case SPWFAssociationParentType.List:
                                parentAssociationColl = ParentList.WorkflowAssociations;
                                break;
                            case SPWFAssociationParentType.ListContentType:
                            case SPWFAssociationParentType.WebContentType:
                                parentAssociationColl = ParentContentType.WorkflowAssociations;
                                break;
                            default:
                                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
                        }
                        foreach (IAveWorkflowAssociation asso in parentAssociationColl)
                        {
                            if (asso.BaseId == SPAssociation.BaseId && asso.Created.Ticks > _max)
                            {
                                tempAsso = asso;
                                _max = asso.Created.Ticks;
                            }
                        }
                    }
                    catch (Exception e) 
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.AP_GetAssociationUintPropertiesException, e.Message + e.StackTrace);
                    }
                    if (tempAsso != null && tempAsso.ID == SPAssociation.ID)
                    {
                        mIsCurrentVersion = 0;
                        return true;
                    }
                    else 
                    {
                        mIsCurrentVersion = 1;
                        return false;
                    }
                }
                return mIsCurrentVersion == 0;
            }
        }

        public bool IsSP13ModelWorkflow
        {
            get
            {
                return mSerializableData.Properties.ContainsKey("Props.13Model");
            }
        }

        public bool IsProjectWorkflow
        {
            get
            {
                return mSerializableData.IsProjectWorkflow;
            }
        }

        public SPWFInternalPlatform WFInternalPlatform
        {
            get
            {
                if(IsProjectWorkflow)
                {
                    return SPWFInternalPlatform.WFProjectPlatformType;
                }
                else if (IsSP13ModelWorkflow)
                {
                    return SPWFInternalPlatform.WF2013PlatformType;
                }
                else
                {
                    return SPWFInternalPlatform.WF2010PlatformType;
                }
            }
        }

        internal SPWFAssociationUnit()
        {
            SerializableData.mInternalVersion = Environment.SharePointVersion.ToString();
        }

        internal SPWFAssociationUnit(SPWFAssociationSerializableData data)
        {
            mSerializableData = data;
            if (mSerializableData.mHistListUnit != null)
                this.mHistListUnit = new SPWorkflowSubListUnit(mSerializableData.mHistListUnit);
            if (mSerializableData.mTaskListUnit != null)
                this.mTaskListUnit = new SPWorkflowSubListUnit(mSerializableData.mTaskListUnit);
            if (mSerializableData.mTemplateLibUnit != null)
                this.mTemplateLibUnit = new SPWorkflowSubListUnit(mSerializableData.mTemplateLibUnit);
            if (mSerializableData.mIssueTrackingRefFields != null)
            {
                this.mIssueTrackingRefFields = new Dictionary<string, AveSPField>();
                foreach (KeyValuePair<string, SPFieldSerializableData> pair in mSerializableData.mIssueTrackingRefFields)
                    this.mIssueTrackingRefFields.Add(pair.Key, new AveSPField(pair.Value));
                mSerializableData.mIssueTrackingRefFields.Clear();
            }
            mSerializableData.mHistListUnit = null;
            mSerializableData.mTaskListUnit = null;
            mSerializableData.mTemplateLibUnit = null;
            mSerializableData.mSourceId = data.mId;
            IsRenamed = false;
        }

        internal void Dispose()
        {
            mCodeBesideAssmMapping.Clear();
            mCodeBesideAssmMapping = null;
        }

        internal void DisposeSubListUnits()
        {
            if (this.mHistListUnit != null)
                mHistListUnit.Dispose();
            if (this.mTaskListUnit != null)
                mTaskListUnit.Dispose();
            if (this.mTemplateLibUnit != null)
                mTemplateLibUnit.Dispose();
        }

        //internal void ReloadParentWeb()
        //{
        //    Guid siteId = SPSiteId;
        //    Guid webId = SPWebId;
        //    if (mParentWeb != null)
        //    {
        //        mParentWeb.Dispose();
        //        mParentWeb = null;
        //    }
        //    using (SPSite site = new SPSite(siteId))
        //    {
        //        mParentWeb = site.AllWebs[webId];
        //    }
        //}
        public IAveWorkflowAssociation CreateSPWorkflowAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList)
        {
            IAveWorkflowAssociation workflowAssociation = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowAssociation();
            switch (this.ParentObjectType)
            {
                case SPWFAssociationParentType.List:
                    return workflowAssociation.CreateListAssociation(baseTemplate, name, taskList, historyList);
                case SPWFAssociationParentType.ListContentType:
                    return workflowAssociation.CreateListContentTypeAssociation(baseTemplate, name, taskList, historyList);
                case SPWFAssociationParentType.Web:
                    return workflowAssociation.CreateWebAssociation(baseTemplate, name, taskList, historyList);
                case SPWFAssociationParentType.WebContentType:
                    return workflowAssociation.CreateSiteContentTypeAssociation(baseTemplate, name, taskList.Title, historyList.Title);
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
            }
        }

        public IAveWorkflowAssociation CreateSPWorkflowAssociation(IAveWorkflowTemplate baseTemplate, string name, string taskListTitle, string historyListTitle)
        {
            IAveWorkflowAssociation workflowAssociation = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowAssociation();
            switch (this.ParentObjectType)
            {
                case SPWFAssociationParentType.List:
                    return workflowAssociation.CreateListAssociation(baseTemplate, name, this.ParentWeb.Lists[taskListTitle], this.ParentWeb.Lists[historyListTitle]);
                case SPWFAssociationParentType.ListContentType:
                    return workflowAssociation.CreateListContentTypeAssociation(baseTemplate, name, this.ParentWeb.Lists[taskListTitle], this.ParentWeb.Lists[historyListTitle]);
                case SPWFAssociationParentType.Web:
                    return workflowAssociation.CreateWebAssociation(baseTemplate, name, this.ParentWeb.Lists[taskListTitle], this.ParentWeb.Lists[historyListTitle]);
                case SPWFAssociationParentType.WebContentType:
                    return workflowAssociation.CreateSiteContentTypeAssociation(baseTemplate, name, taskListTitle, historyListTitle);
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
            }
        }

        public IAveWorkflowAssociation AddSPAssociationToParentObject(IAveWorkflowAssociation newAsso)
        {
            switch (this.ParentObjectType)
            {
                case SPWFAssociationParentType.List:
                    return ParentList.AddWorkflowAssociation(newAsso);
                case SPWFAssociationParentType.Web:
                    return ParentWeb.WorkflowAssociations.Add(newAsso);
                case SPWFAssociationParentType.ListContentType:
                case SPWFAssociationParentType.WebContentType:
                    return ParentContentType.AddWorkflowAssociation(newAsso);
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
            }
        }

        public void UpdateWorkflowAssociation(IAveWorkflowAssociation workflowAssociation)
        {
            switch (this.ParentObjectType)
            {
                case SPWFAssociationParentType.List:
                    ParentList.UpdateWorkflowAssociation(workflowAssociation);
                    break;
                case SPWFAssociationParentType.Web:
                    ParentWeb.WorkflowAssociations.Update(workflowAssociation);
                    return;
                case SPWFAssociationParentType.ListContentType:
                case SPWFAssociationParentType.WebContentType:
                    ParentContentType.UpdateWorkflowAssociation(workflowAssociation);
                    break;
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
            }
            try
            {
                SPWFAssociationProcNative.UpdateModifiedTime(workflowAssociation, workflowAssociation.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(this.SerializableData.mModified));
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }

        public static byte[] Save(SPWFAssociationUnit assoUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWFAssociationUnit.Save");
            try
            {
                if (assoUnit == null)
                    return null;
                assoUnit.FixupSerializableData();
                return LSGZipJsonSerializer.Serialize(assoUnit.SerializableData);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_UnitSaveException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSerializationError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SPWFAssociationUnit.Save");
            }
        }

        public static byte[] SaveBinaryFunction(SPWFAssociationUnit assoUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWFAssociationUnit.Save");
            try
            {
                if (assoUnit == null)
                    return null;
                assoUnit.FixupSerializableData();
                MemoryStream stream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, assoUnit.SerializableData);
                byte[] data = LSUtilityOfBytes.LSStreamToBytes(stream);

                #region Compress MetaData
                using (MemoryStream stream2 = new MemoryStream(data.Length))
                {
                    using (GZipStream stream3 = new GZipStream(stream2, CompressionMode.Compress, true))
                    {
                        stream3.Write(data, 0, data.Length);
                    }
                    data = stream2.GetBuffer();
                    Array.Resize<byte>(ref data, Convert.ToInt32(stream2.Length));
                }
                #endregion
                stream.Dispose();

                return data;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_UnitSaveException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationSerializationError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SPWFAssociationUnit.Save");
            }
        }

        public static SPWFAssociationUnit Load(byte[] serializedMetadata)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWFAssociationUnit.Load");
            try
            {
                SPWFAssociationSerializableData serializableData = LSGZipJsonSerializer.Deserialize<SPWFAssociationSerializableData>(serializedMetadata);
                return new SPWFAssociationUnit(serializableData);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_UnitLoadException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationDeserializationError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SPWFAssociationUnit.Load");
            }
        }

        private void FixupSerializableData()
        {
            if (this.mHistListUnit != null)
                this.SerializableData.mHistListUnit = this.mHistListUnit.FixupSerializableData();
            if (this.mTaskListUnit != null)
                this.SerializableData.mTaskListUnit = this.mTaskListUnit.FixupSerializableData();
            if (this.mTemplateLibUnit != null)
                this.SerializableData.mTemplateLibUnit = this.mTemplateLibUnit.FixupSerializableData();
            if (this.mIssueTrackingRefFields != null)
            {
                this.SerializableData.mIssueTrackingRefFields = new Dictionary<string, SPFieldSerializableData>();
                foreach (KeyValuePair<string, AveSPField> pair in this.mIssueTrackingRefFields)
                {
                    if (pair.Value != null)
                        this.SerializableData.mIssueTrackingRefFields.Add(pair.Key, pair.Value.SerializableData);
                }
            }
        }

        /// <summary>
        /// 由于不同语言环境的SP 2010 workflow的baseid后四位可能不同,统一转化成English的baseid
        /// 所以这里替换后四位，再判断类型
        /// </summary>
        /// <param name="assoBaseId"></param>
        /// <returns></returns>
        private string ToEnglishBaseIdForSP2010(Guid assoBaseId)
        {
            string newId = assoBaseId.ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            if (!AveConstants.Three_State.Equals(newId) && !AveConstants.Disposition_Approval.Equals(newId))
            {
                newId = newId.Substring(0, newId.Length - 4) + "0409";
            }
            return newId;
        }
    }

    public class SPWFInstanceUnit
    {
        #region Serializable Data
        internal string mInternalVersion;
        private SPWorkflowSubItemUnit mInstanceItem;
        #endregion

        private SPWFAssociationUnit mParentAssociationUnit = null;
        private WorkflowFixupParams mFixupParams;

        public Dictionary<Guid, Guid> mWFFieldIDMapping = new Dictionary<Guid, Guid>();
        public Dictionary<string, string> mWFFieldInternalNameMapping = new Dictionary<string, string>();
        public Dictionary<string, string> mWFFieldDisplayNameMapping = new Dictionary<string, string>();

        public string InsternalVersion
        {
            get { return mInternalVersion; }
            set { mInternalVersion = value; }
        }



        public SPWorkflowSubItemUnit InstanceItem
        {
            get { return mInstanceItem; }
            set { mInstanceItem = value; }
        }
        public SPWFAssociationUnit ParentAssociationUnit
        {
            get { return mParentAssociationUnit; }
            set { mParentAssociationUnit = value; }
        }
        public bool HasInstanceData
        {
            get
            {
                if (mInstanceItem != null && mInstanceItem.Properties.ContainsKey("#InstanceDataSize") && (int)mInstanceItem.Properties["#InstanceDataSize"] > 0)
                    return true;
                else
                    return false;
            }
        }

        public SPWFInternalPlatform WFInternalPlatform
        {
            get
            {
                if (this.mInstanceItem.Properties.ContainsKey("Props.13Model"))
                {
                    return SPWFInternalPlatform.WF2013PlatformType;
                }
                else
                {
                    return SPWFInternalPlatform.WF2010PlatformType;
                }
            }
        }

        internal WorkflowFixupParams FixupParameters
        {
            get
            {
                if (mFixupParams == null)
                    mFixupParams = new WorkflowFixupParams();
                return mFixupParams;
            }
        }

        internal string StatusFieldColName
        {
            get;
            set;
        }

        internal string StatusFieldRowOrdinal
        {
            get;
            set;
        }

        internal SPWFInstanceUnit()
        {
            mInternalVersion = "SP2007";
            //string sharepointAssemblyName=typeof(SPSite).Assembly.FullName.ToLower();
            //if (sharepointAssemblyName.Equals())
            //{ 
            //    mInternalVersion = "SP2007";
            //}
            //else if (sharepointAssemblyName.Equals())
            //{
            //    mInternalVersion = "SP2010";
            //}
        }

        public void Dispose()
        {
            if (this.InstanceItem != null)
                this.InstanceItem.Dispose();
            FixupParameters.Dispose();
        }
        /*
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Hist:History as key.")]
        internal Dictionary<string, object> GenerateBaseDictionary()
        {
            Dictionary<string, object> repDictionary = new Dictionary<string, object>();
            //******************************site id
            repDictionary.AddEx(this.FixupParameters.mSiteIdDic.GetKey(0).ToString().ToUpper(), this.FixupParameters.mSiteIdDic.GetValue(0));
            //******************************web id
            repDictionary.AddEx(this.FixupParameters.mWebIdDic.GetKey(0).ToString().ToUpper(), this.FixupParameters.mWebIdDic.GetValue(0));
            //******************************list id
            repDictionary.AddEx(this.FixupParameters.mListIdDic.GetKey(0).ToString().ToUpper(), this.FixupParameters.mListIdDic.GetValue(0));
            //******************************item guid
            repDictionary.AddEx(this.FixupParameters.mItemGuidDic.GetKey(0).ToString().ToUpper(), this.FixupParameters.mItemGuidDic.GetValue(0));
            //******************************instance id
            repDictionary.AddEx(this.FixupParameters.mInstanceIdDic.GetKey(0).ToString().ToUpper(), this.FixupParameters.mInstanceIdDic.GetValue(0));


            //******************************all task item guid
            foreach (KeyValuePair<Guid, Guid> pair in this.FixupParameters.mTaskItemGuidDic)
            {
                repDictionary.AddEx(pair.Key.ToString().ToUpper(), pair.Value);
            }

            foreach (KeyValuePair<Guid, Guid> pair in this.FixupParameters.mSubscriptionIdDic)
            {
                //******************************OnItemDeleted event id
                //******************************OnTaskDelete id
                //******************************OnTaskChange id
                repDictionary.AddEx(pair.Key.ToString().ToUpper(), pair.Value);
            }

            repDictionary.AddEx("m_listId", this.FixupParameters.mListIdDic.GetValue(0).ToString());
            repDictionary.AddEx("taskListId", this.FixupParameters.mTaskListIdDic.GetValue(0).ToString());
            repDictionary.AddEx("m_itemId", this.FixupParameters.mItemIdDic.GetValue(0));
            repDictionary.AddEx("itemId", this.FixupParameters.mItemIdDic.GetValue(0));
            repDictionary.AddEx("historyListId", this.FixupParameters.mHistoryListIdDic.GetValue(0).ToString());
            repDictionary.AddEx("m_siteId", this.FixupParameters.mSiteIdDic.GetValue(0).ToString());
            repDictionary.AddEx("m_webId", this.FixupParameters.mWebIdDic.GetValue(0).ToString());
            //repDictionary.Add("m_originator");
            repDictionary.AddEx("m_itemGuid", this.FixupParameters.mItemGuidDic.GetValue(0).ToString());
            repDictionary.AddEx("m_taskListId", this.FixupParameters.mTaskListIdDic.GetValue(0).ToString());
            //repDictionary.Add("m_templateName");
            repDictionary.AddEx("m_workflowId", this.FixupParameters.mInstanceIdDic.GetValue(0).ToString());
            repDictionary.AddEx("m_histListId", this.FixupParameters.mHistoryListIdDic.GetValue(0).ToString());
            repDictionary.AddEx("_taskItemId", this.FixupParameters.mLastTaskItemIdDic.GetValue(0));
            repDictionary.AddEx("m_taskId", this.FixupParameters.mLastTaskItemGuidDic.GetValue(0).ToString());

            repDictionary.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember + ".__list", new LS.BinarySerialization.Replacer.LSMemberDataInfo(this.FixupParameters.mListIdDic.GetKey(0).ToString(), this.FixupParameters.mListIdDic.GetValue(0).ToString(), LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, "__list"));
            //repDictionary.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.MemberDataProfix + "__tasklist", this.FixupParameters.mTaskListIdDic.GetValue(0).ToString());
            //repDictionary.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.MemberDataProfix + "__historylist", this.FixupParameters.mHistoryListIdDic.GetValue(0).ToString());

            return repDictionary;
        }

        internal Dictionary<string, object> GenerateDictionary(Dictionary<string, string> assemblyMapping)
        {
            Dictionary<string, object> repDictionary = GenerateBaseDictionary();

            foreach (KeyValuePair<string, object> pair in this.FixupParameters.mCustomDic1)
            {
                repDictionary.AddEx(pair.Key, pair.Value);
            }
            foreach (KeyValuePair<Guid, Guid> pair in this.FixupParameters.mCustomDic2)
            {
                repDictionary.AddEx(pair.Key.ToString().ToUpper(), pair.Value);
            }
            foreach (KeyValuePair<int, int> pair in this.FixupParameters.mCustomDic3)
            {
                repDictionary.AddEx(pair.Key.ToString(), pair.Value);
            }
            if (assemblyMapping != null)
            {
                foreach (KeyValuePair<string, string> pair in assemblyMapping)
                {
                    repDictionary.AddEx(pair.Key.ToLower(), pair.Value);
                }
                //assemblyMapping.Clear();
            }
            return repDictionary;
        }

        */
        public static byte[] Save(SPWFInstanceUnit instanceUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWFInstanceUnit.Save");
            if (instanceUnit == null)
                return null;
            SPWFInstanceSerializableData serializableData = instanceUnit.ConvertToData();

            byte[] data = LSGZipJsonSerializer.Serialize(serializableData);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SPWFInstanceUnit.Save");
            return data;
        }

        public static SPWFInstanceUnit Load(byte[] serializedMetadata)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWFInstanceUnit.Load");
            SPWFInstanceSerializableData serializableData = LSGZipJsonSerializer.Deserialize<SPWFInstanceSerializableData>(serializedMetadata);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SPWFInstanceUnit.Load");
            return SPWFInstanceUnit.ConvertToObject(serializableData);
        }

        private SPWFInstanceSerializableData ConvertToData()
        {
            SPWFInstanceSerializableData data = new SPWFInstanceSerializableData();
            if (this.mInstanceItem != null)
                data.mInstanceItem = this.mInstanceItem.ConvertToData();
            data.mInternalVersion = this.mInternalVersion;
            return data;
        }

        private static SPWFInstanceUnit ConvertToObject(SPWFInstanceSerializableData data)
        {
            if (data == null)
                return null;
            SPWFInstanceUnit unit = new SPWFInstanceUnit();
            unit.mInstanceItem = SPWorkflowSubItemUnit.ConvertToObject(data.mInstanceItem);
            unit.mInternalVersion = data.mInternalVersion;
            return unit;
        }
    }

    public class SPContentTypeUnit
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        #region Serializable Data
        private SPContentTypeSerializableData mSerializableData = null;
        public SPContentTypeSerializableData SerializableData
        {
            get
            {
                return mSerializableData;
            }
        }
        #endregion

        public SPContentTypeUnit mParentUnit = null;
        public IAveContentTypeCollection mSPContentTypeCollection;

        public SPContentTypeUnit()
        {
            this.SerializableData.mLevel = 0;
            this.SerializableData.mIndex = 0;
        }

        public SPContentTypeUnit(IAveContentType ct)
        {
            mSerializableData = new SPContentTypeSerializableData();
            this.mSerializableData.mLevel = 0;
            this.mSerializableData.mIndex = 0;
            this.mSerializableData.mInternalVersion = (int)Environment.SharePointVersion;
            SetUnitPropBySPObject(ct, this);
        }

        public SPContentTypeUnit(SPContentTypeSerializableData data)
        {
            mSerializableData = data;
            if (mSerializableData.mParentData != null)
                this.mParentUnit = new SPContentTypeUnit(mSerializableData.mParentData);
        }

        public void Dispose()
        {
            if (mSerializableData != null)
            {
                mSerializableData.Dispose();
            }
            mSerializableData = null;
        }

        public void SetUnitPropBySPObject(IAveContentType ct, SPContentTypeUnit unit)
        {
            unit.SerializableData.mDescription = ct.Description;
            unit.SerializableData.mDisplayFormTemplateName = ct.DisplayFormTemplateName;
            unit.SerializableData.mDisplayFormUrl = ct.DisplayFormUrl;
            unit.SerializableData.mDocumentTemplate = ct.DocumentTemplate;
            unit.SerializableData.mEditFormTemplateName = ct.EditFormTemplateName;
            unit.SerializableData.mEditFormUrl = ct.EditFormUrl;
            unit.SerializableData.mGroup = ct.Group;
            unit.SerializableData.mHidden = ct.Hidden;
            unit.SerializableData.mId = ct.ID.ToString();
            unit.SerializableData.mName = ct.Name;
            unit.SerializableData.mNewId = unit.SerializableData.mId;
            unit.SerializableData.mOriginalName = ct.Name;
            unit.SerializableData.mNewDocumentControl = ct.NewDocumentControl;
            unit.SerializableData.mNewFormTemplateName = ct.NewFormTemplateName;
            unit.SerializableData.mNewFormUrl = ct.NewFormUrl;
            unit.SerializableData.mReadOnly = ct.ReadOnly;
            unit.SerializableData.mRequireClientRenderingOnNew = ct.RequireClientRenderingOnNew;
            unit.SerializableData.mSchemaXml = ct.SchemaXml;
            unit.SerializableData.mScope = ct.Scope;
            unit.SerializableData.mSealed = ct.Sealed;

            if (ct.ParentWeb.ServerRelativeUrl.Equals(ct.Scope, StringComparison.OrdinalIgnoreCase))
                unit.SerializableData.mParentScope = SPContentTypeScope.Web;
            else
                unit.SerializableData.mParentScope = SPContentTypeScope.List;

            unit.SerializableData.mDisplayFormWebUrl = GetWebUrlFromFileUrl(ct.ParentWeb, unit.SerializableData.mDisplayFormUrl);
            unit.SerializableData.mEditFormWebUrl = GetWebUrlFromFileUrl(ct.ParentWeb, unit.SerializableData.mEditFormUrl);
            unit.SerializableData.mNewFormWebUrl = GetWebUrlFromFileUrl(ct.ParentWeb, unit.SerializableData.mNewFormUrl);

            GetXmlDocuments(ct, unit.SerializableData.mXmlDocuments);
        }

        public void SetSPObjectPropByUnit(IAveContentType ct)
        {
            if (ct.ReadOnly || ct.Sealed)
                return;

            ct.Description = this.SerializableData.mDescription;
            ct.DisplayFormTemplateName = this.SerializableData.mDisplayFormTemplateName;
            ct.DocumentTemplate = this.SerializableData.mDocumentTemplate;
            ct.EditFormTemplateName = this.SerializableData.mEditFormTemplateName;
            ct.Group = this.SerializableData.mGroup;
            ct.Hidden = this.SerializableData.mHidden;
            ct.Name = this.SerializableData.mName;
            ct.NewDocumentControl = this.SerializableData.mNewDocumentControl;
            ct.NewFormTemplateName = this.SerializableData.mNewFormTemplateName;
            ct.RequireClientRenderingOnNew = this.SerializableData.mRequireClientRenderingOnNew;

            ct.DisplayFormUrl = GetFileUrlFromWebUrl(this.SerializableData.mDisplayFormUrl, this.SerializableData.mDisplayFormWebUrl, ct.ParentWeb.ServerRelativeUrl);
            ct.EditFormUrl = GetFileUrlFromWebUrl(this.SerializableData.mEditFormUrl, this.SerializableData.mEditFormWebUrl, ct.ParentWeb.ServerRelativeUrl);
            ct.NewFormUrl = GetFileUrlFromWebUrl(this.SerializableData.mNewFormUrl, this.SerializableData.mNewFormWebUrl, ct.ParentWeb.ServerRelativeUrl);

            SetXmlDocuments(ct, this.SerializableData.mXmlDocuments);
            ct.Update();

        }

        public List<SPContentTypeUnit> GetAllParentUnits(bool reverse)
        {
            List<SPContentTypeUnit> parentUnits = new List<SPContentTypeUnit>();
            GetParentUnits(parentUnits, this);
            if (reverse)
                parentUnits.Reverse();
            return parentUnits;
        }

        public List<SPContentTypeUnit> GetAllUnits(bool reverse)
        {
            List<SPContentTypeUnit> allUnits = new List<SPContentTypeUnit>();
            allUnits.Add(this);
            GetParentUnits(allUnits, this);
            if (reverse)
                allUnits.Reverse();

            return allUnits;
        }

        private string GetWebUrlFromFileUrl(IAveWeb curWeb, string fileUrl)
        {
            string rlt = string.Empty;
            if (string.IsNullOrEmpty(fileUrl) || fileUrl.TrimEnd('/').StartsWith("~layouts/") || fileUrl.TrimEnd('/').StartsWith("_layouts/"))
                return rlt;

            IAveFile tempFile = null;
            try
            {
                tempFile = curWeb.GetFile(fileUrl);
                if (tempFile != null && tempFile.ServerRelativeUrl != null && tempFile.ServerRelativeUrl.StartsWith(curWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))//ywzhang
                    rlt = curWeb.ServerRelativeUrl;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.WebUrlGetFailed, ex);
            }
            return rlt;
        }

        private string GetFileUrlFromWebUrl(string oldUrl, string oldWebUrl, string newWebUrl)
        {
            string rlt = oldUrl;
            if (!string.IsNullOrEmpty(oldWebUrl))
            {
                if (rlt.StartsWith(oldWebUrl, StringComparison.OrdinalIgnoreCase) && rlt.Length > oldWebUrl.Length)
                {
                    rlt = rlt.Substring(oldWebUrl.Length);
                    if (rlt.StartsWith("/", StringComparison.Ordinal))
                        rlt = rlt.Substring(1);
                }
            }
            return rlt;
        }

        private void GetParentUnits(List<SPContentTypeUnit> parentUnits, SPContentTypeUnit unit)
        {
            if (unit.mParentUnit != null)
            {
                parentUnits.Add(unit.mParentUnit);
                GetParentUnits(parentUnits, unit.mParentUnit);
            }
        }

        private void GetXmlDocuments(IAveContentType ct, List<string> collection)
        {
            if (ct.XmlDocuments != null && ct.XmlDocuments.Count > 0)
            {
                collection.Clear();
                foreach (string s in ct.XmlDocuments)
                {
                    collection.Add(s);
                }
            }
        }

        private void SetXmlDocuments(IAveContentType ct, List<string> collection)
        {
            if (collection != null && collection.Count > 0)
            {
                XmlDocument doc = null;
                try
                {
                    foreach (string s in collection)
                    {
                        try
                        {
                            doc = new XmlDocument();
                            doc.LoadXml(s);
                            if (ct.XmlDocuments != null)
                            {
                                string temp = ct.XmlDocuments[doc.FirstChild.NamespaceURI];
                                if (string.IsNullOrEmpty(temp))
                                {
                                    ct.XmlDocuments.Add(doc);
                                }
                            }
                            doc.RemoveAll();
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.LoadXmlContentError, ex);
                        }
                    }
                }
                finally
                {
                    if (doc != null)
                        doc.RemoveAll();
                }
            }
        }

        internal SPContentTypeSerializableData FixupSerializableData()
        {
            if (this.mParentUnit != null)
                this.SerializableData.mParentData = this.mParentUnit.FixupSerializableData();
            return this.SerializableData;
        }
    }

    public class InstanceProcCreationParam
    {

        private IAveListItem mParentItem;
        private IAveWeb mParentWeb;
        //private SqlConnection mConn;
        private IAveBackupRestoreQueryService mQueryService;
        private SPWFProcessorType mProcType;
        private SPFieldProcessor mWebLevelFieldProcessor;
        private SPWFAssociationProc mAssociationProc;
        private bool mOverwrite;
        private bool mAppend;

        public IAveListItem ParentItem
        {
            get { return mParentItem; }
            set
            {
                if (value == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceParentItemIsNull);
                try
                {
                    mParentItem = ReloadParentItem(value);
                }
                catch (SPWFProcessorException)
                {
                    //Add for Platform.If source has been deleted,we cannot reload ParentItem.
                    mParentItem = value;
                }
            }
        }
        public IAveWeb ParentWeb
        {
            get { return mParentWeb; }
            set
            {
                if (value == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceParentItemIsNull);
                try
                {
                    mParentWeb = ReloadParentWeb(value);
                }
                catch (SPWFProcessorException)
                {
                    //Add for Platform.If source has been deleted,we cannot reload ParentItem.
                    mParentWeb = value;
                }
            }
        }
        //public SqlConnection Conn
        //{
        //    get { return mConn; }
        //    set { mConn = value; }
        //}

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
            set { mQueryService = value; }
        }

        public SPWFProcessorType ProcType
        {
            get { return mProcType; }
            set { mProcType = value; }
        }
        public SPFieldProcessor WebLevelFieldProcessor
        {
            get { return mWebLevelFieldProcessor; }
            set
            {
                mWebLevelFieldProcessor = value;
                if (mWebLevelFieldProcessor == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.WebLevelFieldProcessorIsNull);
            }
        }
        public SPWFAssociationProc AssociationProc
        {
            get { return mAssociationProc; }
            set { mAssociationProc = value; }
        }

        private List<ICustomWorkflowInstanceProc> mCustomProcs;
        public List<ICustomWorkflowInstanceProc> CustomProcessors
        {
            get
            {
                if (mCustomProcs == null)
                    mCustomProcs = new List<ICustomWorkflowInstanceProc>();
                return mCustomProcs;
            }
            set
            {
                mCustomProcs = value;
            }
        }
        public bool Overwrite
        {
            get { return mOverwrite; }
            set { mOverwrite = value; }
        }
        public bool Append
        {
            get { return mAppend; }
            set { mAppend = value; }
        }

        private IAveListItem ReloadParentItem(IAveListItem item)
        {
            try
            {
                int id = item.ID;

                Guid parentSiteId;
                Guid parentWebId;
                Guid parentListId;

                using (IAveWeb web = item.Web)
                {
                    using (IAveSite site = web.Site)
                    {
                        parentSiteId = site.ID;
                    }
                    parentWebId = web.ID;
                    parentListId = item.ParentList.ID;
                }

                using (IAveSite s = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite(parentSiteId))
                {
                    using (IAveWeb w = s.OpenWeb(parentWebId))
                    {
                        IAveList list = w.Lists[parentListId];
                        return list.GetItemById(id);
                    }
                }
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.ReloadParentItemException, e);
            }

        }
        private IAveWeb ReloadParentWeb(IAveWeb web)
        {
            try
            {


                Guid parentSiteId;
                Guid parentWebId;


                using (IAveSite site = web.Site)
                {

                    parentSiteId = site.ID;
                }
                parentWebId = web.ID;

                using (IAveSite s = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite(parentSiteId))
                {
                    IAveWeb w = s.OpenWeb(parentWebId);
                    return w;
                }
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.ReloadParentItemException, e);
            }

        }
    }

    internal class WorkflowFixupParams
    {
        public Dictionary<Guid, Guid> mWebApplicationIdDic;
        public Dictionary<Guid, Guid> mSiteIdDic;
        public Dictionary<Guid, Guid> mWebIdDic;
        public Dictionary<Guid, Guid> mListIdDic;
        public Dictionary<Guid, Guid> mTaskListIdDic;
        public Dictionary<Guid, Guid> mHistoryListIdDic;
        public Dictionary<Guid, Guid> mItemGuidDic;
        public Dictionary<Guid, Guid> mTaskItemGuidDic;
        public Dictionary<Guid, Guid> mLastTaskItemGuidDic;
        public Dictionary<Guid, Guid> mParentAssoicationIdDic;
        public Dictionary<Guid, Guid> mParentAssociationBaseIdDic;
        public Dictionary<Guid, Guid> mInstanceIdDic;
        public Dictionary<Guid, Guid> mSubscriptionIdDic;
        public Dictionary<int, int> mItemIdDic;
        public Dictionary<int, int> mTaskItemIdDic;
        public Dictionary<int, int> mLastTaskItemIdDic;
        public Dictionary<int, int> mInternalStateDic;
        public Dictionary<string, string> mAssemblyNameDic;

        public Dictionary<string, object> mCustomDic1;
        public Dictionary<Guid, Guid> mCustomDic2;
        public Dictionary<int, int> mCustomDic3;


        public WorkflowFixupParams()
        {
            mWebApplicationIdDic = new Dictionary<Guid, Guid>(1);
            mSiteIdDic = new Dictionary<Guid, Guid>(1);
            mWebIdDic = new Dictionary<Guid, Guid>(1);
            mListIdDic = new Dictionary<Guid, Guid>(1);
            mTaskListIdDic = new Dictionary<Guid, Guid>(1);
            mHistoryListIdDic = new Dictionary<Guid, Guid>(1);
            mItemGuidDic = new Dictionary<Guid, Guid>(1);
            mTaskItemGuidDic = new Dictionary<Guid, Guid>();
            mParentAssoicationIdDic = new Dictionary<Guid, Guid>(1);
            mParentAssociationBaseIdDic = new Dictionary<Guid, Guid>(1);
            mInstanceIdDic = new Dictionary<Guid, Guid>(1);
            mSubscriptionIdDic = new Dictionary<Guid, Guid>();
            mLastTaskItemGuidDic = new Dictionary<Guid, Guid>(1);
            mItemIdDic = new Dictionary<int, int>(1);
            mTaskItemIdDic = new Dictionary<int, int>();
            mLastTaskItemIdDic = new Dictionary<int, int>(1);

            mAssemblyNameDic = new Dictionary<string, string>(1);

            mInternalStateDic = new Dictionary<int, int>(1);


            mCustomDic1 = new Dictionary<string, object>();
            mCustomDic2 = new Dictionary<Guid, Guid>();
            mCustomDic3 = new Dictionary<int, int>();
        }

        public void Dispose()
        {
            mWebApplicationIdDic.Clear();
            mSiteIdDic.Clear();
            mWebIdDic.Clear();
            mListIdDic.Clear();
            mTaskListIdDic.Clear();
            mHistoryListIdDic.Clear();
            mItemGuidDic.Clear();
            mTaskItemGuidDic.Clear();
            mParentAssoicationIdDic.Clear();
            mParentAssociationBaseIdDic.Clear();
            mInstanceIdDic.Clear();
            mItemIdDic.Clear();
            mTaskItemIdDic.Clear();
            mAssemblyNameDic.Clear();

            mInternalStateDic.Clear();
            mSubscriptionIdDic.Clear();
            mLastTaskItemGuidDic.Clear();
            mLastTaskItemIdDic.Clear();

            mCustomDic1.Clear();
            mCustomDic2.Clear();
            mCustomDic3.Clear();
        }
    }

    public class SPWorkflowCommon
    {
        private const string APPROVAL_BASEID = "8AD4D8F0-93A7-4941-9657-CF3706F00409";          //Approval
        private const string COLLECT_FEEDBACK_BASEID = "3BFB07CB-5C6A-4266-849B-8D6711700409";  //Collect Feedback  
        private const string COLLECT_SIGNATURE_BASEID = "77C71F43-F403-484B-BCB2-303710E00409"; //Collect signature
        private const string PUBLISHING_APPROVAL_BASEID = "E43856D2-1BB4-40ef-B08B-016D89A00409";//Publishing Approval

        #region 13 Plantform Type
        public const string PROPS_13MODEL = "Props.13Model";
        public const string PROPS_13MODEL_WFDEFINITION = "Props.13Model.WFDefinition";
        #endregion
        public static string[] BuiltInWorkflowBaseID = new string[] { APPROVAL_BASEID, COLLECT_FEEDBACK_BASEID, COLLECT_SIGNATURE_BASEID, PUBLISHING_APPROVAL_BASEID };

        public const string OriginalUniqueIdFieldName = "OriginalUniqueId";
        public const string GUIDREG = "[A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12}";
        //public const string GUIDREG = "[A-F0-9]^8-([A-F0-9]^4-)^3[A-F0-9]^12"; in visual studio find

        private static Dictionary<string, string> mEmailMapping;
        public static Dictionary<string, string> EmailMapping
        {
            get
            {
                if (mEmailMapping == null)
                    mEmailMapping = new Dictionary<string, string>();
                return mEmailMapping;
            }

        }

        private static Dictionary<string, string> mStatusFieldMapping;
        public static Dictionary<string, string> StatusFieldMapping
        {
            get
            {
                if (mStatusFieldMapping == null)
                    mStatusFieldMapping = new Dictionary<string, string>();
                return mStatusFieldMapping;
            }
        }

        public static bool StringIsGUIDFormat(string inStr)
        {
            bool result = false;
            int startat = 0;
            if (!string.IsNullOrEmpty(inStr))
            {
                if (inStr.StartsWith("{", StringComparison.Ordinal) && inStr.EndsWith("}", StringComparison.Ordinal))
                    startat = 1;
                Regex guidRE = new Regex(GUIDREG, RegexOptions.IgnoreCase);
                MatchCollection guids = guidRE.Matches(inStr, startat);
                if (guids.Count == 1)
                    result = true;
            }
            return result;
        }

        public static string OnModifyEmailAddress(object sender, string srcEmail)
        {
            string desEmail = srcEmail;
            if (EmailMapping.ContainsKey(srcEmail))
            {
                desEmail = EmailMapping[srcEmail];
            }
            return desEmail;
        }
    }

    public class FileCacheService : CacheService
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, string> mServiceParams;
        public FileCacheService()
        { }

        public FileCacheService(Dictionary<string, string> param)
        {
            if (param != null)
            {
                mServiceParams = new Dictionary<string, string>(param);
                param.Clear();
            }
        }

        public override void CacheData(string siteId, string webId, string listId, string parentId, int itemId, string index, byte[] data)
        {
            string rootDirectory = null;
            try
            {
                if (mServiceParams.ContainsKey("RootDirectory"))
                {
                    rootDirectory = mServiceParams["RootDirectory"];
                }
                else
                {
                    return;
                }
                string ctInfoPath = string.Empty;

                string fullDirectoryPath = SecurityUtils.SafeCombinePath(rootDirectory, siteId);
                if (!string.IsNullOrEmpty(webId))
                    fullDirectoryPath = SecurityUtils.SafeCombinePath(fullDirectoryPath, webId);
                if (!string.IsNullOrEmpty(parentId))
                {
                    if (parentId.StartsWith("0x", StringComparison.Ordinal))
                    {
                        fullDirectoryPath = SecurityUtils.SafeCombinePath(fullDirectoryPath, "CTWFDataCache", parentId);
                        if (!string.IsNullOrEmpty(listId))
                        {
                            ctInfoPath = SecurityUtils.SafeCombinePath(fullDirectoryPath, "info.xml");
                        }
                    }
                    else
                    {
                        fullDirectoryPath = SecurityUtils.SafeCombinePath(fullDirectoryPath, "ListWFDataCache", parentId);
                    }
                }
                else
                {
                    fullDirectoryPath = SecurityUtils.SafeCombinePath(fullDirectoryPath, "WebWFDataCache", webId);
                }

                if (!Directory.Exists(fullDirectoryPath))
                    Directory.CreateDirectory(fullDirectoryPath);
                if (!string.IsNullOrEmpty(ctInfoPath))
                {
                    using (StreamWriter sw = new StreamWriter(ctInfoPath))
                    {
                        sw.WriteLine(listId);
                        sw.Flush();
                    }
                }

                StringBuilder filename = new StringBuilder();
                if (itemId < 0)
                {
                    if (itemId == int.MinValue)
                    {
                        filename.Append("_");
                    }
                    itemId = 0;
                }
                filename.Append(itemId.ToString());
                filename.Append(".");
                filename.Append(index);
                filename.Append(".dat");

                var fileNamePath = SecurityUtils.SafeCombinePath(fullDirectoryPath, filename.ToString());

                File.WriteAllBytes(fileNamePath, data);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, "Error in cache workflow data.{0}", e.ToString());
            }
        }

    }

    public class WebPostponeActionService : PostponeActionService
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, string> mServiceParams;
        private const string CT_WFDATA_FOLDER = "\\CTWFDataCache";
        private const string LIST_WFDATA_FOLDER = "\\ListWFDataCache";
        private const string WEB_WFDATA_FOLDER = "\\WebWFDataCache";
        // private const string PROJECT_WFDATA_FOLDER = "\\ProjectWFDataCache";
        private string[] mWFDataFolders = new string[] { CT_WFDATA_FOLDER, LIST_WFDATA_FOLDER, WEB_WFDATA_FOLDER };
        public WebPostponeActionService()
        { }

        public WebPostponeActionService(Dictionary<string, string> param)
        {
            if (param != null)
            {
                mServiceParams = new Dictionary<string, string>(param);
                param.Clear();
            }
        }
        public override void Execute(SPWFAssociationProc associationProcessor, SPWFInstanceProc instanceProcessor)
        {
            string rootDirectory = null;
            if (mServiceParams.ContainsKey("RootDirectory"))
            {
                rootDirectory = mServiceParams["RootDirectory"];
            }
            else
            {
                return;
            }

            //Guid siteId;
            Guid webId;
            Guid listId;

            foreach (string sitePath in Directory.GetDirectories(rootDirectory))
            {
                try
                {
                    string[] temp = sitePath.Split(new char[] { '\\' });
                    string tempString = temp[temp.Length - 1];
                    if (tempString.Length != 36)
                    {
                        //Should not be the workflow cache, TODO: need optimize later, get the accurate path
                        continue;
                    }
                    //siteId = new Guid(temp[temp.Length - 1]);
                    using (IAveSite site = associationProcessor.Site)
                    {
                        foreach (string webPath in Directory.GetDirectories(sitePath))
                        {
                            temp = webPath.Split(new char[] { '\\' });
                            webId = new Guid(temp[temp.Length - 1]);
                            using (IAveWeb web = site.OpenWeb(webId))
                            {
                                try
                                {
                                    foreach (string folderPath in mWFDataFolders)
                                    {
                                        string lists = webPath + folderPath;
                                        if (!Directory.Exists(lists)) continue;
                                        foreach (string listPath in Directory.GetDirectories(lists))
                                        {
                                            temp = listPath.Split(new char[] { '\\' });
                                            string[] files = Directory.GetFiles(listPath, "*.dat");
                                            Array.Sort(files);
                                            object parentObj = null;
                                            switch (folderPath)
                                            {
                                                case LIST_WFDATA_FOLDER:
                                                    listId = new Guid(temp[temp.Length - 1]);
                                                    parentObj = web.Lists[listId];
                                                    break;
                                                case WEB_WFDATA_FOLDER:
                                                    parentObj = web;
                                                    break;
                                                case CT_WFDATA_FOLDER:
                                                    //string[] ids = temp[temp.Length - 1].Split(new char[] { '.' });
                                                    //if (ids.Length == 1)
                                                    //{
                                                    //    parentObj = web.ContentTypes.GetById(ids[0]);
                                                    //}
                                                    //else if (ids.Length == 2)
                                                    //{
                                                    //    parentObj = web.Lists[new Guid(ids[1])].ContentTypes.GetById(ids[0]);
                                                    //}
                                                    string[] infoFile = Directory.GetFiles(listPath, "*.xml");
                                                    string listIdOfCT = string.Empty;
                                                    if (infoFile.Length > 0)
                                                    {
                                                        using (StreamReader sr = new StreamReader(infoFile[0], Encoding.UTF8))
                                                        {
                                                            listIdOfCT = sr.ReadLine();
                                                        }
                                                        parentObj = web.Lists[new Guid(listIdOfCT)].ContentTypes.GetById(temp[temp.Length - 1]);
                                                    }
                                                    else
                                                    {
                                                        parentObj = web.ContentTypes.GetById(temp[temp.Length - 1]);
                                                    }
                                                    break;
                                                default: break;
                                            }

                                            List<SPWFAssociationUnit> allUnit = new List<SPWFAssociationUnit>();
                                            foreach (string fileFullPath in files)
                                            {
                                                temp = fileFullPath.Split(new char[] { '\\' });
                                                string relativePath = temp[temp.Length - 1];
                                                if (relativePath.StartsWith("_")) continue;

                                                string[] temp1 = relativePath.Split(new char[] { '.' });
                                                string extName = temp1[temp1.Length - 1];
                                                int itemId = int.Parse(temp1[0]);

                                                if (itemId == 0)
                                                {
                                                    allUnit.Add(SPWFAssociationUnit.Load(File.ReadAllBytes(fileFullPath)));
                                                    File.Delete(fileFullPath);
                                                }

                                            }

                                            allUnit.Sort(new SPWFAssociationUnitComparerCreatedTime());

                                            foreach (SPWFAssociationUnit unit in allUnit)
                                            {
                                                try
                                                {
                                                    if (parentObj is IAveContentType)
                                                    {
                                                        unit.reusableWFContentTypeName = ((IAveContentType)parentObj).Name;
                                                    }
                                                    associationProcessor.OnRestoreWFAssociation(unit, new RestoreWFDefinitionEventArgs { ParentObject = parentObj });
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.RestoreAssocialUnitError, e.ToString());
                                                }

                                            }

                                            foreach (string fileFullPath in files)
                                            {
                                                try
                                                {
                                                    temp = fileFullPath.Split(new char[] { '\\' });
                                                    string relativePath = temp[temp.Length - 1];

                                                    string[] temp1 = relativePath.Split(new char[] { '.' });
                                                    string extName = temp1[temp1.Length - 1];
                                                    int itemId = int.Parse(temp1[0].TrimStart('_'));

                                                    if (itemId > 0 && folderPath != WEB_WFDATA_FOLDER)
                                                    {
                                                        IAveListItem item = null;
                                                        SPWFAssociationParentType type = SPWFAssociationParentType.Invalid;
                                                        if (folderPath == CT_WFDATA_FOLDER)
                                                        {
                                                            type = SPWFAssociationParentType.ListContentType;
                                                        }
                                                        else if (folderPath == LIST_WFDATA_FOLDER)
                                                        {
                                                            type = SPWFAssociationParentType.List;
                                                        }
                                                        if (parentObj is IAveList)
                                                        {
                                                            item = ((IAveList)parentObj).GetItemById(itemId);
                                                        }
                                                        try
                                                        {
                                                            instanceProcessor.OnRestoreWFInstance(SPWFInstanceUnit.Load(File.ReadAllBytes(fileFullPath)), new RestoreWFInstanceEventArgs() { ParentObject = item, ParentObjectType = type });
                                                        }
                                                        finally
                                                        {
                                                            File.Delete(fileFullPath);
                                                        }
                                                    }
                                                    else if (relativePath.StartsWith("_"))
                                                    {
                                                        try
                                                        {
                                                            instanceProcessor.OnRestoreWFInstance(SPWFInstanceUnit.Load(File.ReadAllBytes(fileFullPath)), new RestoreWFInstanceEventArgs { ParentObject = web, ParentObjectType = SPWFAssociationParentType.Web });
                                                        }
                                                        finally
                                                        {
                                                            File.Delete(fileFullPath);
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    if (!(e is FormatException) || !e.Message.Contains("Guid should contain 32 digits with 4 dashes"))
                                                    {
                                                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.RestoreFileError, e.ToString());
                                                    }
                                                }
                                            }
                                            logger.Info($"Delete Directory [{listPath}].Location:WebPostponeActionService.Execute(2).listPath");
                                            Directory.Delete(listPath, true);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.PostActionError, ex);
                                }
                            }
                            Thread.Sleep(100);
                            logger.Info($"Delete Directory [{webPath}].Location:WebPostponeActionService.Execute(2).webPath");
                            Directory.Delete(webPath, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is FormatException) || !ex.Message.Contains("Guid should contain 32 digits with 4 dashes"))
                    {
                        logger.Log(AveLogLevel.INFO, WrapperWorkflowResource.PostActionError, ex);
                    }
                }
                Thread.Sleep(100);
                logger.Info($"Delete Directory [{sitePath}].Location:WebPostponeActionService.Execute(2).sitePath");
                Directory.Delete(sitePath, true);
            }
        }

        public void Execute(IAveSite spSite, AveMappingManager mapping, RestoreWFDefinitionEventHandler definitionExecution)
        {
            string rootDirectory = null;
            if (mServiceParams.ContainsKey("RootDirectory"))
            {
                rootDirectory = mServiceParams["RootDirectory"];
            }
            else
            {
                return;
            }

            Guid siteId;
            Guid webId;
            Guid listId;

            foreach (string sitePath in Directory.GetDirectories(rootDirectory))
            {
                try
                {
                    string[] temp = sitePath.Split(new char[] { '\\' });
                    string tempString = temp[temp.Length - 1];
                    if (tempString.Length != 36
                        ||!Guid.TryParse(tempString,out siteId)
                        ||siteId!=spSite.ID)
                    {
                        //not guid or not current site id
                        //Should not be the workflow cache, TODO: need optimize later, get the accurate path
                        continue;
                    }
                    IAveSite site = spSite;
                    {
                        foreach (string webPath in Directory.GetDirectories(sitePath))
                        {
                            temp = webPath.Split(new char[] { '\\' });
                            webId = new Guid(temp[temp.Length - 1]);
                            using (IAveWeb web = site.OpenWeb(webId))
                            {
                                try
                                {
                                    foreach (string folderPath in mWFDataFolders)
                                    {
                                        string lists = webPath + folderPath;
                                        if (!Directory.Exists(lists)) continue;
                                        foreach (string listPath in Directory.GetDirectories(lists))
                                        {
                                            temp = listPath.Split(new char[] { '\\' });
                                            string[] files = Directory.GetFiles(listPath, "*.dat");
                                            Array.Sort(files);
                                            object parentObj = null;
                                            switch (folderPath)
                                            {
                                                case LIST_WFDATA_FOLDER:
                                                    listId = new Guid(temp[temp.Length - 1]);
                                                    parentObj = web.Lists[listId];
                                                    break;
                                                case WEB_WFDATA_FOLDER:
                                                    parentObj = web;
                                                    break;
                                                case CT_WFDATA_FOLDER:
                                                    string[] infoFile = Directory.GetFiles(listPath, "*.xml");
                                                    string listIdOfCT = string.Empty;
                                                    if (infoFile.Length > 0)
                                                    {
                                                        using (StreamReader sr = new StreamReader(infoFile[0], Encoding.UTF8))
                                                        {
                                                            listIdOfCT = sr.ReadLine();
                                                        }
                                                        parentObj = web.Lists[new Guid(listIdOfCT)].ContentTypes.GetById(temp[temp.Length - 1]);
                                                    }
                                                    else
                                                    {
                                                        parentObj = web.ContentTypes.GetById(temp[temp.Length - 1]);
                                                    }
                                                    break;
                                                default: break;
                                            }

                                            List<SPWFAssociationUnit> allUnit = new List<SPWFAssociationUnit>();
                                            foreach (string fileFullPath in files)
                                            {
                                                temp = fileFullPath.Split(new char[] { '\\' });
                                                string relativePath = temp[temp.Length - 1];
                                                if (relativePath.StartsWith("_")) continue;

                                                string[] temp1 = relativePath.Split(new char[] { '.' });
                                                string extName = temp1[temp1.Length - 1];
                                                int itemId = int.Parse(temp1[0]);

                                                if (itemId == 0)
                                                {
                                                    allUnit.Add(SPWFAssociationUnit.Load(File.ReadAllBytes(fileFullPath)));
                                                    File.Delete(fileFullPath);
                                                }
                                            }

                                            allUnit.Sort(new SPWFAssociationUnitComparerCreatedTime());

                                            foreach (SPWFAssociationUnit unit in allUnit)
                                            {
                                                try
                                                {
                                                    if (parentObj is IAveContentType)
                                                    {
                                                        unit.reusableWFContentTypeName = ((IAveContentType)parentObj).Name;
                                                    }
                                                    definitionExecution(unit, new RestoreWFDefinitionEventArgs { ParentObject = parentObj,Mapping=mapping });
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.RestoreAssocialUnitError, e.ToString());
                                                }

                                            }
                                            logger.Info($"Delete Directory [{listPath}].Location:WebPostponeActionService.Execute.listPath");
                                            Directory.Delete(listPath, true);
                                            
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.PostActionError, ex);
                                }
                            }
                            Thread.Sleep(100);
                            logger.Info($"Delete Directory [{webPath}].Location:WebPostponeActionService.Execute.WebPath");
                            Directory.Delete(webPath, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is FormatException) || !ex.Message.Contains("Guid should contain 32 digits with 4 dashes"))
                    {
                        logger.Log(AveLogLevel.INFO, WrapperWorkflowResource.PostActionError, ex);
                    }
                }
                Thread.Sleep(100);
                logger.Info($"Delete Directory [{sitePath}].Location:WebPostponeActionService.Execute.SitePath");
                Directory.Delete(sitePath, true);

            }
        }
    }

    public class NativeLanguageMappingService : LanguageMappingService
    {

        private Dictionary<string, string> mServiceParams;


        private Dictionary<string, string> mListTitleMapping = new Dictionary<string, string>();
        private Dictionary<string, string> mFieldNameMapping = new Dictionary<string, string>();
        private Dictionary<string, string> mPermissionMapping = new Dictionary<string, string>();
        public NativeLanguageMappingService()
        { }

        public NativeLanguageMappingService(Dictionary<string, string> param)
        {
            if (param != null)
            {
                mServiceParams = new Dictionary<string, string>(param);
                param.Clear();

                foreach (KeyValuePair<string, string> pair in mServiceParams)
                {
                    string[] temp1 = pair.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in temp1)
                    {
                        string[] temp2 = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (temp2.Length == 2)
                        {
                            LanguageMappingScopeEnum scope = (LanguageMappingScopeEnum)Enum.Parse(typeof(LanguageMappingScopeEnum), pair.Key);
                            switch (scope)
                            {
                                case LanguageMappingScopeEnum.ListTitle:
                                    mListTitleMapping[temp2[0].ToLower()] = temp2[1];
                                    break;
                                case LanguageMappingScopeEnum.FieldName:
                                    mFieldNameMapping[temp2[0].ToLower()] = temp2[1];
                                    break;
                                case LanguageMappingScopeEnum.Permission:
                                    mPermissionMapping[temp2[0].ToLower()] = temp2[1];
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

        }

        public override void Dispose()
        {
            mListTitleMapping.Clear();
            mFieldNameMapping.Clear();
            mPermissionMapping.Clear();
            base.Dispose();
        }
        public override string GetMappedName(LanguageMappingScopeEnum scope, string originalName)
        {
            string result = originalName;
            Dictionary<string, string> temp = mListTitleMapping;
            switch (scope)
            {
                case LanguageMappingScopeEnum.ListTitle:
                    temp = mListTitleMapping;
                    break;
                case LanguageMappingScopeEnum.FieldName:
                    temp = mFieldNameMapping;
                    break;
                case LanguageMappingScopeEnum.Permission:
                    temp = mPermissionMapping;
                    break;
                default:
                    temp = null;
                    break;
            }
            if (temp != null && temp.ContainsKey(originalName.ToLower()))
            {
                result = temp[originalName.ToLower()];
            }
            return result;
        }
    }

    public class WorkflowServiceFactory : AvePoint.Common.ISingleton
    {
        private IAveWorkflowServicesManager mAveWorkflowServiceManager = null;
        private IAveWeb mWeb = null;

        public string CurrentWebUrl
        {
            get { return mWeb.Url; }
        }

        internal IAveWorkflowServicesManager WFServiceManager
        {
            get { return mAveWorkflowServiceManager; }
        }

        public IAveWorkflowSubscriptionService WFSubscriptionService
        {
            get
            {
                return mAveWorkflowServiceManager == null ? null : mAveWorkflowServiceManager.GetWorkflowSubscriptionService();
            }
        }

        public IAveWorkflowDeploymentService WFDeploymentService
        {
            get { return mAveWorkflowServiceManager == null ? null : mAveWorkflowServiceManager.GetWorkflowDeploymentService(); }
        }

        public IAveWorkflowInstanceService WFInstanceService
        {
            get { return mAveWorkflowServiceManager == null ? null : mAveWorkflowServiceManager.GetWorkflowInstanceService(); }
        }

        private WorkflowServiceFactory(IAveWeb web)
        {
            if (web != null)
            {
                mWeb = web;
                this.mAveWorkflowServiceManager = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowServicesManager(web);
            }
        }

        /// <summary>
        /// 只需要在开始还原Workflow Association甚至是SPWeb的时候执行此方法。
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        public bool UpdateWorkflowServiceManager(IAveWeb web)
        {
            if (mWeb == null || !web.ID.Equals(mWeb.ID))
            {
                if (web == null)
                {
                    return false;
                }
                mWeb = web;
                this.mAveWorkflowServiceManager = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowServicesManager(web);
                return true;
            }
            return false;
        }
    }

}
