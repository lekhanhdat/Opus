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

namespace AvePoint.ObjectModel.Server19
{
    class AveWorkflowTaskCollection : AveAbstractCommonCollection<IAveWorkflowTask>, IAveWorkflowTaskCollection
    {
        private SPWorkflowTaskCollection mWorkflowTaskCollection;

        private IAveWeb mParentWeb;

        internal IAveWeb ParentWeb
        {
            get { return mParentWeb; }
            set { mParentWeb = value; }
        }

        //[Obsolete]
        //public AveWorkflowTaskCollection(SPWorkflowTaskCollection workflowTaskCollection)
        //    : base(workflowTaskCollection)
        //{
        //    mWorkflowTaskCollection = workflowTaskCollection;
        //}

        public AveWorkflowTaskCollection(IAveWeb parentWeb, SPWorkflowTaskCollection workflowTaskCollection)
            : base(workflowTaskCollection)
        {
            mParentWeb = parentWeb;
            mWorkflowTaskCollection = workflowTaskCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            SPWorkflowTask task = t as SPWorkflowTask;
            //AveSite site = new AveSite(task.Web.Site);
            //AveWeb web = new AveWeb(site,task.Web);
            AveList list = (ParentWeb.Lists as AveListCollection).CreateListByType(task.ParentList);
            AveWorkflowInternalTaskCollection workflowInternalTasks = new AveWorkflowInternalTaskCollection(list, task.ListItems);
            return new AveWorkflowTask(workflowInternalTasks, t as SPWorkflowTask);
        }

        public override int Count
        {
            get 
            {
                return mWorkflowTaskCollection.Count;
            }
        }
    }
}
