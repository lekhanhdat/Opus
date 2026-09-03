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
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Common;

namespace LS.SPWorkflowProcessor
{
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
                        mListId = mAveWeb.ID;
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



        public void SetCustomProc(List<ICustomWorkflowInstanceProc> customProcessors)
        {
            CustomProcessors = customProcessors;
        }

        internal void AddConflictRecord(Guid siteId, Guid webId, Guid listId, Guid parentAssoId, int itemId, int conflictStatus)
        {
            if (mConflictRecords == null)
                mConflictRecords = new List<ConflictRecord>();

            foreach (ConflictRecord record in mConflictRecords)
            {
                if (record.mSiteId == siteId &&
                    record.mWebId == webId &&
                    record.mListId == listId &&
                    record.mParentAssoId == parentAssoId &&
                    record.mItemId == itemId)
                {
                    return;
                }
            }

            ConflictRecord newRecord = new ConflictRecord();
            newRecord.mSiteId = siteId;
            newRecord.mWebId = webId;
            newRecord.mListId = listId;
            newRecord.mParentAssoId = parentAssoId;
            newRecord.mItemId = itemId;
            newRecord.mConflictStatus = conflictStatus;
            mConflictRecords.Add(newRecord);

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
        protected bool IsAvailableStatusFieldName(IAveList parentList, Guid parentAssociationBaseId, string statusFieldInternalName)
        {
            statusFieldInternalName = statusFieldInternalName.ToLower();
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
                            if (!temp.ContainsKey(status.ToLower()))
                            {
                                //temp.Add(status.ToLower(), asso.Id);
                                temp.Add(status.ToLower(), new List<Guid>());
                            }
                            if (!temp[status.ToLower()].Contains(asso.BaseId))
                            {
                                temp[status.ToLower()].Add(asso.BaseId);
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
                                if (!temp.ContainsKey(status.ToLower()))
                                {
                                    temp.Add(status.ToLower(), new List<Guid>());
                                }
                                if (!temp[status.ToLower()].Contains(asso.BaseId))
                                {
                                    temp[status.ToLower()].Add(asso.BaseId);
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
                if (temp[statusFieldInternalName].Count == 1 && temp[statusFieldInternalName][0] == parentAssociationBaseId)
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
            statusFieldInternalName = statusFieldInternalName.ToLower();
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
                    subUnit.Properties["_0_AssignedTo"] = SPPermissionProcessor.GetUserLoginNameFromId(web, (int)subUnit.Properties["_0_AssignedTo"]);
                }
                else if (subUnit.ItemType == WorkflowSubItemType.History && subUnit.Properties.ContainsKey("_0_User"))
                {
                    subUnit.Properties["_0_User"] = SPPermissionProcessor.GetUserLoginNameFromId(web, (int)subUnit.Properties["_0_User"]);
                }
                else if (subUnit.ItemType == WorkflowSubItemType.Instance && subUnit.Properties.ContainsKey("#Author"))
                {
                    subUnit.Properties["#Author"] = SPPermissionProcessor.GetUserLoginNameFromId(web, (int)subUnit.Properties["#Author"]);
                    return;
                }
                if (subUnit.Properties.ContainsKey("_0_Author"))
                {
                    subUnit.Properties["_0_Author"] = SPPermissionProcessor.GetUserLoginNameFromId(web, (int)subUnit.Properties["_0_Author"]);
                }

                if (subUnit.Properties.ContainsKey("_0_Editor"))
                {
                    subUnit.Properties["_0_Editor"] = SPPermissionProcessor.GetUserLoginNameFromId(web, (int)subUnit.Properties["_0_Editor"]);
                }
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FixupUserLoginName");
        }

        public override List<byte[]> Backup()
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "InstanceMainBackup");

            string monitor = mMainMonitorLog = "Instance Backup";

            List<byte[]> rlt = new List<byte[]>();
            List<Guid> tempIds = new List<Guid>();
            try
            {

                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                mPerformanceMonitor.StartMonitor(monitor);
                #endregion
                if (mQueryService != null)
                {
                    mQueryService.GetWorkflowId(tempIds, mParentItem.SiteID, mParentItem.WebID, mParentItem.ItemID, mParentItem.ListID);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_GetItemInstanceException, e.Message);
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
                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + instanceId);
                mPerformanceMonitor.StartMonitor(monitor);
                #endregion

                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupOneInstance");
                mInnerExceptions = new List<SPWFProcessorException>();
                try
                {
                    SPWorkflowSubItemUnit instanceItemUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Instance);
                    instanceItemUnit.Properties.AddEx("#Id", instanceId);
                    BackupInstanceSelf(instanceItemUnit);
                    BackupTasks(instanceItemUnit);
                    BackupSubscription(instanceItemUnit);
                    BackupHistory(instanceItemUnit);
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
                    mInnerExceptions.Add(procException);
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceException, instanceId.ToString(), e.Message);
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

