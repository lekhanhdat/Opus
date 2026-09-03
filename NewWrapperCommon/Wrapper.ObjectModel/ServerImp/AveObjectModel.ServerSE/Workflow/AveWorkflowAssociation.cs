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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using AvePoint.Wrapper.Common;
using System.Collections;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWorkflowAssociation : AveAutoSerializingObject, IAveWorkflowAssociation
    {
        private SPWorkflowAssociation mWorkflowAssociation;

        private IAveSite mParentSite;
        private IAveWeb mParentWeb;
        private IAveList mParentList;

        protected IAveWorkflowAssociationCollection mWorkflowAssociations;

        //[Obsolete]
        //public AveWorkflowAssociation(SPWorkflowAssociation workflowAssociation)
        //    : base(workflowAssociation)
        //{
        //    mWorkflowAssociation = workflowAssociation;
        //}

        public AveWorkflowAssociation(IAveWorkflowAssociationCollection associationCollection, SPWorkflowAssociation workflowAssociation)
        {
            mWorkflowAssociations = associationCollection;
            mWorkflowAssociation = workflowAssociation;
            if (associationCollection != null)
            {
                mParentSite = (associationCollection as AveWorkflowAssociationCollection).ParentSite;
                mParentWeb = (associationCollection as AveWorkflowAssociationCollection).ParentWeb;
                mParentList = (associationCollection as AveWorkflowAssociationCollection).ParentList;
            }
        }

        public AveWorkflowAssociation()
        { }

        internal SPWorkflowAssociation WorkflowAssociation
        {
            get
            {
                return mWorkflowAssociation;
            }
        }

        #region IAveWorkflowAssociation Members

        public bool AllowManual
        {
            get
            {
                return mWorkflowAssociation.AllowManual;
            }
            set
            {
                mWorkflowAssociation.AllowManual = value;
            }
        }

        public bool AutoStartChange
        {
            get
            {
                return mWorkflowAssociation.AutoStartChange;
            }
            set
            {
                mWorkflowAssociation.AutoStartChange = value;
            }
        }

        public bool AutoStartCreate
        {
            get
            {
                return mWorkflowAssociation.AutoStartCreate;
            }
            set
            {
                mWorkflowAssociation.AutoStartCreate = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return mWorkflowAssociation.Enabled;
            }
            set
            {
                mWorkflowAssociation.Enabled = value;
            }
        }

        public Guid ID
        {
            get
            {
                return mWorkflowAssociation.Id;
            }
        }

        public string Name
        {
            get
            {
                return mWorkflowAssociation.Name;
            }
            set
            {
                mWorkflowAssociation.Name = value;
            }
        }

        public AveBasePermissions PermissionsManual
        {
            get
            {
                return (AveBasePermissions)mWorkflowAssociation.PermissionsManual;
            }
            set
            {
                mWorkflowAssociation.PermissionsManual = (SPBasePermissions)value;
            }
        }

        public int RunningInstances
        {
            get { return mWorkflowAssociation.RunningInstances; }
        }

        public void SetHistoryList(IAveList list)
        {
            mWorkflowAssociation.SetHistoryList((list as AveList).List);
        }

        public void SetTaskList(IAveList list)
        {
            mWorkflowAssociation.SetTaskList((list as AveList).List);
        }

        public string ExportToXml()
        {
            return mWorkflowAssociation.ExportToXml();
        }

        public string Description
        {
            get
            {
                return mWorkflowAssociation.Description;
            }
            set
            {
                mWorkflowAssociation.Description = value;
            }
        }

        public IAveWorkflowAssociation CreateWebAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList)
        {
            return new AveWorkflowAssociation(null,SPWorkflowAssociation.CreateWebAssociation((baseTemplate as AveWorkflowTemplate).WorkflowTemplate, name, (taskList as AveList).List, (historyList as AveList).List));
        }

        public IAveWorkflowAssociation CreateWebContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, string taskListName, string historyListName)
        {
            return new AveWorkflowAssociation(null, SPWorkflowAssociation.CreateWebContentTypeAssociation((baseTemplate as AveWorkflowTemplate).WorkflowTemplate, name, taskListName, historyListName));
        }

        public IAveWorkflowAssociation CreateListAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList)
        {
            SPWorkflowAssociation association = SPWorkflowAssociation.CreateListAssociation((baseTemplate as AveWorkflowTemplate).WorkflowTemplate, name, (taskList as AveList).List, (historyList as AveList).List);
            if (association == null)
            {
                return null;
            }
            return new AveWorkflowAssociation(null, association);
        }

        public IAveWorkflowAssociation CreateListContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList)
        {
            SPWorkflowAssociation association = SPWorkflowAssociation.CreateListContentTypeAssociation((baseTemplate as AveWorkflowTemplate).WorkflowTemplate, name, (taskList as AveList).List, (historyList as AveList).List);
            if (association == null)
            {
                return null;
            }
            return new AveWorkflowAssociation(null, association);
        }

        public IAveWorkflowAssociation CreateSiteContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, string strTaskList, string strHistoryList)
        {
            SPWorkflowAssociation association = SPWorkflowAssociation.CreateSiteContentTypeAssociation((baseTemplate as AveWorkflowTemplate).WorkflowTemplate, name, strTaskList, strHistoryList);
            if (association == null)
            {
                return null;
            }
            return new AveWorkflowAssociation(null, association);
        }

        public string AssociationData
        {
            get
            {
                return mWorkflowAssociation.AssociationData;
            }
            set
            {
                mWorkflowAssociation.AssociationData = value;
            }
        }

        public Guid BaseId
        {
            get
            {
                return mWorkflowAssociation.BaseId;
            }
        }

        //private IAveWeb mParentWeb;

        public IAveWeb ParentWeb
        {
            get
            {
                //if (mParentWeb == null)
                //{
                    //mParentWeb现在不可能为null，在初始化AveWorkflow时mParentWeb已经赋值
                    //mParentWeb = new AveWeb(new AveSite(mWorkflowAssociation.ParentWeb.Site), mWorkflowAssociation.ParentWeb);
                //}
                return mParentWeb;
            }
        }

        public int Author
        {
            get
            {
                return mWorkflowAssociation.Author;
            }
        }

        public int AutoCleanupDays
        {
            get
            {
                return mWorkflowAssociation.AutoCleanupDays;
            }
            set
            {
                mWorkflowAssociation.AutoCleanupDays = value;
            }
        }

        public DateTime Created
        {
            get
            {
                return mWorkflowAssociation.Created;
            }
        }

        //private IAveList mParentList;

        public IAveList ParentList
        {
            get
            {
                return mParentList;
                //if (mWorkflowAssociation.ParentList == null)
                //{
                //    return null;
                //}
                //if (mParentList == null)
                //{
                //    mParentList = (new AveListCollection(ParentWeb as AveWeb, mWorkflowAssociation.ParentList.Lists)).CreateListByType(mWorkflowAssociation.ParentList);
                //}
                //return mParentList;
            }
        }

        public Guid HistoryListId
        {
            get
            {
                return mWorkflowAssociation.HistoryListId;
            }
        }

        public string HistoryListTitle
        {
            get
            {
                return mWorkflowAssociation.HistoryListTitle;
            }
            set
            {
                mWorkflowAssociation.HistoryListTitle = value;
            }
        }

        public DateTime Modified
        {
            get
            {
                return mWorkflowAssociation.Modified;
            }
        }

        public Guid TaskListId
        {
            get
            {
                return mWorkflowAssociation.TaskListId;
            }
        }

        public string TaskListTitle
        {
            get
            {
                return mWorkflowAssociation.TaskListTitle;
            }
            set
            {
                mWorkflowAssociation.TaskListTitle = value;
            }
        }

        public bool IsDeclarative
        {
            get
            {
                return mWorkflowAssociation.IsDeclarative;
            }
        }

        public string InternalName
        {
            get
            {
                return mWorkflowAssociation.InternalName;
            }
        }

        public bool AllowAsyncManualStart
        {
            get
            {
                return mWorkflowAssociation.AllowAsyncManualStart;
            }
            set
            {
                mWorkflowAssociation.AllowAsyncManualStart = value;
            }
        }

        public bool MarkedForDelete
        {
            get
            {
                return mWorkflowAssociation.MarkedForDelete;
            }
            set
            {
                mWorkflowAssociation.MarkedForDelete = value;
            }
        }

        public IAveWorkflowTemplate BaseTemplate
        {
            get
            {
                return new AveWorkflowTemplate(ParentWeb.WorkflowTemplates, mWorkflowAssociation.BaseTemplate);
            }
        }


        public Wrapper.Common.AveWorkflowAssociationCollection.Configuration Configuration
        {
            get
            {
                return (Wrapper.Common.AveWorkflowAssociationCollection.Configuration)AveAssemblyUtility.GetPropertyValue(mWorkflowAssociation, "Configuration");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mWorkflowAssociation, "Configuration", (SPWorkflowAssociationCollection.Configuration)value);
            }
        }

        public IAveContentTypeId ContentTypeId
        {
            get
            {
                SPContentTypeId contentTypeId = (SPContentTypeId)AveAssemblyUtility.GetPropertyValue(mWorkflowAssociation, "ContentTypeId");
                if (contentTypeId == null)
                {
                    return null;
                }
                return new AveContentTypeId(contentTypeId);
            }
        }

        public int Version
        {
            get
            {
                return (int)AveAssemblyUtility.GetPropertyValue(mWorkflowAssociation, "Version");
            }
        }

        public string InternalNameStatusField
        {
            get
            {
                return (string)AveAssemblyUtility.GetPropertyValue(mWorkflowAssociation, "InternalNameStatusField");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mWorkflowAssociation, "InternalNameStatusField", value);
            }
        }

        public Hashtable MetaData
        {
            get
            {
                return (Hashtable)AveAssemblyUtility.GetPropertyValue(mWorkflowAssociation, "MetaData");
            }
        }

        public bool CompressInstanceData
        {
            get { return mWorkflowAssociation.CompressInstanceData; }
        }

        public Guid ParentAssociationId
        {
            get { return mWorkflowAssociation.ParentAssociationId; }
            set { AveAssemblyUtility.SetFieldValue(mWorkflowAssociation, "m_parentId", value); }
        }

        #endregion

        public void Dispose()
        {
            if (mParentWeb != null)
            {
                mParentWeb.Dispose();
                mParentWeb = null;
            }
        }
    }
}
