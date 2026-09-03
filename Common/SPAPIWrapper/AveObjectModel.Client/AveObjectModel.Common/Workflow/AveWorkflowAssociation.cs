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
using System.Text;
using System.Collections;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    class AveWorkflowAssociation : AveClientObject, IAveWorkflowAssociation
    {
        private string mWorkfolwSource;

        private AveWeb mWeb;
        private AveList mList;

        private IAveRequest mRequest;

        public AveWorkflowAssociation() 
        {

        }
        public AveWorkflowAssociation(IAveWeb web, IAveList list, string workflowSource, IDictionary<string, object> prop)
        {
            mList = (AveList)list;
            mWeb = (AveWeb)web;
            mRequest = ((AveSite)(mWeb.Site)).Request;
            mWorkfolwSource = workflowSource;
            base.DataCache.AddPropertyies(prop);
        }

        #region IAveWorkflowAssociation Members

        public bool AllowManual
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowManual");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowManual", value);
            }
        }

        public bool AutoStartChange
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AutoStartChange");
            }
            set
            {
                base.DataCache.AddChangedProperty("AutoStartChange", value);
            }
        }

        public bool AutoStartCreate
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AutoStartCreate");
            }
            set
            {
                base.DataCache.AddChangedProperty("AutoStartCreate", value);
            }
        }

        public bool Enabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Enabled");
            }
            set
            {
                base.DataCache.AddChangedProperty("Enabled", value);
            }
        }

        public Guid Id
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }

        public AveBasePermissions PermissionsManual
        {
            get
            {
                return base.DataCache.GetProperty<AveBasePermissions>("PermissionsManual");
            }
            set
            {
                base.DataCache.AddChangedProperty("PermissionsManual", value);
            }
        }

        public int RunningInstances
        {
            get
            {
                return base.DataCache.GetProperty<int>("RunningInstances");
            }
        }

        public IAveWorkflowAssociation CreateWebAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList)
        {
            Dictionary<string, object> cacheDic = new Dictionary<string, object>();
            cacheDic.Add("HistoryListId", historyList.ID);
            cacheDic.Add("TaskListId", taskList.ID);
            cacheDic.Add("BaseTemplate", baseTemplate);
            cacheDic.Add("Name", name);
            base.DataCache.AddPropertyies(cacheDic);
            return this;
        }

        public IAveWorkflowAssociation CreateWebContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, string taskListName, string historyListName)
        {
            throw new NotImplementedException();
        }

        public void SetHistoryList(IAveList list)
        {
            if (this.HistoryListId != list.ID)
            {
                this.HistoryListId = list.ID;
                this.HistoryListTitle = list.Title;
            }
        }

        public void SetTaskList(IAveList list)
        {
            if (this.TaskListId != list.ID)
            {
                this.TaskListId = list.ID;
                this.TaskListTitle = list.Title;
            }

        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                base.DataCache.AddChangedProperty("Description", value);
            }
        }

        #endregion


        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }

        public Guid ID
        {
            get { return base.DataCache.GetProperty<Guid>("Id"); }
        }

        public string InternalNameStatusField
        {
            get
            {
                return base.DataCache.GetProperty<string>("InternalNameStatusField");
            }
            set
            {
                base.DataCache.AddChangedProperty("InternalNameStatusField", value);
            }
        }


        public IAveWorkflowAssociation CreateListAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList)
        {
            Dictionary<string, object> cacheDic = new Dictionary<string, object>();
            cacheDic.Add("HistoryListId", historyList.ID);
            cacheDic.Add("TaskListId", taskList.ID);
            cacheDic.Add("BaseTemplate", baseTemplate);
            cacheDic.Add("Name", name);
            base.DataCache.AddPropertyies(cacheDic);
            return this;
        }

        public IAveWorkflowAssociation CreateListContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, IAveList taskList, IAveList historyList)
        {
            Dictionary<string, object> cacheDic = new Dictionary<string, object>();
            cacheDic.Add("HistoryListId", historyList.ID);
            cacheDic.Add("TaskListId", taskList.ID);
            cacheDic.Add("BaseTemplate", baseTemplate);
            cacheDic.Add("Name", name);
            base.DataCache.AddPropertyies(cacheDic);
            return this;
        }

        public IAveWorkflowAssociation CreateSiteContentTypeAssociation(IAveWorkflowTemplate baseTemplate, string name, string strTaskList, string strHistoryList)
        {
            Dictionary<string, object> cacheDic = new Dictionary<string, object>();
            cacheDic.Add("HistoryListTitle", strHistoryList);
            cacheDic.Add("TaskListTitle", strTaskList);
            cacheDic.Add("BaseTemplate", baseTemplate);
            cacheDic.Add("Name", name);
            base.DataCache.AddPropertyies(cacheDic);
            return this;
        }
        
        public string ExportToXml()
        {
            return string.Empty;
        }

        public string AssociationData
        {
            get
            {
                return base.DataCache.GetProperty<string>("AssociationData");
            }
            set
            {
                base.DataCache.AddChangedProperty("AssociationData", value);
            }
        }

        public Guid BaseId
        {
            get
            { 
                return base.DataCache.GetProperty<Guid>("BaseId"); 
            }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                return this.mWeb;
            }
        }

        public int Author
        {
            get 
            {
                return base.DataCache.GetProperty<int>("Author"); 
            }
        }

        public int AutoCleanupDays
        {
            get
            {
                return base.DataCache.GetProperty<int>("AutoCleanupDays"); 
            }
            set
            {
                base.DataCache.AddChangedProperty("AutoCleanupDays", value);
            }
        }

        public DateTime Created
        {
            get 
            {
                return base.DataCache.GetProperty<DateTime>("Created");
            }
        }

        public IAveList ParentList
        {
            get 
            {
                return mList;
            }
        }

        public Guid HistoryListId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("HistoryListId");
            }
            private set
            {
                base.DataCache.AddChangedProperty("HistoryListId", value);
            }
        }

        public string HistoryListTitle
        {
            get
            {
                return base.DataCache.GetProperty<string>("HistoryListTitle");
            }
            set
            {
                base.DataCache.AddChangedProperty("HistoryListTitle", value);
            }
        }

        public DateTime Modified
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Modified");
            }
        }

        public Guid TaskListId
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("TaskListId");
            }
            private set
            {
                base.DataCache.AddChangedProperty("TaskListId", value);
            }
        }

        public string TaskListTitle
        {
            get
            {
                return base.DataCache.GetProperty<string>("TaskListTitle");
            }
            set
            {
                base.DataCache.AddChangedProperty("TaskListTitle", value);
            }
        }

        public bool IsDeclarative
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("IsDeclarative");
            }
        }

        public string InternalName
        {
            get
            {
                return base.DataCache.GetProperty<string>("InternalName");
            }
        }

        public bool AllowAsyncManualStart
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowAsyncManualStart");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowAsyncManualStart", value);
            }
        }

        public bool MarkedForDelete
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MarkedForDelete");
            }
            set
            {
                base.DataCache.AddChangedProperty("MarkedForDelete", value);
            }
        }

        public IAveWorkflowTemplate BaseTemplate
        {
            get 
            {
                return base.DataCache.GetProperty<IAveWorkflowTemplate>("BaseTemplate");
            }
        }

        public Wrapper.Common.AveWorkflowAssociationCollection.Configuration Configuration
        {
            get
            {
                //return base.DataCache.GetProperty<Wrapper.Common.AveWorkflowAssociationCollection.Configuration>("Configuration");
                AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration cfg = Wrapper.Common.AveWorkflowAssociationCollection.Configuration.None;
                if(this.AllowManual)
                {
                    cfg = cfg | Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AllowManualStart;
                }
                if (this.AllowAsyncManualStart)
                {
                    cfg = cfg | Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AllowAsyncManualStart;
                }
                if (this.AutoStartChange) 
                {
                    cfg = cfg | Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartChange;
                }
                if (this.AutoStartCreate) 
                {
                    cfg = cfg | Wrapper.Common.AveWorkflowAssociationCollection.Configuration.AutoStartAdd;
                }
                return cfg;
            }
            set
            {
                base.DataCache.AddChangedProperty("Configuration", value);
            }
        }

        public IAveContentTypeId ContentTypeId
        {
            get
            {
                if (base.DataCache.IsPropertyAvailable("ContentTypeId"))
                {
                    return base.DataCache.GetProperty<IAveContentTypeId>("ContentTypeId");
                }
                return new AveContentTypeId();
            }
        }

        public int Version
        {
            get 
            {
                return base.DataCache.GetProperty<int>("Version");
            }
        }

        public Hashtable MetaData
        {
            get 
            {
                return base.DataCache.GetProperty<Hashtable>("MetaData");
            }
        }

        #region IAveWorkflowAssociation Members


        public bool CompressInstanceData
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("CompressInstanceData");
            }
        }

        #endregion

        public Guid ParentAssociationId
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("ParentAssociationId");
            }
        }

        internal void Update() 
        {
            mRequest.UpdateWorkflowAssociation(mWeb.ServerRelativeUrl, mList.Title, mList.ID, ContentTypeId.ToString(), Id, "list.workflows", base.DataCache.ChangedProperties);
        }

        internal void Update(string ctID)
        {
            if (mList != null)
            {
                mRequest.UpdateWorkflowAssociation(mWeb.ServerRelativeUrl, mList.Title, mList.ID, ctID, Id, "contentType.workflows", base.DataCache.ChangedProperties);
            }
            else
            {
                mRequest.UpdateWorkflowAssociation(mWeb.ServerRelativeUrl, null, Guid.Empty, ctID, Id, "contentType.workflows", base.DataCache.ChangedProperties);
            }
        }
    }
}
