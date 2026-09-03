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
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Globalization;
using LS.SPWorkflowProcessor.SerializableObjects;
using LS.SPWorkflowProcessor.Services;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using LS.SPWorkflowProcessor.Common;
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Resource.Workflow;

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
        public object ParentObject { get; set; }
        public SPWFAssociationParentType ParentObjectType { get; set; }
    }

    public class SPWFAssociationUnit
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

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
        private IAveSPWeb mParentAveSPWeb;
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
        //private List<Guid> m2010BuildinBaseIds;
        //private List<Guid> m2007BuildinBaseIds;
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
        public IAveSPWeb ParentAveSPWeb
        {
            get
            {
                return mParentAveSPWeb;
            }
            set
            {
                mParentAveSPWeb = value;
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

        public void ReloadSPAssociation()
        {
            mSPAssociation = SPAssoicationCollection[mSPAssociation.ID];
        }

        #region Reload Parent Object

        /// <summary>
        /// reload SPWFAssociationUnit的parent object，尽量使用内部封装的reload方法，避免乱reload导致对象不一致
        /// 正常情况下，reload parent，object本身也需要reload，由于ContentType没有reload方法，所以需要reload list或web后重取ContentType
        /// </summary>
        public void ReloadParentWeb()
        {
            switch (this.ParentObjectType)
            {

                case SPWFAssociationParentType.List:
                    ((IAveList)ParentObject).ParentWeb.ReloadWeb();
                    ReloadParentList();
                    break;
                case SPWFAssociationParentType.Web:
                    ((IAveWeb)ParentObject).ReloadWeb();
                    break;
                case SPWFAssociationParentType.ListContentType:
                    ((IAveContentType)ParentObject).ParentList.ParentWeb.ReloadWeb();
                    ReloadParentContentType();
                    break;
                case SPWFAssociationParentType.WebContentType:
                    ReloadParentContentType();
                    break;
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
            }
        }

        public void ReloadParentList()
        {
            switch (this.ParentObjectType)
            {

                case SPWFAssociationParentType.List:
                    ((IAveList)ParentObject).Reload();
                    break;
                case SPWFAssociationParentType.ListContentType:
                    ReloadParentContentType();
                    break;
                default:
                    break;
            }
        }

        public void ReloadParentContentType()
        {
            switch (this.ParentObjectType)
            {
                case SPWFAssociationParentType.ListContentType:
                    ((IAveContentType)ParentObject).ParentList.Reload();
                    ParentObject = ((IAveContentType)ParentObject).ParentList.ContentTypes[ParentContentType.ID];
                    break;
                case SPWFAssociationParentType.WebContentType:
                    ((IAveContentType)ParentObject).ParentWeb.ReloadWeb();
                    ParentObject = ParentWeb.ContentTypes[ParentContentType.ID];
                    break;
                default:
                    break;
            }
        }

        public void ReloadParentObject()
        {
            switch (this.ParentObjectType)
            {

                case SPWFAssociationParentType.List:
                    ReloadParentList();
                    break;
                case SPWFAssociationParentType.Web:
                    ReloadParentWeb();
                    break;
                case SPWFAssociationParentType.ListContentType:
                    ReloadParentContentType();
                    break;
                case SPWFAssociationParentType.WebContentType:
                    ReloadParentContentType();
                    break;
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationParentTypeNotSupported);
            }
        }

        #endregion

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

        bool wfServiceManagerInited = false;
        public IAveWorkflowServicesManager WFServiceManager
        {
            get
            {
                if (!wfServiceManagerInited)
                {
                    try
                    {
                        mWorkflowServiceManager = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowServicesManager(ParentWeb);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Can not get workflow service manager for web {0}. Error {1}", ParentWeb.Url, e);
                    }
                    wfServiceManagerInited = true;
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
                            //using (IAveSite site = ParentWeb.Site)
                            //{
                            mSiteId = ParentWeb.Site.ID;
                            //}
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

        /// <summary>
        /// List workflow association对应的ParentList对象
        /// 属性中以后不要再加reload,如有必要reload该对象，请在调用处reload
        /// </summary>
        public IAveList ParentList
        {
            get
            {
                if (ParentObjectType == SPWFAssociationParentType.List)
                {
                    return (IAveList)ParentObject;
                }
                else if (ParentObjectType == SPWFAssociationParentType.ListContentType)
                {
                    return ((IAveContentType)ParentObject).ParentList;
                }
                else
                    return null;
            }
        }
        public IAveContentType ParentContentType
        {
            get
            {
                if (ParentObjectType == SPWFAssociationParentType.ListContentType ||
                    ParentObjectType == SPWFAssociationParentType.WebContentType)
                {
                    IAveContentType ct = (IAveContentType)ParentObject;
                    return ct;
                }
                return null;
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
                return (IsBuiltinBaseIdForSP2007 || IsBuiltinBaseIdForSP2010);
            }
        }
        public bool IsBuiltinBaseIdForSP2007
        {
            get
            {
                //buildin workflow baseid 判断都挪到BuildInWorkflowBaseIdCollection中
                return BuiltinWorkflowBaseIdCollection.IsBuiltinBaseIdForSP2007(this.SerializableData.mBaseId);
            }
        }
        public bool IsBuiltinBaseIdForSP2010
        {
            get
            {
                //buildin workflow baseid 判断都挪到BuildInWorkflowBaseIdCollection中
                return BuiltinWorkflowBaseIdCollection.IsBuiltinBaseIdForSP2010(this.SerializableData.mBaseId);
            }
        }

        public SPWorkflowSubListUnit TaskListUnit
        {
            get { return mTaskListUnit; }
        }
        public SPWorkflowSubListUnit HistListUnit
        {
            get { return mHistListUnit; }
        }

        public SPWorkflowSubListUnit TemplateLibUnit
        {
            get { return mTemplateLibUnit; }
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
                        logger.Warn("An exception occurred while get association unit properties. exception:{0}", e.ToString());
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
            set { mIsCurrentVersion = value ? 0 : 1; }
        }

        public bool IsSP13ModelWorkflow
        {
            get
            {
                return mSerializableData.Properties.ContainsKey("Props.13Model");
            }
        }

        public bool IsExportedNintexWorkflow
        {
            get
            {
                return mSerializableData.Properties.ContainsKey(SPWorkflowCommon.PROPS_ExportedNintex);
            }
        }

        public SPWFInternalPlatform WFInternalPlatform
        {
            get
            {
                if (IsSP13ModelWorkflow)
                {
                    return SPWFInternalPlatform.WF2013PlatformType;
                }
                else if (IsExportedNintexWorkflow)
                {
                    return SPWFInternalPlatform.WFExportedNintex;
                }
                else
                {
                    return SPWFInternalPlatform.WF2010PlatformType;
                }
            }
        }

        public WorkflowType WorkflowType { get; set; }

        public byte[] ExportFile { get; set; }

        public bool IsPostAction { get; set; }

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
            if (mSerializableData.ExportFileUnit != null)
            {
                this.ExportFile = mSerializableData.ExportFileUnit;
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
                    try
                    {
                        IAveList taskList = this.ParentWeb.Lists[taskListTitle];
                        IAveList historyList = this.ParentWeb.Lists[historyListTitle];
                    }
                    catch (Exception e)
                    {
                        //ADO-94145 继承自site content type上的list content type workflow association关联的tasks list和history list可能还没有创建出来
                        SPWorkflowProcessorRuntime.Log("cannot get task list:{0} or history list:{1}. Possible reason: this content type and workflow inherit from its parent web . exception:{2}", taskListTitle, historyListTitle, e.Message);
                        logger.Log(AveLogLevel.DEBUG, "An error occurred while creating workflow association, error message: {0}", e);
                        //如果是365，相关list没有创建出来的话，create 出来的association往ParentList中Add的时候会出错，
                        //这是API的一个limitation或者说是bug,365无法在关联的list不存在的情况下创建list contentType workflow association
                        return workflowAssociation.CreateSiteContentTypeAssociation(baseTemplate, name, taskListTitle, historyListTitle);
                    }
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
            //ADO-92709 keep workflow association's parent id.
            if (this.SerializableData != null && this.SerializableData.mParentAssociationId != null && this.SerializableData.mParentAssociationId != Guid.Empty)
            {
                newAsso.ParentAssociationId = this.SerializableData.mParentAssociationId;
            }
            //need to reload web even it is a list workflow association ,as the association is created by a new web,not the parent web
            switch (this.ParentObjectType)
            {
                case SPWFAssociationParentType.List:
                    //task list history list是通过新获取的web对象创建的，不是ParentList.ParentWeb,所以需要reload parentWeb，避免添加workflowAssociation时出错
                    //后续需要考虑去掉ParentWeb属性中的reload逻辑，只在需要reload的调用的地方添加reload
                    //ParentList.ParentWeb.ReloadWeb();
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

        private string GetFieldlTitle(IAveWorkflowAssociation workflowAssociation, ref IAveField field)
        {
            if (!string.IsNullOrEmpty(workflowAssociation.InternalNameStatusField) && ParentList != null)
            {
                field = ParentList.Fields.GetFieldByInternalName(workflowAssociation.InternalNameStatusField);
                return field.Title;
            }
            return null;
        }

        public void UpdateWorkflowAssociation(IAveWorkflowAssociation workflowAssociation)
        {
            switch (this.ParentObjectType)
            {
                case SPWFAssociationParentType.List:
                    IAveField field = null;
                    string fieldTitle = GetFieldlTitle(workflowAssociation, ref field);
                    ParentList.UpdateWorkflowAssociation(workflowAssociation);
                    string newFieldTitle = GetFieldlTitle(workflowAssociation, ref field);
                    if (!string.IsNullOrEmpty(fieldTitle) && !string.IsNullOrEmpty(newFieldTitle) && !fieldTitle.Equals(newFieldTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        field.Title = fieldTitle;
                        try
                        {
                            field.Update();
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while update field within updating workflow association, exception message: {0}", e.ToString());
                            workflowAssociation.ParentWeb.ReloadWeb();
                            workflowAssociation.ParentList.Reload();
                            field = workflowAssociation.ParentList.Fields.GetFieldByInternalName(workflowAssociation.InternalNameStatusField);
                            field.Title = fieldTitle;
                            field.Update();
                        }
                    }
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
                logger.Log(AveLogLevel.DEBUG, "An error occurred while updating workflow association, error message: {0}", e);
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
                logger.Warn("An exception occurred while save unit. exception:{0}", e.ToString());
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
                byte[] decompressedData = new byte[0];

                #region Decompress serialized Metadata
                using (MemoryStream tempStream = new MemoryStream(serializedMetadata))
                {
                    tempStream.Position = 0L;
                    byte[] temp = new byte[4096];
                    using (GZipStream gzipStream = new GZipStream(tempStream, CompressionMode.Decompress, true))
                    {
                        int readLen;
                        while ((readLen = gzipStream.Read(temp, 0, 4096)) != 0)
                        {
                            LSUtilityOfBytes.LSAppendBytes(ref decompressedData, temp, 0, readLen);
                        }
                    }
                    temp = null;
                }
                #endregion

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new WorkflowSerializationBinder();
                MemoryStream stream = new MemoryStream(decompressedData);
                SPWFAssociationSerializableData serializableData = (SPWFAssociationSerializableData)formatter.Deserialize(stream);
                stream.Dispose();
                decompressedData = null;
                return new SPWFAssociationUnit(serializableData);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AP_UnitLoadException, e.Message);
                logger.Warn("An exception occurred while load unit. exception:{0}", e.ToString());
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
            if (this.ExportFile != null)
            {
                this.SerializableData.ExportFileUnit = this.ExportFile;
            }
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

        public bool RestartRunningInstance
        {
            get;
            set;
        }

        public string InsternalVersion
        {
            get { return mInternalVersion; }
            set { mInternalVersion = value; }
        }

        public Hashtable mExtensionProperties = new Hashtable();

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
            if (mExtensionProperties != null) 
            {
                mExtensionProperties.Clear();
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Hist:History as key.")]
        internal Dictionary<string, object> GenerateBaseDictionary()
        {
            Dictionary<string, object> repDictionary = new Dictionary<string, object>();
            //******************************site id
            repDictionary.AddEx(this.FixupParameters.mSiteIdDic.GetKey(0).ToString().ToUpper(CultureInfo.InvariantCulture), this.FixupParameters.mSiteIdDic.GetValue(0));
            //******************************web id
            repDictionary.AddEx(this.FixupParameters.mWebIdDic.GetKey(0).ToString().ToUpper(CultureInfo.InvariantCulture), this.FixupParameters.mWebIdDic.GetValue(0));
            //******************************list id
            repDictionary.AddEx(this.FixupParameters.mListIdDic.GetKey(0).ToString().ToUpper(CultureInfo.InvariantCulture), this.FixupParameters.mListIdDic.GetValue(0));
            //******************************item guid
            repDictionary.AddEx(this.FixupParameters.mItemGuidDic.GetKey(0).ToString().ToUpper(CultureInfo.InvariantCulture), this.FixupParameters.mItemGuidDic.GetValue(0));
            //******************************instance id
            repDictionary.AddEx(this.FixupParameters.mInstanceIdDic.GetKey(0).ToString().ToUpper(CultureInfo.InvariantCulture), this.FixupParameters.mInstanceIdDic.GetValue(0));


            //******************************all task item guid
            foreach (KeyValuePair<Guid, Guid> pair in this.FixupParameters.mTaskItemGuidDic)
            {
                repDictionary.AddEx(pair.Key.ToString().ToUpper(CultureInfo.InvariantCulture), pair.Value);
            }

            foreach (KeyValuePair<Guid, Guid> pair in this.FixupParameters.mSubscriptionIdDic)
            {
                //******************************OnItemDeleted event id
                //******************************OnTaskDelete id
                //******************************OnTaskChange id
                repDictionary.AddEx(pair.Key.ToString().ToUpper(CultureInfo.InvariantCulture), pair.Value);
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
            repDictionary.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember + ".__workflowId", new LS.BinarySerialization.Replacer.LSMemberDataInfo(this.FixupParameters.mInstanceIdDic.GetKey(0).ToString(), this.FixupParameters.mInstanceIdDic.GetValue(0).ToString(), LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, "__workflowId"));
            repDictionary.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember + ".__historylist", new LS.BinarySerialization.Replacer.LSMemberDataInfo(this.FixupParameters.mHistoryListIdDic.GetKey(0).ToString(), this.FixupParameters.mHistoryListIdDic.GetValue(0).ToString(), LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, "__historylist"));
            repDictionary.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember + ".__tasklist", new LS.BinarySerialization.Replacer.LSMemberDataInfo(this.FixupParameters.mTaskListIdDic.GetKey(0).ToString(), this.FixupParameters.mTaskListIdDic.GetValue(0).ToString(), LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, "__tasklist"));
            repDictionary.AddEx(LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember + ".__itemId", new LS.BinarySerialization.Replacer.LSMemberDataInfo(this.FixupParameters.mItemIdDic.GetKey(0), this.FixupParameters.mItemIdDic.GetValue(0), LS.BinarySerialization.Replacer.LSBinarySerReplacer.ProfixOfActivityMember, "__itemId"));

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
                repDictionary.AddEx(pair.Key.ToString().ToUpper(CultureInfo.InvariantCulture), pair.Value);
            }
            foreach (KeyValuePair<int, int> pair in this.FixupParameters.mCustomDic3)
            {
                repDictionary.AddEx(pair.Key.ToString(), pair.Value);
            }
            foreach (KeyValuePair<Guid, Guid> pair in this.ParentAssociationUnit.AllGUIDInTemplate)
            {
                repDictionary.AddEx(pair.Key.ToString().ToUpper(CultureInfo.InvariantCulture), pair.Value);
            }
            if (assemblyMapping != null)
            {
                foreach (KeyValuePair<string, string> pair in assemblyMapping)
                {
                    repDictionary.AddEx(pair.Key.ToLower(CultureInfo.InvariantCulture), pair.Value);
                }
                //assemblyMapping.Clear();
            }
            return repDictionary;
        }


        public static byte[] Save(SPWFInstanceUnit instanceUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWFInstanceUnit.Save");
            if (instanceUnit == null)
                return null;
            SPWFInstanceSerializableData serializableData = instanceUnit.ConvertToData();

            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, serializableData);
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
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SPWFInstanceUnit.Save");
            return data;
        }

        public static SPWFInstanceUnit Load(byte[] serializedMetadata)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWFInstanceUnit.Load");
            byte[] decompressedData = new byte[0];

            #region Decompress serialized Metadata
            MemoryStream tempStream = new MemoryStream(serializedMetadata);
            tempStream.Position = 0L;
            byte[] temp = new byte[4096];
            using (GZipStream gzipStream = new GZipStream(tempStream, CompressionMode.Decompress, true))
            {
                int readLen;
                while ((readLen = gzipStream.Read(temp, 0, 4096)) != 0)
                {
                    LSUtilityOfBytes.LSAppendBytes(ref decompressedData, temp, 0, readLen);
                }
            }
            #endregion

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new WorkflowSerializationBinder();
            MemoryStream stream = new MemoryStream(decompressedData);
            SPWFInstanceSerializableData serializableData = (SPWFInstanceSerializableData)formatter.Deserialize(stream);
            stream.Dispose();
            temp = null;
            decompressedData = null;
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
            GetResourceFolderFiles(ct, unit.SerializableData.ResourceFolderFiles);

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
            SetResourceFolderFiles(ct, this.SerializableData.ResourceFolderFiles);
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
            if (string.IsNullOrEmpty(fileUrl))
            {
                return rlt;
            }
            if (fileUrl.StartsWith("~", StringComparison.OrdinalIgnoreCase))
            {
                return curWeb.Site.RootWeb.ServerRelativeUrl;
            }

            IAveFile tempFile = null;
            try
            {
                tempFile = curWeb.GetFile(fileUrl);
                if (tempFile != null && tempFile.Exists && tempFile.ServerRelativeUrl.StartsWith(curWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
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

        private void GetResourceFolderFiles(IAveContentType ct, List<SPWorkflowSubFileSerializableData> resourceFolderFiles)
        {
            try
            {
                if (ct.ResourceFolderExists && ct.ResourceFolder!=null)
                {
                    foreach (IAveFile file in ct.ResourceFolder.Files)
                    {
                        SPWorkflowSubFileSerializableData fileData = new SPWorkflowSubFileSerializableData();
                        fileData.mName = file.Name;
                        fileData.mContent = file.OpenBinary();
                        fileData.mCreated = file.TimeCreated;
                        fileData.mModified = file.TimeLastModified;
                        resourceFolderFiles.Add(fileData);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get content type resource file error. content type:{0}, exception:{1}", ct.Name, e.ToString());
            }
        }

        private void SetResourceFolderFiles(IAveContentType ct, List<SPWorkflowSubFileSerializableData> resourceFolderFiles)
        {

            foreach (SPWorkflowSubFileSerializableData fileData in resourceFolderFiles)
            {
                try
                {
                    string url = ct.ResourceFolder.Url + "/" + fileData.mName;
                    var file = ct.Web.GetFile(url);
                    if (file == null || (!file.Exists) || file.TimeLastModified != fileData.mModified)
                    {
                        if (fileData.mModified != DateTime.MinValue)
                        {
                            ct.ResourceFolder.Files.Add(url, fileData.mContent, null, ct.Web.Author, ct.Web.Author, fileData.mCreated, fileData.mModified, true);
                        }
                        else
                        {
                            ct.ResourceFolder.Files.Add(url, fileData.mContent, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Set content type resource file error. file:{0}, exception:{1}", fileData.mName, e.ToString());
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
        private bool mNeedReloadParent = true;
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public bool NeedReloadParent
        {
            get { return mNeedReloadParent; }
            set { mNeedReloadParent = value; }
        }

        public IAveListItem ParentItem
        {
            get { return mParentItem; }
            set
            {
                if (value == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceParentItemIsNull);
                try
                {
                    mParentItem = mNeedReloadParent ? ReloadParentItem(value) : value;
                }
                catch (SPWFProcessorException e)
                {
                    //Add for Platform.If source has been deleted,we cannot reload ParentItem.
                    mParentItem = value;
                    logger.Log(AveLogLevel.DEBUG, "An error occurred while setting parent item in workflow instance project, error message: {0}.", e);
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
                    mParentWeb = mNeedReloadParent ? ReloadParentWeb(value) : value;
                }
                catch (SPWFProcessorException e)
                {
                    //Add for Platform.If source has been deleted,we cannot reload ParentItem.
                    mParentWeb = value;
                    logger.Log(AveLogLevel.DEBUG, "An error occurred while setting parent web in workflow instance project, error message: {0}.", e);
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
                    using (IAveWeb w = s.OpenWeb( parentWebId))
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
        public const string PROPS_13MODEL_WebLanguageId = "Props.13Model.WebLanguageId";
        public const string PROPS_ExportedNintex = "Props.ExportedNintex";
        #endregion
        public static string[] BuiltInWorkflowBaseID = new string[] { APPROVAL_BASEID, COLLECT_FEEDBACK_BASEID, COLLECT_SIGNATURE_BASEID, PUBLISHING_APPROVAL_BASEID };

        public const string OriginalUniqueIdFieldName = "OriginalUniqueId";
        //public const string GUIDREG = "[A-F0-9]^8-([A-F0-9]^4-)^3[A-F0-9]^12"; in visual studio find
        private static object lockObj = new object();

        /// <summary>
        /// configuration只load 一次，所以没有问题
        /// </summary>
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

        public static bool ContainsStatusField(string key)
        {
            if(mStatusFieldMapping != null)
            {
                lock(mStatusFieldMapping)
                {
                    return mStatusFieldMapping.ContainsKey(key);
                }
            }

            return false;
        }

        public static string GetStatusFieldValue(string key)
        {
            string keyValue = null;
            if(mStatusFieldMapping != null)
            {
                lock(mStatusFieldMapping)
                {
                    mStatusFieldMapping.TryGetValue(key, out keyValue);
                }
            }

            return keyValue;
        }

        public static void AddStatusFieldValue(string key, string value)
        {
            lock (lockObj)
            {
                if (mStatusFieldMapping == null)
                {
                    mStatusFieldMapping = new Dictionary<string, string>(StringComparer.Ordinal);
                }
                mStatusFieldMapping[key] = value;
            }
        }

        //public static Dictionary<string, string> StatusFieldMapping
        //{
        //    get
        //    {
        //        if (mStatusFieldMapping == null)
        //            mStatusFieldMapping = new Dictionary<string, string>();
        //        return mStatusFieldMapping;
        //    }
        //}

        public static bool StringIsGUIDFormat(string inStr)
        {
            bool result = false;
            int startat = 0;
            if (!string.IsNullOrEmpty(inStr))
            {
                if (inStr.StartsWith("{", StringComparison.Ordinal) && inStr.EndsWith("}", StringComparison.Ordinal))
                    startat = 1;
                Regex guidRE = new Regex(AveRegexCommon.GUIDREG, RegexOptions.IgnoreCase);
                MatchCollection guids = guidRE.Matches(inStr, startat);
                if (guids.Count == 1 && inStr.Length <= 38)
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

    public enum WFCacheType
    {
        CT_WFDATA,
        LIST_WFDATA,
        WEB_WFDATA,
        CUSTOM_WFDATA
    }
    public class WFCache : IEquatable<WFCache>
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public WFCacheType CacheType;
        public string SiteId { get; private set; }
        public string SiteUrl { get; private set; }
        public string WebId { get; private set; }
        public string ListId { get; private set; }
        public string ParentId { get; private set; }
        public List<string> TempFiles = new List<string>();
        public WFCache(string siteId, string siteUrl, string webId, string listId, string parentId, WFCacheType cacheType)
        {
            this.SiteId = siteId;
            this.SiteUrl = siteUrl;
            this.WebId = webId;
            this.ListId = listId;
            this.ParentId = parentId;
            this.CacheType = cacheType;
        }
        public bool Equals(WFCache other)
        {
            if (other.CacheType == this.CacheType
                && string.Equals(other.SiteId, this.SiteId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(other.WebId, this.WebId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(other.ListId, this.ListId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(other.ParentId, this.ParentId, StringComparison.OrdinalIgnoreCase)
                )
            {
                return true;
            }
            return false;
        }
    }
    public class FileCacheService : CacheService
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, string> mServiceParams;
        public static List<WFCache> WfCaches = new List<WFCache>();
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

        public override void CacheData(string siteUrl, string siteId, string webId, string listId, string parentId, int itemId, string index, byte[] data)
        {
            try
            {
                if (!mServiceParams.ContainsKey("RootDirectory"))
                {
                    return;
                }
                var fullFolderPath = mServiceParams["RootDirectory"] + "\\" + Guid.NewGuid().ToString("N");
                logger.Debug("Cache workflow data.SiteUrl:{0},SiteId:{1},WebId:{2},listId:{3},parentId:{4},itemId:{5},index:{6},CacheLocation:{7}", siteUrl, siteId, webId, listId, parentId, itemId, index, fullFolderPath);
                WFCache tempCache = new WFCache(siteId, siteUrl, webId, listId, parentId, GetCacheType(parentId));
                var cache = WfCaches.Find(c => c.Equals(tempCache));
                if(cache == null)
                {
                    cache = tempCache;
                    WfCaches.Add(tempCache);
                }
                if (!Directory.Exists(fullFolderPath))
                {
                    Directory.CreateDirectory(fullFolderPath);
                }

                StringBuilder filename = new StringBuilder();
                filename.Append(fullFolderPath + "\\");
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
                cache.TempFiles.Add(filename.ToString());
                File.WriteAllBytes(filename.ToString(), data);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, "Error in cache workflow data.{0}", e);
            }
        }
        private WFCacheType GetCacheType(string parentId)
        {
            WFCacheType cacheType;
            if (!string.IsNullOrEmpty(parentId))
            {
                if (parentId.StartsWith("0x", StringComparison.Ordinal))
                {
                    cacheType = WFCacheType.CT_WFDATA;
                }
                else
                {
                    if (string.Equals(parentId, "CustomData", StringComparison.OrdinalIgnoreCase))
                    {
                        cacheType = WFCacheType.CUSTOM_WFDATA;
                    }
                    else
                    {
                        cacheType = WFCacheType.LIST_WFDATA;
                    }
                }
            }
            else
            {
                cacheType = WFCacheType.WEB_WFDATA;
            }
            return cacheType;
        }
    }

    public class WebPostponeActionService : PostponeActionService
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, string> mServiceParams;
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

        private IAveSite GetSite(string siteUrl, string siteId, IAveSite site)
        {
            try
            {
                if (site != null)
                {
                    if (site.ID == new Guid(siteId))
                    {
                        return site;
                    }
                    site.Dispose();
                    site = null;
                }
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    site = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite(siteUrl);
                }
                if (site == null)
                {
                    if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                    {
                        site = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite();
                    }
                    else
                    {
                        site = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite(siteId);
                    }
                }
                return site;
            }
            catch(Exception e)
            {
                logger.Error("Failed get site when restore workflow cache data. SiteId: {0}, SiteUrl: {1} Error: {2}", siteId, siteUrl, e);
                return null;
            }
        }

        private IAveWeb GetWeb(string webId, IAveSite site, IAveWeb web)
        {
            try
            {
                if (web != null)
                {
                    if (web.ID == new Guid(webId))
                    {
                        return web;
                    }
                    web.Dispose();
                }
                return site.AllWebs[new Guid(webId)];
            }
            catch(Exception e)
            {
                logger.Error("Failed get web when restore workflow cache data. WebId: {0}, SiteId: {1}. Error: {2}", webId, site.ID, e);
                return null;
            }
        }

        private void DeleteFileAndParentFolder(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    var index = filePath.LastIndexOf('\\');
                    var folderPath = filePath.Substring(0, index);
                    Directory.Delete(folderPath);
                }
            }
            catch(Exception e)
            {
                logger.Warn("An error occurred while deletting a file. Path: {0}, Error: {1}", filePath, e);
            }
        }

        public override void Execute(SPWFAssociationProc associationProcessor, SPWFInstanceProc instanceProcessor)
        {
            string rootDirectory = null;
            if (mServiceParams.ContainsKey("RootDirectory"))
            {
                rootDirectory = mServiceParams["RootDirectory"];
                if (!Directory.Exists(rootDirectory))
                {
                    logger.Info("RootDirectory {0} not exist,skip execute workflow post action.", rootDirectory);
                    return;
                }
            }
            else
            {
                return;
            }
            IAveSite site = null;
            IAveWeb web = null;
            try
            {
                //从低Level到高Level的顺序还原。CT_WFDATA,LIST_WFDATA,WEB_WFDATA,CUSTOM_WFDATA。
                FileCacheService.WfCaches.Sort((x, y) => { return x.CacheType - y.CacheType; });
                foreach (var cache in FileCacheService.WfCaches)
                {
                    site = GetSite(cache.SiteUrl, cache.SiteId, site);
                    web = GetWeb(cache.WebId, site, web);
                    if (site == null || web == null)
                    {
                        continue;
                    }
                    logger.Debug("Begin to restore workflow cache data for web.Title:{0},Url:{1},Web Id:{2},SiteId:{3},", web.Title, web.Url, web.ID, web.Site.ID);
                    #region Restore CustomData
                    if (cache.CacheType == WFCacheType.CUSTOM_WFDATA)
                    {
                        try
                        {
                            foreach (var tempFilePath in cache.TempFiles.ToList())
                            {
                                if (!File.Exists(tempFilePath))
                                {
                                    continue;
                                }
                                SPWorkflowProcessorRuntime.RestoreCustomData(web, File.ReadAllBytes(tempFilePath), true);
                                DeleteFileAndParentFolder(tempFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("An error occurred while restore custom data in post action.Error:{0}", ex);
                        }
                        continue;
                    }
                    #endregion
                    #region Init Information
                    object parentObj = null;
                    try
                    {
                        switch (cache.CacheType)
                        {
                            case WFCacheType.LIST_WFDATA:
                                parentObj = web.Lists[new Guid(cache.ListId)];
                                break;
                            case WFCacheType.CT_WFDATA:
                                parentObj = string.IsNullOrEmpty(cache.ListId) ?
                                    web.ContentTypes.GetById(cache.ParentId)
                                    : web.Lists[new Guid(cache.ListId)].ContentTypes.GetById(cache.ParentId);
                                break;
                            case WFCacheType.WEB_WFDATA:
                                parentObj = web;
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while getting parent info when restore workflow cache data. CacheType: {0}, listId: {1}, parentId: {2}. Error: {3}",
                            cache.CacheType, cache.ListId, cache.ParentId, e);
                        continue;
                    }
                    #endregion
                    #region  restore workflow definition
                    List<SPWFAssociationUnit> allUnit = new List<SPWFAssociationUnit>();
                    foreach (string filePath in cache.TempFiles.ToList())
                    {
                        if (!File.Exists(filePath))
                        {
                            continue;
                        }
                        var temp = filePath.Split(new char[] { '\\' });
                        string relativePath = temp[temp.Length - 1];
                        if (relativePath.StartsWith("_", StringComparison.Ordinal)) continue;

                        string[] temp1 = relativePath.Split(new char[] { '.' });
                        string extName = temp1[temp1.Length - 1];
                        int itemId = int.Parse(temp1[0]);
                        if (itemId == 0)
                        {
                            allUnit.Add(SPWFAssociationUnit.Load(File.ReadAllBytes(filePath)));
                            DeleteFileAndParentFolder(filePath);
                        }
                    }

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
                            logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.RestoreAssocialUnitError, e);
                        }

                    }
                    #endregion
                    #region restore workflow instance
                    foreach (string filePath in cache.TempFiles.ToList())
                    {
                        if (!File.Exists(filePath))
                        {
                            continue;
                        }
                        try
                        {
                            var temp = filePath.Split(new char[] { '\\' });
                            string relativePath = temp[temp.Length - 1];

                            string[] temp1 = relativePath.Split(new char[] { '.' });
                            string extName = temp1[temp1.Length - 1];
                            int itemId = int.Parse(temp1[0].TrimStart('_'));
                            if (itemId > 0 && cache.CacheType != WFCacheType.WEB_WFDATA)
                            {
                                IAveListItem item = null;
                                SPWFAssociationParentType type = SPWFAssociationParentType.Invalid;
                                if (cache.CacheType == WFCacheType.CT_WFDATA)
                                {
                                    type = SPWFAssociationParentType.ListContentType;
                                }
                                else if (cache.CacheType == WFCacheType.LIST_WFDATA)
                                {
                                    type = SPWFAssociationParentType.List;
                                }
                                if (parentObj is IAveList)
                                {
                                    item = ((IAveList)parentObj).GetItemById(itemId);
                                }
                                try
                                {
                                    instanceProcessor.OnRestoreWFInstance(SPWFInstanceUnit.Load(File.ReadAllBytes(filePath)), new RestoreWFInstanceEventArgs() { ParentObject = item, ParentObjectType = type });
                                }
                                finally
                                {
                                    DeleteFileAndParentFolder(filePath);
                                }
                            }
                            else if (relativePath.StartsWith("_", StringComparison.Ordinal))
                            {
                                try
                                {
                                    instanceProcessor.OnRestoreWFInstance(SPWFInstanceUnit.Load(File.ReadAllBytes(filePath)), new RestoreWFInstanceEventArgs { ParentObject = web, ParentObjectType = SPWFAssociationParentType.Web });
                                }
                                finally
                                {
                                    DeleteFileAndParentFolder(filePath);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.RestoreFileError, e);
                        }
                    }
                    #endregion
                }
            }
            catch (FormatException e)
            {
                logger.Log(AveLogLevel.DEBUG, "An format error occurred, error message: {0}", e);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.PostActionError, ex);
            }
            finally
            {
                if (web != null)
                {
                    web.Dispose();
                }
                if (site != null)
                {
                    site.Dispose();
                }
                FileCacheService.WfCaches.Clear();
            }
        }
    }

    internal class NativeLanguageMappingService : LanguageMappingService
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
                                    mListTitleMapping.Add(temp2[0].ToLower(CultureInfo.CurrentCulture), temp2[1]);
                                    break;
                                case LanguageMappingScopeEnum.FieldName:
                                    mFieldNameMapping.Add(temp2[0].ToLower(CultureInfo.CurrentCulture), temp2[1]);
                                    break;
                                case LanguageMappingScopeEnum.Permission:
                                    mPermissionMapping.Add(temp2[0].ToLower(CultureInfo.CurrentCulture), temp2[1]);
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
            if (temp != null && temp.ContainsKey(originalName.ToLower(CultureInfo.CurrentCulture)))
            {
                result = temp[originalName.ToLower(CultureInfo.CurrentCulture)];
            }
            return result;
        }
    }

    public class WorkflowServiceFactory : AvePoint.Common.ISingleton 
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
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

        public bool IsCurrentWorkflowServiceConnected()
        {
            return mAveWorkflowServiceManager != null && mAveWorkflowServiceManager.IsConnected;
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

        [SuppressMessage("FxCopCustomRules", "C100013:CheckExistingExceptionHandlingBlocks", Justification = "Do not need to handle exception")]
        private WorkflowServiceFactory(IAveWeb web)
        {
            if (web != null)
            {
                try
                {
                    mWeb = web;
                    this.mAveWorkflowServiceManager = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowServicesManager(web);
                }
                catch(Exception e)
                {
                    logger.Info("Get Workflow Service Manager for web:{0} failed.Error:{1}",web.Url,e);
                    //如果没有workflow service manager 这里会出异常
                }
            }
        }

        /// <summary>
        /// 只需要在开始还原Workflow Association甚至是SPWeb的时候执行此方法。
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        public bool UpdateWorkflowServiceManager(IAveWeb web)
        {
            return UpdateWorkflowServiceManager(web, false);
        }

        [SuppressMessage("FxCopCustomRules", "C100013:CheckExistingExceptionHandlingBlocks", Justification = "Do not need to handle exception")]
        public bool UpdateWorkflowServiceManager(IAveWeb web, bool forceUpdate)
        {
            if (forceUpdate || mWeb == null || !web.ID.Equals(mWeb.ID) || web != mWeb)
            {
                if (web == null)
                {
                    return false;
                }
                mWeb = web;
                try
                {
                    this.mAveWorkflowServiceManager = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowServicesManager(web);
                }
                catch
                {//如果没有workflow service manager，这里会出异常
                    return false;
                }
                return true;
            }
            return false;
        }
    }

    public class WorkflowSerializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName.StartsWith("AgentCommonWrapperWorkflow", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(WorkflowSerializationBinder).Assembly.GetType(typeName);
            }
            if (assemblyName.StartsWith("mscorlib, Version", StringComparison.OrdinalIgnoreCase))
            {
                return Type.GetType(MappingToDestType(typeName));
            }
            return Type.GetType(typeName);
        }

        private string MappingToDestType(string typeName)
        {
            string pattern = "AgentCommonWrapperWorkflow.*?Version=1.0.0.0, Culture=neutral, PublicKeyToken=fffb45e56dd478e3";
            string sourceAssemblyName = Regex.Match(typeName, pattern).Value;
            if (string.IsNullOrEmpty(sourceAssemblyName)) 
            {
                return typeName;
            }
            return typeName.Replace(sourceAssemblyName, typeof(WorkflowSerializationBinder).Assembly.FullName);
        }
    }
}
