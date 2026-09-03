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
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NWSequenceActivityActionProcessor : NWActionProcessorBase
    {
        public NWSequenceActivityActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#SequenceActivity";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                x49x49 = 0,
                y49x49 = 0,
                x30x30 = 0,
                y30x30 = 0,
                x16x16 = 0,
                y16x16 = 0
            };
        }

        public override WorkflowAction UpgradeWorkflowAction(Native13NinTexWorkflowEntity.NWActionConfig nwActionConfig)
        {
            var workflowAction = new WorkflowAction()
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = new Configuration()
                {
                    Id = Guid.NewGuid().ToString(),
                    Image = new Image(),
                }
            };
            if (nwActionConfig != null)
            {
                this.workflowActionProcessor.AddChildrenWorkflowAction(workflowAction, nwActionConfig.ChildActivities);
            }
            return workflowAction;
        }
    }
}