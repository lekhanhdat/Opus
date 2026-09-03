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
    class AveWorkflowInstanceService : IAveWorkflowInstanceService
    {
        private WorkflowInstanceService mWorkflowInstanceService = null;

        internal AveWorkflowInstanceService(WorkflowInstanceService workflowInstanceService)
        {
            mWorkflowInstanceService = workflowInstanceService;
        }

        public void CancelWorkflow(IAveWorkflowInstance instance) 
        {
            AveWorkflowInstance workflowInstance = (AveWorkflowInstance)instance;
            mWorkflowInstanceService.CancelWorkflow(workflowInstance.WFInstance);
        }

        public int CountInstances(IAveWorkflowSubscription parentSubscription)
        {
            return mWorkflowInstanceService.CountInstances(((AveWorkflowSubscription)parentSubscription).WorkflowSubScription);
        }

        public int CountInstancesWithStatus(IAveWorkflowSubscription parentSubscription, AveWorkflowStatus status) 
        {
            return mWorkflowInstanceService.CountInstancesWithStatus(((AveWorkflowSubscription)parentSubscription).WorkflowSubScription,(WorkflowStatus) Enum.Parse(typeof(WorkflowStatus), status.ToString()));
        }

        public IAveWorkflowInstanceCollection Enumerate(IAveWorkflowSubscription parentSubscription) 
        {
            WorkflowInstanceCollection collection = mWorkflowInstanceService.Enumerate(((AveWorkflowSubscription)parentSubscription).WorkflowSubScription);
            return new AveWorkflowInstanceCollection(collection);
        }

        public IAveWorkflowInstanceCollection Enumerate(IAveWorkflowSubscription parentSubscription, int offset) 
        {
            WorkflowInstanceCollection collection = mWorkflowInstanceService.Enumerate(((AveWorkflowSubscription)parentSubscription).WorkflowSubScription, offset);
            return new AveWorkflowInstanceCollection(collection);
        }

        public IAveWorkflowInstanceCollection EnumerateInstancesForListItem(Guid listId, int itemId) 
        {
            WorkflowInstanceCollection collection = mWorkflowInstanceService.EnumerateInstancesForListItem(listId, itemId);
            return new AveWorkflowInstanceCollection(collection);
        }

        public IAveWorkflowInstanceCollection EnumerateInstancesForListItem(Guid listId, int itemId, int offset) 
        {
            WorkflowInstanceCollection collection = mWorkflowInstanceService.EnumerateInstancesForListItem(listId, itemId, offset);
            return new AveWorkflowInstanceCollection(collection);
        }

        public IAveWorkflowInstanceCollection EnumerateInstancesForSite()
        {
            WorkflowInstanceCollection collection = mWorkflowInstanceService.EnumerateInstancesForSite();
            return new AveWorkflowInstanceCollection(collection);
        }

        public IAveWorkflowInstanceCollection EnumerateInstancesForSite(int offset) 
        {
            WorkflowInstanceCollection collection = mWorkflowInstanceService.EnumerateInstancesForSite(offset);
            return new AveWorkflowInstanceCollection(collection);
        }

        public string GetDebugInfo(IAveWorkflowInstance instance)
        {
            AveWorkflowInstance aveWFInstance = (AveWorkflowInstance)instance;
            return mWorkflowInstanceService.GetDebugInfo(aveWFInstance.WFInstance);
        }

        public IAveWorkflowInstance GetInstance(Guid instanceId) 
        {
            return new AveWorkflowInstance(mWorkflowInstanceService.GetInstance(instanceId));
        }

        public void PublishCustomEvent(IAveWorkflowInstance instance, string eventName, string payload) 
        {
            mWorkflowInstanceService.PublishCustomEvent(((AveWorkflowInstance)instance).WFInstance, eventName, payload);
        }

        public Guid StartWorkflow(IAveWorkflowSubscription subscription, IDictionary<string, object> payload) 
        {
            return mWorkflowInstanceService.StartWorkflow(((AveWorkflowSubscription)subscription).WorkflowSubScription, payload);
        }

        public Guid StartWorkflowOnListItem(IAveWorkflowSubscription subscription, int itemId, IDictionary<string, object> payload) 
        {
            return mWorkflowInstanceService.StartWorkflowOnListItem(((AveWorkflowSubscription)subscription).WorkflowSubScription, itemId, payload);
        }

        public void TerminateWorkflow(IAveWorkflowInstance instance) 
        {
            mWorkflowInstanceService.TerminateWorkflow(((AveWorkflowInstance)instance).WFInstance);
        }
    }
}
