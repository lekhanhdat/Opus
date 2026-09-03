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
    class AveWorkflowDefinition : IAveWorkflowDefinition
    {
        private WorkflowDefinition mWorkflowDefinition = null;
        //public AveWorkflowDefinition(){}
        //public AveWorkflowDefinition(Func<string> xamlFunc) { }

        public AveWorkflowDefinition() 
        {
            mWorkflowDefinition = new WorkflowDefinition();
        }

        public AveWorkflowDefinition(WorkflowDefinition workflowDefinition)
        {
            mWorkflowDefinition = workflowDefinition;
        }

        public WorkflowDefinition WFDefinition 
        {
            get 
            {
                return mWorkflowDefinition;
            }
        }
        // private string GetProperty(string propertyName);
        public void SetProperties(IDictionary<string, string> value) 
        {
            mWorkflowDefinition.SetProperties(value);
        }

        public void SetProperty(string propertyName, string value) 
        {
            mWorkflowDefinition.SetProperty(propertyName, value);
        }

        // Properties

        public string AssociationUrl 
        {   
            get
            {
                return mWorkflowDefinition.AssociationUrl ;
            }
            set 
            {
                mWorkflowDefinition.AssociationUrl = value;
            }
        }

        //internal string CanonicalId { get; }

        public string Description 
        {
            get 
            {
                return mWorkflowDefinition.Description;
            }
            set 
            {
                mWorkflowDefinition.Description = value;
            }
        }

        public string DisplayName 
        {
            get 
            {
                return mWorkflowDefinition.DisplayName;
            }
            set 
            {
                mWorkflowDefinition.DisplayName = value;
            }
        }

        public string DraftVersion
        {
            get 
            {
                return mWorkflowDefinition.DraftVersion;
            }
            set 
            {
                mWorkflowDefinition.DraftVersion = value;
            }
        }

        public string FormField 
        {
            get
            {
                return mWorkflowDefinition.FormField;
            }
            set 
            {
                mWorkflowDefinition.FormField = value;
            }
        }

        public Guid Id 
        {
            get 
            {
                return mWorkflowDefinition.Id;
            }
            set 
            {
                mWorkflowDefinition.Id = value;
            }
        }

        public string InitiationUrl 
        {
            get 
            {
                return mWorkflowDefinition.InitiationUrl;
            }
            set 
            {
                mWorkflowDefinition.InitiationUrl = value;
            }
        }

        public IDictionary<string, string> Properties
        {
            get 
            {
                return mWorkflowDefinition.Properties;
            }
        }

        public bool Published
        {
            get 
            {
                return mWorkflowDefinition.Published;
            }
            set 
            {
                mWorkflowDefinition.Published = value;
            }
        }

        public bool RequiresAssociationForm 
        {
            get 
            {
                return mWorkflowDefinition.RequiresAssociationForm;
            }
            set 
            {
                mWorkflowDefinition.RequiresAssociationForm = value;
            }
        }

        public bool RequiresInitiationForm
        {
            get 
            {
                return mWorkflowDefinition.RequiresInitiationForm;
            }
            set 
            {
                mWorkflowDefinition.RequiresInitiationForm = value;
            }
        }

        public string RestrictToScope 
        {
            get 
            {
                return mWorkflowDefinition.RestrictToScope;
            }
            set 
            {
                mWorkflowDefinition.RestrictToScope = value;
            }
        }

        public string RestrictToType 
        {
            get 
            {
                return mWorkflowDefinition.RestrictToType;
            }
            set
            {
                mWorkflowDefinition.RestrictToType = value;
            }
        }

        public IDictionary<string, string> UpdatedProperties 
        {
            get 
            {
                return mWorkflowDefinition.UpdatedProperties;
            }
        }

        public string Xaml 
        {
            get
            {
                return mWorkflowDefinition.Xaml;
            }
            set 
            {
                mWorkflowDefinition.Xaml = value;
            }
        }
    }
}
