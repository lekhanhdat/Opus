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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Server19
{
    class AveWorkflowDefinitionCollection : AveAbstractCommonCollection<IAveWorkflowDefinition>, IAveWorkflowDefinitionCollection
    {
        private WorkflowDefinitionCollection mWorkflowDefinitionCollection = null;
        // Methods
        public AveWorkflowDefinitionCollection(IList<IAveWorkflowDefinition> list)
            : base(list)
        {
            IList<WorkflowDefinition> tempList = new List<WorkflowDefinition>();
            foreach (AveWorkflowDefinition definition in list)
            {
                tempList.Add(definition.WFDefinition);
            }
            this.mWorkflowDefinitionCollection = new WorkflowDefinitionCollection(tempList);
        }

        public AveWorkflowDefinitionCollection(WorkflowDefinitionCollection workflowDefinitionCollection)
            : base(workflowDefinitionCollection)
        {
            this.mWorkflowDefinitionCollection = workflowDefinitionCollection;
        }

        public override int Count
        {
            get { return mWorkflowDefinitionCollection.Count; }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveWorkflowDefinition(t as WorkflowDefinition);
        }
    }
}
