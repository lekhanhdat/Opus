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
using System.Net;

namespace AvePoint.ObjectModel.Common
{
    class AveWorkflowServicesManager : AveClientObject, IAveWorkflowServicesManager
    {
        private AveWeb mWeb;
        private IAveRequest mRequest;
        private static string mWebUrl;
        private static IAveWorkflowServicesManager defaultWorkflowserviceManager;

        private AveWorkflowServicesManager(IAveWeb web)
        {
            mWeb = web as AveWeb;
            mWebUrl = mWeb.ServerRelativeUrl;
            mRequest = ((AveSite)mWeb.Site).Request as IAveRequest;
            base.DataCache.AddPropertyies(mRequest.GetWorkflowServicesManager(mWeb.ServerRelativeUrl));
        }

        public static IAveWorkflowServicesManager CreateWorkflowServiceManager(IAveWeb web)
        {
            if (null == defaultWorkflowserviceManager || !String.Equals(mWebUrl, web.ServerRelativeUrl))
            {
                defaultWorkflowserviceManager = new AveWorkflowServicesManager(web);
            }
            return defaultWorkflowserviceManager;
        }

        public IAveWorkflowDeploymentService GetWorkflowDeploymentService() 
        {
            return new AveWorkflowDeploymentService(mWeb);
        }

        public IAveWorkflowInstanceService GetWorkflowInstanceService()
        {
            return null;
        }

        public IAveWorkflowSubscriptionService GetWorkflowSubscriptionService() 
        {
            return new AveWorkflowSubscriptionService(mWeb);
        }



        public string AppId 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("AppId");
            }
        }

        public bool IsConnected 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("IsConnected");
            }
            set 
            {
                base.DataCache.AddChangedProperty("IsConnected", value);
            }
        }
        public bool IsPartitioned
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("IsPartitioned");
            }
            set 
            {
                base.DataCache.AddChangedProperty("IsPartitioned", value);
            }
        }
        public string ScopePath 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("ScopePath");
            }
        }
        public Uri WorkflowServiceAddress 
        {
            get 
            {
                return base.DataCache.GetProperty<Uri>("WorkflowServiceAddress");
            }
        }
    }
}