                if (mInnerExceptions.Count > 0)
                    mExceptions.Add(instanceId, mInnerExceptions);
            }

            #region Performance Monitor Region
            mPerformanceMonitor.RemoveMonitor(monitor);
            #endregion

            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceMainBackup");
            return rlt;
        }

        private void BackupInstanceSelf(SPWorkflowSubItemUnit instanceUnit)
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
                mQueryService.BackupInstanceSelf((Guid)instanceUnit.Properties["#WebId"], (Guid)instanceUnit.Properties["#TemplateId"], instanceUnit.Properties, CustomFieldProfix);

                BackupCustomUnit(instanceUnit);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceSelfException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupSelfError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupInstanceSelf");
            }
        }

        internal void BackupTasks(SPWorkflowSubItemUnit parentUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupTasks");
            string monitor = "Backup Tasks";
            try
            {
                mPerformanceMonitor.StartMonitor(monitor);
                using (IAveQueryDataReader sdr = mQueryService.BackupTasks((Guid)parentUnit.Properties["#SiteId"], (Guid)parentUnit.Properties["#WebId"], (Guid)parentUnit.Properties[CustomFieldProfix + "TaskListId"], (Guid)parentUnit.Properties["#Id"]))
                {
                    while (sdr.Read())
                    {
                        SPWorkflowSubItemUnit taskUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Task, parentUnit);
                        Dictionary<string, string> fieldDic = mWebLevelFieldProcessor.GetDBFieldToSPFieldDic(mParentItem.Web.Lists[(Guid)parentUnit.Properties[CustomFieldProfix + "TaskListId"]]);
                        taskUnit.SetPropsFromDataReader(sdr, 0, fieldDic, 0);
                        FixupUserLoginName(taskUnit);
                        BackupPermissionUnit(taskUnit);
                        BackupSubscription(taskUnit);
                        BackupCustomUnit(taskUnit);
                        parentUnit.ChildUnits.Add(taskUnit);
                    }
                }
            }
            catch (SPWFProcessorException ex)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_BackupTaskItemsException, ex.Message);
                throw;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_BackupTaskItemsException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupTaskError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupTasks");
            }
        }

        internal void BackupSubscription(SPWorkflowSubItemUnit parentUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupSubscription");
            string monitor = "Backup Subscriptions";
            //Fortify fix: Unreleased Resource: Database
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = mParentItem.Web.Site.ContentDatabase.DatabaseConnectionString;
                try
                {
                    mPerformanceMonitor.StartMonitor(monitor);
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        #region Backup Task Item Context
                        if (parentUnit.ItemType == WorkflowSubItemType.Task)
                        {

                            cmd.CommandText = "SELECT * FROM EventReceivers WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND ContextCollectionId=@ContextCollectionId";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@SiteId", parentUnit.ParentUnit.Properties["#SiteId"]);
                            cmd.Parameters.AddWithValue("@WebId", parentUnit.ParentUnit.Properties["#WebId"]);
                            cmd.Parameters.AddWithValue("@HostId", parentUnit.Properties["~0_tp_ListId"]);
                            cmd.Parameters.AddWithValue("@ContextCollectionId", ((Guid)parentUnit.Properties["_0_GUID"]).ToByteArray());

                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                                if (sdr.Read())
                                {
                                    SPWorkflowSubItemUnit eventUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Subscription, parentUnit);
                                    eventUnit.SetPropsFromDataReader(sdr, 0, null, 0);
                                    BackupCustomUnit(eventUnit);
                                    parentUnit.ChildUnits.Add(eventUnit);
                                }
                            }

                        }
                        #endregion
                        #region Backup Instance Events
                        else if (parentUnit.ItemType == WorkflowSubItemType.Instance)
                        {
                            #region Backup Parent Item Context
                            if (parentUnit.Properties.Contains("#ItemGUID"))
                            {
                                cmd.CommandText = "SELECT * FROM EventReceivers WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND HostType=2 AND " +
                                    "Type=32767 AND ContextCollectionId=@ContextCollectionId AND ContextObjectId IS NULL AND ContextId IS NULL AND " +
                                    "ContextType IS NULL AND ContextEventType IS NULL AND SequenceNumber=10000 AND Assembly='' AND Class=''";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@SiteId", parentUnit.Properties["#SiteId"]);
                                cmd.Parameters.AddWithValue("@WebId", parentUnit.Properties["#WebId"]);
                                cmd.Parameters.AddWithValue("@HostId", parentUnit.Properties["#ListId"]);
                                cmd.Parameters.AddWithValue("@ContextCollectionId", ((Guid)parentUnit.Properties["#ItemGUID"]).ToByteArray());
                            }
                            else
                            {
                                cmd.CommandText = "SELECT * FROM EventReceivers WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND HostType=2 AND " +
                                    "Type=32767 AND ContextCollectionId IS NULL AND ContextObjectId IS NULL AND ContextId IS NULL AND " +
                                    "ContextType IS NULL AND ContextEventType IS NULL AND SequenceNumber=10000 AND Assembly='' AND Class=''";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@SiteId", parentUnit.Properties["#SiteId"]);
                                cmd.Parameters.AddWithValue("@WebId", parentUnit.Properties["#WebId"]);
                                cmd.Parameters.AddWithValue("@HostId", parentUnit.Properties["#ListId"]);
                            }

                            using (SqlDataReader sdr = cmd.ExecuteReader())
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
                            cmd.CommandText = "SELECT * FROM EventReceivers WHERE SiteId=@SiteId AND WebId=@WebId AND HostType=5 AND ContextCollectionId=@ContextCollectionId ORDER BY ItemId";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@SiteId", parentUnit.Properties["#SiteId"]);
                            cmd.Parameters.AddWithValue("@WebId", parentUnit.Properties["#WebId"]);
                            cmd.Parameters.AddWithValue("@ContextCollectionId", ((Guid)parentUnit.Properties["#Id"]).ToByteArray());

                            using (SqlDataReader sdr = cmd.ExecuteReader())
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
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupEventReceiversException, e.Message);
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupSubscriptionError, e, parentUnit.ItemType.ToString());
                }
                finally
                {
                    mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                    mPerformanceMonitor.RemoveMonitor(monitor);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupSubscription");
                }
            }
        }

        internal void BackupHistory(SPWorkflowSubItemUnit parentUnit)
        {
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
                        }
                    }
                }
            }
            catch (SPWFProcessorException procException)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_BackupHistoriesException, procException.Message);
                mInnerWarnings.Add(procException);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_BackupHistoriesException, e.Message);
                mInnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupHistoryError, e));
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupHistory");
            }
        }

        private void BackupCustomUnit(SPWorkflowSubItemUnit parentUnit)
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
                throw;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_BackupCustomUnitsException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceCustomDataBackupError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupCustomUnit");
            }
        }

        private void BackupPermissionUnit(SPWorkflowSubItemUnit parentUnit)
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
                throw;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_BackupPermissionsException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionUnitBackupException, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupPermissionUnit");
            }
        }
        #endregion


        #region ************************Restore Region************************
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
            throw new NotSupportedException();
            //SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "InstanceRestore");
            //string monitor = mMainMonitorLog = "Instance Restore";
            //bool eventFiringDisabled = false;

            //TaskItemGuidMapping.Clear();
            //byte[] cacheData = SPWFInstanceUnit.Save(unit);
            //try
            //{
            //    mInstanceUnit = unit;

            //    SPWorkflowSubItemUnit instanceItemUnit = unit.InstanceItem;


            //    #region Performance Monitor Region
            //    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
            //    mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + instanceItemUnit.Properties["#Id"]);
            //    mPerformanceMonitor.StartMonitor(monitor);
            //    #endregion

            //    #region Get Parent Association
            //    try
            //    {
            //        Guid parentAssoId = (Guid)instanceItemUnit.Properties.GetEx("#TemplateId");
            //        if (unit.ParentAssociationUnit == null)
            //        {
            //            if (mAssociationProc.UnitsOfRestored.ContainsKey(parentAssoId))
            //                unit.ParentAssociationUnit = mAssociationProc.UnitsOfRestored[parentAssoId];
            //            else
            //                throw new SPWFProcessorException(SPWFProcessorErrorCode.ParentAssociationCannotBeFound);
            //        }
            //    }
            //    catch (SPWFProcessorException procException)
            //    {
            //        SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, procException.Message);
            //        try
            //        {
            //            if (procException.ErrorCode == 9999)
            //            {
            //                SPWorkflowProcessorRuntime.OnCacheData(this.mParentItem.SiteID.ToString(), this.mParentItem.WebID.ToString(), this.mParentItem.ListID.ToString(), this.mParentItem.ListID.ToString(), this.mParentItem.ItemID, unit.InstanceItem.Properties["#Id"].ToString(), cacheData);
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.CacheDataError, e.ToString());
            //        }
            //        throw;
            //    }
            //    catch (Exception e)
            //    {
            //        SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreParentNotFoundException, e.Message);
            //        throw new SPWFProcessorException(SPWFProcessorErrorCode.GetParentAssociationError, e);
            //    }
            //    #endregion

            //    #region Performance Monitor Region
            //    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Get Parent Association. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
            //    #endregion

            //    InitializeFixupParams(unit);

            //    #region Performance Monitor Region
            //    mPerformanceMonitor.ResetCurrentDuration(monitor); ;
            //    #endregion

            //    int conflictStatus = 0;// HandleWorkflowInstanceConflict(unit.FixupParameters);
            //    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreConflictStatus, conflictStatus.ToString());

            //    #region Performance Monitor Region
            //    mPerformanceMonitor.WriteMonitorLog(monitor, " --> ", "Handle Conflict. Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
            //    #endregion

            //    if (conflictStatus <= 0)
            //    {
            //        eventFiringDisabled = SPEventManagerWrapper.EventFiringDisabled;
            //        if (!eventFiringDisabled)
            //        {
            //            SPEventManagerWrapper.DisableEventFiring();
            //        }
            //        RestoreWorkflowSubItem(unit);
            //        return 0;
            //    }
            //    else
            //    {
            //        throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceConflict);
            //    }
            //}
            //catch (SPWFProcessorException procException)
            //{
            //    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceException, unit.InstanceItem.Properties.GetEx("#Id").ToString(), procException.Message);
            //    throw;
            //}
            //catch (Exception e)
            //{
            //    SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceException, unit.InstanceItem.Properties.GetEx("#Id").ToString(), e.Message);
            //    return 3;
            //}
            //finally
            //{
            //    if (!eventFiringDisabled)
            //        SPEventManagerWrapper.EnableEventFiring();

            //    #region Performance Monitor Region
            //    mPerformanceMonitor.StopMonitor(monitor);
            //    mPerformanceMonitor.WriteMonitorLog(monitor, " Finished. Duration: ", mPerformanceMonitor.GetDuration(monitor));
            //    mPerformanceMonitor.RemoveMonitor(monitor);
            //    mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
            //    #endregion

            //    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceRestore");
            //}
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="workflowProps"></param>
        /// <returns>0:the source or the destination or both are(have) not running instance
        /// 1:the source or the destination or both are(have) running instance
        /// -1:there is no instance on destination
        /// </returns>



    

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


        internal int RestoreTask(SPWFInstanceUnit parentUnit)
        {
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

                        FixupUserIds(child);
                        if (child.Properties.ContainsKey("_0_WorkflowInstanceID"))
                            child.Properties["_0_WorkflowInstanceID"] = parentUnit.FixupParameters.mInstanceIdDic.GetValue(0);
                        if (child.Properties.ContainsKey("_0_WorkflowListId"))
                            child.Properties["_0_WorkflowListId"] = parentUnit.FixupParameters.mListIdDic.GetValue(0);
                        if (child.Properties.ContainsKey("_0_WF4InstanceId"))
                            child.Properties["_0_WF4InstanceId"] = parentUnit.FixupParameters.mInstanceIdDic.GetValue(0).ToString().Trim(new char[] { '{', '}' });
                        if (child.Properties.ContainsKey("_0_WorkflowLink"))
                        {
                            string origWebUrl = parentUnit.ParentAssociationUnit.mTaskListUnit.SerializableData.mParentWebServerRelativeUrl;
                            string url = (string)child.Properties["_0_WorkflowLink"];
                            if (url.StartsWith(origWebUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                if (mParentItem.ListItem.File != null)
                                    child.Properties["_0_WorkflowLink"] = mParentItem.ListItem.File.ServerRelativeUrl;
                                else
                                    child.Properties["_0_WorkflowLink"] = mParentItem.Web.ServerRelativeUrl + url.Substring(origWebUrl.Length);
                            }
                        }
                        if (child.Properties.ContainsKey("_2_WorkflowLink"))
                        {
                            if (mParentItem.ListItem.File != null)
                            {
                                string name = mParentItem.ListItem.File.Name;
                                int extIndex = name.LastIndexOf('.');
                                if (extIndex > 0)
                                    name = name.Substring(0, extIndex);
                                child.Properties["_2_WorkflowLink"] = name; ;
                            }
                            else
                                child.Properties["_2_WorkflowLink"] = mParentItem.ListItem.Title;
                        }

                        if (child.Properties.ContainsKey("_0_ContentTypeId") && parentUnit.ParentAssociationUnit.mTaskListUnit.mContentTypeIdMapping != null)
                        {
                            byte[] ctIdBytes = (byte[])child.Properties["_0_ContentTypeId"];
                            string origCTId = LSUtilityOfBytes.LSBytesToHexString(ctIdBytes);
                            string fixupCTIdString = "0x" + origCTId.ToUpper();
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

                            RestorePermissionUnit(child.PermissionUnit, item);
                        }
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreTaskItemsException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreTaskError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreTasks");
            }
        }

        internal int RestoreSubscription(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
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

                            IAveWorkflowService service = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWorkflowService();
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
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreSubscriptionError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreSubscription");
            }
        }

        internal int RestoreHistory(SPWFInstanceUnit parentUnit)
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

                        FixupUserIds(child);
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
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreHistoryError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreHistory");
            }
        }

        private int RestoreCustomSubItem(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreCustomSubItem");
            string monitor = "Restore Custom Data";
            try
            {
                mPerformanceMonitor.StartMonitor(monitor);
                switch (parentItem.ItemType)
                {
                    case WorkflowSubItemType.Task:
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
                throw;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreCustomUnitsException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreCustomDataError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreCustomSubItem");
            }
        }

        private IAveListItem RestoreSPListItem(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreSPListItem");
            string monitor = "Restore SharePoint List Item";
            using (IAveSite site = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateSite(parentUnit.FixupParameters.mSiteIdDic.GetValue(0)))
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
                        throw;
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.IP_CreateSPItemException, e.Message);
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

        private void RestorePermissionUnit(SPPermissionUnit permUnit, IAveSecurableObject parent)
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
                throw;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_RestorePermissionsException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionUnitRestoreException, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestorePermissionUnit");
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
                throw;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InsertTableRow");
            }
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

        


        #region ************************Backup  Region************************

        public override void SetInstanceProcParameters(InstanceProcCreationParam param)
        {
            mSPWFInstanceProcNative10Model.ParentItem = this.ParentItem;
            mSPWFInstanceProcNative10Model.SetInstanceProcParameters(param);
            base.SetInstanceProcParameters(param);
        }

        public override List<byte[]> Backup()
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
                IAveWorkflowInstanceCollection aveWFInstanceCollection = WFInstanceService.EnumerateInstancesForListItem(mParentItem.ListID, mParentItem.ItemID);
                if (aveWFInstanceCollection == null)
                {
                    return rlt;
                }
                foreach (IAveWorkflowInstance aveWFInstance in aveWFInstanceCollection)
                {
                    tempIds.Add(aveWFInstance.Id);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_GetItemInstanceException, e.Message);
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
                #region Performance Monitor Region
                mPerformanceMonitor.WriteMonitorLog("**********************************************************************************");
                mPerformanceMonitor.WriteMonitorLog(monitor, " started: " + instanceId);
                mPerformanceMonitor.StartMonitor(monitor);
                #endregion

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
                    mInnerExceptions.Add(procException);
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.IP_BackupInstanceException, instanceId.ToString(), e.Message);
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

                if (mInnerExceptions.Count > 0)
                    mExceptions.Add(instanceId, mInnerExceptions);
            }

            #region Performance Monitor Region
            mPerformanceMonitor.RemoveMonitor(monitor);
            #endregion

            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InstanceMainBackup");
            return rlt;
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
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceBackupSelfError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupInstanceSelfByAPI");
            }
        }
        #endregion

        #region ************************Restore Region************************

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
                    try
                    {
                        if (procException.ErrorCode == 9999)
                        {
                            SPWorkflowProcessorRuntime.OnCacheData(this.mParentItem.SiteID.ToString(), this.mParentItem.WebID.ToString(), this.mParentItem.ListID.ToString(), this.mParentItem.ListID.ToString(), this.mParentItem.ItemID, unit.InstanceItem.Properties["#Id"].ToString(), cacheData);
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
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.GetParentAssociationError, e);
                }
                #endregion

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
                throw;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceException, unit.InstanceItem.Properties.GetEx("#Id").ToString(), e.Message);
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

        private void InitializeFixupParams(SPWFInstanceUnit unit)
        {
            try
            {
                SPWorkflowSubItemUnit instanceItem = unit.InstanceItem;
                unit.FixupParameters.Dispose();
                unit.FixupParameters.mInstanceIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#Id"), Guid.NewGuid());
                unit.FixupParameters.mWebApplicationIdDic.AddEx(Guid.Empty, mParentItem.Web.Site.WebApplication.ID);
                unit.FixupParameters.mParentAssoicationIdDic.AddEx((Guid)instanceItem.Properties.GetEx("#TemplateId"), unit.ParentAssociationUnit.SerializableData.mId);
                unit.FixupParameters.mParentAssociationBaseIdDic.AddEx(Guid.Empty, unit.ParentAssociationUnit.SerializableData.mBaseId);
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
                    unit.FixupParameters.mItemGuidDic.AddEx((Guid)instanceItem.Properties.GetEx("#ItemGUID"), new Guid((string)mParentItem.ListItem["GUID"]));
                }
                // unit.FixupParameters.mInternalStateDic.AddEx((int)instanceItem.Properties.GetEx("#InternalState"), 0);
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InitializeFixupParamError, e);
            }
        }

        /// <summary>
        /// When call CreateWorkflow function failed, this function will be called to get or create the source status field.
        /// There will be a problem if the source status field is using by another workflow associaion
        /// </summary>
        /// <param name="instanceUnit"></param>
        /// <returns></returns>
        


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

                if (Environment.SharePointVersion == SharePointVersion.SharePoint2007)
                {
                    throw new NotSupportedException("Cannot support exception(2007).");
                }
                else if (Environment.SharePointVersion == SharePointVersion.SharePoint2010)
                {
                    throw new NotSupportedException("Cannot support exception(2010).");
                }
                else if (Environment.SharePointVersion == SharePointVersion.SharePoint2013)
                {
                    var payLoad = new Dictionary<string, object>();
                    Guid newWFInstanceID = WFInstanceService.StartWorkflowOnListItem(unit.ParentAssociationUnit.WorkflowSubscription, mParentItem.ItemID, payLoad);
                    mInstance13Model = WFInstanceService.GetInstance(newWFInstanceID);
                }
                else
                {
                    throw new NotSupportedException("Cannot support exception(Unkown platform).");
                    return;
                }

                unit.FixupParameters.mInstanceIdDic.AddEx((Guid)unit.InstanceItem.Properties.GetEx("#Id"), this.mInstance13Model.Id);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_CreateInstanceException, e.Message);
                InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.CreateInstanceUnknownException, e));
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "CreateWorkflow");
            }

        }

        private int RestoreWorkflowSubItem(SPWFInstanceUnit unit)
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
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ResotreWFSubItemError, e.ToString());
                throw;
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreWorkflowSubItem");
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
                    System.Diagnostics.Trace.WriteLine(e.ToString());
                }

                return 0;
            }
            catch (SPWFProcessorException procException)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceSelfException, procException.Message);
                throw;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_RestoreInstanceSelfException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreSelfError, e);
            }
            finally
            {
                mPerformanceMonitor.WriteMonitorLog(mMainMonitorLog, " --> ", monitor, ". Duration: ", mPerformanceMonitor.GetCurrentDuration(monitor));
                mPerformanceMonitor.RemoveMonitor(monitor);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreInstance");
            }
        }

        private int ProcessWorkflowStatus(SPWFInstanceUnit unit)
        {
            if (SPWorkflowProcessorRuntime.RestoreHistoryOnly)
            {
                if (mInstance13Model != null && mInstance13Model.Status != AveWorkflowStatus13Model.Completed)
                {
                    WFInstanceService.CancelWorkflow(mInstance13Model);
                }
                if (SPWorkflowProcessorRuntime.RestartRunningInstance && (AveWorkflowStatus13Model)mInstanceUnit.InstanceItem.Properties["Status"] == AveWorkflowStatus13Model.Started)
                {
                    System.Threading.Thread.Sleep(SPWorkflowProcessorRuntime.PauseTimeBeforeRestartWorkflow * 1000);
                    CreateWorkflow(unit, AveWorkflowRunOptions.Asynchronous);
                }
                return 0;
            }
            return 1;
        }
        #endregion
    }
}
