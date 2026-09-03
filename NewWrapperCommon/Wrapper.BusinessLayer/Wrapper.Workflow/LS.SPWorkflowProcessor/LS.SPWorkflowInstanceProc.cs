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
using System.Text;
using System.Xml;
using LS.BinarySerialization.Replacer;
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Common;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.SPWorkflowProcessor
{
    public class Assigns
    {
        public string Type;
        public string Login;
        public string DisplayName;

        public override bool Equals(object obj)
        {
            var other = obj as Assigns;

            if (other != null)
            {
                return string.Equals(other.Login, this.Login, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
    internal class ConflictRecord
    {
        public Guid mSiteId;
        public Guid mWebId;
        public Guid mListId;
        public Guid mParentAssoId;
        public int mItemId;

        public int mConflictStatus;
    }

    public enum WFParentItemType
    {
        ListItem,
        Web,
        None
    }
    public class AveWFParentItem
    {


        private int mItemId = 0;
        public int ItemID
        {
            get
            {
                switch (ParentItemType)
                {
                    case WFParentItemType.ListItem:
                        mItemId = mAveListItem.ID;
                        break;
                    case WFParentItemType.Web:
                        mItemId = -1;
                        break;
                }
                return mItemId;
            }
            set
            {
                mItemId = value;
            }
        }
        private Guid mWebId = Guid.Empty;
        public Guid WebID
        {
            get
            {
                switch (ParentItemType)
                {
                    case WFParentItemType.ListItem:
                        mWebId = mAveListItem.ParentList.ParentWeb.ID;
                        break;
                    case WFParentItemType.Web:
                        mWebId = mAveWeb.ID;
                        break;
                }
                return mWebId;
            }
            set
            {
                mWebId = value;
            }
        }
        private Guid mSiteId = Guid.Empty;
        public Guid SiteID
        {
            get
            {
                switch (ParentItemType)
                {
                    case WFParentItemType.ListItem:
                        mSiteId = mAveListItem.ParentList.ParentWeb.Site.ID;
                        break;
                    case WFParentItemType.Web:
                        mSiteId = mAveWeb.Site.ID;
                        break;
                }
                return mSiteId;
            }
            set
            {
                mSiteId = value;
            }
        }

        private Guid mListId = Guid.Empty;
        public Guid ListID
        {
            get
            {
                switch (ParentItemType)
                {
                    case WFParentItemType.ListItem:
                        mListId = mAveListItem.ParentList.ID;
                        break;
                    case WFParentItemType.Web:
                        mListId = Guid.Empty;
                        break;
                }
                return mListId;
            }
            set
            {
                mListId = value;
            }
        }
        private IAveWorkflowManager mWFManager = null;
        public IAveWorkflowManager WFManager
        {
            get
            {
                switch (ParentItemType)
                {
                    case WFParentItemType.ListItem:
                        mWFManager = mAveListItem.ParentList.ParentWeb.Site.WorkflowManager;
                        break;
                    case WFParentItemType.Web:
                        mWFManager = mAveWeb.Site.WorkflowManager;
                        break;
                }
                return mWFManager;
            }
            set
            {
                mWFManager = value;
            }
        }
        private WFParentItemType mParentItemType = WFParentItemType.None;
        public WFParentItemType ParentItemType
        {
            get
            {
                return mParentItemType;
            }
            set
            {
                mParentItemType = value;
            }

        }

        private IAveWeb mAveWeb = null;
        public IAveWeb Web
        {
            get
            {
                switch (ParentItemType)
                {
                    case WFParentItemType.ListItem:
                        return mAveListItem.Web;
                    case WFParentItemType.Web:
                        return mAveWeb;
                    default:
                        return null;
                }

            }
            set
            {
                mAveWeb = value;
            }
        }

        public object ParentItem
        {
            get
            {
                switch (ParentItemType)
                {
                    case WFParentItemType.ListItem:
                        return mAveListItem;
                    case WFParentItemType.Web:
                        return mAveWeb;
                    default:
                        return null;
                }
            }
        }

        private IAveListItem mAveListItem = null;
        public IAveListItem ListItem
        {
            get
            {
                switch (ParentItemType)
                {
                    case WFParentItemType.ListItem:
                        return mAveListItem;
                    case WFParentItemType.Web:
                        return null;
                    default:
                        return null;
                }
            }
            set
            {
                mAveListItem = value;
            }

        }

    }

    public class SPWFInstanceProc : IDisposable
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected const int ConflictCondition = (int)(AveWorkflowState.Expiring |
                AveWorkflowState.Faulting |
                AveWorkflowState.Locked |
                AveWorkflowState.Running |
                AveWorkflowState.Suspended);


        public event RestoreWFInstanceEventHandler RestoreWFInstanceEvent;
        public void OnRestoreWFInstance(object sender, RestoreWFInstanceEventArgs e)
        {
            RestoreWFInstanceEvent(sender, e);
        }

        protected List<Guid> mWorkflowIds;
        protected List<Guid> mTempBaseIds;
        protected AveWFParentItem mParentItem;
        protected IAveWeb mParentWeb;
        protected SPWFProcessorType mProcType;
        protected SqlConnection mConn;
        protected SqlCommand mCmd;
        protected IAveBackupRestoreQueryService mQueryService;
        protected SPFieldProcessor mWebLevelFieldProcessor;
        protected SPWFAssociationProc mAssociationProc;
        protected bool mIsOverwriteRestore;
        protected bool mIsAppendRestore;

        protected IAveWorkflowManager mWFManager;
        protected IAveWorkflow mInstance;
        protected IAveWorkflowInstance mInstance13Model;
        protected LSPerformanceMonitor mPerformanceMonitor;
        protected string mMainMonitorLog = string.Empty;
        private List<ConflictRecord> mConflictRecords;


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

        protected Dictionary<Guid, Guid> mTaskItemGuidMapping;
        protected Dictionary<Guid, Guid> TaskItemGuidMapping
        {
            get { return mTaskItemGuidMapping; }
        }

        protected Dictionary<Guid, List<Guid>> mTaskListIdAndInstanceMapping;
        public Dictionary<Guid, List<Guid>> TaskListIdAndInstanceMapping
        {
            get { return mTaskListIdAndInstanceMapping; }
        }

        protected Dictionary<Guid, List<Guid>> mHistoryListIdAndInstanceMapping;
        public Dictionary<Guid, List<Guid>> HistoryListIdAndInstanceMapping
        {
            get { return mHistoryListIdAndInstanceMapping; }
        }

        public SqlConnection SQLConnection
        {
            get { return mConn; }
            set
            {
                mConn = value;
                if (this.mCmd != null)
                {
                    this.mCmd.Dispose();
                    this.mCmd = null;
                }
                this.mCmd = new SqlCommand();
                this.mCmd.Connection = this.mConn;
            }
        }

        public AveWFParentItem ParentItem
        {
            get { return mParentItem; }
            set { mParentItem = value; }
        }

        protected Guid TryGetMappingBaseId(Guid baseId)
        {
            Guid mappingBaseId = Guid.Empty;
            try
            {
                if (!SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.TryGetWorkflowBaseId(baseId, out mappingBaseId))
                {
                    mappingBaseId = baseId;
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while get mapping base id, error: {0}", e);
            }
            return mappingBaseId == Guid.Empty ? baseId : mappingBaseId;
        }
        public IAveWeb ParentWeb
        {
            get { return mParentWeb; }
            set { mParentWeb = value; }
        }

        public SPWFInstanceProc()
        {
            mExceptions = new Dictionary<Guid, List<SPWFProcessorException>>();
            mInnerWarnings = new List<SPWFProcessorException>();
            mPerformanceMonitor = new LSPerformanceMonitor();

            mTaskItemGuidMapping = new Dictionary<Guid, Guid>();
            mTaskListIdAndInstanceMapping = new Dictionary<Guid, List<Guid>>();
            mHistoryListIdAndInstanceMapping = new Dictionary<Guid, List<Guid>>();
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Guid, List<SPWFProcessorException>> pair in mExceptions)
            {
                pair.Value.Clear();
            }
            mExceptions.Clear();
            mInnerExceptions = null;

            mInnerWarnings.Clear();
            mInnerWarnings = null;

            mTaskItemGuidMapping.Clear();

            if (mPerformanceMonitor != null)
                mPerformanceMonitor.Dispose();

            //CustomProcessors.Clear();
        }

        /// <summary>
        /// Create a custom workflow instance processor
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="procType"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static SPWFInstanceProc CreateBackupInstance(InstanceProcCreationParam param)
        {
            SPWFInstanceProc proc = null;
            switch (param.ProcType)
            {
                case SPWFProcessorType.API:
                    proc = new SPWFInstanceProcAPI();
                    break;
                case SPWFProcessorType.Native:
                default:
                    proc = new SPWFInstanceProcNative();
                    break;
            }

            proc.mParentItem.ListItem = param.ParentItem;
            proc.mProcType = param.ProcType;
            proc.mQueryService = param.QueryService;
            //proc.mConn = param.Conn;
            //proc.mCmd = new SqlCommand();
            //proc.mCmd.Connection = proc.mConn;
            proc.mWebLevelFieldProcessor = param.WebLevelFieldProcessor;
            return proc;
        }

        public static SPWFInstanceProc CreateRestoreInstance(InstanceProcCreationParam param)
        {
            SPWFInstanceProc proc = null;
            switch (param.ProcType)
            {
                case SPWFProcessorType.API:
                    proc = new SPWFInstanceProcAPI();
                    break;
                case SPWFProcessorType.Native:
                default:
                    proc = new SPWFInstanceProcNative();
                    break;
            }

            proc.mParentItem.ListItem = param.ParentItem;
            proc.mProcType = param.ProcType;
            proc.mQueryService = param.QueryService;
            //proc.mCmd = new SqlCommand();
            //proc.mCmd.Connection = proc.mConn;
            proc.mIsOverwriteRestore = param.Overwrite;
            proc.mIsAppendRestore = param.Append;
            proc.mAssociationProc = param.AssociationProc;
            return proc;
        }

        public static SPWFInstanceProc CreateInstance(SPWFProcessorType procType)
        {
            SPWFInstanceProc proc = null;
            switch (procType)
            {
                case SPWFProcessorType.API:
                    proc = new SPWFInstanceProcAPI();
                    break;
                case SPWFProcessorType.Native13Model:
                    proc = new SPWFInstanceProcNative13Model();
                    break;
                case SPWFProcessorType.Native:
                default:
                    proc = new SPWFInstanceProcNative();
                    break;
            }
            return proc;
        }

        public virtual List<byte[]> Backup()
        {
            return new List<byte[]>();
        }

        public virtual List<byte[]> BackupSchedules()
        {
            return new List<byte[]>();
        }

        public virtual int RestoreSchedule(SPWFInstanceUnit unit)
        {
            return 0;
        }

        public virtual int Restore(SPWFInstanceUnit unit)
        {
            return 0;
        }

        public virtual int Restore(byte[] serializedData)
        {
            return 0;
        }

        public virtual void SetInstanceProcParameters(InstanceProcCreationParam param)
        {
            this.mParentItem.ListItem = param.ParentItem;
            this.mParentItem.Web = param.ParentWeb;
            this.mProcType = param.ProcType;
            this.mQueryService = param.QueryService;
            //this.mCmd = new SqlCommand();
            //this.mCmd.Connection = this.mConn;
            this.mIsOverwriteRestore = param.Overwrite;
            this.mIsAppendRestore = param.Append;
            this.mAssociationProc = param.AssociationProc;
            this.mWebLevelFieldProcessor = param.WebLevelFieldProcessor;
            this.mCustomProcs = param.CustomProcessors;
        }

        public void SetWorkflowIds(List<Guid> ids)
        {
            this.mWorkflowIds = ids;
        }

        public void SetTempBaseIds(List<Guid> ids)
        {
            this.mTempBaseIds = ids;
        }

        public void SetCustomProc(List<ICustomWorkflowInstanceProc> customProcessors)
        {
            CustomProcessors = customProcessors;
        }

        /// <summary>
        /// Survey类型list下的response的item没有GUID这个field，所以需要用query service查出来，其他类型的list下的item还是使用API处理
        /// </summary>
        /// <param name="item"></param>
        /// <returns>list item tp_Guid</returns>
        protected Guid GetListItemGuid(AveWFParentItem item)
        {
            Guid tp_Guid = Guid.Empty;
            if (item != null && item.ListItem != null && item.ListItem.ParentList != null && item.ListItem.ParentList.BaseType == AveBaseType.Survey)
            {
                tp_Guid = mQueryService.GetListItemGuid(item.ListID, item.ListItem.ID);
            }
            if (tp_Guid == Guid.Empty)
            {
                tp_Guid = new Guid((string)mParentItem.ListItem["GUID"]);
            }
            return tp_Guid;
        }

        internal ConflictRecord GetConflictRecord(Guid siteId, Guid webId, Guid listId, Guid parentAssoId, int itemId)
        {
            foreach (ConflictRecord record in mConflictRecords)
            {
                if (record.mSiteId == siteId &&
                    record.mWebId == webId &&
                    record.mListId == listId &&
                    record.mParentAssoId == parentAssoId &&
                    record.mItemId == itemId)
                {
                    return record;
                }
            }
            return null;
        }


        //private Dictionary<Guid, Dictionary<string, Guid>> mStatusFields;//Dictionary<listid,Dictionary<status field internal name,association id>>
        private Dictionary<Guid, Dictionary<string, List<Guid>>> mStatusFields;
        protected bool IsAvailableStatusFieldName(IAveList parentList, Guid parentAssociationBaseId, IAveField statusField, string sourceFieldDisplayName)
        {
            var statusFieldInternalName = statusField.InternalName.ToLower(CultureInfo.CurrentCulture);
            if (mStatusFields == null)
            {
                //mStatusFields = new Dictionary<Guid, Dictionary<string, Guid>>();
                mStatusFields = new Dictionary<Guid, Dictionary<string, List<Guid>>>();
            }

            //Dictionary<string, Guid> temp = null;
            Dictionary<string, List<Guid>> temp = null;
            try
            {
                if (!mStatusFields.ContainsKey(parentList.ID))//not same parent list
                {
                    mStatusFields.Clear();
                    //temp = new Dictionary<string, Guid>();
                    temp = new Dictionary<string, List<Guid>>();
                    mStatusFields.Add(parentList.ID, temp);

                    foreach (IAveWorkflowAssociation asso in parentList.WorkflowAssociations)
                    {
                        string status = (string)LSInvoker.GetProperty(asso, "InternalNameStatusField");
                        if (!string.IsNullOrEmpty(status))
                        {
                            if (!temp.ContainsKey(status.ToLower(CultureInfo.CurrentCulture)))
                            {
                                //temp.Add(status.ToLower(), asso.Id);
                                temp.Add(status.ToLower(CultureInfo.CurrentCulture), new List<Guid>());
                            }
                            if (!temp[status.ToLower(CultureInfo.CurrentCulture)].Contains(asso.BaseId))
                            {
                                temp[status.ToLower(CultureInfo.CurrentCulture)].Add(asso.BaseId);
                            }
                        }
                    }

                    foreach (IAveContentType ct in parentList.ContentTypes)
                    {
                        foreach (IAveWorkflowAssociation asso in ct.WorkflowAssociations)
                        {
                            string status = (string)LSInvoker.GetProperty(asso, "InternalNameStatusField");
                            if (!string.IsNullOrEmpty(status))
                            {
                                if (!temp.ContainsKey(status.ToLower(CultureInfo.CurrentCulture)))
                                {
                                    temp.Add(status.ToLower(CultureInfo.CurrentCulture), new List<Guid>());
                                }
                                if (!temp[status.ToLower(CultureInfo.CurrentCulture)].Contains(asso.BaseId))
                                {
                                    temp[status.ToLower(CultureInfo.CurrentCulture)].Add(asso.BaseId);
                                }
                            }
                        }
                    }
                }
                else
                {
                    temp = mStatusFields[parentList.ID];
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.StatusFieldNameIsAvailble, ex);
            }

            if (temp.ContainsKey(statusFieldInternalName))
            {
                //原端目的端都有reusable 会有问题
                if (temp[statusFieldInternalName].Count == 1 && temp[statusFieldInternalName][0] == parentAssociationBaseId
                    && string.Equals(statusField.Title, sourceFieldDisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                //if(temp[statusFieldInternalName].Contains(parentAssociationId))
                //    return true;
                //else
                //    return false;

                //if (temp[statusFieldInternalName] == parentAssociationId)
                //    return true;
                //else
                //    return false;
            }
            else
                return true;
        }

        protected void AddStatusFieldNameMapping(IAveList parentList, Guid parentAssociationBaseId, string statusFieldInternalName)
        {
            statusFieldInternalName = statusFieldInternalName.ToLower(CultureInfo.CurrentCulture);
            if (mStatusFields == null)
            {
                //mStatusFields = new Dictionary<Guid, Dictionary<string, Guid>>();
                mStatusFields = new Dictionary<Guid, Dictionary<string, List<Guid>>>();
            }

            Dictionary<string, List<Guid>> temp = null;

            if (!mStatusFields.ContainsKey(parentList.ID))//not same parent list
            {
                mStatusFields.Clear();
                //temp = new Dictionary<string, Guid>();
                //mStatusFields.Add(parentList.ID, temp);
                temp = new Dictionary<string, List<Guid>>();
                mStatusFields.Add(parentList.ID, temp);
            }
            else
            {
                temp = mStatusFields[parentList.ID];
            }


            if (!temp.ContainsKey(statusFieldInternalName))
            {
                //temp.Add(statusFieldInternalName, parentAssociationId);
                temp.Add(statusFieldInternalName, new List<Guid>());
                temp[statusFieldInternalName].Add(parentAssociationBaseId);
            }
            else if (!temp[statusFieldInternalName].Contains(parentAssociationBaseId))
            {
                temp[statusFieldInternalName].Add(parentAssociationBaseId);
                //throw new Exception("Status field mapping already exists."+statusFieldInternalName+"::"+parentAssociationId);
            }
        }

        internal class WFInstanceInforStatistics
        {
            public string Name { get; set; }
            public int TaskCount { get; set; }
            public int SubscriptionCount { get; set; }
            public int ScheduledWorkItemCount { get; set; }
            public int HistoryCount { get; set; }

            public WFInstanceInforStatistics(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                try
                {
                    builder.AppendFormat("Name:{0} ,TaskCount:{1},SubscriptionCount:{2},ScheduledWorkItemCount:{3},HistoryCount:{4}", Name, TaskCount, SubscriptionCount, ScheduledWorkItemCount, HistoryCount);
                }
                catch (Exception e)
                {
                    builder.AppendFormat("An error occurred while get statistics for a workflow instance.Error:{0}", e.ToString());
                }
                return builder.ToString();
            }
        }
    }



    internal sealed class SPWFInstanceProcAPI : SPWFInstanceProc
    { }

    internal sealed class SPWFInstanceProcNative : SPWFInstanceProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string CustomFieldProfix = "LS.";
        internal string CustomFieldProfixProp
        {
            get
            {
                return CustomFieldProfix;
            }
        }

        private List<string> mExcludeFieldList;

        private SPWFInstanceUnit mInstanceUnit;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Fields of Table Workflow")]
        public SPWFInstanceProcNative()
        {
            mExcludeFieldList = new List<string>();
            mExcludeFieldList.Add("#tp_id");
            mExcludeFieldList.Add("#tp_listid");
            mExcludeFieldList.Add("#tp_siteid");
            mExcludeFieldList.Add("#tp_rowordinal");
            mExcludeFieldList.Add("#tp_version");
            mExcludeFieldList.Add("#tp_ordering");
            mExcludeFieldList.Add("#tp_threadindex");
            mExcludeFieldList.Add("#tp_moderationstatus");
            mExcludeFieldList.Add("#tp_iscurrent");
            mExcludeFieldList.Add("#tp_itemorder");
            mExcludeFieldList.Add("#tp_instanceid");
            mExcludeFieldList.Add("#tp_guid");
            mExcludeFieldList.Add("#tp_dirname");
            mExcludeFieldList.Add("#tp_leafname");
            //mExcludeFieldList.Add("#uniqueidentifier1");
            mExcludeFieldList.Add("#tp_level");
            mExcludeFieldList.Add("#tp_iscurrentversion");
            //mExcludeFieldList.Add("#tp_uiversion");
            mExcludeFieldList.Add("#tp_calculatedversion");
            mExcludeFieldList.Add("#tp_uiversionstring");
            mExcludeFieldList.Add("#tp_parentid");
            mExcludeFieldList.Add("#tp_docid");
        }

        private void GetHistoryColNames(string schema, out string instanceIdCol, out string associationIdCol, out string parentListIdCol, out string baseIdCol, out string itemIdCol)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetHistoryColNames");
            instanceIdCol = string.Empty;
            associationIdCol = string.Empty;
            parentListIdCol = string.Empty;
            baseIdCol = string.Empty;
            itemIdCol = string.Empty;

            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.LoadXml(schema);

                XmlNode instanceNode = doc.SelectSingleNode("/Fields/Field[@StaticName='WorkflowInstance'][@Type='Text']");
                instanceIdCol = instanceNode.Attributes["ColName"].Value;

                XmlNode assoNode = doc.SelectSingleNode("/Fields/Field[@StaticName='WorkflowAssociation'][@Type='Text']");
                associationIdCol = assoNode.Attributes["ColName"].Value;

                XmlNode baseIdNode = doc.SelectSingleNode("/Fields/Field[@StaticName='WorkflowTemplate'][@Type='Text']");
                baseIdCol = baseIdNode.Attributes["ColName"].Value;

                XmlNode listIdNode = doc.SelectSingleNode("/Fields/Field[@StaticName='List'][@Type='Text']");
                parentListIdCol = listIdNode.Attributes["ColName"].Value;

                XmlNode itemIdNode = doc.SelectSingleNode("/Fields/Field[@StaticName='Item'][@Type='Integer']");
                itemIdCol = itemIdNode.Attributes["ColName"].Value;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_GetHistoryFieldException, e.Message);
                logger.Warn("An exception occurred while get history field. exception:{0}", e.ToString());
                throw new SPWFProcessorException(SPWFProcessorErrorCode.GetHistoryFieldsError, e);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetHistoryColNames");
            }

        }


        #region ************************Backup  Region************************

        public void FixupUserLoginName(SPWorkflowSubItemUnit subUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "FixupUserLoginName");
            using (IAveWeb web = mParentItem.Web)
            {
                if (subUnit.ItemType == WorkflowSubItemType.Task && subUnit.Properties.ContainsKey("_0_AssignedTo"))
                {
                    subUnit.Properties["_0_AssignedTo"] = SPPermissionProcessor.GetUserOrGroupLoginNameFromId(web, (int)subUnit.Properties["_0_AssignedTo"]);
                }
                else if (subUnit.ItemType == WorkflowSubItemType.History && subUnit.Properties.ContainsKey("_0_User"))
                {
                    subUnit.Properties["_0_User"] = SPPermissionProcessor.GetUserOrGroupLoginNameFromId(web, (int)subUnit.Properties["_0_User"]);
                }
                else if (subUnit.ItemType == WorkflowSubItemType.Instance && subUnit.Properties.ContainsKey("#Author"))
                {
                    subUnit.Properties["#Author"] = SPPermissionProcessor.GetUserOrGroupLoginNameFromId(web, (int)subUnit.Properties["#Author"]);
                    return;
                }
                if (subUnit.Properties.ContainsKey("_0_Author"))
                {
                    subUnit.Properties["_0_Author"] = SPPermissionProcessor.GetUserOrGroupLoginNameFromId(web, (int)subUnit.Properties["_0_Author"]);
                }

                if (subUnit.Properties.ContainsKey("_0_Editor"))
                {
                    subUnit.Properties["_0_Editor"] = SPPermissionProcessor.GetUserOrGroupLoginNameFromId(web, (int)subUnit.Properties["_0_Editor"]);
                }
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FixupUserLoginName");
        }

        public override List<byte[]> Backup()
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.InstanceMainBackup"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "InstanceMainBackup");

                string monitor = mMainMonitorLog = "Instance Backup";

                List<byte[]> rlt = new List<byte[]>();
                try
                {

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    mPerformanceMonitor.StartMonitor(monitor);
                    #endregion
                    if (mWorkflowIds == null)
                    {
                        mWorkflowIds = new List<Guid>();
                        logger.Log(AveLogLevel.DEBUG, "Begin to backup workflow ids by native for 10 model");
                        mQueryService.GetWorkflowId(mWorkflowIds, mParentItem.SiteID, mParentItem.WebID, mParentItem.ItemID, mParentItem.ListID == Guid.Empty ? mParentItem.WebID : mParentItem.ListID);
                    }
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_GetItemInstanceException, e.Message);
                    logger.Warn("An exception occurred while get item instance. exception:{0}", e.ToString());
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceMainBackup");
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.GetWorkflowInstanceError, e);
                }
                #region Performance Monitor Region
                mPerformanceMonitor.StopMonitor(monitor);
                mPerformanceMonitor.WriteMonitorLog(monitor, " Get Instance Count: ", mWorkflowIds.Count, ". Duration: ", mPerformanceMonitor.GetDuration(monitor));
                #endregion

                SPWorkflowProcessorRuntime.Log(Logs.IP_InstanceCount, mWorkflowIds.Count.ToString());
                foreach (Guid instanceId in mWorkflowIds)
                {
                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + instanceId);
                    mPerformanceMonitor.StartMonitor(monitor);
                    #endregion
                    logger.Log(AveLogLevel.DEBUG, "Begin to assemble workflow instance unit for 10 model one by one, instance id: {0}", instanceId.ToString());
                    WFInstanceInforStatistics statistic = new WFInstanceInforStatistics(instanceId.ToString());
                    using (AvePerformanceScope pf1 = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance"))
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupOneInstance");
                        mInnerExceptions = new List<SPWFProcessorException>();
                        try
                        {
                            SPWorkflowSubItemUnit instanceItemUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Instance);
                            instanceItemUnit.Properties.AddEx("#Id", instanceId);
                            instanceItemUnit.Properties.AddEx("#SiteId", mParentItem.SiteID);
                            BackupInstanceSelf(instanceItemUnit);
                            statistic.ScheduledWorkItemCount = BackupScheduledWorkItem(instanceItemUnit);
                            statistic.TaskCount = BackupTasks(instanceItemUnit);
                            statistic.SubscriptionCount = BackupSubscription(instanceItemUnit);
                            statistic.HistoryCount = BackupHistory(instanceItemUnit);
                            //BackupCustomUnit(instanceItemUnit);

                            #region Performance Monitor Region
                            mPerformanceMonitor.ResetCurrentDuration(monitor);
                            #endregion

                            SPWFInstanceUnit instanceUnit = new SPWFInstanceUnit();
                            instanceUnit.InstanceItem = instanceItemUnit;
                            byte[] data = SPWFInstanceUnit.Save(instanceUnit);
                            instanceItemUnit.Dispose();

                            #region Performance Monitor Region
                            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Serialize Instance Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            #endregion

                            rlt.Add(data);
                        }
                        catch (SPWFProcessorException procException)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceException, instanceId.ToString(), procException.Message);
                            logger.Warn("An exception occurred while backup instance. exception:{0}", procException.ToString());
                            mInnerExceptions.Add(procException);
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceException, instanceId.ToString(), e.Message);
                            logger.Warn("An exception occurred while backup instance. exception:{0}", e.ToString());
                            mInnerExceptions.Add(new SPWFProcessorException(SPWFProcessorErrorCode.InstanceUnknownError, e, instanceId));
                        }
                        finally
                        {
                            logger.Log(AveLogLevel.DEBUG, "Finish assemble workflow instance unit for 10 model one by one, instance id: {0}", statistic.ToString());
                            #region Performance Monitor Region
                            mPerformanceMonitor.StopMonitor(monitor);
                            mPerformanceMonitor.WriteMonitorLog(monitor, " finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                            mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                            #endregion
                            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupOneInstance");
                        }
                    }

                    if (mInnerExceptions.Count > 0)
                        mExceptions.Add(instanceId, mInnerExceptions);
                }

                #region Performance Monitor Region
                mPerformanceMonitor.RemoveMonitor(monitor);
                #endregion

                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceMainBackup");
                return rlt;
            }
        }

        public override List<byte[]> BackupSchedules()
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowSchedule.ScheduleMainBackup"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "ScheduleMainBackup");

                string monitor = mMainMonitorLog = "Schedule Backup";

                List<byte[]> rlt = new List<byte[]>();
                try
                {

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    mPerformanceMonitor.StartMonitor(monitor);
                    #endregion
                    if (mTempBaseIds == null)
                    {
                        mTempBaseIds = new List<Guid>();
                        mQueryService.GetWorkflowAssociationId(mTempBaseIds, mParentItem.SiteID, mParentItem.WebID, mParentItem.ListID == Guid.Empty ? mParentItem.WebID : mParentItem.ListID);
                    }
                    //if (mParentItem.ParentItemType == WFParentItemType.ListItem)
                    //{
                    //    if (mParentItem.ListItem.ParentList != null && mParentItem.ListItem.ParentList.WorkflowAssociations != null)
                    //    {
                    //        foreach (IAveWorkflowAssociation spWFAss in mParentItem.ListItem.ParentList.WorkflowAssociations)
                    //        {
                    //            if (spWFAss.BaseId != Guid.Empty)
                    //            {
                    //                tempBaseIds.Add(spWFAss.BaseId);
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (mParentItem.Web != null && mParentItem.Web.WorkflowAssociations != null)
                    //    {
                    //        foreach (IAveWorkflowAssociation spWFAss in mParentItem.Web.WorkflowAssociations)
                    //        {
                    //            if (spWFAss.BaseId != Guid.Empty)
                    //            {
                    //                tempBaseIds.Add(spWFAss.BaseId);
                    //            }
                    //        }
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, "An error occurred while backing up schedules, error message: {0}", e);
                    e.ToString();
                    //SPWorkflowProcessorRuntime.Log(Logs.IP_GetItemScheduelBaseIDException, e.Message);
                    //SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "ScheduleMainBackup");
                    //throw new SPWFProcessorException(SPWFProcessorErrorCode.GetWorkflowBaseIdError, e);
                }
                #region Performance Monitor Region
                mPerformanceMonitor.StopMonitor(monitor);
                mPerformanceMonitor.WriteMonitorLog(monitor, " Get BaseId Count: ", mTempBaseIds.Count, ". Duration: ", mPerformanceMonitor.GetDuration(monitor));
                #endregion

                //SPWorkflowProcessorRuntime.Log(Logs.IP_BaseIdCount, tempBaseIds.Count.ToString());
                foreach (Guid baseId in mTempBaseIds)
                {
                    using (AvePerformanceScope pf1 = new AvePerformanceScope("BackupWorkflowSchedule.BackupOneSchedule"))
                    {
                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                        mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + baseId);
                        mPerformanceMonitor.StartMonitor(monitor);
                        #endregion

                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupOneSchedule");
                        mInnerExceptions = new List<SPWFProcessorException>();
                        try
                        {
                            SPWorkflowSubItemUnit instanceItemUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Schedule);
                            instanceItemUnit.Properties.AddEx("#SiteId", mParentItem.Web.Site.ID);
                            instanceItemUnit.Properties.AddEx("#WebID", mParentItem.Web.ID);
                            instanceItemUnit.Properties.AddEx("#ListID", mParentItem.ListID);
                            instanceItemUnit.Properties.AddEx("#ItemID", mParentItem.ItemID);
                            instanceItemUnit.Properties.AddEx("#WorkflowID", baseId);
                            BackupCustomUnit(instanceItemUnit);

                            #region Performance Monitor Region
                            mPerformanceMonitor.ResetCurrentDuration(monitor);
                            #endregion

                            if (instanceItemUnit.ChildUnits.Count > 0)
                            {
                                SPWFInstanceUnit instanceUnit = new SPWFInstanceUnit();
                                instanceUnit.InstanceItem = instanceItemUnit;
                                byte[] data = SPWFInstanceUnit.Save(instanceUnit);
                                instanceItemUnit.Dispose();

                                #region Performance Monitor Region
                                mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Serialize Schedule Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                                #endregion

                                rlt.Add(data);
                            }
                        }
                        catch (SPWFProcessorException procException)
                        {
                            //SPWorkflowProcessorRuntime.Log(Logs.IP_BackupScheduleException, baseId.ToString(), procException.Message);
                            logger.Log(AveLogLevel.DEBUG, "An processor error occurred while backing up schedules, error message: {0}", procException);
                            mInnerExceptions.Add(procException);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, "An error occurred while backing up schedules, error message: {0}", e);
                            e.ToString();
                            //SPWorkflowProcessorRuntime.Log(Logs.IP_BackupScheduleException, baseId.ToString(), e.Message);
                            //mInnerExceptions.Add(new SPWFProcessorException(SPWFProcessorErrorCode.ScheduleUnknownError, e, baseId));
                        }
                        finally
                        {
                            #region Performance Monitor Region
                            mPerformanceMonitor.StopMonitor(monitor);
                            mPerformanceMonitor.WriteMonitorLog(monitor, " finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                            mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                            #endregion
                            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupOneSchedule");
                        }
                    }

                    if (mInnerExceptions.Count > 0)
                        mExceptions.Add(baseId, mInnerExceptions);
                }

                #region Performance Monitor Region
                mPerformanceMonitor.RemoveMonitor(monitor);
                #endregion

                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "ScheduleMainBackup");
                return rlt;
            }
        }

        public override int RestoreSchedule(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowSchedule.ScheduleMainRestore"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "ScheduleRestore");
                string monitor = mMainMonitorLog = "Schedule Restore";

                try
                {
                    mInstanceUnit = unit;

                    SPWorkflowSubItemUnit instanceItemUnit = unit.InstanceItem;

                    InitializeScheduleWFFixupParams(unit);
                    RestoreWorkflowScheduleSubItem(unit);
                    return 0;
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceException, unit.InstanceItem.Properties.GetEx("#Id").ToString(), procException.Message);
                    logger.Log(AveLogLevel.DEBUG, "An error occurred while restoring workflow schedule, error message: {0}", procException);
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceException, unit.InstanceItem.Properties.GetEx("#Id").ToString(), e.Message);
                    logger.Warn("An exception occurred while restore workflow instance. exception:{0}", e.ToString());
                    return 3;
                }
                finally
                {
                    #region Performance Monitor Region
                    mPerformanceMonitor.StopMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    #endregion

                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceRestore");
                }
            }
        }

        private void InitializeScheduleWFFixupParams(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowSchedule.ScheduleMainRestore.InitializeScheduleWFFixupParams"))
            {
                try
                {
                    SPWorkflowSubItemUnit instanceItem = unit.InstanceItem;
                    unit.FixupParameters.Dispose();
                    unit.FixupParameters.mListIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#ListId"), mParentItem.ListID);
                    unit.FixupParameters.mWebIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#WebId"), mParentItem.Web.ID);
                    unit.FixupParameters.mSiteIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#SiteId"), mParentItem.Web.Site.ID);
                    unit.FixupParameters.mItemIdDic.AddEx((int)instanceItem.Properties.GetEx("#ItemId"), mParentItem.ItemID);
                    unit.FixupParameters.mParentAssociationBaseIdDic.AddEx(Guid.Empty, TryGetMappingBaseId((Guid)instanceItem.Properties.GetEx("#WorkflowID")));
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InitializeFixupParamError, e);
                }
            }
        }

        private int RestoreWorkflowScheduleSubItem(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowSchedule.ScheduleMainRestore.RestoreWorkflowScheduleSubItem"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreWorkflowSubItem");
                try
                {
                    RestoreCustomSubItem(unit, unit.InstanceItem);
                    return 0;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreWorkflowSubItem");
                }
            }
        }

        private void BackupInstanceSelf(SPWorkflowSubItemUnit instanceUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupInstanceSelf"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupInstanceSelf");
                string monitor = "Backup Instance";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    using (IAveQueryDataReader sdr = mQueryService.BackupInstance((Guid)instanceUnit.Properties["#Id"]))
                    {
                        if (sdr.Read())
                        {
                            instanceUnit.SetPropsFromDataReader(sdr, 0, null, 0);
                            FixupUserLoginName(instanceUnit);
                        }
                    }
                    mQueryService.BackupInstanceSelf((Guid)instanceUnit.Properties["#SiteId"], (Guid)instanceUnit.Properties["#WebId"], (Guid)instanceUnit.Properties["#TemplateId"], instanceUnit.Properties, CustomFieldProfix);

                    BackupCustomUnit(instanceUnit);
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceSelfException, e.Message);
                    logger.Warn("An exception occurred while backup instance self. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupSelfError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupInstanceSelf");
                }
            }
        }

        internal int BackupScheduledWorkItem(SPWorkflowSubItemUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupScheduledWorkItem"))
            {
                int result = 0;
                try
                {
                    Guid siteId = (Guid)parentUnit.Properties["#SiteId"];
                    Guid instanceId = (Guid)parentUnit.Properties["#Id"];
                    using (IAveQueryDataReader sdr = mQueryService.BackupScheduledWorkItem(siteId, instanceId))
                    {
                        if (sdr.Read())
                        {
                            SPWorkflowSubItemUnit scheduleWorkItemUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.ScheduledWorkItem, parentUnit);
                            scheduleWorkItemUnit.SetPropsFromDataReader(sdr, 0, null, 0);
                            parentUnit.ChildUnits.Add(scheduleWorkItemUnit);
                            result++;
                        }
                    }
                }
                catch (SPWFProcessorException ex)
                {
                    logger.Warn("Backup instance scheduled work item error: {0}", ex.Message);
                    throw;
                }
                catch (Exception e)
                {
                    logger.Warn("Backup instance scheduled work item error: {0}", e.Message);
                    throw new SPWFProcessorException("Backup instance scheduled work item error.", e);
                }
                return result;
            }
        }

        internal int BackupTasks(SPWorkflowSubItemUnit parentUnit)
        {
            if (!SPWorkflowProcessorRuntime.BackupInstanceOption.ProcessTaskItem)
            {
                logger.Info("Skip backup task item info for instance.");
                return 0;
            }

            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupTasks"))
            {
                int result = 0;
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupTasks");
                string monitor = "Backup Tasks";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    var tasks = new List<SPWorkflowSubItemUnit>();
                    Guid siteId = (Guid)parentUnit.Properties["#SiteId"];
                    Guid webId = (Guid)parentUnit.Properties["#WebId"];
                    Guid hostId = (Guid)parentUnit.Properties[CustomFieldProfix + "TaskListId"];
                    Guid instanceId = (Guid)parentUnit.Properties["#Id"];
                    using (IAveQueryDataReader sdr = mQueryService.BackupTasks(siteId, webId, hostId, instanceId))
                    {
                        while (sdr.Read())
                        {
                            SPWorkflowSubItemUnit taskUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Task, parentUnit);
                            Dictionary<string, string> fieldDic = mWebLevelFieldProcessor.GetDBFieldToSPFieldDic(mParentItem.Web.Lists[(Guid)parentUnit.Properties[CustomFieldProfix + "TaskListId"]]);
                            taskUnit.SetPropsFromDataReader(sdr, 0, fieldDic, 0);
                            FixupUserLoginName(taskUnit);
                            BackupPermissionUnit(taskUnit);
                            //BackupSubscription(taskUnit);
                            BackupCustomUnit(taskUnit);
                            tasks.Add(taskUnit);
                            result++;
                        }
                    }
                    //BackupSubscription also use mQueryService,can not get two QueryDataReader one time.
                    foreach (var taskUnit in tasks)
                    {
                        BackupSubscription(taskUnit);
                        parentUnit.ChildUnits.Add(taskUnit);
                    }
                }
                catch (SPWFProcessorException ex)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupTaskItemsException, ex.Message);
                    logger.Warn("An exception occurred while backup task items. exception:{0}", ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupTaskItemsException, e.Message);
                    logger.Warn("An exception occurred while backup task items. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupTaskError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupTasks");
                }
                return result;
            }
        }

        internal int BackupSubscription(SPWorkflowSubItemUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupSubscription"))
            {
                int result = 0;
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupSubscription");
                string monitor = "Backup Subscriptions";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    #region Backup Task Item Context
                    if (parentUnit.ItemType == WorkflowSubItemType.Task)
                    {
                        Guid siteId = (Guid)parentUnit.ParentUnit.Properties["#SiteId"];
                        Guid webId = (Guid)parentUnit.ParentUnit.Properties["#WebId"];
                        Guid hostId = (Guid)parentUnit.Properties["~0_tp_ListId"];
                        byte[] contextCollectionId = ((Guid)parentUnit.Properties["_0_GUID"]).ToByteArray();
                        using (IAveQueryDataReader sdr = mQueryService.BackupTaskItemEvents(siteId, webId, hostId, contextCollectionId))
                        {
                            if (sdr.Read())
                            {
                                SPWorkflowSubItemUnit eventUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Subscription, parentUnit);
                                eventUnit.SetPropsFromDataReader(sdr, 0, null, 0);
                                BackupCustomUnit(eventUnit);
                                parentUnit.ChildUnits.Add(eventUnit);
                                result++;
                            }
                        }
                    }
                    #endregion
                    #region Backup Instance Events
                    else if (parentUnit.ItemType == WorkflowSubItemType.Instance)
                    {
                        #region Backup Parent Item Context
                        Guid siteId = (Guid)parentUnit.Properties["#SiteId"];
                        Guid webId = (Guid)parentUnit.Properties["#WebId"];
                        Guid hostId = (Guid)parentUnit.Properties["#ListId"];
                        byte[] contextCollectionId = null;
                        if (parentUnit.Properties.Contains("#ItemGUID"))
                        {
                            contextCollectionId = ((Guid)parentUnit.Properties["#ItemGUID"]).ToByteArray();
                        }
                        using (IAveQueryDataReader sdr = mQueryService.BackupInstanceParentItemEvents(siteId, webId, hostId, contextCollectionId))
                        {
                            if (sdr.Read())
                            {
                                SPWorkflowSubItemUnit eventUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Subscription, parentUnit);
                                eventUnit.SetPropsFromDataReader(sdr, 0, null, 0);
                                eventUnit.Properties.Add(CustomFieldProfix + "Data", "ParentItemContext");
                                parentUnit.ChildUnits.Add(eventUnit);
                            }
                        }
                        #endregion

                        #region Backup Workflow Events
                        contextCollectionId = ((Guid)parentUnit.Properties["#Id"]).ToByteArray();
                        using (IAveQueryDataReader sdr = mQueryService.BackupInstanceEvents(siteId, webId, contextCollectionId))
                        {
                            while (sdr.Read())
                            {
                                SPWorkflowSubItemUnit eventUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Subscription, parentUnit);
                                eventUnit.SetPropsFromDataReader(sdr, 0, null, 0);
                                BackupCustomUnit(eventUnit);
                                parentUnit.ChildUnits.Add(eventUnit);
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupEventReceiversException, e.Message);
                    logger.Warn("An exception occurred while backup workflow instance event receivers. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupSubscriptionError, e, parentUnit.ItemType.ToString());
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupSubscription");
                }
                return result;
            }
        }

        internal int BackupHistory(SPWorkflowSubItemUnit parentUnit)
        {
            if (!SPWorkflowProcessorRuntime.BackupInstanceOption.ProcessHistoryItem)
            {
                logger.Info("Skip backup history info for instance.");
                return 0;
            }

            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupHistory"))
            {
                int result = 0;
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupHistory");
                string monitor = "Backup Histories";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    string instanceIdColName = string.Empty;
                    string associationIdColName = string.Empty;
                    string parentListIdColName = string.Empty;
                    string baseIdColName = string.Empty;
                    string itemIdColName = string.Empty;
                    GetHistoryColNames(mParentItem.Web.Lists[(Guid)parentUnit.Properties[CustomFieldProfix + "HistoryListId"]].Fields.SchemaXml,
                        out instanceIdColName,
                        out associationIdColName,
                        out parentListIdColName,
                        out baseIdColName,
                        out itemIdColName);

                    if (!string.IsNullOrEmpty(instanceIdColName))
                    {
                        using (IAveQueryDataReader sdr = mQueryService.BackupHistory((Guid)parentUnit.Properties["#SiteId"], (Guid)parentUnit.Properties["#WebId"], (Guid)parentUnit.Properties[CustomFieldProfix + "HistoryListId"], (Guid)parentUnit.Properties["#Id"], instanceIdColName))
                        {
                            while (sdr.Read())
                            {
                                SPWorkflowSubItemUnit histUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.History, parentUnit);
                                Dictionary<string, string> fieldDic = mWebLevelFieldProcessor.GetDBFieldToSPFieldDic(mParentItem.Web.Lists[(Guid)parentUnit.Properties[CustomFieldProfix + "HistoryListId"]]);
                                histUnit.SetPropsFromDataReader(sdr, 0, fieldDic, 0);
                                FixupUserLoginName(histUnit);
                                BackupCustomUnit(histUnit);
                                parentUnit.ChildUnits.Add(histUnit);
                                result++;
                            }
                        }
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupHistoriesException, procException.Message);
                    logger.Warn("An exception occurred while backup workflow instance history items. exception:{0}", procException.ToString());
                    mInnerWarnings.Add(procException);
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupHistoriesException, e.Message);
                    logger.Warn("An exception occurred while backup workflow instance history items. exception:{0}", e.ToString());
                    mInnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupHistoryError, e));
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupHistory");
                }
                return result;
            }
        }

        private void BackupCustomUnit(SPWorkflowSubItemUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupCustomUnit"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupCustomUnit");
                string monitor = "Backup Custom Units";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    switch (parentUnit.ItemType)
                    {
                        case WorkflowSubItemType.Task:
                        case WorkflowSubItemType.Instance:
                        case WorkflowSubItemType.Schedule:
                            #region Backup Custom Workflow
                            CustomWorkflowInstanceProc customInstanceProc = new CustomWorkflowInstanceProc(CustomProcessors);
                            customInstanceProc.FireBackupCustomWorkflowDataEvent(parentUnit);
                            #endregion
                            break;
                        case WorkflowSubItemType.Subscription:
                        case WorkflowSubItemType.History:
                        case WorkflowSubItemType.Invalid:
                        default:
                            break;
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupCustomUnitsException, procException.Message);
                    logger.Warn("An exception occurred while backup custom units. exception:{0}", procException.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupCustomUnitsException, e.Message);
                    logger.Warn("An exception occurred while backup custom units. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceCustomDataBackupError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupCustomUnit");
                }
            }
        }

        private void BackupPermissionUnit(SPWorkflowSubItemUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance.BackupOneInstance.BackupPermissionUnit"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupPermissionUnit");
                string monitor = "Backup Permission Units";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    if (parentUnit.Properties.Contains("_0__IsCurrentVersion") && (bool)parentUnit.Properties["_0__IsCurrentVersion"])
                    {
                        if (parentUnit.Properties.Contains("_0_ID") && parentUnit.Properties.Contains("~0_tp_ListId"))
                        {
                            using (IAveWeb web = mParentItem.Web)
                            {
                                Guid listId = (Guid)parentUnit.Properties["~0_tp_ListId"];
                                int itemId = (int)parentUnit.Properties["_0_ID"];
                                IAveListItem item = web.Lists[listId].GetItemById(itemId);
                                using (SPPermissionProcessor permProc = SPPermissionProcessor.CreateInstance(SPPermissionScope.Item, item))
                                {
                                    parentUnit.PermissionUnit = permProc.BackupWithoutSerialization();
                                }
                            }
                        }
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupPermissionsException, procException.Message);
                    logger.Warn("An exception occurred while backup permission. exception:{0}", procException.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupPermissionsException, e.Message);
                    logger.Warn("An exception occurred while backup custom units. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionUnitBackupException, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupPermissionUnit");
                }
            }
        }
        #endregion


        #region ************************Restore Region************************
        private WorkflowFixupParams mFixupParams = new WorkflowFixupParams();

        public override int Restore(byte[] serializedData)
        {
            try
            {
                SPWFInstanceUnit unit = SPWFInstanceUnit.Load(serializedData);
                return Restore(unit);
            }
            catch (SPWFProcessorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceUnknownError, e);
            }
        }

        public override int Restore(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "InstanceRestore");
                string monitor = mMainMonitorLog = "Instance Restore";
                bool eventFiringDisabled = false;

                TaskItemGuidMapping.Clear();
                byte[] cacheData = SPWFInstanceUnit.Save(unit);
                try
                {
                    mInstanceUnit = unit;

                    SPWorkflowSubItemUnit instanceItemUnit = unit.InstanceItem;


                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + instanceItemUnit.Properties["#Id"]);
                    mPerformanceMonitor.StartMonitor(monitor);
                    #endregion

                    using (AvePerformanceScope pf1 = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.GetParentAssociation"))
                    {
                        #region Get Parent Association
                        try
                        {
                            Guid parentAssoId = (Guid)instanceItemUnit.Properties.GetEx("#TemplateId");
                            if (unit.ParentAssociationUnit == null)
                            {
                                if (mAssociationProc.UnitsOfRestored.ContainsKey(parentAssoId))
                                    unit.ParentAssociationUnit = mAssociationProc.UnitsOfRestored[parentAssoId];
                                else
                                    throw new SPWFProcessorException(SPWFProcessorErrorCode.ParentAssociationCannotBeFound);
                            }
                        }
                        catch (SPWFProcessorException procException)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
                            logger.Log(AveLogLevel.DEBUG, "An processor error occurred while restoring workflow instance, error message: {0}", procException);
                            try
                            {
                                if (procException.ErrorCode == SPWFProcessorErrorCode.PutIntoPostAction)
                                {
                                    logger.Log(AveLogLevel.DEBUG, "The processor exception error code is 9999");
                                    SPWorkflowProcessorRuntime.OnCacheData(this.mParentItem.Web.Site.Url, this.mParentItem.SiteID.ToString(), this.mParentItem.WebID.ToString(), this.mParentItem.ListID.ToString(), this.mParentItem.ListID.ToString(), this.mParentItem.ItemID, unit.InstanceItem.Properties["#Id"].ToString(), cacheData);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.CacheDataError, e.ToString());
                            }
                            throw;
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, e.Message);
                            logger.Warn("Restore parent not found exception:{0}", e.ToString());
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.GetParentAssociationError, e);
                        }
                        #endregion
                    }

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Get Parent Association. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    InitializeFixupParams(unit);

                    #region Performance Monitor Region
                    mPerformanceMonitor.ResetCurrentDuration(monitor); ;
                    #endregion

                    int conflictStatus = 0;// HandleWorkflowInstanceConflict(unit.FixupParameters);
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreConflictStatus, conflictStatus.ToString());

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Handle Conflict. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    if (conflictStatus <= 0)
                    {
                        eventFiringDisabled = SPEventManagerWrapper.EventFiringDisabled;
                        if (!eventFiringDisabled)
                        {
                            SPEventManagerWrapper.DisableEventFiring();
                        }
                        RestoreWorkflowSubItem(unit);
                        return 0;
                    }
                    else
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceConflict);
                    }
                }
                finally
                {
                    if (!eventFiringDisabled)
                        SPEventManagerWrapper.EnableEventFiring();

                    #region Performance Monitor Region
                    mPerformanceMonitor.StopMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    #endregion

                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceRestore");
                }
            }
        }


        private void InitializeFixupParams(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.InitializeFixupParams"))
            {
                try
                {
                    SPWorkflowSubItemUnit instanceItem = unit.InstanceItem;
                    unit.FixupParameters.Dispose();
                    unit.FixupParameters.mInstanceIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#Id"), Guid.NewGuid());
                    unit.FixupParameters.mWebApplicationIdDic.AddEx(Guid.Empty, mParentItem.Web.Site.WebApplication.ID);
                    unit.FixupParameters.mParentAssoicationIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#TemplateId"), unit.ParentAssociationUnit.SerializableData.mId);
                    unit.FixupParameters.mParentAssociationBaseIdDic.AddEx(Guid.Empty, TryGetMappingBaseId(unit.ParentAssociationUnit.SerializableData.mBaseId));
                    if (mParentItem.ParentItemType == WFParentItemType.ListItem)
                    {
                        unit.FixupParameters.mListIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#ListId"), unit.ParentAssociationUnit.mListId);
                    }
                    else
                    {
                        unit.FixupParameters.mListIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#ListId"), unit.ParentAssociationUnit.mWebId);
                    }
                    unit.FixupParameters.mWebIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#WebId"), unit.ParentAssociationUnit.mWebId);
                    unit.FixupParameters.mSiteIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#SiteId"), unit.ParentAssociationUnit.mSiteId);
                    unit.FixupParameters.mTaskListIdDic.AddEx((Guid)instanceItem.Properties.GetEx(CustomFieldProfix + "TaskListId"), unit.ParentAssociationUnit.SerializableData.mTaskListId);
                    unit.FixupParameters.mHistoryListIdDic.AddEx((Guid)instanceItem.Properties.GetEx(CustomFieldProfix + "HistoryListId"), unit.ParentAssociationUnit.SerializableData.mHistoryListId);
                    unit.FixupParameters.mItemIdDic.AddEx((int)instanceItem.Properties.GetEx("#ItemId"), mParentItem.ItemID);
                    if (mParentItem.ParentItemType == WFParentItemType.ListItem)
                    {
                        Guid tp_Guid = GetListItemGuid(mParentItem);
                        if (tp_Guid != Guid.Empty)
                        {
                            unit.FixupParameters.mItemGuidDic.AddEx((Guid)instanceItem.Properties.GetEx("#ItemGUID"), tp_Guid);
                        }
                        else
                        {
                            logger.Warn("An error occurred while Getting the list item Guid.");
                        }
                    }
                    unit.FixupParameters.mInternalStateDic.AddEx((int)instanceItem.Properties.GetEx("#InternalState"), 0);
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InitializeFixupParamError, e);
                }
            }
        }

        /// <summary>
        /// When call CreateWorkflow function failed, this function will be called to get or create the source status field.
        /// There will be a problem if the source status field is using by another workflow associaion
        /// </summary>
        /// <param name="instanceUnit"></param>
        /// <returns></returns>
        //todo:wbhu,处理workflow status column的逻辑需要重新理顺,添加注释或者参照API查找status column的逻辑重写,由于涉及的case比较多,暂时不动
        private string GetOrCreateStatusField(SPWFInstanceUnit instanceUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetOrCreateStatusField");
            string colName = string.Empty;
            string rowOrdinal = string.Empty;
            XmlDocument doc = null;
            instanceUnit.StatusFieldColName = null;
            Guid mSourceFieldID = Guid.Empty;
            string mSourceFieldInternalName = string.Empty;
            string mSourceFieldDisplayName = string.Empty;
            try
            {
                string statusName = instanceUnit.ParentAssociationUnit.SPAssociation.InternalNameStatusField;
                if (string.IsNullOrEmpty(statusName))
                    statusName = instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldName;
                if (string.IsNullOrEmpty(statusName))
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_EmptyStatusFieldName);
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldSchema))
                {
                    logger.Debug("status field schema is empty.");
                    return string.Empty;
                }

                var sourceFieldInfo = new AveSPField(instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldSchema);

                //instanceUnit.ParentAssociationUnit.ReloadParentWeb();
                IAveWeb web = instanceUnit.ParentAssociationUnit.ParentWeb;
                IAveList list = instanceUnit.ParentAssociationUnit.ParentList;
                if (list == null)
                {
                    list = web.Lists[mParentItem.ListID];
                }
                {
                    var statusFieldObj = list.Fields.GetFieldByInternalName(statusName, false);
                    if (statusFieldObj != null)
                    {

                        if (statusFieldObj.Type != AveFieldType.WorkflowStatus)
                            statusFieldObj = null;
                        else if (!IsAvailableStatusFieldName(list, instanceUnit.ParentAssociationUnit.SPAssociation.BaseId, statusFieldObj, sourceFieldInfo.SerializableData.mSrcDisplayName))
                            statusFieldObj = null;
                    }

                    if (statusFieldObj == null)
                    {
                        doc = new XmlDocument();
                        doc.LoadXml("<Root>" + instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldSchema + "</Root>");
                        XmlElement xe = (XmlElement)doc.FirstChild.ChildNodes[0];
                        mSourceFieldID = new Guid(xe.Attributes["ID"].Value);
                        mSourceFieldInternalName = xe.Attributes["Name"].Value;
                        mSourceFieldDisplayName = xe.Attributes["DisplayName"].Value;
                        xe.RemoveAttribute("ID");
                        AveSPFieldCollection fields = new AveSPFieldCollection { CurrentFieldCollection = list.Fields };
                        statusFieldObj = fields.CreateSPField(xe, true, AveAddFieldOptions.DefaultValue);
                    }
                    else
                    {
                        if (list.DefaultView != null && !list.DefaultView.ViewFields.Exists(statusFieldObj.InternalName))
                        {
                            IAveView defaultView = list.DefaultView;
                            defaultView.ViewFields.Add(statusFieldObj.InternalName);
                            defaultView.Update();
                        }
                    }
                    if (statusFieldObj == null)
                        return string.Empty;
                    var statusField = list.Fields.GetFieldByInternalName(statusFieldObj.InternalName);
                    colName = AveSPField.GetColNameFromSchema("ColName", statusField.SchemaXml);
                    rowOrdinal = AveSPField.GetColNameFromSchema("RowOrdinal", statusField.SchemaXml);
                    try
                    {
                        instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldName = statusField.InternalName;
                        instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldSchema = statusField.SchemaXml;
                        LSInvoker.SetProperty(instanceUnit.ParentAssociationUnit.SPAssociation, "InternalNameStatusField", statusField.InternalName);
                        //instanceUnit.ParentAssociationUnit.UpdateWorkflowAssociation(instanceUnit.ParentAssociationUnit.SPAssociation);//doesn't work
                        SPWFAssociationProcNative.UpdateStatusFieldName(instanceUnit.ParentAssociationUnit.SPAssociation);
                        if (!instanceUnit.ParentAssociationUnit.IsBuiltinBaseId)
                        {
                            foreach (var parentAsso in instanceUnit.ParentAssociationUnit.SPAssoicationCollection)
                            {
                                if (!parentAsso.BaseId.Equals(instanceUnit.ParentAssociationUnit.SPAssociation.BaseId))
                                {
                                    continue;
                                }
                                parentAsso.InternalNameStatusField = statusField.InternalName;
                                SPWFAssociationProcNative.UpdateStatusFieldName(parentAsso);
                            }
                        }
                        AddStatusFieldNameMapping(list, instanceUnit.ParentAssociationUnit.SPAssociation.BaseId, statusField.InternalName);
                        //IsCurrentVersion判断不准确，而且此处没必要再给status field 更新DisplayName,keep原端的就可以
                        //if (instanceUnit.ParentAssociationUnit.IsCurrentVersion)
                        //{
                        //    statusField.Title = instanceUnit.ParentAssociationUnit.SPAssociation.Name;
                        //    try
                        //    {
                        //        statusField.Update();
                        //    }
                        //    catch (Exception exc)
                        //    {
                        //        logger.Warn("An exception occurred while update the workflow field, exception: {0}", exc.ToString());
                        //        web.ReloadWeb();
                        //        list.Reload();
                        //        statusField = list.Fields.GetFieldByInternalName(statusName);
                        //        statusField.Title = instanceUnit.ParentAssociationUnit.SPAssociation.Name;
                        //        statusField.Update();
                        //    }
                        //}

                        if (!instanceUnit.ParentAssociationUnit.isCreateField)
                        {
                            if (mSourceFieldID != Guid.Empty && !instanceUnit.mWFFieldIDMapping.ContainsKey(mSourceFieldID))
                            {
                                instanceUnit.mWFFieldIDMapping.Add(mSourceFieldID, statusField.ID);
                            }
                            if (mSourceFieldInternalName != string.Empty && !instanceUnit.mWFFieldInternalNameMapping.ContainsKey(mSourceFieldInternalName))
                            {
                                instanceUnit.mWFFieldInternalNameMapping.Add(mSourceFieldInternalName, statusField.InternalName);
                            }
                            if (mSourceFieldDisplayName != string.Empty && !instanceUnit.mWFFieldDisplayNameMapping.ContainsKey(mSourceFieldDisplayName))
                            {
                                instanceUnit.mWFFieldDisplayNameMapping.Add(mSourceFieldDisplayName, statusField.Title);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.IP_ResetStatusFieldException, e.Message);
                        logger.Warn("An exception occurred while reset status field. exception:{0}", e.ToString());
                    }
                    SPWorkflowProcessorRuntime.Log(Logs.IP_StatusFieldName, colName);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.StatusColumnGetorAddError, mSourceFieldInternalName, ex);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetOrCreateStatusField");
            }
            instanceUnit.StatusFieldColName = colName;
            instanceUnit.StatusFieldRowOrdinal = rowOrdinal;
            return colName;
        }


        private void FixupUserIds(SPWorkflowSubItemUnit subUnit, SPWFInstanceUnit parentUnit, List<Assigns> assignToUsers)
        {
            using (IAveWeb web = mParentItem.Web)
            {
                if (subUnit.ItemType == WorkflowSubItemType.Task)
                {
                    if (subUnit.Properties.ContainsKey("_0_AssignedTo"))
                    {
                        #region Fixup AssignedTo

                        if (((string)subUnit.Properties["_0_AssignedTo"]).Equals(string.Empty))
                        {
                            subUnit.Properties.Remove("_0_AssignedTo");
                        }
                        else
                        {
                            string loginName = (string)subUnit.Properties["_0_AssignedTo"];
                            IAvePrincipal member = SPPermissionProcessor.GetOrCreateMember(loginName);
                            if (member != null)
                            {
                                if (assignToUsers != null)
                                {
                                    var newAss = new Assigns()
                                    {
                                        Login = member.LoginName,
                                        DisplayName = member.Name
                                    };
                                    if (member is IAveUser)
                                    {
                                        newAss.Type = "User";
                                    }
                                    else // IAveGroup
                                    {
                                        newAss.Type = "SharePointGroup";
                                    }
                                    if (!assignToUsers.Contains(newAss))
                                    {
                                        assignToUsers.Add(newAss);
                                    }
                                }
                                subUnit.Properties["_0_AssignedTo"] = member.ID;
                            }
                            else
                            {
                                subUnit.Properties.Remove("_0_AssignedTo");
                            }
                        }
                        #endregion
                    }
                }
                else if (subUnit.ItemType == WorkflowSubItemType.History)
                {
                    if (subUnit.Properties.ContainsKey("_0_User"))
                    {
                        #region Fixup User Column

                        if (((string)subUnit.Properties["_0_User"]).Equals(string.Empty))
                        {
                            subUnit.Properties.Remove("_0_User");
                        }
                        else
                        {
                            string loginName = (string)subUnit.Properties["_0_User"];
                            IAvePrincipal user = SPPermissionProcessor.GetOrCreateMember(loginName);
                            if (user != null)
                            {
                                subUnit.Properties["_0_User"] = user.ID;
                            }
                            else
                            {
                                subUnit.Properties.Remove("_0_User");
                            }
                        }
                        #endregion
                    }
                }
                else if (subUnit.ItemType == WorkflowSubItemType.Instance)
                {
                    if (subUnit.Properties.ContainsKey("#Author"))
                    {
                        #region Fixup Instance Author
                        if (((string)subUnit.Properties["#Author"]).Equals(string.Empty))
                        {
                            subUnit.Properties["#Author"] = web.CurrentUser.ID;
                        }
                        else
                        {
                            string loginName = (string)subUnit.Properties["#Author"];
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(loginName);
                            if (user != null)
                            {
                                subUnit.Properties["#Author"] = user.ID;
                            }
                            else
                            {
                                subUnit.Properties["#Author"] = web.CurrentUser.ID;
                            }
                        }
                        return;
                        #endregion
                    }
                }


                #region Fixup Author
                if (((string)subUnit.Properties["_0_Author"]).Equals(string.Empty))
                {
                    subUnit.Properties.Remove("_0_Author");
                }
                else
                {
                    string loginName = (string)subUnit.Properties["_0_Author"];
                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(loginName);
                    if (user != null)
                    {
                        subUnit.Properties["_0_Author"] = user.ID;
                    }
                    else
                    {
                        subUnit.Properties.Remove("_0_Author");
                    }
                }
                #endregion


                #region Fixup Editor
                if (((string)subUnit.Properties["_0_Editor"]).Equals(string.Empty))
                {
                    subUnit.Properties.Remove("_0_Editor");
                }
                else
                {
                    string loginName = (string)subUnit.Properties["_0_Editor"];
                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(loginName);
                    if (user != null)
                    {
                        subUnit.Properties["_0_Editor"] = user.ID;
                    }
                    else
                    {
                        subUnit.Properties.Remove("_0_Editor");
                    }
                }
                #endregion
            }
        }

        private void CreateWorkflow(SPWFInstanceUnit unit, AveWorkflowRunOptions options, List<Assigns> assigns)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.CreateWorkflow"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "CreateWorkflow");
                string monitor = "Create Workflow";
                if ((mParentItem == null) || (unit.ParentAssociationUnit == null) || (unit.ParentAssociationUnit.SPAssociation == null))
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "CreateWorkflow");
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.CreateInstanceArgumentNullException);
                }


                bool enabled = unit.ParentAssociationUnit.SPAssociation.Enabled;
                bool isAllowManualStart = unit.ParentAssociationUnit.SPAssociation.AllowManual;
                bool isAutoStart = unit.ParentAssociationUnit.SPAssociation.AutoStartCreate;
                bool isAutoStartChange = unit.ParentAssociationUnit.SPAssociation.AutoStartChange;
                bool needUpdate = false;

                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    mWFManager = mParentItem.WFManager;

                    if (unit.ParentAssociationUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                    {
                        unit.ParentAssociationUnit.ReloadSPAssociation();
                    }

                    if (!enabled || !isAllowManualStart)
                    {
                        needUpdate = true;
                        unit.ParentAssociationUnit.ReloadSPAssociation();
                        unit.ParentAssociationUnit.SPAssociation.Enabled = true;
                        unit.ParentAssociationUnit.SPAssociation.AllowManual = true;
                        unit.ParentAssociationUnit.UpdateWorkflowAssociation(unit.ParentAssociationUnit.SPAssociation);
                    }

                    mInstance = null;

                    GetOrCreateStatusField(unit);

                    //只有启动instance使用
                    IAveWorkflowAssociation spAssociation = unit.ParentAssociationUnit.SPAssociation;
                    //todo:wbhu,无论是什么类型的workflow association，使用还原definition时缓存的SP对象也不合理，应该重新获取，
                    //而且没有必要有这么复杂的判断逻辑，通过ID重取即可，66暂不修改，67处理
                    //Note:还原association时的SPAssociation对象没必要缓存，在还原instance时重取更合理
                    if (options == AveWorkflowRunOptions.Asynchronous)
                    {
                        if (!unit.ParentAssociationUnit.IsBuiltinBaseId)
                        {
                            foreach (var parentAsso in unit.ParentAssociationUnit.SPAssoicationCollection)
                            {
                                if (!parentAsso.BaseId.Equals(spAssociation.BaseId))
                                {
                                    continue;
                                }
                                if (parentAsso.ID != spAssociation.ID)
                                {
                                    continue;
                                }
                                int configuration = (int)(parentAsso.Configuration);
                                if (configuration.IsContainsBinaryBitValue((int)AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AllowManualStart)
                                    && !configuration.IsContainsBinaryBitValue((int)AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration.NoNewWorkflows))
                                {
                                    spAssociation = parentAsso;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //缓存的association对象可能有问题（ParentCollection为空），会导致在非system account作为agent account时，API内部提权后重取association返回null，最终导致启动instance失败
                        //还原instance时option为1025，也会走此处，避免start instance出错
                        //异步启动上面已经有处理，此处处理非异步启动的情况
                        //todo:wbhu, 以后考虑是否需要在启动instance时外部提权，避免API内部提权导致的各种问题
                        spAssociation = unit.ParentAssociationUnit.SPAssoicationCollection[spAssociation.ID];
                    }

                    var startWorkflowObjects = PreProcessInstanceCreation(mParentItem, spAssociation);
                    var assData = unit.ParentAssociationUnit.SPAssociation.AssociationData;
                    if (assigns != null && assigns.Count > 0)
                    {
                        //ADO-167263 支持start instance时填写assign to user case
                        AddAssignUsesrFromProperty(assigns, unit);
                        assData = ChangeAssoData(assigns, spAssociation, assData);
                    }
                    mInstance = mWFManager.StartWorkflow(startWorkflowObjects.Item1, startWorkflowObjects.Item2, assData, options);

                    //fixup status field name
                    try
                    {
                        if (unit.ParentAssociationUnit.SerializableData.mStatusFieldName != mInstance.ParentAssociation.InternalNameStatusField)
                        {
                            unit.ParentAssociationUnit.SerializableData.mStatusFieldName = mInstance.ParentAssociation.InternalNameStatusField;
                            GetOrCreateStatusField(unit);
                        }
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.IP_ResetStatusFieldException, e.Message);
                        logger.Warn("An exception occurred while reset status field. exception:{0}", e.ToString());
                    }

                    unit.FixupParameters.mInstanceIdDic.AddEx((Guid)unit.InstanceItem.Properties.GetEx("#Id"), mInstance.InstanceId);
                }
                catch (AveWrapperCheckoutFileException ex)
                {
                    //restart,出错就往外抛，restore instance时可以通过数据库插入instance,需要继续还原
                    if ((int)options != 1025)
                    {
                        throw;
                    }
                    logger.Warn("An exception occurred while create instance on checked out file, the user does not have enough permission .Option,{0} exception:{1}", options, ex);
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_CreateInstanceException, e.Message);
                    logger.Warn("An exception occurred while create instance.Option,{0} exception:{1}", options, e);
                    InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.CreateInstanceUnknownException, e));
                }
                finally
                {
                    if (needUpdate)
                    {
                        unit.ParentAssociationUnit.ReloadSPAssociation();
                        unit.ParentAssociationUnit.SPAssociation.AllowManual = isAllowManualStart;
                        unit.ParentAssociationUnit.SPAssociation.Enabled = enabled;
                        unit.ParentAssociationUnit.UpdateWorkflowAssociation(unit.ParentAssociationUnit.SPAssociation);
                    }

                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "CreateWorkflow");
                }
            }
        }

        /// <summary>
        /// support to start workflow instance on checkout file
        /// </summary>
        /// <param name="parentItem"></param>
        /// <param name="spAssociation"></param>
        /// <returns></returns>
        private Tuple<object, IAveWorkflowAssociation> PreProcessInstanceCreation(AveWFParentItem parentItem, IAveWorkflowAssociation spAssociation)
        {
            RemoveRunningInstanceBeforeAdd(spAssociation);

            #region reload ListItem And Association if necessary
            try
            {
                var association = spAssociation;
                var parentObject = parentItem.ParentItem;
                var item = parentItem.ListItem;
                if (parentItem.ItemID > 0 && item.File != null && item.File.CheckOutStatus != AveCheckOutStatus.None)
                {
                    var checkedOutWeb = item.Web.Site.GetCheckoutWeb(item.Web.Site.ID, item.Web, item.ParentList, item.File.CheckedOutByUser, item.File.UniqueId, false, true);
                    var checkedOutList = checkedOutWeb.Lists.GetById(item.ParentList.ID);
                    if (string.Equals(spAssociation.ContentTypeId.ToString(), AveBuiltInContentTypeId.System, StringComparison.OrdinalIgnoreCase))
                    {
                        association = checkedOutList.WorkflowAssociations[spAssociation.ID];
                    }
                    else
                    {
                        association = checkedOutList.ContentTypes[spAssociation.ContentTypeId].WorkflowAssociations[spAssociation.ID];
                    }
                    parentObject = checkedOutList.GetItemById(item.ID);
                }
                //Dispose query service connection.
                parentItem.ListItem.ParentList.ParentWeb.Site.Dispose();
                return new Tuple<object, IAveWorkflowAssociation>(parentObject, association);
            }
            catch (AveWrapperCheckoutFileException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while ReloadObjectIfNecessary,Error:{0}", e);
            }
            return new Tuple<object, IAveWorkflowAssociation>(parentItem.ParentItem, spAssociation);
            #endregion
        }

        private void AddAssignUsesrFromProperty(List<Assigns> assigns, SPWFInstanceUnit unit)
        {
            //assign to 的是串行的话,这是只对一个人建出了task,其他的人存在DB的这个字段里,需要把还没建出task的这些人也给找出来,放到unit data的xml里
            if (unit.InstanceItem != null && unit.InstanceItem.Properties.ContainsKey("#Modifications"))
            {
                var modifications = unit.InstanceItem.Properties["#Modifications"].ToString();
                try
                {
                    var needContinue = true;
                    var docstr = System.Web.HttpUtility.HtmlDecode(modifications);
                    Regex regPersonStart = new Regex("<[a-z]*:Person", RegexOptions.IgnoreCase);
                    Regex regPersonEnd = new Regex("</[a-z]*:Person>", RegexOptions.IgnoreCase);
                    Regex regAccountTypeStart = new Regex("<[a-z]*:AccountType>", RegexOptions.IgnoreCase);
                    Regex regAccountTypeEnd = new Regex("</[a-z]*:AccountType>", RegexOptions.IgnoreCase);
                    Regex regAccountIdStart = new Regex("<[a-z]*:AccountId>", RegexOptions.IgnoreCase);
                    Regex regAccountIdEnd = new Regex("</[a-z]*:AccountId>", RegexOptions.IgnoreCase);

                    while (needContinue)
                    {
                        var personStartMatch = regPersonStart.Match(docstr);
                        var personStartIndex = personStartMatch.Index;
                        if (personStartIndex >= 0)
                        {
                            var personEndMatch = regPersonEnd.Match(docstr);
                            var personEndIndex = personEndMatch.Index + personEndMatch.Value.Length;
                            var oneperson = docstr.Substring(personStartIndex, personEndIndex - personStartIndex);

                            var accountTypeStartMatch = regAccountTypeStart.Match(oneperson);
                            var typeStartIndex = accountTypeStartMatch.Index;
                            var accountTypeEndMatch = regAccountTypeEnd.Match(oneperson);
                            var typeEndIndex = accountTypeEndMatch.Index + accountTypeEndMatch.Value.Length;
                            var type = oneperson.Substring(typeStartIndex, typeEndIndex - typeStartIndex).Replace(accountTypeStartMatch.Value, "").Replace(accountTypeEndMatch.Value, "");

                            var accountIdStartMatch = regAccountIdStart.Match(oneperson);
                            var accountStartIndex = accountIdStartMatch.Index;
                            var accountIdEndMatch = regAccountIdEnd.Match(oneperson);
                            var accountEndIndex = accountIdEndMatch.Index + accountIdEndMatch.Value.Length;
                            var account = oneperson.Substring(accountStartIndex, accountEndIndex - accountStartIndex).Replace(accountIdStartMatch.Value, "").Replace(accountIdEndMatch.Value, "");


                            if (string.Equals(type, "SharePointGroup", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    IAveGroup group = unit.ParentAssociationUnit.ParentWeb.SiteGroups[account];
                                    if (assigns != null)
                                    {
                                        var newAss = new Assigns()
                                        {
                                            Type = "SharePointGroup",
                                            Login = group.LoginName,
                                            DisplayName = group.Name
                                        };
                                        if (!assigns.Contains(newAss))
                                        {
                                            assigns.Add(newAss);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn("The Assign to group {0} does not exist. Error : {1}", account.ToString(), ex);
                                }
                            }
                            else
                            {
                                IAveUser user = SPPermissionProcessor.GetOrCreateUser(account);
                                if (user != null)
                                {
                                    var newAss = new Assigns
                                    {
                                        Type = "User",
                                        Login = user.LoginName,
                                        DisplayName = user.Name
                                    };
                                    if (!assigns.Contains(newAss))
                                    {
                                        assigns.Add(newAss);
                                    }
                                }
                            }

                            docstr = docstr.Substring(personEndIndex);
                        }
                        else
                        {
                            needContinue = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while Adding assign users to list .Assign XML:{0},Error:{1}", modifications, e);
                }
            }
        }

        private string ChangeAssoData(List<Assigns> assigns, IAveWorkflowAssociation spAssociation, string assData)
        {
            var baseId = spAssociation.BaseId;
            if (BuiltinWorkflowBaseIdCollection.Is07ApprovalOrFeedbackBaseId(baseId))
            {
                //处理 07 Approval\Feedback 中assign to user
                assData = Get07AssignToUser(assData, assigns);
            }
            else if (BuiltinWorkflowBaseIdCollection.Is10ApprovalOrFeedbackBaseId(baseId))
            {
                //处理 10 Approval\Feedback 中assign to user
                //Approve workflow
                assData = Get10Approvers(assData, assigns);
                //Collec workflow
                assData = Get10Reviewers(assData, assigns);
            }

            return assData;
        }

        private string Get07AssignToUser(string assData, List<Assigns> assigns)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(assData);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                var nurl = doc.DocumentElement.GetNamespaceOfPrefix("my");
                nsmgr.AddNamespace("my", nurl);

                var reviewersNodes = doc.SelectNodes("/my:myFields/my:Reviewers", nsmgr);
                foreach (XmlNode node in reviewersNodes)
                {
                    if (string.IsNullOrEmpty(node.InnerText))
                    {
                        //只有源端是在start instance时输入填写Assign to User才会change
                        foreach (var ass in assigns)
                        {
                            var person = doc.CreateElement("my:Person", nurl);
                            var display = doc.CreateElement("my:DisplayName", nurl);
                            display.InnerText = ass.DisplayName;
                            var accountid = doc.CreateElement("my:AccountId", nurl);
                            accountid.InnerText = ass.Login;
                            var acctype = doc.CreateElement("my:AccountType", nurl);
                            acctype.InnerText = ass.Type;
                            person.AppendChild(display);
                            person.AppendChild(accountid);
                            person.AppendChild(acctype);
                            node.AppendChild(person);
                        }
                        logger.Debug("Finish change 07 association data {0} assign to user.", assigns.Count);
                    }
                }
                return doc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while modifying the association data.Error:{0}", e);
            }
            return assData;
        }
        private string Get10AssignToUser(string assData, List<Assigns> assigns, string xPath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(assData);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                var nurl1 = doc.DocumentElement.GetNamespaceOfPrefix("d");
                nsmgr.AddNamespace("d", nurl1);
                var nurl2 = doc.DocumentElement.GetNamespaceOfPrefix("dfs");
                nsmgr.AddNamespace("dfs", nurl2);
                var nurl3 = doc.DocumentElement.GetNamespaceOfPrefix("pc");
                nsmgr.AddNamespace("pc", nurl3);
                var reviewNodes = doc.SelectNodes(xPath, nsmgr);
                foreach (XmlNode node in reviewNodes)
                {
                    if (string.IsNullOrEmpty(node.InnerText))
                    {
                        foreach (var ass in assigns)
                        {
                            var person = doc.CreateElement("pc:Person", nurl3);
                            var display = doc.CreateElement("pc:DisplayName", nurl3);
                            display.InnerText = ass.DisplayName;
                            var accountid = doc.CreateElement("pc:AccountId", nurl3);
                            accountid.InnerText = ass.Login;
                            var acctype = doc.CreateElement("pc:AccountType", nurl3);
                            acctype.InnerText = ass.Type;
                            person.AppendChild(display);
                            person.AppendChild(accountid);
                            person.AppendChild(acctype);
                            node.AppendChild(person);
                        }
                        logger.Debug("Finish change 10 association data {0} assign to user.", assigns.Count);
                    }
                }
                return doc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while modifying the association data.Error:{0}", e);
            }
            return assData;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "dfs")]
        private string Get10Reviewers(string assData, List<Assigns> assigns)
        {
            return Get10AssignToUser(assData, assigns, "/dfs:myFields/dfs:dataFields/d:SharePointListItem_RW/d:Reviewers/d:Assignment/d:Assignee");
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "dfs")]
        private string Get10Approvers(string assData, List<Assigns> assigns)
        {
            return Get10AssignToUser(assData, assigns, "/dfs:myFields/dfs:dataFields/d:SharePointListItem_RW/d:Approvers/d:Assignment/d:Assignee");
        }

        /// <summary>
        /// ADO-126348，在start item workflow instance之前先删除同一association对应的instance，避免在同一个item中start多个instance时出错。
        /// 需要考虑item和web两种parentObject,mParentItem.ParentItem可能是IAveListItem或者IAveWeb
        /// </summary>
        /// <param name="spAssociation"></param>
        private void RemoveRunningInstanceBeforeAdd(IAveWorkflowAssociation spAssociation)
        {
            try
            {
                object parentItem = mParentItem.ParentItem;
                IAveWorkflow needRemoveInstance = null;
                IAveWorkflowManager manager = null;
                if (parentItem is IAveListItem)
                {
                    IAveListItem item = mParentItem.ParentItem as IAveListItem;
                    manager = item.Web.Site.WorkflowManager;
                    if (item != null && item.WorkFlows != null && item.WorkFlows.Count != 0)
                    {
                        foreach (IAveWorkflow workflow in item.WorkFlows)
                        {
                            if (!workflow.IsCompleted && workflow.AssociationId.Equals(spAssociation.ID))
                            {
                                needRemoveInstance = workflow;
                            }
                        }
                    }
                }
                if (needRemoveInstance != null && manager != null)
                {
                    manager.RemoveWorkflowFromListItem(needRemoveInstance);
                }
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while removing running workflow instance before add, error message: {0}", e);
            }
        }

        private int RestoreWorkflowSubItem(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreWorkflowSubItem"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreWorkflowSubItem");
                try
                {
                    if (SPWorkflowProcessorRuntime.SkipRunningInstance && ((int)unit.InstanceItem.Properties["#InternalState"] == 2 || (int)unit.InstanceItem.Properties["#Status1"] == 2))
                    {
                        mInstance = null;
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceIsRunningException);
                        //return 0;
                    }
                    CreateWorkflow(unit, (AveWorkflowRunOptions)1025, null);
                    RestoreCustomSubItems(unit);
                    RestoreScheduledWorkItem(unit);
                    var assigns = RestoreTask(unit);
                    RestoreSubscription(unit, unit.InstanceItem);
                    RestoreInstance(unit);
                    RestoreHistory(unit);
                    ProcessWorkflowStatus(unit, assigns);
                    return 0;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.ResotreWFSubItemError, e.ToString());
                    if (mInstance != null)
                        mWFManager.RemoveWorkflowFromListItem(mInstance);
                    throw;
                }
                finally
                {
                    //if (mWFManager != null)
                    //    mWFManager.Dispose();

                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreWorkflowSubItem");
                }
            }
        }

        internal int RestoreScheduledWorkItem(SPWFInstanceUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreScheduledWorkItem"))
            {
                if (SPWorkflowProcessorRuntime.SkipRunningInstance)
                {
                    return 0;
                }
                SPWorkflowSubItemUnit instanceUnit = parentUnit.InstanceItem;
                if (instanceUnit == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceUnitIsNull);

                foreach (SPWorkflowSubItemUnit child in instanceUnit.ChildUnits)
                {
                    if (child.ItemType == WorkflowSubItemType.ScheduledWorkItem)
                    {
                        child.Properties["#SiteId"] = parentUnit.FixupParameters.mSiteIdDic.GetValue(0);
                        child.Properties["#Id"] = parentUnit.FixupParameters.mInstanceIdDic.GetValue(0);
                        Guid oldParentId = (Guid)child.Properties["#ParentId"];
                        Guid newParentId = Guid.Empty;
                        if (parentUnit.FixupParameters.mListIdDic.ContainsKey(oldParentId))
                        {
                            newParentId = parentUnit.FixupParameters.mListIdDic[oldParentId];
                        }
                        else if (parentUnit.FixupParameters.mWebIdDic.ContainsKey(oldParentId))
                        {
                            newParentId = parentUnit.FixupParameters.mWebIdDic[oldParentId];
                        }
                        else
                        {
                            logger.Warn("Unknown scheduled work item parent id {0}, workflow instance id {1}", oldParentId, child.Properties["#Id"]);
                            return 0;
                        }
                        child.Properties["#ParentId"] = newParentId;
                        if (child.Properties["#BatchId"] != null)
                        {
                            child.Properties["#BatchId"] = (parentUnit.FixupParameters.mInstanceIdDic.GetValue(0)).ToByteArray();
                        }
                        if (child.Properties["#WebId"] != null)
                        {
                            child.Properties["#WebId"] = (parentUnit.FixupParameters.mWebIdDic.GetValue(0)).ToByteArray();
                        }
                        bool hasScheduledWorkItem = false;
                        using (IAveQueryDataReader sdr = mQueryService.BackupScheduledWorkItem((Guid)child.Properties["#SiteId"], (Guid)child.Properties["#Id"]))
                        {
                            if (sdr.Read())
                            {
                                hasScheduledWorkItem = true;
                            }
                        }
                        if (hasScheduledWorkItem)
                        {
                            logger.Info("Scheduled work item already existed.");
                        }
                        else
                        {
                            InsertTableRow(child.Properties, "ScheduledWorkItems");
                        }
                        break;
                    }
                }
                return 0;
            }
        }

        internal List<Assigns> RestoreTask(SPWFInstanceUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreTask"))
            {
                var assigns = new List<Assigns>();
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreTasks");
                string monitor = "Restore Tasks";
                SPWorkflowSubItemUnit instanceUnit = parentUnit.InstanceItem;
                if (instanceUnit == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceUnitIsNull);

                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    foreach (SPWorkflowSubItemUnit child in instanceUnit.ChildUnits)
                    {
                        if (child.ItemType == WorkflowSubItemType.Task)
                        {
                            #region Fixup Some Parameters

                            FixupUserIds(child, parentUnit, assigns);
                            if (child.Properties.ContainsKey("_0_WorkflowInstanceID"))
                                child.Properties["_0_WorkflowInstanceID"] = parentUnit.FixupParameters.mInstanceIdDic.GetValue(0);
                            if (child.Properties.ContainsKey("_0_WorkflowListId"))
                                child.Properties["_0_WorkflowListId"] = parentUnit.FixupParameters.mListIdDic.GetValue(0);
                            if (child.Properties.ContainsKey("_0_WF4InstanceId"))
                                child.Properties["_0_WF4InstanceId"] = parentUnit.FixupParameters.mInstanceIdDic.GetValue(0).ToString().Trim(new char[] { '{', '}' });

                            string sourceAbsoluteWorkflowLink = null;
                            string destAbsoluteWorkflowLink = null;
                            if (child.Properties.ContainsKey("_0_WorkflowLink") && mParentItem.ListItem != null)
                            {
                                string origWebUrl = parentUnit.ParentAssociationUnit.mTaskListUnit.SerializableData.mParentWebServerRelativeUrl;
                                string oldUrl = (string)child.Properties["_0_WorkflowLink"];
                                string newUrl = null;
                                if (oldUrl.StartsWith(origWebUrl, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!string.IsNullOrEmpty(parentUnit.ParentAssociationUnit.mTaskListUnit.SerializableData.mParentWebUrl))
                                    {
                                        sourceAbsoluteWorkflowLink = parentUnit.ParentAssociationUnit.mTaskListUnit.SerializableData.mParentWebUrl + oldUrl.Substring(origWebUrl.TrimEnd('/').Length);
                                    }
                                    if (mParentItem.ParentItem is IAveListItem && mParentItem.ListItem.File != null)
                                    {
                                        newUrl = mParentItem.ListItem.File.ServerRelativeUrl;
                                    }
                                    else
                                    {
                                        newUrl = mParentItem.ListItem.ParentList.DefaultDisplayFormUrl + "?ID=" + parentUnit.FixupParameters.mItemIdDic.GetValue(0);
                                    }
                                }
                                if (!string.IsNullOrEmpty(newUrl))
                                {
                                    child.Properties["_0_WorkflowLink"] = newUrl;
                                    destAbsoluteWorkflowLink = mParentItem.ListItem.ParentList.ParentWeb.Url + newUrl.Substring(mParentItem.ListItem.ParentList.ParentWeb.ServerRelativeUrl.TrimEnd('/').Length);
                                }
                            }
                            if (!string.IsNullOrEmpty(sourceAbsoluteWorkflowLink) && !string.IsNullOrEmpty(destAbsoluteWorkflowLink))
                            {
                                if (child.Properties.ContainsKey("_0_Body"))
                                {
                                    string value = (string)child.Properties["_0_Body"];
                                    if (value.IndexOf(sourceAbsoluteWorkflowLink, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        child.Properties["_0_Body"] = value.Replace(sourceAbsoluteWorkflowLink, destAbsoluteWorkflowLink);
                                    }
                                }
                                if (child.Properties.ContainsKey("_0_EmailBody"))
                                {
                                    string value = (string)child.Properties["_0_EmailBody"];
                                    if (value.IndexOf(sourceAbsoluteWorkflowLink, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        child.Properties["_0_EmailBody"] = value.Replace(sourceAbsoluteWorkflowLink, destAbsoluteWorkflowLink);
                                    }
                                }
                            }
                            if (child.Properties.ContainsKey("_2_WorkflowLink"))
                            {
                                if (mParentItem.ParentItem is IAveListItem)
                                {
                                    if (mParentItem.ListItem.File != null)
                                    {
                                        string name = mParentItem.ListItem.File.Name;
                                        int extIndex = name.LastIndexOf('.');
                                        if (extIndex > 0)
                                            name = name.Substring(0, extIndex);
                                        child.Properties["_2_WorkflowLink"] = name;
                                    }
                                    else
                                        child.Properties["_2_WorkflowLink"] = mParentItem.ListItem.Title;
                                }
                            }

                            if (child.Properties.ContainsKey("_0_ContentTypeId") && parentUnit.ParentAssociationUnit.mTaskListUnit.mContentTypeIdMapping != null)
                            {
                                byte[] ctIdBytes = (byte[])child.Properties["_0_ContentTypeId"];
                                string origCTId = LSUtilityOfBytes.LSBytesToHexString(ctIdBytes);
                                string fixupCTIdString = "0x" + origCTId.ToUpper(CultureInfo.InvariantCulture);
                                if (parentUnit.ParentAssociationUnit.mTaskListUnit.mContentTypeIdMapping.ContainsKey(fixupCTIdString))
                                {
                                    byte[] newCTId = LSUtilityOfBytes.LSStringToHexBytes(parentUnit.ParentAssociationUnit.mTaskListUnit.mContentTypeIdMapping[fixupCTIdString].Substring(2));
                                    child.Properties["_0_ContentTypeId"] = newCTId;
                                }
                            }

                            if (!child.Properties.ContainsKey("_0_WorkflowOutcome"))
                                child.Properties.Add("_0_WorkflowOutcome", string.Empty);
                            #endregion

                            IAveListItem item = RestoreSPListItem(parentUnit, child);
                            if (item != null)
                            {
                                RestoreSubscription(parentUnit, child);
                                RestoreCustomSubItem(parentUnit, child);
                                Hashtable conditionParam = new Hashtable();
                                conditionParam.Add("#tp_SiteId", parentUnit.FixupParameters.mSiteIdDic.GetValue(0));
                                conditionParam.Add("#tp_ListId", parentUnit.FixupParameters.mTaskListIdDic.GetValue(0));
                                conditionParam.Add("#tp_Id", item.ID);
                                conditionParam.Add("#tp_UIVersion", item["_UIVersion"]);
                                UpdateTableRow(child.Properties, mExcludeFieldList, conditionParam, "AllUserData", " WHERE tp_SiteId=@tp_SiteId AND tp_ListId=@tp_ListId AND tp_Id=@tp_Id AND tp_UIVersion=@tp_UIVersion AND tp_DeleteTransactionId=0x");
                                UpdateTableNameValuePair(item, parentUnit);
                                RestorePermissionUnit(child.PermissionUnit, item);
                            }
                        }
                    }

                    return assigns;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreTaskItemsException, e.Message);
                    logger.Warn("An exception occurred while restore task items. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreTaskError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreTasks");
                }
            }
        }

        internal int RestoreSubscription(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreSubscription"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreSubscription");
                string monitor = "Restore Subscriptions";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    if (SPWorkflowProcessorRuntime.RestoreHistoryOnly)
                    {
                        return 0;
                    }
                    foreach (SPWorkflowSubItemUnit child in parentItem.ChildUnits)
                    {
                        if (child.ItemType == WorkflowSubItemType.Subscription)
                        {
                            #region Task Item Context
                            if (parentItem.ItemType == WorkflowSubItemType.Task)
                            {
                                Guid oldId = (Guid)child.Properties["#Id"];
                                Guid newId = Guid.NewGuid();
                                Guid siteId = parentUnit.FixupParameters.mSiteIdDic.GetValue(0);
                                Guid webId = parentUnit.FixupParameters.mWebIdDic.GetValue(0);
                                Guid hostId = parentUnit.FixupParameters.mTaskListIdDic.GetValue(0);
                                int itemId = parentUnit.FixupParameters.mTaskItemIdDic[(int)child.Properties["#ItemId"]];
                                byte[] contextCollectionId = parentUnit.FixupParameters.mTaskItemGuidDic[new Guid((byte[])child.Properties["#ContextCollectionId"])].ToByteArray();
                                child.Properties["#Id"] = newId;
                                child.Properties["#SiteId"] = siteId;
                                child.Properties["#WebId"] = webId;
                                child.Properties["#HostId"] = hostId;
                                child.Properties["#ItemId"] = itemId;
                                child.Properties["#ContextCollectionId"] = contextCollectionId;
                                parentUnit.FixupParameters.mSubscriptionIdDic.AddEx(oldId, newId);
                                RestoreCustomSubItem(parentUnit, child);
                                mQueryService.DeleteSpecificEventFromEventReceiver(siteId, webId, hostId, contextCollectionId, child.Properties["#SequenceNumber"]);
                                InsertTableRow(child.Properties, "EventReceivers");
                            }
                            #endregion
                            else if (parentItem.ItemType == WorkflowSubItemType.Instance)
                            {
                                Guid oldId = (Guid)child.Properties["#Id"];
                                Guid newId = Guid.NewGuid();

                                IAveWrapperWorkflowService service = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowService();
                                #region Parent Item Context
                                if (child.Properties.ContainsKey(CustomFieldProfix + "Data") && ((string)child.Properties[CustomFieldProfix + "Data"]).Equals("ParentItemContext"))
                                {
                                    Guid siteId = parentUnit.FixupParameters.mSiteIdDic.GetValue(0);
                                    Guid webId = parentUnit.FixupParameters.mWebIdDic.GetValue(0);
                                    Guid hostId = parentUnit.FixupParameters.mListIdDic.GetValue(0);
                                    int itemId = parentUnit.FixupParameters.mItemIdDic.GetValue(0);
                                    byte[] contextCollectionId = parentUnit.FixupParameters.mItemGuidDic.GetValue(0).ToByteArray();
                                    child.Properties["#Id"] = newId;
                                    child.Properties["#SiteId"] = siteId;
                                    child.Properties["#WebId"] = webId;
                                    child.Properties["#HostId"] = hostId;
                                    child.Properties["#ItemId"] = itemId;
                                    child.Properties["#ContextCollectionId"] = contextCollectionId;
                                    parentUnit.FixupParameters.mSubscriptionIdDic.AddEx(oldId, newId);
                                    RestoreCustomSubItem(parentUnit, child);
                                    mQueryService.DeleteSpecificEventFromEventReceiver(siteId, webId, hostId, contextCollectionId, child.Properties["#SequenceNumber"]);
                                    InsertTableRow(child.Properties, "EventReceivers");
                                    continue;
                                }
                                #endregion

                                #region fixup tasklistid,taskitemGuid,...

                                child.Properties["#Id"] = newId;
                                child.Properties["#SiteId"] = parentUnit.FixupParameters.mSiteIdDic.GetValue(0);
                                child.Properties["#WebId"] = parentUnit.FixupParameters.mWebIdDic.GetValue(0);
                                child.Properties["#ContextCollectionId"] = (parentUnit.FixupParameters.mInstanceIdDic.GetValue(0)).ToByteArray();

                                if (!child.Properties.ContainsKey("#ContextType") || child.Properties["#ContextType"] == null)
                                    continue;

                                Guid contextTypeGuid = new Guid(((byte[])child.Properties["#ContextType"]));
                                Guid origInstanceId = parentUnit.FixupParameters.mInstanceIdDic.GetKey(0);
                                if (contextTypeGuid == service.SharePointServiceGUID)
                                {
                                    child.Properties["#HostId"] = parentUnit.FixupParameters.mListIdDic.GetValue(0);
                                    child.Properties["#ItemId"] = parentUnit.FixupParameters.mItemIdDic.GetValue(0);
                                    if (child.Properties["#ContextId"] != null)
                                    {
                                        Guid temp1 = new Guid((byte[])child.Properties["#ContextId"]);
                                        if (temp1.Equals(origInstanceId))
                                        {
                                            child.Properties["#ContextId"] = (parentUnit.FixupParameters.mInstanceIdDic.GetValue(0)).ToByteArray();
                                        }
                                    }
                                    child.Properties["#ContextObjectId"] = (parentUnit.FixupParameters.mItemGuidDic.GetValue(0)).ToByteArray();
                                }
                                else if (contextTypeGuid == service.TaskServiceGUID)
                                {
                                    child.Properties["#HostId"] = parentUnit.FixupParameters.mTaskListIdDic.GetValue(0);
                                    if (child.Properties.ContainsKey("#ItemId") && parentUnit.FixupParameters.mTaskItemIdDic.ContainsKey((int)child.Properties["#ItemId"]))
                                    {
                                        child.Properties["#ItemId"] = parentUnit.FixupParameters.mTaskItemIdDic[(int)child.Properties["#ItemId"]];
                                    }
                                    child.Properties["#ContextObjectId"] = parentUnit.FixupParameters.mTaskItemGuidDic[new Guid((byte[])child.Properties["#ContextObjectId"])].ToByteArray();
                                }
                                else if (contextTypeGuid == service.ListItemServiceGUID)
                                {
                                    //SP2010 Approval - SharePoint2010
                                    child.Properties["#HostId"] = parentUnit.FixupParameters.mListIdDic.GetValue(0);
                                    child.Properties["#ItemId"] = parentUnit.FixupParameters.mItemIdDic.GetValue(0);
                                    if (child.Properties["#ContextId"] != null)
                                    {
                                        Guid temp1 = new Guid((byte[])child.Properties["#ContextId"]);
                                        if (temp1.Equals(origInstanceId))
                                        {
                                            child.Properties["#ContextId"] = (parentUnit.FixupParameters.mInstanceIdDic.GetValue(0)).ToByteArray();
                                        }
                                    }
                                    child.Properties["#ContextObjectId"] = (parentUnit.FixupParameters.mItemGuidDic.GetValue(0)).ToByteArray();

                                    //throw new SPWFProcessorException(SPWFProcessorErrorCode.ServiceHandlerNotImplement);
                                }
                                else if (contextTypeGuid == service.WorkflowModificationServiceGUID)
                                {
                                    throw new SPWFProcessorException(SPWFProcessorErrorCode.ServiceHandlerNotImplement);
                                }
                                parentUnit.FixupParameters.mSubscriptionIdDic.AddEx(oldId, newId);
                                #endregion
                                RestoreCustomSubItem(parentUnit, child);
                                InsertTableRow(child.Properties, "EventReceivers");
                            }
                        }
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreEventReceiversException, e.Message);
                    logger.Warn("An exception occurred while restore event receivers. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreSubscriptionError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreSubscription");
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Fields of Table Workflow")]
        private int RestoreInstance(SPWFInstanceUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreInstance"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreInstance");
                string monitor = "Restore Instance Self";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    SPWorkflowSubItemUnit instanceItem = parentUnit.InstanceItem;
                    instanceItem.Properties["#Id"] = parentUnit.FixupParameters.mInstanceIdDic.GetValue(0);
                    instanceItem.Properties["#TemplateId"] = parentUnit.FixupParameters.mParentAssoicationIdDic.GetValue(0);
                    instanceItem.Properties["#ListId"] = parentUnit.FixupParameters.mListIdDic.GetValue(0);
                    instanceItem.Properties["#WebId"] = parentUnit.FixupParameters.mWebIdDic.GetValue(0);
                    instanceItem.Properties["#SiteId"] = parentUnit.FixupParameters.mSiteIdDic.GetValue(0);
                    instanceItem.Properties["#TaskListId"] = parentUnit.FixupParameters.mTaskListIdDic.GetValue(0);
                    instanceItem.Properties["#ItemId"] = parentUnit.FixupParameters.mItemIdDic.GetValue(0);
                    instanceItem.Properties["#ItemGUID"] = parentUnit.FixupParameters.mItemGuidDic.GetValue(0);
                    //ADO-92010 设置nintex workflow task reminder action，对应workflow instance的internalState大于maxState，经过以下逻辑处理，internalState值发生改变，导致目的端task reminder设置的暂停时间有误差，故注释。
                    //int internalState = (int)instanceItem.Properties["#InternalState"];
                    //int maxState = ((int)AveWorkflowState.All) + 1;
                    //if (internalState > maxState)
                    //    instanceItem.Properties["#InternalState"] = internalState - maxState;
                    FixupUserIds(instanceItem, null, null);

                    #region Instance Data
                    try
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.IP_HasInstanceData, parentUnit.HasInstanceData.ToString());
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Replace Instance Data");

                        if (parentUnit.HasInstanceData && SPWorkflowProcessorRuntime.RestoreHistoryOnly)
                        {
                            if ((2 == (int)instanceItem.Properties["#Status1"] || (int)instanceItem.Properties["#Status1"] >= 15 && (int)instanceItem.Properties["#InternalState"] == 2))
                            {
                                parentUnit.mExtensionProperties.AddEx("#Old.Status1", instanceItem.Properties["#Status1"]);
                                parentUnit.mExtensionProperties.AddEx("#Old.InternalState", instanceItem.Properties["#InternalState"]);
                                instanceItem.Properties["#Status1"] = 4;
                                instanceItem.Properties["#InternalState"] = 4;
                            }
                            instanceItem.Properties["#InstanceDataSize"] = 0;
                            instanceItem.Properties.Remove("#InstanceData");
                        }

                        if (parentUnit.HasInstanceData)
                        {
                            Dictionary<string, object> dic = parentUnit.GenerateDictionary(parentUnit.ParentAssociationUnit.mCodeBesideAssmMapping);
                            byte[] srcData = (byte[])instanceItem.Properties["#InstanceData"];
                            byte[] dstData = null;
                            if (logger.IsDebugEnabled)
                            {
                                StringBuilder mappingStringBuilder = new StringBuilder();
                                mappingStringBuilder.Append("Workflow Instance Replace Dictionary Data");
                                foreach (var key in dic.Keys)
                                {
                                    mappingStringBuilder.AppendFormat("[{0} --> {1}]", key, dic[key]);
                                }
                                logger.Debug(mappingStringBuilder.ToString());
                            }
                            mPerformanceMonitor.ResetCurrentDuration(monitor);


                            LSBinarySerReplacer.ModifyContentTypeIdEvent += new ModifyContentTypeIdEventHandler(this.OnModifyContentTypeId);
                            LSBinarySerReplacer.ModifyLoginEvent += new ModifyLoginEventHandler(SPPermissionProcessor.OnModifyLogin);
                            LSBinarySerReplacer.ModifyEmailAddressEvent += new ModifyEmailAddressEventHandler(SPWorkflowCommon.OnModifyEmailAddress);
                            if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
                            {
                                LSBinarySerReplacer.ModifyListIdEvent += new ModifyListIdEventHandler(this.OnModifyListId);
                            }

                            if (LSBinarySerReplacer.ExecuteWithCompress(srcData, dic, out dstData, parentUnit.ParentAssociationUnit.SPAssociation.CompressInstanceData))
                            {
                                instanceItem.Properties["#InstanceData"] = dstData;
                                instanceItem.Properties["#InstanceDataSize"] = dstData.Length;
                            }
                            else
                                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceDataReplacerInternalError);
                            mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", "Replace Instance Data. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                        }
                    }
                    catch (SPWFProcessorException procException)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.IP_ReplaceInstanceDataException, procException.Message);
                        logger.Warn("An exception occurred while replace instance data. exception:{0}", procException.ToString());
                        throw;
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.IP_ReplaceInstanceDataException, e.Message);
                        logger.Warn("An exception occurred while replace instance data. exception:{0}", e.ToString());
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceDataReplaceError, e);
                    }
                    finally
                    {
                        LSBinarySerReplacer.ModifyContentTypeIdEvent -= new ModifyContentTypeIdEventHandler(this.OnModifyContentTypeId);
                        LSBinarySerReplacer.ModifyLoginEvent -= new ModifyLoginEventHandler(SPPermissionProcessor.OnModifyLogin);
                        LSBinarySerReplacer.ModifyEmailAddressEvent -= new ModifyEmailAddressEventHandler(SPWorkflowCommon.OnModifyEmailAddress);
                        LSBinarySerReplacer.ModifyListIdEvent -= new ModifyListIdEventHandler(this.OnModifyListId);
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Replace Instance Data");
                    }
                    #endregion

                    if (mInstance == null)
                    {
                        InsertTableRow(instanceItem.Properties, "Workflow");
                    }
                    else
                    {

                        #region Update Workflow Properties
                        List<string> excludeParam = new List<string>();
                        excludeParam = new List<string>();
                        excludeParam.Add("#id");
                        excludeParam.Add("#templateid");
                        excludeParam.Add("#listid");
                        excludeParam.Add("#webid");
                        excludeParam.Add("#siteid");
                        excludeParam.Add("#tasklistid");
                        excludeParam.Add("#itemid");
                        excludeParam.Add("#itemguid");

                        Hashtable conditionParam = new Hashtable();
                        conditionParam.Add("#SiteId", instanceItem.Properties["#SiteId"]);
                        conditionParam.Add("#WebId", instanceItem.Properties["#WebId"]);
                        conditionParam.Add("#ListId", instanceItem.Properties["#ListId"]);
                        conditionParam.Add("#Id", instanceItem.Properties["#Id"]);

                        UpdateTableRow(instanceItem.Properties, excludeParam, conditionParam, "Workflow", " Where SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND Id=@Id");
                        #endregion
                    }

                    #region Prepare The Correct InstanceId

                    Dictionary<String, object> workflowInfoForUpdate = null;
                    string judgeBy = string.Empty;
                    try
                    {
                        bool hasRunningInstance = false;
                        List<Dictionary<String, object>> workflowInfos = mQueryService.TryGetWorkflowInfo(mParentItem.SiteID, mParentItem.WebID, mParentItem.ListID, mParentItem.ItemID, parentUnit.ParentAssociationUnit.Id, out hasRunningInstance);

                        judgeBy = "#Created";
                        if (!hasRunningInstance)
                        {
                            judgeBy = "#Modified";
                        }

                        foreach (Dictionary<String, object> workflowInfo in workflowInfos)
                        {
                            //如果目的端有running instance, 则只和目的端的running instance做比较。
                            if (hasRunningInstance && !(2 == (int)workflowInfo["#Status1"] || (int)workflowInfo["#Status1"] >= 15 && (int)workflowInfo["#InternalState"] == 2))
                            {
                                continue;
                            }
                            if (workflowInfoForUpdate == null || (DateTime)workflowInfoForUpdate[judgeBy] < (DateTime)workflowInfo[judgeBy])
                            {
                                workflowInfoForUpdate = workflowInfo;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.RestoreWFInUnexpectError, e.ToString());
                        workflowInfoForUpdate = null;
                    }

                    #endregion

                    #region Update Status Field
                    if (parentUnit.ParentAssociationUnit.ParentObjectType != SPWFAssociationParentType.Web)
                    {
                        //this.DefaultView.GetLookupFieldCount() >= this.ParentWeb.Site.WebApplication.MaxQueryLookupFields  超过阀值不显示wf column，SP 也没有对应的column 给wf，因此也不需要更新这个值。
                        if (!string.IsNullOrEmpty(parentUnit.StatusFieldColName))
                        {
                            if (string.IsNullOrEmpty(parentUnit.StatusFieldRowOrdinal))
                            {
                                parentUnit.StatusFieldRowOrdinal = "0";
                            }
                            mQueryService.UpdateWorkflowStatusFieldValue(parentUnit.FixupParameters.mSiteIdDic.GetValue(0), parentUnit.FixupParameters.mListIdDic.GetValue(0), parentUnit.FixupParameters.mItemGuidDic.GetValue(0), parentUnit.FixupParameters.mItemIdDic.GetValue(0), workflowInfoForUpdate == null ? (parentUnit.FixupParameters.mInstanceIdDic.GetValue(0)).ToByteArray() : ((Guid)workflowInfoForUpdate["#Id"]).ToByteArray(), short.Parse(parentUnit.StatusFieldRowOrdinal), parentUnit.StatusFieldColName);
                        }
                    }
                    #endregion

                    #region Recalculate Running Instance Count
                    if (parentUnit.ParentAssociationUnit.SPAssociation != null)
                    {
                        AveWorkflowRunningInstanceRecalculationService.AddAssociationToCache((Guid)instanceItem.Properties["#SiteId"], (Guid)instanceItem.Properties["#WebId"], (Guid)instanceItem.Properties["#ListId"], parentUnit.ParentAssociationUnit.SPAssociation.ID, parentUnit.ParentAssociationUnit.SPAssociation.Name);
                    }
                    #endregion
                    RestoreCustomSubItem(parentUnit, instanceItem);
                    return 0;
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceSelfException, procException.Message);
                    logger.Warn("An exception occurred while restore instance self. exception:{0}", procException.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceSelfException, e.Message);
                    logger.Warn("An exception occurred while restore instance self. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreSelfError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreInstance");
                }
            }
        }

        internal int RestoreHistory(SPWFInstanceUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreHistory"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreHistory");
                string monitor = "Restore Histories";
                SPWorkflowSubItemUnit instanceUnit = parentUnit.InstanceItem;
                if (instanceUnit == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceUnitIsNull);
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    foreach (SPWorkflowSubItemUnit child in instanceUnit.ChildUnits)
                    {
                        if (child.ItemType == WorkflowSubItemType.History)
                        {

                            FixupUserIds(child, null, null);
                            if (child.Properties.ContainsKey("_0_WorkflowInstance"))
                                child.Properties["_0_WorkflowInstance"] = parentUnit.WFInternalPlatform == SPWFInternalPlatform.WF2010PlatformType ? parentUnit.FixupParameters.mInstanceIdDic.GetValue(0).ToString("B") : parentUnit.FixupParameters.mInstanceIdDic.GetValue(0).ToString().Trim(new char[] { '{', '}' });
                            if (child.Properties.ContainsKey("_0_WorkflowAssociation"))
                                child.Properties["_0_WorkflowAssociation"] = parentUnit.WFInternalPlatform == SPWFInternalPlatform.WF2010PlatformType ? parentUnit.FixupParameters.mParentAssoicationIdDic.GetValue(0).ToString("B") : parentUnit.FixupParameters.mParentAssoicationIdDic.GetValue(0).ToString().Trim(new char[] { '{', '}' });
                            if (child.Properties.ContainsKey("_0_WorkflowTemplate"))
                                child.Properties["_0_WorkflowTemplate"] = parentUnit.FixupParameters.mParentAssociationBaseIdDic.GetValue(0).ToString("B");
                            if (child.Properties.ContainsKey("_0_List"))
                                child.Properties["_0_List"] = parentUnit.FixupParameters.mListIdDic.GetValue(0).ToString("B");
                            if (child.Properties.ContainsKey("_0_Item"))
                                child.Properties["_0_Item"] = parentUnit.FixupParameters.mItemIdDic.GetValue(0);
                            if (child.Properties.ContainsKey("_0_Description"))
                            {
                                string value = (string)child.Properties["_0_Description"];
                                string sourceParentWebUrl = parentUnit.ParentAssociationUnit.mHistListUnit.SerializableData.mParentWebUrl;
                                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(sourceParentWebUrl) && value.IndexOf(sourceParentWebUrl, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    child.Properties["_0_Description"] = value.Replace(parentUnit.ParentAssociationUnit.mHistListUnit.SerializableData.mParentWebUrl, ParentItem.Web.Url);
                                }
                            }


                            IAveListItem item = RestoreSPListItem(parentUnit, child);
                            if (item != null)
                            {
                                RestoreCustomSubItem(parentUnit, child);
                                Hashtable conditionParam = new Hashtable();
                                conditionParam.Add("#tp_SiteId", parentUnit.FixupParameters.mSiteIdDic.GetValue(0));
                                conditionParam.Add("#tp_ListId", parentUnit.FixupParameters.mHistoryListIdDic.GetValue(0));
                                conditionParam.Add("#tp_Id", item.ID);
                                conditionParam.Add("#tp_UIVersion", item["_UIVersion"]);
                                UpdateTableRow(child.Properties, mExcludeFieldList, conditionParam, "AllUserData", " WHERE tp_SiteId=@tp_SiteId AND tp_ListId=@tp_ListId AND tp_Id=@tp_Id AND tp_UIVersion=@tp_UIVersion AND tp_DeleteTransactionId=0x");
                            }
                        }
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreHistoriesException, e.Message);
                    logger.Warn("An exception occurred while restore history items. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreHistoryError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreHistory");
                }
            }
        }

        private int RestoreCustomSubItem(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreCustomSubItem"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreCustomSubItem");
                string monitor = "Restore Custom Data";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    switch (parentItem.ItemType)
                    {
                        case WorkflowSubItemType.Task:
                        case WorkflowSubItemType.Schedule:
                            CustomWorkflowInstanceProc customTaskProc = new CustomWorkflowInstanceProc(CustomProcessors);
                            customTaskProc.FireRestoreCustomWorkflowDataEvent(parentUnit, parentItem);
                            break;
                        case WorkflowSubItemType.Subscription:
                            break;
                        case WorkflowSubItemType.Instance:
                            CustomWorkflowInstanceProc customInstanceProc = new CustomWorkflowInstanceProc(CustomProcessors);
                            customInstanceProc.FireResetData(parentUnit);
                            break;
                        case WorkflowSubItemType.History:
                            break;
                        case WorkflowSubItemType.Custom:
                            break;
                        case WorkflowSubItemType.Invalid:
                        default:
                            throw new NotSupportedException("Not supported");
                    }
                    return 0;
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreCustomUnitsException, procException.Message);
                    logger.Warn("An exception occurred while restore custom units. exception:{0}", procException.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreCustomUnitsException, e.Message);
                    logger.Warn("An exception occurred while restore custom units. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreCustomDataError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreCustomSubItem");
                }
            }
        }

        private int RestoreCustomSubItems(SPWFInstanceUnit parentUnit)
        {
            using (AvePerformanceScope pf1 = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreCustomSubItems"))
            {
                foreach (SPWorkflowSubItemUnit child in parentUnit.InstanceItem.ChildUnits)
                {
                    if (child.ItemType == WorkflowSubItemType.Custom)
                    {
                        CustomWorkflowInstanceProc customTaskProc = new CustomWorkflowInstanceProc(CustomProcessors);
                        customTaskProc.FireRestoreCustomWorkflowDataEvent(parentUnit, parentUnit.InstanceItem);
                    }
                }
                return 0;
            }
        }

        private int ProcessWorkflowStatus(SPWFInstanceUnit unit, List<Assigns> assigns)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.ProcessWorkflowStatus"))
            {
                int status1 = 0;
                int internalState = 0;
                if (unit.mExtensionProperties.ContainsKey("#Old.Status1") && unit.mExtensionProperties.ContainsKey("#Old.InternalState"))
                {
                    status1 = (int)unit.mExtensionProperties["#Old.Status1"];
                    internalState = (int)unit.mExtensionProperties["#Old.InternalState"];
                }
                if (SPWorkflowProcessorRuntime.RestoreHistoryOnly
                    && (2 == status1 || status1 >= 15 && internalState == 2))
                {
                    //不使用API的方式cancel workflow, 因为这样会直接导致workflow task丢失,与产品逻辑不符，在RestoreInstance的方法中，在更新workflow instance之前，将状态变成canceled状态, 而且对于canceled掉的workflow instance也不用处理instanceData,效率也有所提升。
                    //if (mInstance != null) 
                    //{
                    //    mWFManager.CancelWorkflow(mInstance);
                    //}
                    if (unit.RestartRunningInstance)
                    {
                        logger.Debug("Restart workflow instance");
                        //ADO-101833,ADO-112763使用同步方式启动workflow instance，在Nintex workflow上出现hang的问题，使用异步方式有时候出现instance没有完全启动的问题，权衡考虑对build inworkflow采用同步方式，非build使用异步方式。
                        if (unit.ParentAssociationUnit.IsBuiltinBaseId)
                        {
                            CreateWorkflow(unit, AveWorkflowRunOptions.Synchronous, assigns);
                        }
                        else
                        {
                            CreateWorkflow(unit, AveWorkflowRunOptions.Asynchronous, assigns);
                        }
                    }
                    return 0;
                }
                return 1;
            }
        }

        private IAveListItem RestoreSPListItem(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestoreSPListItem"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreSPListItem");
                string monitor = "Restore SharePoint List Item";
                //[ADO-69036]fix performance issue.  using (IAveSite site = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite(parentUnit.FixupParameters.mSiteIdDic.GetValue(0)))
                IAveSite site = parentUnit.ParentAssociationUnit.ParentWeb.Site;
                {
                    using (IAveWeb web = site.OpenWeb(parentUnit.FixupParameters.mWebIdDic.GetValue(0)))
                    {
                        Guid listId = Guid.Empty;
                        SPFieldProcessor fieldProcessor = null;
                        switch (parentItem.ItemType)
                        {
                            case WorkflowSubItemType.Task:
                                listId = parentUnit.FixupParameters.mTaskListIdDic.GetValue(0);
                                fieldProcessor = parentUnit.ParentAssociationUnit.mTaskListUnit.FieldProcessor;
                                break;
                            case WorkflowSubItemType.History:
                                listId = parentUnit.FixupParameters.mHistoryListIdDic.GetValue(0);
                                fieldProcessor = parentUnit.ParentAssociationUnit.mHistListUnit.FieldProcessor;
                                break;
                            default:
                                return null;
                        }
                        try
                        {
                            mPerformanceMonitor.StartMonitor(monitor);
                            IAveList list = web.Lists[listId];
                            Guid origUniqueId = (Guid)parentItem.Properties["_0_GUID"];
                            if (parentItem.Properties.ContainsKey("_0_" + SPWorkflowCommon.OriginalUniqueIdFieldName))
                                origUniqueId = new Guid(parentItem.Properties["_0_" + SPWorkflowCommon.OriginalUniqueIdFieldName].ToString());
                            bool isAppend = false;
                            if (isAppend)
                            {
                                if (TaskItemGuidMapping.ContainsKey(origUniqueId))
                                {
                                    origUniqueId = TaskItemGuidMapping[origUniqueId];
                                }
                                else
                                {
                                    Guid temp = Guid.NewGuid();
                                    TaskItemGuidMapping.AddEx(origUniqueId, temp);
                                    origUniqueId = temp;
                                }
                            }
                            IAveListItem item = list.GetItemByOriginalUniqueId(origUniqueId);

                            if (item != null && !TaskItemGuidMapping.ContainsKey(origUniqueId))
                            {
                                item.Delete();
                                item = null;
                            }

                            if (item == null)
                            {
                                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "ConvertPropsToDBField");
                                fieldProcessor.ConvertPropsToDBField(parentItem.Properties, null, list, true);
                                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "ConvertPropsToDBField");
                                item = list.Items.Add();
                                foreach (DictionaryEntry de in parentItem.Properties)
                                {
                                    string key = de.Key.ToString();
                                    if (!key.StartsWith("_", StringComparison.Ordinal))
                                        continue;
                                    string name = key.Substring(1);
                                    item[name] = de.Value;
                                }
                                try
                                {
                                    try
                                    {
                                        item[SPWorkflowCommon.OriginalUniqueIdFieldName] = ((Guid)parentItem.Properties["#tp_GUID"]).ToString("B");
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemUniqueIdError, e.ToString());
                                    }//no need to log
                                    item.SystemUpdate();
                                }
                                catch (Exception e)
                                {
                                    throw new SPWFProcessorException(SPWFProcessorErrorCode.CreateSPListItemError, e);
                                }
                            }
                            else
                            {
                                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "ConvertPropsToDBField");
                                fieldProcessor.ConvertPropsToDBField(parentItem.Properties, null, list, false);
                                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "ConvertPropsToDBField");
                                int uiVersion = (int)parentItem.Properties["#tp_UIVersion"];
                                IAveListItemVersionCollection versions = item.Versions;
                                IAveListItemVersion v = versions.GetVersionFromID(uiVersion);
                                if (v == null)
                                {
                                    item.Update();
                                }

                                try
                                {
                                    item["Workflow List ID"] = parentUnit.FixupParameters.mListIdDic.GetValue(0);
                                    item.SystemUpdate(false);
                                }
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetListWFIdError, e.ToString());
                                }
                            }
                            if (!TaskItemGuidMapping.ContainsKey(origUniqueId))
                            {
                                TaskItemGuidMapping.AddEx(origUniqueId, origUniqueId);
                            }
                            parentUnit.FixupParameters.mTaskItemIdDic.AddEx((int)parentItem.Properties["#tp_Id"], item.ID);
                            parentUnit.FixupParameters.mTaskItemGuidDic.AddEx((Guid)parentItem.Properties["#tp_GUID"], new Guid(item["GUID"].ToString()));

                            parentUnit.FixupParameters.mLastTaskItemGuidDic.Clear();
                            parentUnit.FixupParameters.mLastTaskItemIdDic.Clear();
                            parentUnit.FixupParameters.mLastTaskItemGuidDic.AddEx((Guid)parentItem.Properties["#tp_GUID"], new Guid(item["GUID"].ToString()));
                            parentUnit.FixupParameters.mLastTaskItemIdDic.AddEx((int)parentItem.Properties["#tp_Id"], item.ID);

                            return item;
                        }
                        catch (SPWFProcessorException procException)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_CreateSPItemException, procException.Message);
                            logger.Warn("An exception occurred while create item. exception:{0}", procException.ToString());
                            throw;
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_CreateSPItemException, e.Message);
                            logger.Warn("An exception occurred while create item. exception:{0}", e.ToString());
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.CreateSPListItemUnknowError, e);
                        }
                        finally
                        {
                            mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            mPerformanceMonitor.RemoveMonitor(monitor);
                            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreSPListItem");
                        }
                    }
                }
            }
        }

        private void RestorePermissionUnit(SPPermissionUnit permUnit, IAveSecurableObject parent)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance.InstanceRestore.RestorePermissionUnit"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestorePermissionUnit");
                string monitor = "Restore Permission";
                if (permUnit == null)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_MissingPermissions);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestorePermissionUnit");
                    return;
                }
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    using (SPPermissionProcessor permProc = SPPermissionProcessor.CreateInstance(SPPermissionScope.Item, parent))
                    {
                        permProc.RestorePermissionUnit(permUnit);
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestorePermissionsException, procException.Message);
                    logger.Warn("An exception occurred while restore permissions. exception:{0}", procException.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestorePermissionsException, e.Message);
                    logger.Warn("An exception occurred while restore permissions. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionUnitRestoreException, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestorePermissionUnit");
                }
            }
        }

        private int UpdateTableNameValuePair(IAveListItem item, SPWFInstanceUnit parentUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "UpdateTableNameValuePair");
            try
            {
                var siteId = parentUnit.FixupParameters.mSiteIdDic.GetValue(0);
                var listId = parentUnit.FixupParameters.mTaskListIdDic.GetValue(0);
                var instanceId = parentUnit.FixupParameters.mInstanceIdDic.GetValue(0);
                var fieldId = item.Fields.GetFieldByInternalName("WorkflowInstanceID").ID;
                var level = (int)item.Level;
                return mQueryService.UpdateTableNameValuePairForWFInstance(siteId, listId, item.ID, instanceId, fieldId, level);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_UpdateException, e.Message);
                logger.Warn("An exception occurred while update table name value pair. exception:{0}", e);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.UpdateTableNameValuePair, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "UpdateTableRow");
            }
        }

        private int UpdateTableRow(Hashtable metadata, List<string> excludeField, Hashtable conditionParam, string tableName, string condition)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "UpdateTableRow");
            try
            {
                return mQueryService.UpdateTableRow(metadata, excludeField, conditionParam, tableName, condition);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_UpdateException, e.Message);
                logger.Warn("An exception occurred while update table row. exception:{0}", e.ToString());
                throw new SPWFProcessorException(SPWFProcessorErrorCode.UpdateItemMetaDataError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "UpdateTableRow");
            }
        }

        private void InsertTableRow(Hashtable data, string tableName)
        {
            try
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "InsertTableRow");
                mQueryService.InsertTableRow(data, tableName);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_InsertException, e.Message);
                logger.Warn("An exception occurred while insert table row. exception:{0}", e.ToString());
                throw;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InsertTableRow");
            }
        }

        //private string BuildInsertCmdText(Hashtable ht, string tableName)
        //{
        //    StringBuilder cmdText = new StringBuilder();

        //    cmdText.Append("INSERT INTO ");
        //    cmdText.Append(tableName);
        //    cmdText.Append(" (");

        //    bool isFirstItem = true;

        //string key;
        //foreach (string tempKey in ht.Keys)
        //{
        //    if (tempKey.StartsWith("#", StringComparison.Ordinal))
        //    {
        //        key = tempKey.Substring(1);

        //            if (isFirstItem)
        //            {
        //                isFirstItem = false;
        //                cmdText.Append(key);
        //            }
        //            else
        //            {
        //                cmdText.Append(", ");
        //                cmdText.Append(key);
        //            }
        //        }
        //    }

        //    cmdText.Append(") VALUES (");

        //    isFirstItem = true;

        //    foreach (DictionaryEntry entry in ht)
        //    {
        //        string tempKey = (string)entry.Key;

        //if (tempKey.StartsWith("#", StringComparison.Ordinal))
        //{
        //    key = tempKey.Substring(1);

        //            if (isFirstItem)
        //            {
        //                isFirstItem = false;
        //                if (entry.Value == null)
        //                {
        //                    cmdText.Append("NULL");
        //                }
        //                else
        //                {
        //                    cmdText.Append("@");
        //                    cmdText.Append(key);
        //                }
        //            }
        //            else
        //            {
        //                if (entry.Value == null)
        //                {
        //                    cmdText.Append(", NULL");
        //                }
        //                else
        //                {
        //                    cmdText.Append(", @");
        //                    cmdText.Append(key);
        //                }
        //            }
        //        }
        //    }

        //    cmdText.Append(")");
        //    return cmdText.ToString();
        //}

        //private void SetParameters(Hashtable ht)
        //{
        //    mCmd.Parameters.Clear();

        //foreach (DictionaryEntry entry in ht)
        //{
        //    string key = (string)entry.Key;
        //    object value = entry.Value;
        //    if (value == null)
        //        continue;
        //    if (key.StartsWith("#", StringComparison.Ordinal))
        //    {
        //        key = key.Substring(1);

        //            mCmd.Parameters.AddWithValue("@" + key, value);
        //        }
        //    }
        //}

        //private void ExecuteCmdText(string cmdText)
        //{
        //    try
        //    {
        //        mCmd.CommandText = cmdText;
        //        mCmd.ExecuteNonQuery();
        //    }
        //    catch (SqlException sqlex)
        //    {
        //        if (sqlex.Number == 1205)
        //        {//当前的是死锁
        //            int retryCount = 3;
        //            while (retryCount > 0)
        //            {
        //                #region //重试当前操作3次。
        //                try
        //                {
        //                    //mLog.Warn("start deadlock retry:" + retryCount);
        //                    System.Threading.Thread.Sleep(1000);
        //                    mCmd.ExecuteNonQuery();
        //                    break;
        //                }
        //                catch (SqlException retryEX)
        //                {
        //                    if (retryEX.Number == 1205)
        //                    {
        //                        retryCount--;//仍然冲突重新作。
        //                        //mLog.Warn("deadlock retry:" + retryCount, retryEX);
        //                    }
        //                    else
        //                    {
        //                        //不是死锁的错误，那么直接退出，将异常抛出。
        //                        throw retryEX;
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw ex;//不是死锁的错误，那么直接退出，将异常抛出。
        //                }
        //                #endregion
        //            }
        //        }
        //        throw sqlex;
        //    }
        //    catch (Exception ex)
        //    {
        //        //mLog.Log(AveLogSeverity.Warn, "IM07nection00330", ex.Message, ex.StackTrace);
        //        throw ex;
        //    }
        //}


        private string OnModifyListId(object sender, string origValue)
        {
            string newValue = origValue;
            if (SPWorkflowCommon.StringIsGUIDFormat(origValue))
            {
                Guid temp = new Guid(origValue);
                if (this.mInstanceUnit.ParentAssociationUnit.mAllGUIDInTemplate.ContainsKey(temp))
                {
                    if (origValue.StartsWith("{", StringComparison.Ordinal))
                    {
                        newValue = this.mInstanceUnit.ParentAssociationUnit.mAllGUIDInTemplate[temp].ToString("B").ToUpper(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        newValue = this.mInstanceUnit.ParentAssociationUnit.mAllGUIDInTemplate[temp].ToString();
                    }
                }
            }
            else
            {
                Regex guidRE = new Regex(AveRegexCommon.GUIDREG, RegexOptions.IgnoreCase);
                //the GUID include siteId, webId, listId
                MatchCollection guids = guidRE.Matches(origValue);
                foreach (Match m in guids)
                {
                    Guid id = new Guid(m.Value);
                    if (this.mInstanceUnit.ParentAssociationUnit.mAllGUIDInTemplate.ContainsKey(id))
                    {
                        Guid newId = this.mInstanceUnit.ParentAssociationUnit.mAllGUIDInTemplate[id];
                        newValue = newValue.Replace(id.ToString().ToUpper(CultureInfo.InvariantCulture), newId.ToString().ToUpper(CultureInfo.InvariantCulture));
                        newValue = newValue.Replace(id.ToString().ToLower(CultureInfo.InvariantCulture), newId.ToString().ToLower(CultureInfo.InvariantCulture));
                    }
                }
            }
            return newValue;
        }

        private string OnModifyContentTypeId(object sender, string origValue)
        {
            string newValue = origValue;

            if (this.mInstanceUnit.ParentAssociationUnit.mTaskListUnit.mContentTypeIdMapping.ContainsKey(origValue))
            {
                newValue = this.mInstanceUnit.ParentAssociationUnit.mTaskListUnit.mContentTypeIdMapping[origValue];
            }
            return newValue;
        }

        #endregion
    }

    internal sealed class SPWFInstanceProcNative13Model : SPWFInstanceProc
    {
        private SPWFInstanceProcNative mSPWFInstanceProcNative10Model = new SPWFInstanceProcNative();
        public IAveWorkflowInstanceService WFInstanceService
        {
            get
            {
                return Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).WFInstanceService;
            }
        }

        public IAveWorkflowSubscriptionService WFSubscriptionService
        {
            get
            {
                return Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { ParentWeb }).WFSubscriptionService;
            }
        }

        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string CustomFieldProfix = "LS.";

        private List<string> mExcludeFieldList;

        private SPWFInstanceUnit mInstanceUnit;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Fields of Table Workflow")]
        public SPWFInstanceProcNative13Model()
        {
            mExcludeFieldList = new List<string>();
            mExcludeFieldList.Add("#tp_id");
            mExcludeFieldList.Add("#tp_listid");
            mExcludeFieldList.Add("#tp_siteid");
            mExcludeFieldList.Add("#tp_rowordinal");
            mExcludeFieldList.Add("#tp_version");
            mExcludeFieldList.Add("#tp_ordering");
            mExcludeFieldList.Add("#tp_threadindex");
            mExcludeFieldList.Add("#tp_moderationstatus");
            mExcludeFieldList.Add("#tp_iscurrent");
            mExcludeFieldList.Add("#tp_itemorder");
            mExcludeFieldList.Add("#tp_instanceid");
            mExcludeFieldList.Add("#tp_guid");
            mExcludeFieldList.Add("#tp_dirname");
            mExcludeFieldList.Add("#tp_leafname");
            //mExcludeFieldList.Add("#uniqueidentifier1");
            mExcludeFieldList.Add("#tp_level");
            mExcludeFieldList.Add("#tp_iscurrentversion");
            //mExcludeFieldList.Add("#tp_uiversion");
            mExcludeFieldList.Add("#tp_calculatedversion");
            mExcludeFieldList.Add("#tp_uiversionstring");
            mExcludeFieldList.Add("#tp_parentid");
            mExcludeFieldList.Add("#tp_docid");
        }

        private void GetHistoryColNames(string schema, out string instanceIdCol, out string associationIdCol, out string parentListIdCol, out string baseIdCol, out string itemIdCol)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetHistoryColNames");
            instanceIdCol = string.Empty;
            associationIdCol = string.Empty;
            parentListIdCol = string.Empty;
            baseIdCol = string.Empty;
            itemIdCol = string.Empty;

            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.LoadXml(schema);

                XmlNode instanceNode = doc.SelectSingleNode("/Fields/Field[@StaticName='WorkflowInstance'][@Type='Text']");
                instanceIdCol = instanceNode.Attributes["ColName"].Value;

                XmlNode assoNode = doc.SelectSingleNode("/Fields/Field[@StaticName='WorkflowAssociation'][@Type='Text']");
                associationIdCol = assoNode.Attributes["ColName"].Value;

                XmlNode baseIdNode = doc.SelectSingleNode("/Fields/Field[@StaticName='WorkflowTemplate'][@Type='Text']");
                baseIdCol = baseIdNode.Attributes["ColName"].Value;

                XmlNode listIdNode = doc.SelectSingleNode("/Fields/Field[@StaticName='List'][@Type='Text']");
                parentListIdCol = listIdNode.Attributes["ColName"].Value;

                XmlNode itemIdNode = doc.SelectSingleNode("/Fields/Field[@StaticName='Item'][@Type='Integer']");
                itemIdCol = itemIdNode.Attributes["ColName"].Value;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_GetHistoryFieldException, e.Message);
                logger.Warn("An exception occurred while get history field. exception:{0}", e.ToString());
                throw new SPWFProcessorException(SPWFProcessorErrorCode.GetHistoryFieldsError, e);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetHistoryColNames");
            }

        }


        #region ************************Backup  Region************************

        public override void SetInstanceProcParameters(InstanceProcCreationParam param)
        {
            mSPWFInstanceProcNative10Model.ParentItem = this.ParentItem;
            mSPWFInstanceProcNative10Model.SetInstanceProcParameters(param);
            base.SetInstanceProcParameters(param);
        }

        public override List<byte[]> Backup()
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance13Model.InstanceMainBackup"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "InstanceMainBackup");

                string monitor = mMainMonitorLog = "Instance Backup";

                List<byte[]> rlt = new List<byte[]>();
                List<Guid> tempIds = new List<Guid>();
                //Set parameters for 10model processor.
                mSPWFInstanceProcNative10Model.ParentItem = this.ParentItem;
                try
                {
                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    mPerformanceMonitor.StartMonitor(monitor);
                    #endregion
                    //mQueryService.GetWorkflowId(tempIds, mParentItem.SiteID, mParentItem.WebID, mParentItem.ItemID, mParentItem.ListID);
                    logger.Log(AveLogLevel.DEBUG, "Begin to backup workflow ids by native for 13 model");
                    IAveWorkflowInstanceCollection aveWFInstanceCollection = WFInstanceService.EnumerateInstancesForListItem(mParentItem.ListID, mParentItem.ItemID);
                    if (aveWFInstanceCollection == null)
                    {
                        return rlt;
                    }
                    List<IAveWorkflowInstance> workflowInstanceCollSort = new List<IAveWorkflowInstance>();
                    foreach (IAveWorkflowInstance instance in aveWFInstanceCollection)
                    {
                        workflowInstanceCollSort.Add(instance);
                    }
                    workflowInstanceCollSort.Sort(new SPWorkflowInstance13ModelComparer());
                    foreach (IAveWorkflowInstance aveWFInstance in workflowInstanceCollSort)
                    {
                        tempIds.Add(aveWFInstance.Id);
                    }
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_GetItemInstanceException, e.Message);
                    logger.Warn("An exception occurred while get item instance. exception:{0}", e.ToString());
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceMainBackup");
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.GetWorkflowInstanceError, e);
                }
                #region Performance Monitor Region
                mPerformanceMonitor.StopMonitor(monitor);
                mPerformanceMonitor.WriteMonitorLog(monitor, " Get Instance Count: ", tempIds.Count, ". Duration: ", mPerformanceMonitor.GetDuration(monitor));
                #endregion

                SPWorkflowProcessorRuntime.Log(Logs.IP_InstanceCount, tempIds.Count.ToString());
                foreach (Guid instanceId in tempIds)
                {
                    using (AvePerformanceScope pf1 = new AvePerformanceScope("BackupWorkflowInstance13Model.BackupOneInstance"))
                    {
                        #region Performance Monitor Region
                        mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                        mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + instanceId);
                        mPerformanceMonitor.StartMonitor(monitor);
                        #endregion
                        logger.Log(AveLogLevel.DEBUG, "Begin to assemble workflow instance unit for 13 model one by one, instance id: {0}", instanceId.ToString());
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupOneInstance");
                        mInnerExceptions = new List<SPWFProcessorException>();
                        try
                        {
                            SPWorkflowSubItemUnit instanceItemUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Instance);
                            instanceItemUnit.Properties.AddEx("#Id", instanceId);
                            if (SPWorkflowProcessorRuntime.Process13ModelWFInstanceByNative)
                            {
                                BackupInstanceSelf(instanceItemUnit);
                            }
                            else
                            {
                                BackupInstanceSelfByAPI(instanceItemUnit);
                            }
                            this.mSPWFInstanceProcNative10Model.BackupTasks(instanceItemUnit);
                            this.mSPWFInstanceProcNative10Model.BackupSubscription(instanceItemUnit);
                            this.mSPWFInstanceProcNative10Model.BackupHistory(instanceItemUnit);
                            //BackupCustomUnit(instanceItemUnit);

                            #region Performance Monitor Region
                            mPerformanceMonitor.ResetCurrentDuration(monitor);
                            #endregion

                            SPWFInstanceUnit instanceUnit = new SPWFInstanceUnit();
                            instanceUnit.InstanceItem = instanceItemUnit;
                            byte[] data = SPWFInstanceUnit.Save(instanceUnit);
                            instanceItemUnit.Dispose();

                            #region Performance Monitor Region
                            mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Serialize Instance Unit. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                            #endregion

                            rlt.Add(data);
                        }
                        catch (SPWFProcessorException procException)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceException, instanceId.ToString(), procException.Message);
                            logger.Warn("An exception occurred while backup instance. exception:{0}", procException.ToString());
                            mInnerExceptions.Add(procException);
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceException, instanceId.ToString(), e.Message);
                            logger.Warn("An exception occurred while backup instance. exception:{0}", e.ToString());
                            mInnerExceptions.Add(new SPWFProcessorException(SPWFProcessorErrorCode.InstanceUnknownError, e, instanceId));
                        }
                        finally
                        {
                            #region Performance Monitor Region
                            mPerformanceMonitor.StopMonitor(monitor);
                            mPerformanceMonitor.WriteMonitorLog(monitor, " finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                            mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                            #endregion
                            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupOneInstance");
                        }
                    }

                    if (mInnerExceptions.Count > 0)
                        mExceptions.Add(instanceId, mInnerExceptions);
                }

                #region Performance Monitor Region
                mPerformanceMonitor.RemoveMonitor(monitor);
                #endregion

                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceMainBackup");
                return rlt;
            }
        }

        private void BackupInstanceSelf(SPWorkflowSubItemUnit instanceUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupInstanceSelf");
            string monitor = "Backup Instance";
            try
            {
                mPerformanceMonitor.StartMonitor(monitor);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceSelfException, e.Message);
                logger.Warn("An exception occurred while backup instance self. exception:{0}", e.ToString());
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupSelfError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupInstanceSelf");
            }
        }

        private void BackupInstanceSelfByAPI(SPWorkflowSubItemUnit instanceUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupWorkflowInstance13Model.BackupOneInstance.BackupInstanceSelfByAPI"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupInstanceSelfByAPI");
                string monitor = "Backup Instance";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    IAveWorkflowInstance workflowInstance = WFInstanceService.GetInstance((Guid)instanceUnit.Properties["#Id"]);
                    instanceUnit.Properties.AddEx("Props.13Model", new Hashtable());
                    instanceUnit.Properties.AddEx("FaultInfo", workflowInstance.FaultInfo);
                    instanceUnit.Properties.AddEx("InstanceCreated", workflowInstance.InstanceCreated);
                    instanceUnit.Properties.AddEx("LastUpdated", workflowInstance.LastUpdated);
                    instanceUnit.Properties.AddEx("Properties", workflowInstance.Properties);
                    instanceUnit.Properties.AddEx("Status", workflowInstance.Status);
                    instanceUnit.Properties.AddEx("UserStatus", workflowInstance.UserStatus);
                    instanceUnit.Properties.AddEx("WorkflowSubscriptionId", workflowInstance.WorkflowSubscriptionId);

                    IAveWorkflowSubscription subscription = WFSubscriptionService.GetSubscription(workflowInstance.WorkflowSubscriptionId);
                    const string taskIdStr = "TaskListId";
                    const string historyIdStr = "HistoryListId";
                    instanceUnit.Properties.AddEx("LS.ParentAssociationName", subscription.Name);
                    if (subscription.PropertyDefinitions.ContainsKey(taskIdStr))
                    {
                        instanceUnit.Properties.AddEx(mSPWFInstanceProcNative10Model.CustomFieldProfixProp + taskIdStr, new Guid(subscription.PropertyDefinitions[taskIdStr]));
                    }
                    if (subscription.PropertyDefinitions.ContainsKey(historyIdStr))
                    {
                        instanceUnit.Properties.AddEx(mSPWFInstanceProcNative10Model.CustomFieldProfixProp + historyIdStr, new Guid(subscription.PropertyDefinitions[historyIdStr]));
                    }
                    if (subscription.PropertyDefinitions.ContainsKey("SharePointWorkflowContext.ActivationProperties.WebId"))
                    {
                        instanceUnit.Properties.AddEx("#WebId", new Guid(subscription.PropertyDefinitions["SharePointWorkflowContext.ActivationProperties.WebId"]));
                    }
                    else
                    {
                        instanceUnit.Properties.AddEx("#WebId", this.ParentItem.Web.ID);
                    }
                    if (subscription.PropertyDefinitions.ContainsKey("SharePointWorkflowContext.ActivationProperties.SiteId"))
                    {
                        instanceUnit.Properties.AddEx("#SiteId", new Guid(subscription.PropertyDefinitions["SharePointWorkflowContext.ActivationProperties.SiteId"]));
                    }
                    else
                    {
                        instanceUnit.Properties.AddEx("#SiteId", this.ParentItem.Web.Site.ID);
                    }
                    if (subscription.PropertyDefinitions.ContainsKey("SharePointWorkflowContext.ActivationProperties.ListId"))
                    {
                        instanceUnit.Properties.AddEx("#ListId", new Guid(subscription.PropertyDefinitions["SharePointWorkflowContext.ActivationProperties.ListId"]));
                    }
                    else
                    {
                        instanceUnit.Properties.AddEx("#ListId", this.ParentItem.ListID);
                    }
                    instanceUnit.Properties.AddEx("#TemplateId", workflowInstance.WorkflowSubscriptionId);
                    instanceUnit.Properties.AddEx("#ItemId", int.Parse(workflowInstance.Properties["Microsoft.SharePoint.ActivationProperties.ItemId"]));
                    instanceUnit.Properties.AddEx("#ItemGUID", new Guid(workflowInstance.Properties["Microsoft.SharePoint.ActivationProperties.ItemGuid"]));
                    mSPWFInstanceProcNative10Model.FixupUserLoginName(instanceUnit);
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceSelfException, e.Message);
                    logger.Warn("An exception occurred while backup instance self. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupSelfError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupInstanceSelfByAPI");
                }
            }
        }
        #endregion

        #region ************************Restore Region************************
        private WorkflowFixupParams mFixupParams = new WorkflowFixupParams();

        public override int Restore(byte[] serializedData)
        {
            try
            {
                SPWFInstanceUnit unit = SPWFInstanceUnit.Load(serializedData);
                return Restore(unit);
            }
            catch (SPWFProcessorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceUnknownError, e);
            }
        }

        public override int Restore(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance13Model.InstanceRestore"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "InstanceRestore");
                string monitor = mMainMonitorLog = "Instance Restore";
                bool eventFiringDisabled = false;

                TaskItemGuidMapping.Clear();
                byte[] cacheData = SPWFInstanceUnit.Save(unit);
                try
                {
                    mInstanceUnit = unit;

                    SPWorkflowSubItemUnit instanceItemUnit = unit.InstanceItem;


                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + instanceItemUnit.Properties["#Id"]);
                    mPerformanceMonitor.StartMonitor(monitor);
                    #endregion

                    using (AvePerformanceScope pf1 = new AvePerformanceScope("RestoreWorkflowInstance13Model.InstanceRestore.GetParentAssociation"))
                    {
                        #region Get Parent Association
                        try
                        {
                            Guid parentAssoId = (Guid)instanceItemUnit.Properties.GetEx("#TemplateId");
                            if (unit.ParentAssociationUnit == null)
                            {
                                if (mAssociationProc.UnitsOfRestored.ContainsKey(parentAssoId))
                                    unit.ParentAssociationUnit = mAssociationProc.UnitsOfRestored[parentAssoId];
                                else
                                    throw new SPWFProcessorException(SPWFProcessorErrorCode.ParentAssociationCannotBeFound);
                            }
                        }
                        catch (SPWFProcessorException procException)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
                            logger.Warn("Restore parent not found. exception:{0}", procException.ToString());
                            try
                            {
                                if (procException.ErrorCode == SPWFProcessorErrorCode.PutIntoPostAction)
                                {
                                    SPWorkflowProcessorRuntime.OnCacheData(this.mParentItem.Web.Site.Url, this.mParentItem.SiteID.ToString(), this.mParentItem.WebID.ToString(), this.mParentItem.ListID.ToString(), this.mParentItem.ListID.ToString(), this.mParentItem.ItemID, unit.InstanceItem.Properties["#Id"].ToString(), cacheData);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.CacheDataError, e.ToString());
                            }
                            throw;
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, e.Message);
                            logger.Warn("Restore parent not found. exception:{0}", e.ToString());
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.GetParentAssociationError, e);
                        }
                        #endregion
                    }

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Get Parent Association. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    InitializeFixupParams(unit);

                    #region Performance Monitor Region
                    mPerformanceMonitor.ResetCurrentDuration(monitor); ;
                    #endregion

                    int conflictStatus = 0;// HandleWorkflowInstanceConflict(unit.FixupParameters);
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreConflictStatus, conflictStatus.ToString());

                    #region Performance Monitor Region
                    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Handle Conflict. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    #endregion

                    if (conflictStatus <= 0)
                    {
                        eventFiringDisabled = SPEventManagerWrapper.EventFiringDisabled;
                        if (!eventFiringDisabled)
                        {
                            SPEventManagerWrapper.DisableEventFiring();
                        }
                        RestoreWorkflowSubItem(unit);
                        return 0;
                    }
                    else
                    {
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceConflict);
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceException, unit.InstanceItem.Properties.GetEx("#Id").ToString(), procException.Message);
                    logger.Warn("An exception occurred while restore instance. exception:{0}", procException.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceException, unit.InstanceItem.Properties.GetEx("#Id").ToString(), e.Message);
                    logger.Warn("An exception occurred while restore instance. exception:{0}", e.ToString());
                    return 3;
                }
                finally
                {
                    if (!eventFiringDisabled)
                        SPEventManagerWrapper.EnableEventFiring();

                    #region Performance Monitor Region
                    mPerformanceMonitor.StopMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                    #endregion

                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceRestore");
                }
            }
        }


        private void InitializeFixupParams(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance13Model.InstanceRestore.InitializeFixupParams"))
            {
                try
                {
                    SPWorkflowSubItemUnit instanceItem = unit.InstanceItem;
                    unit.FixupParameters.Dispose();
                    unit.FixupParameters.mInstanceIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#Id"), Guid.NewGuid());
                    unit.FixupParameters.mWebApplicationIdDic.AddEx(Guid.Empty, mParentItem.Web.Site.WebApplication.ID);
                    unit.FixupParameters.mParentAssoicationIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#TemplateId"), unit.ParentAssociationUnit.SerializableData.mId);
                    unit.FixupParameters.mParentAssociationBaseIdDic.AddEx(Guid.Empty, TryGetMappingBaseId(unit.ParentAssociationUnit.SerializableData.mBaseId));
                    if (mParentItem.ParentItemType == WFParentItemType.ListItem)
                    {
                        unit.FixupParameters.mListIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#ListId"), unit.ParentAssociationUnit.mListId);
                    }
                    else
                    {
                        unit.FixupParameters.mListIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#ListId"), unit.ParentAssociationUnit.mWebId);
                    }
                    unit.FixupParameters.mWebIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#WebId"), unit.ParentAssociationUnit.mWebId);
                    unit.FixupParameters.mSiteIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#SiteId"), unit.ParentAssociationUnit.mSiteId);
                    unit.FixupParameters.mTaskListIdDic.AddEx((Guid)instanceItem.Properties.GetEx(CustomFieldProfix + "TaskListId"), unit.ParentAssociationUnit.SerializableData.mTaskListId);
                    unit.FixupParameters.mHistoryListIdDic.AddEx((Guid)instanceItem.Properties.GetEx(CustomFieldProfix + "HistoryListId"), unit.ParentAssociationUnit.SerializableData.mHistoryListId);
                    unit.FixupParameters.mItemIdDic.AddEx((int)instanceItem.Properties.GetEx("#ItemId"), mParentItem.ItemID);
                    if (mParentItem.ParentItemType == WFParentItemType.ListItem)
                    {
                        Guid tp_Guid = GetListItemGuid(mParentItem);
                        if (tp_Guid != Guid.Empty)
                        {
                            unit.FixupParameters.mItemGuidDic.AddEx((Guid)instanceItem.Properties.GetEx("#ItemGUID"), tp_Guid);
                        }
                        else
                        {
                            logger.Warn("An error occurred while Getting the list item Guid.");
                        }
                    }
                    // unit.FixupParameters.mInternalStateDic.AddEx((int)instanceItem.Properties.GetEx("#InternalState"), 0);
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InitializeFixupParamError, e);
                }
            }
        }

        /// <summary>
        /// When call CreateWorkflow function failed, this function will be called to get or create the source status field.
        /// There will be a problem if the source status field is using by another workflow associaion
        /// </summary>
        /// <param name="instanceUnit"></param>
        /// <returns></returns>
        private string GetOrCreateStatusField(SPWFInstanceUnit instanceUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetOrCreateStatusField");
            string colName = string.Empty;
            string rowOrdinal = string.Empty;
            XmlDocument doc = null;
            instanceUnit.StatusFieldColName = null;
            Guid mSourceFieldID = Guid.Empty;
            string mSourceFieldInternalName = string.Empty;
            string mSourceFieldDisplayName = string.Empty;
            try
            {
                string statusName = (string)LSInvoker.GetProperty(instanceUnit.ParentAssociationUnit.SPAssociation, "InternalNameStatusField");
                if (string.IsNullOrEmpty(statusName))
                    statusName = instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldName;
                if (string.IsNullOrEmpty(statusName))
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_EmptyStatusFieldName);
                    return string.Empty;
                }

                //instanceUnit.ParentAssociationUnit.ReloadParentWeb();
                IAveWeb web = instanceUnit.ParentAssociationUnit.ParentWeb;
                {
                    IAveList list = web.Lists[mParentItem.ListID];
                    object statusFieldObj = list.Fields.GetFieldByInternalName(statusName, false);
                    if (statusFieldObj != null)
                    {
                        IAveField temp = (IAveField)statusFieldObj;

                        if (temp.Type != AveFieldType.WorkflowStatus)
                            statusFieldObj = null;
                        else if (!IsAvailableStatusFieldName(list, instanceUnit.ParentAssociationUnit.SPAssociation.BaseId, null, statusName))
                            statusFieldObj = null;
                    }

                    if (statusFieldObj == null)
                    {
                        doc = new XmlDocument();
                        doc.LoadXml("<Root>" + instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldSchema + "</Root>");
                        XmlElement xe = (XmlElement)doc.FirstChild.ChildNodes[0];
                        mSourceFieldID = new Guid(xe.Attributes["ID"].Value);
                        mSourceFieldInternalName = xe.Attributes["Name"].Value;
                        mSourceFieldDisplayName = xe.Attributes["DisplayName"].Value;
                        xe.RemoveAttribute("ID");
                        AveSPFieldCollection fields = new AveSPFieldCollection();
                        fields.CurrentFieldCollection = list.Fields;
                        statusFieldObj = fields.CreateSPField(xe, true, AveAddFieldOptions.DefaultValue);
                    }
                    if (statusFieldObj == null)
                        return string.Empty;

                    IAveField statusField = list.Fields.GetFieldByInternalName(((IAveField)statusFieldObj).InternalName); ;
                    colName = AveSPField.GetColNameFromSchema("ColName", statusField.SchemaXml);
                    rowOrdinal = AveSPField.GetColNameFromSchema("RowOrdinal", statusField.SchemaXml);
                    try
                    {
                        instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldName = statusField.InternalName;
                        instanceUnit.ParentAssociationUnit.SerializableData.mStatusFieldSchema = statusField.SchemaXml;
                        LSInvoker.SetProperty(instanceUnit.ParentAssociationUnit.SPAssociation, "InternalNameStatusField", statusField.InternalName);
                        //instanceUnit.ParentAssociationUnit.UpdateWorkflowAssociation(instanceUnit.ParentAssociationUnit.SPAssociation);//doesn't work
                        SPWFAssociationProcNative.UpdateStatusFieldName(instanceUnit.ParentAssociationUnit.SPAssociation);
                        AddStatusFieldNameMapping(list, instanceUnit.ParentAssociationUnit.SPAssociation.BaseId, statusField.InternalName);
                        statusField.Title = instanceUnit.ParentAssociationUnit.SPAssociation.Name;
                        statusField.Update();

                        if (!instanceUnit.ParentAssociationUnit.isCreateField)
                        {
                            if (mSourceFieldID != Guid.Empty && !instanceUnit.mWFFieldIDMapping.ContainsKey(mSourceFieldID))
                            {
                                instanceUnit.mWFFieldIDMapping.Add(mSourceFieldID, statusField.ID);
                            }
                            if (mSourceFieldInternalName != string.Empty && !instanceUnit.mWFFieldInternalNameMapping.ContainsKey(mSourceFieldInternalName))
                            {
                                instanceUnit.mWFFieldInternalNameMapping.Add(mSourceFieldInternalName, statusField.InternalName);
                            }
                            if (mSourceFieldDisplayName != string.Empty && !instanceUnit.mWFFieldDisplayNameMapping.ContainsKey(mSourceFieldDisplayName))
                            {
                                instanceUnit.mWFFieldDisplayNameMapping.Add(mSourceFieldDisplayName, statusField.Title);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.IP_ResetStatusFieldException, e.Message);
                        logger.Warn("An exception occurred while reset status field. exception:{0}", e.ToString());
                    }
                    SPWorkflowProcessorRuntime.Log(Logs.IP_StatusFieldName, colName);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.StatusColumnGetorAddError, mSourceFieldInternalName, ex);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetOrCreateStatusField");
            }
            instanceUnit.StatusFieldColName = colName;
            instanceUnit.StatusFieldRowOrdinal = rowOrdinal;
            return colName;
        }


        private void FixupUserIds(SPWorkflowSubItemUnit subUnit)
        {
            using (IAveWeb web = mParentItem.Web)
            {
                if (subUnit.ItemType == WorkflowSubItemType.Task)
                {
                    if (subUnit.Properties.ContainsKey("_0_AssignedTo"))
                    {
                        #region Fixup AssignedTo

                        if (((string)subUnit.Properties["_0_AssignedTo"]).Equals(string.Empty))
                        {
                            subUnit.Properties.Remove("_0_AssignedTo");
                        }
                        else
                        {
                            string loginName = (string)subUnit.Properties["_0_AssignedTo"];
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(loginName);
                            if (user != null)
                            {
                                subUnit.Properties["_0_AssignedTo"] = user.ID;
                            }
                            else
                            {
                                subUnit.Properties.Remove("_0_AssignedTo");
                            }
                        }
                        #endregion
                    }
                }
                else if (subUnit.ItemType == WorkflowSubItemType.History)
                {
                    if (subUnit.Properties.ContainsKey("_0_User"))
                    {
                        #region Fixup User Column

                        if (((string)subUnit.Properties["_0_User"]).Equals(string.Empty))
                        {
                            subUnit.Properties.Remove("_0_User");
                        }
                        else
                        {
                            string loginName = (string)subUnit.Properties["_0_User"];
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(loginName);
                            if (user != null)
                            {
                                subUnit.Properties["_0_User"] = user.ID;
                            }
                            else
                            {
                                subUnit.Properties.Remove("_0_User");
                            }
                        }
                        #endregion
                    }
                }
                else if (subUnit.ItemType == WorkflowSubItemType.Instance)
                {
                    if (subUnit.Properties.ContainsKey("#Author"))
                    {
                        #region Fixup Instance Author
                        if (((string)subUnit.Properties["#Author"]).Equals(string.Empty))
                        {
                            subUnit.Properties["#Author"] = web.CurrentUser.ID;
                        }
                        else
                        {
                            string loginName = (string)subUnit.Properties["#Author"];
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(loginName);
                            if (user != null)
                            {
                                subUnit.Properties["#Author"] = user.ID;
                            }
                            else
                            {
                                subUnit.Properties["#Author"] = web.CurrentUser.ID;
                            }
                        }
                        return;
                        #endregion
                    }
                }


                #region Fixup Author
                if (((string)subUnit.Properties["_0_Author"]).Equals(string.Empty))
                {
                    subUnit.Properties.Remove("_0_Author");
                }
                else
                {
                    string loginName = (string)subUnit.Properties["_0_Author"];
                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(loginName);
                    if (user != null)
                    {
                        subUnit.Properties["_0_Author"] = user.ID;
                    }
                    else
                    {
                        subUnit.Properties.Remove("_0_Author");
                    }
                }
                #endregion


                #region Fixup Editor
                if (((string)subUnit.Properties["_0_Editor"]).Equals(string.Empty))
                {
                    subUnit.Properties.Remove("_0_Editor");
                }
                else
                {
                    string loginName = (string)subUnit.Properties["_0_Editor"];
                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(loginName);
                    if (user != null)
                    {
                        subUnit.Properties["_0_Editor"] = user.ID;
                    }
                    else
                    {
                        subUnit.Properties.Remove("_0_Editor");
                    }
                }
                #endregion
            }
        }

        private void CreateWorkflow(SPWFInstanceUnit unit, AveWorkflowRunOptions options)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance13Model.InstanceRestore.CreateWorkflow"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "CreateWorkflow");
                string monitor = "Create Workflow";
                if ((mParentItem == null) || (unit.ParentAssociationUnit == null) || (unit.ParentAssociationUnit.WorflowDefinition == null))
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "CreateWorkflow");
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.CreateInstanceArgumentNullException);
                }

                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);

                    mInstance13Model = null;

                    //GetOrCreateStatusField(unit);

                    if (Environment.SharePointVersion == SharePointVersion.SharePoint2007)
                    {
                        throw new NotSupportedException("Cannot support exception(2007).");
                    }
                    else if (Environment.SharePointVersion == SharePointVersion.SharePoint2010)
                    {
                        throw new NotSupportedException("Cannot support exception(2010).");
                    }
                    else if (Environment.SharePointVersion == SharePointVersion.SharePoint2013 ||
                        Environment.SharePointVersion == SharePointVersion.SharePoint2016)
                    {
                        var payLoad = new Dictionary<string, object>();
                        Guid newWFInstanceID = WFInstanceService.StartWorkflowOnListItem(unit.ParentAssociationUnit.WorkflowSubscription, mParentItem.ItemID, payLoad);
                        mInstance13Model = WFInstanceService.GetInstance(newWFInstanceID);
                    }
                    else
                    {
                        throw new NotSupportedException("Cannot support exception(Unknown platform).");
                        return;
                    }

                    //fixup status field name
                    try
                    {

                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, "An error occurred while creating workflow instance for 13 model, error message: {0}", e);
                        SPWorkflowProcessorRuntime.Log(Logs.IP_ResetStatusFieldException, e.Message);
                    }

                    unit.FixupParameters.mInstanceIdDic.AddEx((Guid)unit.InstanceItem.Properties.GetEx("#Id"), this.mInstance13Model.Id);
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_CreateInstanceException, e.Message);
                    logger.Warn("An exception occurred while create instance. exception:{0}", e.ToString());
                    InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.CreateInstanceUnknownException, e));
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "CreateWorkflow");
                }
            }

        }

        private int RestoreWorkflowSubItem(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance13Model.InstanceRestore.RestoreWorkflowSubItem"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreWorkflowSubItem");
                try
                {
                    if (SPWorkflowProcessorRuntime.SkipRunningInstance && (AveWorkflowStatus13Model)mInstanceUnit.InstanceItem.Properties["Status"] == AveWorkflowStatus13Model.Started)
                    {
                        return 0;
                    }
                    CreateWorkflow(unit, (AveWorkflowRunOptions)1025);
                    //Delete task item and histroy item by default.
                    if (!SPWorkflowProcessorRuntime.Process13ModelWFInstanceByNative)
                    {
                        if (!mTaskListIdAndInstanceMapping.ContainsKey(unit.ParentAssociationUnit.TaskListUnit.mSPList.ID))
                        {
                            mTaskListIdAndInstanceMapping.Add(unit.ParentAssociationUnit.TaskListUnit.mSPList.ID, new List<Guid>());
                        }
                        mTaskListIdAndInstanceMapping[unit.ParentAssociationUnit.TaskListUnit.mSPList.ID].Add(mInstance13Model.Id);

                        if (!mHistoryListIdAndInstanceMapping.ContainsKey(unit.ParentAssociationUnit.HistListUnit.mSPList.ID))
                        {
                            mHistoryListIdAndInstanceMapping.Add(unit.ParentAssociationUnit.HistListUnit.mSPList.ID, new List<Guid>());
                        }
                        mHistoryListIdAndInstanceMapping[unit.ParentAssociationUnit.HistListUnit.mSPList.ID].Add(mInstance13Model.Id);
                    }
                    this.mSPWFInstanceProcNative10Model.RestoreTask(unit);
                    this.mSPWFInstanceProcNative10Model.RestoreSubscription(unit, unit.InstanceItem);
                    if (SPWorkflowProcessorRuntime.Process13ModelWFInstanceByNative)
                    {
                        RestoreInstance(unit);
                    }
                    else
                    {
                        RestoreInstanceByAPI(unit);
                    }
                    this.mSPWFInstanceProcNative10Model.RestoreHistory(unit);
                    ProcessWorkflowStatus(unit);
                    return 0;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.ResotreWFSubItemError, e.ToString());
                    throw;
                }
                finally
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreWorkflowSubItem");
                }
            }
        }

        /// <summary>
        /// Connect to workflow manager, and update by query service
        /// </summary>
        /// <param name="parentUnit"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Fields of Table Workflow")]
        private int RestoreInstance(SPWFInstanceUnit parentUnit)
        {
            return 0;
        }

        private int RestoreInstanceByAPI(SPWFInstanceUnit parentUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance13Model.InstanceRestore.RestoreInstanceByAPI"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreInstanceByAPI");
                string monitor = "Restore Instance Self";
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    SPWorkflowSubItemUnit instanceItem = parentUnit.InstanceItem;
                    instanceItem.Properties["#Id"] = parentUnit.FixupParameters.mInstanceIdDic.GetValue(0);
                    instanceItem.Properties["#TemplateId"] = parentUnit.FixupParameters.mParentAssoicationIdDic.GetValue(0);
                    instanceItem.Properties["#ListId"] = parentUnit.FixupParameters.mListIdDic.GetValue(0);
                    instanceItem.Properties["#WebId"] = parentUnit.FixupParameters.mWebIdDic.GetValue(0);
                    instanceItem.Properties["#SiteId"] = parentUnit.FixupParameters.mSiteIdDic.GetValue(0);
                    instanceItem.Properties["#TaskListId"] = parentUnit.FixupParameters.mTaskListIdDic.GetValue(0);
                    instanceItem.Properties["#ItemId"] = parentUnit.FixupParameters.mItemIdDic.GetValue(0);
                    instanceItem.Properties["#ItemGUID"] = parentUnit.FixupParameters.mItemGuidDic.GetValue(0);

                    try
                    {
                        FixupUserIds(instanceItem);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, "An error occurred while restoring workflow instance by API, error message: {0}", e);
                        System.Diagnostics.Trace.WriteLine(e.ToString());
                    }

                    #region Update Status Field
                    //if (parentUnit.ParentAssociationUnit.ParentObjectType != SPWFAssociationParentType.Web)
                    //{
                    //    if (string.IsNullOrEmpty(parentUnit.StatusFieldColName))
                    //        throw new SPWFProcessorException(SPWFProcessorErrorCode.StatusFieldIsNull);
                    //    if (string.IsNullOrEmpty(parentUnit.StatusFieldRowOrdinal))
                    //    {
                    //        parentUnit.StatusFieldRowOrdinal = "0";
                    //    }
                    //    mQueryService.UpdateStatusFieldValue(parentUnit.FixupParameters.mSiteIdDic.GetValue(0), parentUnit.FixupParameters.mListIdDic.GetValue(0), parentUnit.FixupParameters.mItemGuidDic.GetValue(0), parentUnit.FixupParameters.mItemIdDic.GetValue(0), (parentUnit.FixupParameters.mInstanceIdDic.GetValue(0)).ToByteArray(), short.Parse(parentUnit.StatusFieldRowOrdinal), parentUnit.StatusFieldColName);
                    //}
                    #endregion

                    #region Recalculate Running Instance Count
                    #endregion

                    return 0;
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceSelfException, procException.Message);
                    logger.Warn("An exception occurred while restore instance self. exception:{0}", procException.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceSelfException, e.Message);
                    logger.Warn("An exception occurred while restore instance self. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreSelfError, e);
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreInstance");
                }
            }
        }

        private int ProcessWorkflowStatus(SPWFInstanceUnit unit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreWorkflowInstance13Model.InstanceRestore.ProcessWorkflowStatus"))
            {
                if (SPWorkflowProcessorRuntime.RestoreHistoryOnly)
                {
                    if (mInstance13Model != null && mInstance13Model.Status != AveWorkflowStatus13Model.Completed)
                    {
                        WFInstanceService.CancelWorkflow(mInstance13Model);
                    }
                    if (unit.RestartRunningInstance && (AveWorkflowStatus13Model)mInstanceUnit.InstanceItem.Properties["Status"] == AveWorkflowStatus13Model.Started)
                    {
                        System.Threading.Thread.Sleep(SPWorkflowProcessorRuntime.PauseTimeBeforeRestartWorkflow * 1000);
                        CreateWorkflow(unit, AveWorkflowRunOptions.Asynchronous);
                    }
                    return 0;
                }
                return 1;
            }
        }
        #endregion

    }

    internal class SPWorkflowInstance13ModelComparer : IComparer<IAveWorkflowInstance>
    {
        public int Compare(IAveWorkflowInstance x, IAveWorkflowInstance y)
        {
            if (x != null && y != null)
            {
                if (x.InstanceCreated != null && y.InstanceCreated != null)
                {
                    long tickX = x.InstanceCreated.Ticks;
                    long tickY = y.InstanceCreated.Ticks;

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
}
