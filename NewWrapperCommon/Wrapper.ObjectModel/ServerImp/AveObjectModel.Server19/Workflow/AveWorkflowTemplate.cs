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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using AvePoint.Wrapper.Common;
using System.Collections.Specialized;

namespace AvePoint.ObjectModel.Server19
{
    class AveWorkflowTemplate : AveAutoSerializingObject, IAveWorkflowTemplate
    {
        private IAveWorkflowTemplateCollection mWorkflowTemplateCollection;

        private SPWorkflowTemplate mWorkflowTemplate;

        //[Obsolete]
        //public AveWorkflowTemplate(SPWorkflowTemplate workflowTemplate)
        //    : base(workflowTemplate)
        //{
        //    mWorkflowTemplate = workflowTemplate;
        //}

        public AveWorkflowTemplate(IAveWorkflowTemplateCollection templateCollection,SPWorkflowTemplate workflowTemplate)
            : base(workflowTemplate)
        {
            mWorkflowTemplateCollection = templateCollection;
            mWorkflowTemplate = workflowTemplate;
        }

        public object this[string property]
        {
            get { return mWorkflowTemplate[property]; }
            set { mWorkflowTemplate[property] = value; }
        }

        internal SPWorkflowTemplate WorkflowTemplate
        {
            get
            {
                return mWorkflowTemplate;
            }
        }

        #region IAveWorkflowTemplate Members

        public bool AllowDefaultContentApproval
        {
            get
            {
                return mWorkflowTemplate.AllowDefaultContentApproval;
            }
            set
            {
                mWorkflowTemplate.AllowDefaultContentApproval = value;
            }
        }

        public bool AutoStartChange
        {
            get
            {
                return mWorkflowTemplate.AutoStartChange;
            }
            set
            {
                mWorkflowTemplate.AutoStartChange = value;
            }
        }

        public bool AutoStartCreate
        {
            get
            {
                return mWorkflowTemplate.AutoStartCreate;
            }
            set
            {
                mWorkflowTemplate.AutoStartCreate = value;
            }
        }

        public Guid BaseId
        {
            get { return mWorkflowTemplate.BaseId; }
        }

        public string Description
        {
            get
            {
                return mWorkflowTemplate.Description;
            }
            set
            {
                mWorkflowTemplate.Description = value;
            }
        }

        public Guid ID
        {
            get { return mWorkflowTemplate.Id; }
        }

        public bool IsRootPublic
        {
            get { return "RootPublic".Equals(this["Visibility"]); }
        }

        public string Name
        {
            get { return mWorkflowTemplate.Name; }
        }

        public AveBasePermissions PermissionsManual
        {
            get
            {
                return (AveBasePermissions)mWorkflowTemplate.PermissionsManual;
            }
            set
            {
                mWorkflowTemplate.PermissionsManual = (SPBasePermissions)value;
            }
        }

        public IAveWorkflowTemplateIdSet TemplateIdSet
        {
            get
            {
                if (mWorkflowTemplate == null)
                {
                    return null;
                }
                return new AveWorkflowTemplateIdSet(mWorkflowTemplate);
            }
        }

        public StringCollection GetStatusChoices(IAveWeb web)
        {
            return mWorkflowTemplate.GetStatusChoices((web as AveWeb).Web);
        }

        #endregion
    }
}
