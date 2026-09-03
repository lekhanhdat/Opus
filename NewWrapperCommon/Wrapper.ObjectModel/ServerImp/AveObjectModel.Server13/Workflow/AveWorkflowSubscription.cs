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

namespace AvePoint.ObjectModel.Server13
{
    public class AveWorkflowSubscription : IAveWorkflowSubscription
    {
          // Fields
        // private readonly IDictionary<string, string> propertyDefinitions;
        private WorkflowSubscription mWorkflowSubscription = null;
        // Methods
        public WorkflowSubscription WorkflowSubScription 
        {
            get 
            {
                return mWorkflowSubscription;
            }
        }

        public AveWorkflowSubscription() 
        {
            this.mWorkflowSubscription = new WorkflowSubscription();
        }

        public AveWorkflowSubscription(WorkflowSubscription subscription)
        {
            this.mWorkflowSubscription = subscription;
        }

        public string GetProperty(string name) 
        {
            return mWorkflowSubscription.GetProperty(name);
        }

        public void SetProperty(string name, string value) 
        {
            mWorkflowSubscription.SetProperty(name, value);
        }

        // Properties
        //internal string CanonicalId { get; }
        public Guid DefinitionId 
        {
            get 
            {
                return mWorkflowSubscription.DefinitionId;
            }

            set 
            {
                mWorkflowSubscription.DefinitionId = value;
            }
        }

        public bool Enabled 
        {
            get 
            {
                return mWorkflowSubscription.Enabled;
            }

            set 
            {
                mWorkflowSubscription.Enabled = value;
            }
        }

        public Guid EventSourceId
        {
            get
            {
                return mWorkflowSubscription.EventSourceId;
            }

            set 
            {
                mWorkflowSubscription.EventSourceId = value;
            }
        }

        public List<string> EventTypes 
        {
            get 
            {
                return mWorkflowSubscription.EventTypes;
            }

            set 
            {
                mWorkflowSubscription.EventTypes = value;
            }
        }

        public Guid Id 
        {
            get 
            {
                return mWorkflowSubscription.Id;
            }
            set 
            {
                mWorkflowSubscription.Id = value;
            }
        }

        public bool ManualStartBypassesActivationLimit
        {
            get
            {
                return mWorkflowSubscription.ManualStartBypassesActivationLimit;
            }
            set 
            {
                mWorkflowSubscription.ManualStartBypassesActivationLimit = value;
            }
        }

        public string Name 
        {
            get 
            {
                return mWorkflowSubscription.Name;
            }
            set
            {
                mWorkflowSubscription.Name = value;
            }
        }

        public IDictionary<string, string> PropertyDefinitions 
        {
            get 
            {
                return mWorkflowSubscription.PropertyDefinitions;
            }
        }
        public bool StatusColumnCreated 
        {
            get 
            {
                return mWorkflowSubscription.StatusColumnCreated;
            }

            set 
            {
                mWorkflowSubscription.StatusColumnCreated = value;
            }
        }

        public string StatusFieldName
        {
            get 
            {
                return mWorkflowSubscription.StatusFieldName;
            }
            set 
            {
                mWorkflowSubscription.StatusFieldName = value;
            }
        }
    }
}
