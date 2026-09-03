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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server16
{
    class AveWorkflow : IAveWorkflow,IDisposable
    {
        private SPWorkflow mWorkflow;
        private AveUser mOwnerUser;
        private AveWeb mParentWeb;
        private AveUser mAuthorUser;
        private AveWorkflowTaskCollection mTasks;
        private IAveWorkflowCollection mWorkflowCollection;
        private IAveWorkflowAssociation mParentAssociation;

        //[Obsolete]
        //public AveWorkflow(SPWorkflow workflow)
        //{
        //    mWorkflow = workflow;
        //}

        public AveWorkflow(IAveWorkflowCollection workflowCollection,SPWorkflow workflow)
        {
            mWorkflowCollection = workflowCollection;
            mParentWeb = (workflowCollection as AveWorkflowCollection).ParentWeb as AveWeb;
            mWorkflow = workflow;
        }

        public AveWorkflow(IAveWorkflowAssociation parentAssociation, SPWorkflow workflow)
        {
            mParentAssociation = parentAssociation;
            mParentWeb = parentAssociation.ParentWeb as AveWeb;
            mWorkflow = workflow;
        }

        #region IAveWorkflow Members

        public Guid InstanceId
        {
            get
            {
                return mWorkflow.InstanceId;
            }
        }

        public DateTime Created
        {
            get
            {
                return mWorkflow.Created;
            }
        }

        public DateTime Modified
        {
            get
            {
                return mWorkflow.Modified;
            }
        }

        internal SPWorkflow Workflow
        {
            get
            {
                return mWorkflow;
            }
        }

        public Guid AssociationId
        {
            get
            {
                return mWorkflow.AssociationId;
            }
        }

        public IAveWorkflowAssociation ParentAssociation
        {
            get
            {
                if (mParentAssociation == null)
                {
                    mParentAssociation = GetParentAssociation();
                }
                return mParentAssociation; 
            }
        }

         [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Associatons is part of property name")]
        private IAveWorkflowAssociation GetParentAssociation()
        {
            if (mWorkflowCollection != null && (mWorkflowCollection as AveWorkflowCollection).ParentItem != null)
            {
                SPListItem parentItem=((mWorkflowCollection as AveWorkflowCollection).ParentItem as AveListItem).ListItem;
                if (parentItem != null 
                    &&!(bool)AveAssemblyUtility.GetPropertyValue(parentItem,"DirtyListWorkflowAssociatons")
                    &&!(bool)AveAssemblyUtility.GetPropertyValue(parentItem,"DirtyContentTypeWorkflowAssociatons"))
                {
                    var factoryCollection = new AveWorkflowAssociationCollection();
                    IAveWorkflowAssociation associationForListItemById = factoryCollection.GetAssociationForListItemById((mWorkflowCollection as AveWorkflowCollection).ParentItem, mWorkflow.AssociationId);
                    if (associationForListItemById != null)
                    {
                        return associationForListItemById;
                    }
                }
            }
            SPWorkflowAssociationCollection sPReadOnlyWorkflowAssociationCollection = AveAssemblyUtility.CreateInstance("Microsoft.SharePoint.Workflow.SPReadOnlyWorkflowAssociationCollection", new Type[] { typeof(SPWeb) }, new object[] { (ParentWeb as AveWeb).Web }) as SPWorkflowAssociationCollection;
            SPWorkflowAssociation asso = sPReadOnlyWorkflowAssociationCollection[mWorkflow.AssociationId];
            if (asso == null) return null;
            return new AveWorkflowAssociation(new AveWorkflowAssociationCollection(ParentWeb, sPReadOnlyWorkflowAssociationCollection), asso);
        }

        public IAveUser AuthorUser
        {
            get 
            {
                if (mAuthorUser == null)
                {
                    SPUser authorUser = mWorkflow.AuthorUser;
                    if (authorUser != null)
                    {
                        mAuthorUser = new AveUser((AveWeb)ParentWeb, authorUser);
                    }
                }
                return mAuthorUser;
            }
        }

        public bool IsCompleted
        {
            get { return mWorkflow.IsCompleted; }
        }

        public int ItemId
        {
            get { return mWorkflow.ItemId; }
        }

        public IAveUser OwnerUser
        {
            get
            {
                if (mOwnerUser == null)
                {
                    SPUser ownerUser = mWorkflow.OwnerUser;
                    if (ownerUser != null)
                    {
                        mOwnerUser = new AveUser((AveWeb)ParentWeb, ownerUser);
                    }
                }
                return mOwnerUser;
            }
        }

        public IAveWorkflowTaskCollection Tasks
        {
            get 
            {
                if (mTasks == null)
                {
                    mTasks = new AveWorkflowTaskCollection(ParentWeb,mWorkflow.Tasks);
                }
                return mTasks;
            }
        }

        public AveWorkflowState InternalState
        {
            get { return (AveWorkflowState)mWorkflow.InternalState; }
        }

        public IAveWeb ParentWeb
        {
            get 
            {
                //if (mParentWeb == null)
                //{
                    //mParentWeb现在不可能为null，在初始化AveWorkflow时mParentWeb已经赋值
                    //SPWeb parentWeb = mWorkflow.ParentWeb;
                    //if (parentWeb != null)
                    //{
                    //    mParentWeb = new AveWeb(new AveSite(parentWeb.Site),parentWeb);
                    //}
                //}
                return mParentWeb;
            }
        }

        public int GetIStatus(int i)
        {
            return (int)AveAssemblyUtility.InvokeMethod(mWorkflow, "GetIStatus", new Type[] { typeof(int) }, new object[] { i });
        }

        public string GetIStatusAsText(int iStatus)
        {
            return (string)AveAssemblyUtility.InvokeMethod(mWorkflow, "GetIStatusAsText", new Type[] { typeof(int) }, new object[] { iStatus });
        }

        #endregion

        public void Dispose()
        {
            //mParentWeb.Dispose();
            //parent web should not be disposed here
        }
    }
}
