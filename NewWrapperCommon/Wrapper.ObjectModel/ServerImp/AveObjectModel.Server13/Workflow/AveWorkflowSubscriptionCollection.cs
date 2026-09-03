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

using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.WorkflowServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Server13
{
    class AveWorkflowSubscriptionCollection : AveAbstractCommonCollection<IAveWorkflowSubscription>, IAveWorkflowSubscriptionCollection
    {
        private WorkflowSubscriptionCollection mWorkflowSubscriptionCollection = null;

        protected override object CreatElementInstance(object t)
        {
            return new AveWorkflowSubscription(t as WorkflowSubscription);
        }

        public override int Count
        {
            get { return this.mWorkflowSubscriptionCollection.Count; }
        }

        public override IAveWorkflowSubscription this[int index]
        {
            get
            {
                return new AveWorkflowSubscription(this.mWorkflowSubscriptionCollection[index]);
            }
        }
        //public AveWorkflowSubscriptionCollection() :
        //{
        //    mWorkflowSubscriptionCollection = new WorkflowSubscriptionCollection();
        //}

        //public AveWorkflowSubscriptionCollection(IList<AveWorkflowSubscription> list) 
        //{
        //    IList<WorkflowSubscription> tempList = new List<WorkflowSubscription>();
        //    foreach(AveWorkflowSubscription subscription in list)
        //    {
        //        tempList.Add(subscription.WorkflowSubScription);
        //    }
        //    this.mWorkflowSubscriptionCollection = new WorkflowSubscriptionCollection(tempList);
        //}

        public AveWorkflowSubscriptionCollection(WorkflowSubscriptionCollection collection) :
            base(collection)
        {
            this.mWorkflowSubscriptionCollection = collection; 
        }
        public void Sort() 
        {
            mWorkflowSubscriptionCollection.Sort();
        }
        public IAveWorkflowSubscription GetSubscriptionByName(string name)
        {
            IAveWorkflowSubscriptionCollection subscriptionCollection = new AveWorkflowSubscriptionCollection(this.mWorkflowSubscriptionCollection);
            foreach (IAveWorkflowSubscription subscription in subscriptionCollection)
            {
                if(subscription.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return subscription;
                }
            }
            return null;
        }
    }
}
