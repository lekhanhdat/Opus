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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Workflow;
using System.Collections.ObjectModel;

namespace AvePoint.ObjectModel.Server16
{
    class AveWorkflowCollection : AveAbstractCommonCollection<IAveWorkflow>, IAveWorkflowCollection
    {
        private SPWorkflowCollection mWorkflowCollection;

        private IAveListItem mParentItem;
        private Guid mAssociationId;
        private IAveList mParentList;
        private IAveWeb mParentWeb;
        private IAveSite mParentSite;

        internal IAveSite ParentSite
        {
            get { return mParentSite; }
            set { mParentSite = value; }
        }

        internal IAveWeb ParentWeb
        {
            get { return mParentWeb; }
            set { mParentWeb = value; }
        }

        internal IAveList ParentList
        {
            get { return mParentList; }
            set { mParentList = value; }
        }

        internal IAveListItem ParentItem
        {
            get { return mParentItem; }
            set { mParentItem = value; }
        }

        private AveWorkflowCollection(SPWorkflowCollection workflowCollection)
            : base(workflowCollection)
        {
            mWorkflowCollection = workflowCollection;
        }

        public AveWorkflowCollection(IAveList list, Guid associationId)
            :this(new SPWorkflowCollection((list as AveList).List, associationId))
        {
            mParentList = list;
            mParentWeb = list.ParentWeb;
            mParentSite = list.ParentWeb.Site;
            mAssociationId = associationId;
        }

        public AveWorkflowCollection(IAveWeb web)
            : this(new SPWorkflowCollection((web as AveWeb).Web))
        {
            mParentWeb = web;
            mParentSite = web.Site;
        }

        public AveWorkflowCollection(IAveListItem parentItem,SPWorkflowCollection workflowCollection)
            : this(workflowCollection)
        {
            mParentItem = parentItem;
            mParentList = parentItem.ParentList;
            mParentWeb = parentItem.ParentList.ParentWeb;
            mParentSite = parentItem.ParentList.ParentWeb.Site;
        }

        public AveWorkflowCollection(IAveWeb parentWeb, SPWorkflowCollection workflowCollection)
            : this(workflowCollection)
        {

            mParentWeb = parentWeb;
            mParentSite = parentWeb.Site;
        }

        #region IAveWorkflowCollection Members

        public override int Count
        {
            get
            {
                return mWorkflowCollection.Count;
            }
        }
        public string Xml
        {
            get { return mWorkflowCollection.Xml; }
        }

        public IAveWorkflow this[Guid instanceId]
        {
            get 
            {
                SPWorkflow wf = mWorkflowCollection[instanceId];
                return wf == null ? null : new AveWorkflow(this,wf);
            }
        }

        public Collection<Guid> GetInstanceIds()
        {
            return mWorkflowCollection.GetInstanceIds();
        }

        #endregion

        #region IEnumerable Members

        public new System.Collections.IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveWorkflow(this,t as SPWorkflow);
        }

        #endregion

    }
}
