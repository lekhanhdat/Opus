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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWorkflowServicesManager : IAveWorkflowServicesManager
    {
        private WorkflowServicesManager mWorkflowServiceManager = null;

        public AveWorkflowServicesManager(IAveWeb web) 
        {
            mWorkflowServiceManager = new WorkflowServicesManager(((AveWeb)web).Web);
        }
        public AveWorkflowServicesManager(AveWeb web, ICredentials credentials) 
        {
            mWorkflowServiceManager = new WorkflowServicesManager(web.Web, credentials);
        }
        // internal T GetProvider<T>() where T: SolutionProvider;
        public IAveWorkflowDeploymentService GetWorkflowDeploymentService() 
        {
            WorkflowDeploymentService workflowDeploymentService = mWorkflowServiceManager.GetWorkflowDeploymentService();
            if (workflowDeploymentService == null) 
            {
                return null;
            }
            return new AveWorkflowDeploymentService(workflowDeploymentService);
        }

        public IAveWorkflowInstanceService GetWorkflowInstanceService() 
        {
            WorkflowInstanceService workflowInstanceService = mWorkflowServiceManager.GetWorkflowInstanceService();
            if (workflowInstanceService == null) 
            {
                return null;
            }
            return new AveWorkflowInstanceService(workflowInstanceService);
        }

        public IAveWorkflowSubscriptionService GetWorkflowSubscriptionService() 
        {
            WorkflowSubscriptionService workflowSubscriptionService = mWorkflowServiceManager.GetWorkflowSubscriptionService();
            if (workflowSubscriptionService == null) 
            {
                return null;
            }
            return new AveWorkflowSubscriptionService(workflowSubscriptionService);
        }

        public string AppId 
        {
            get 
            {
                return mWorkflowServiceManager.AppId;
            }
        }

        //public static AveWorkflowServicesManager Current
        //{
        //    get;
        //}

        public bool IsConnected
        {
            get 
            {
                return mWorkflowServiceManager.IsConnected;
            }
            set 
            {
                mWorkflowServiceManager.IsConnected = value;
            }
        }

        public bool IsPartitioned 
        {
            get
            {
                return mWorkflowServiceManager.IsPartitioned;
            }
        }

        public string ScopePath 
        {
            get
            {
                return mWorkflowServiceManager.ScopePath;
            }
        }
        //internal bool SecurityEnabled { get; }
        //public IWorkflowService WorkflowService { get; }
        public Uri WorkflowServiceAddress
        {
            get 
            {
                return mWorkflowServiceManager.WorkflowServiceAddress;
            }
        }
    }
}
