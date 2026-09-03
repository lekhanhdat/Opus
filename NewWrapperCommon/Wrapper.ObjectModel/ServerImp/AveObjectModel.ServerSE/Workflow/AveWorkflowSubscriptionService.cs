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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWorkflowSubscriptionService : IAveWorkflowSubscriptionService
    {
        private WorkflowSubscriptionService mWorkflowSubscriptionService = null;

        public AveWorkflowSubscriptionService(WorkflowSubscriptionService service) 
        {
            mWorkflowSubscriptionService = service;
        }

        public void DeleteSubscription(Guid subscriptionId) 
        {
            mWorkflowSubscriptionService.DeleteSubscription(subscriptionId);
        }

        public IAveWorkflowSubscriptionCollection EnumerateSubscriptions() 
        {
            return new AveWorkflowSubscriptionCollection(mWorkflowSubscriptionService.EnumerateSubscriptions());
        }

        public IAveWorkflowSubscriptionCollection EnumerateSubscriptionsByDefinition(Guid definitionId) 
        {
            return new AveWorkflowSubscriptionCollection(mWorkflowSubscriptionService.EnumerateSubscriptionsByDefinition(definitionId));
        }

        public IAveWorkflowSubscriptionCollection EnumerateSubscriptionsByEventSource(Guid eventSourceId) 
        {
            return new AveWorkflowSubscriptionCollection(mWorkflowSubscriptionService.EnumerateSubscriptionsByEventSource(eventSourceId));
        }

        public IAveWorkflowSubscriptionCollection EnumerateSubscriptionsByList(Guid listId) 
        {
            return new AveWorkflowSubscriptionCollection(mWorkflowSubscriptionService.EnumerateSubscriptionsByList(listId));
        }

        public IAveWorkflowSubscription GetSubscription(Guid subscriptionId) 
        {
            return new AveWorkflowSubscription(mWorkflowSubscriptionService.GetSubscription(subscriptionId));
        }

        public Guid PublishSubscription(IAveWorkflowSubscription subscription)
        {
            return mWorkflowSubscriptionService.PublishSubscription(((AveWorkflowSubscription)subscription).WorkflowSubScription);
        }

        public Guid PublishSubscriptionForList(IAveWorkflowSubscription subscription, Guid listId) 
        {
            return mWorkflowSubscriptionService.PublishSubscriptionForList(((AveWorkflowSubscription)subscription).WorkflowSubScription, listId);
        }

        public void RegisterInterestInList(Guid listId, string eventName) 
        {
            mWorkflowSubscriptionService.RegisterInterestInList(listId, eventName);
        }

        public void UnregisterInterestInList(Guid listId, string eventName) 
        {
            mWorkflowSubscriptionService.UnregisterInterestInList(listId, eventName);
        }

        //public static AveWorkflowSubscriptionService Current
        //{
        //    get;
        //}
    }
}
