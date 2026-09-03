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

namespace AvePoint.ObjectModel.Server19
{
    class AveWorkflowInstance : IAveWorkflowInstance
    {
        private WorkflowInstance mWorkflowInstance = null;
        internal AveWorkflowInstance(WorkflowInstance workflowInstance)
        {
            mWorkflowInstance = workflowInstance;
        }

        internal WorkflowInstance WFInstance
        {
            get 
            {
                return mWorkflowInstance;
            }
        }

        public string CanonicalId 
        {
            get 
            {
                return string.Empty;
            }
        }

        public string FaultInfo 
        {
            get 
            {
                return mWorkflowInstance.FaultInfo;
            }
            set 
            {
                mWorkflowInstance.FaultInfo = value;
            }
        }

        public Guid Id
        {
            get 
            {
                return mWorkflowInstance.Id;
            }
        }

        public DateTime InstanceCreated
        {
            get 
            {
                return mWorkflowInstance.InstanceCreated;
            }
            set 
            {
                mWorkflowInstance.InstanceCreated = value;
            }
        }

        public DateTime LastUpdated 
        {
            get 
            {
                return mWorkflowInstance.LastUpdated;
            }
            set 
            {
                mWorkflowInstance.LastUpdated = value;
            }
        }

        public Dictionary<string, string> Properties
        {
            get 
            {
                return mWorkflowInstance.Properties;
            }
        }

        public AveWorkflowStatus13Model Status 
        {
            get 
            {
                return (AveWorkflowStatus13Model)Enum.Parse(typeof(AveWorkflowStatus13Model), mWorkflowInstance.Status.ToString());
            }
        }

        public string UserStatus 
        {
            get 
            {
                return mWorkflowInstance.UserStatus;
            }
            set 
            {
                mWorkflowInstance.UserStatus = value;
            }
        }

        public Guid WorkflowSubscriptionId 
        {
            get 
            {
                return mWorkflowInstance.WorkflowSubscriptionId;
            }
            set 
            {
                mWorkflowInstance.WorkflowSubscriptionId = value;
            }
        }
    }
}
