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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveWorkflowSubscriptionService : AveClientObject, IAveWorkflowSubscriptionService
    {
        private IAveWeb mWeb;
        private IAveRequest mRequest;

        public AveWorkflowSubscriptionService(IAveWeb web) 
        {
            mWeb = web as AveWeb;
            mRequest = ((AveSite)mWeb.Site).Request as IAveRequest;
            //base.DataCache.AddPropertyies(mRequest.GetWorkflowServicesManager(mWeb.ServerRelativeUrl));
        }

        public void DeleteSubscription(Guid subscriptionId) 
        {

        }

        public IAveWorkflowSubscriptionCollection EnumerateSubscriptions() 
        {
            return null;
        }

        public IAveWorkflowSubscriptionCollection EnumerateSubscriptionsByDefinition(Guid definitionId) 
        {
            return null;
        }

        public IAveWorkflowSubscriptionCollection EnumerateSubscriptionsByEventSource(Guid eventSourceId) 
        {
            return new AveWorkflowSubscriptionCollection(mWeb, "eventSource.workflowSubscriptions", mRequest.EnumerateSubscriptionsByEventSource(mWeb.ServerRelativeUrl, eventSourceId));
        }

        public IAveWorkflowSubscriptionCollection EnumerateSubscriptionsByList(Guid listId) 
        {
            return new AveWorkflowSubscriptionCollection(mWeb, "list.workflowSubscriptions", mRequest.EnumerateSubscriptionsByList(mWeb.ServerRelativeUrl, listId));
        }

        public IAveWorkflowSubscription GetSubscription(Guid subscriptionId) 
        {
            return new AveWorkflowSubscription(mWeb, "list.workflowSubscription", mRequest.GetSubscription(mWeb.ServerRelativeUrl, subscriptionId));
        }

        public Guid PublishSubscription(IAveWorkflowSubscription subscription) 
        {
            return mRequest.PublishSubscription(mWeb.ServerRelativeUrl, subscription, Guid.Empty);
        }

        public Guid PublishSubscriptionForList(IAveWorkflowSubscription subscription, Guid listId) 
        {
            return mRequest.PublishSubscription(mWeb.ServerRelativeUrl, subscription, listId);
        }

        public void RegisterInterestInList(Guid listId, string eventName) 
        {

        }

        public void UnregisterInterestInList(Guid listId, string eventName) 
        {

        }
    }
}
