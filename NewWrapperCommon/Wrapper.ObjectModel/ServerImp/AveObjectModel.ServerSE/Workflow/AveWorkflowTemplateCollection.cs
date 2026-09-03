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
using Microsoft.SharePoint.Workflow;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using System.Globalization;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWorkflowTemplateCollection : AveAbstractCommonCollection<IAveWorkflowTemplate>, IAveWorkflowTemplateCollection
    {
        private SPWorkflowTemplateCollection mWorkflowTemplateCollection;

        private IAveWeb mParentWeb;

        internal IAveWeb ParentWeb
        {
            get { return mParentWeb; }
            set { mParentWeb = value; }
        }

        //[Obsolete]
        //public AveWorkflowTemplateCollection(SPWorkflowTemplateCollection workflowTemplateCollection)
        //    : base(workflowTemplateCollection)
        //{
        //    mWorkflowTemplateCollection = workflowTemplateCollection;
        //}

        public AveWorkflowTemplateCollection(IAveWeb parentWeb,SPWorkflowTemplateCollection workflowTemplateCollection)
            : base(workflowTemplateCollection)
        {
            mParentWeb = parentWeb;
            mWorkflowTemplateCollection = workflowTemplateCollection;
        }

        public IAveWorkflowTemplate GetTemplateByBaseID(Guid baseTemplateId)
        {
            SPWorkflowTemplate workflowTemplate = mWorkflowTemplateCollection.GetTemplateByBaseID(baseTemplateId);
            if (workflowTemplate == null)
            {
                return null;
            }
            return new AveWorkflowTemplate(this,workflowTemplate);
        }

        public IAveWorkflowTemplate GetTemplateByNmae(string templateName, CultureInfo cultureInfo)
        {
            SPWorkflowTemplate workflowTemplate = mWorkflowTemplateCollection.GetTemplateByName(templateName,cultureInfo);
            if (workflowTemplate == null)
            {
                return null;
            }
            return new AveWorkflowTemplate(this, workflowTemplate);
        }

        public void Add(IAveWorkflowTemplate template)
        {
            AveAssemblyUtility.InvokeMethod(mWorkflowTemplateCollection, "Add", new object[] { (template as AveWorkflowAssociation).WorkflowAssociation });
        }

        public override IAveWorkflowTemplate this[int index]
        {
            get
            {
                SPWorkflowTemplate workFlowTemplate = mWorkflowTemplateCollection[index];
                if (workFlowTemplate == null)
                {
                    return null;
                }
                return new AveWorkflowTemplate(this,workFlowTemplate);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveWorkflowTemplate(this,t as SPWorkflowTemplate);
        }

        public override int Count
        {
            get { return mWorkflowTemplateCollection.Count; }
        }

        public IAveWorkflowTemplate this[Guid guid]
        {
            get { return mWorkflowTemplateCollection[guid] == null ? null : new AveWorkflowTemplate(this,mWorkflowTemplateCollection[guid]); }
        }
    }
}
