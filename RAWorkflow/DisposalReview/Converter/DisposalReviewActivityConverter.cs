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
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.Workflow.Builder.Interface;
using AvePoint.RA.Workflow.DisposalReview.Activities;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Workflow.DisposalReview
{
    internal class DisposalReviewActivityConverter
    {
        public static Activity Convert(WorkflowNode node, bool needNotification = true)
        {
            var activity = new DisposalReviewActivity()
            {
                StepId = node.Id,
                DisplayName = node.Name,
                BookmarkName = node.Name,
                NeedNotification = needNotification
            };

            activity.RequestInfo = new VisualBasicValue<DisposalReviewRequestInfo>(DisposalReviewWorkflowBuilder.ArgRequestInfoName);
            activity.Result = new VisualBasicReference<DisposalReviewActionEnum>(DisposalReviewWorkflowBuilder.VarApproveActionName);
            activity.OutRequestInfo = new VisualBasicReference<DisposalReviewRequestInfo>(DisposalReviewWorkflowBuilder.VarRequestInfoName);
            return activity;
        }
    }
}
