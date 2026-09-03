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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidWorkflowParameterFilter : BaseActionFilter
    {
        private IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService<IEmailTemplateService>();

        private string action;

        public ValidWorkflowParameterFilter()
        {

        }

        public ValidWorkflowParameterFilter(string action)
        {
            this.action = action;
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.First() as WorkflowDefinitionDto;
            var workflowSteps = parameter.Content.WorkflowNodes;
            var startStep = workflowSteps[0];
            var beginStep = workflowSteps[1];
            var destroyStep = workflowSteps[2];
            var notDestroyStep = workflowSteps[3];
            var endDestroyStep = workflowSteps[4];
            var reviewSteps = workflowSteps.Skip(5);

            if(startStep.NodeType != WorkflowNodeType.Start || beginStep.NodeType != WorkflowNodeType.BeginDisposalReview || destroyStep.NodeType != WorkflowNodeType.Destroy
                || notDestroyStep.NodeType != WorkflowNodeType.NotDestroy || endDestroyStep.NodeType != WorkflowNodeType.End
                || reviewSteps.Any(step => step.NodeType != WorkflowNodeType.DisposalReview))
            {
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }

            var allCustomTemplates = (EmailTemplateService.GetAllCustomEmailTemplates()).Select(template => template.UniqueId);
            var reviewNodes = workflowSteps.Where(step => step.NodeType == WorkflowNodeType.BeginDisposalReview || step.NodeType == WorkflowNodeType.DisposalReview);
            var customNodes = reviewNodes.Where(node => node.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom);

            foreach(var customNode in customNodes)
            {
                if(customNode.CustomIntervalSetting.Count > 5 || customNode.CustomIntervalSetting.Count < 2)
                {
                    actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }

                if(customNode.CustomIntervalSetting.Any(setting => !allCustomTemplates.Contains(new Guid(setting.UsedEmailTemplateId)) && new Guid(setting.UsedEmailTemplateId) != Guid.Empty))
                {
                    actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }

                if (customNode.CustomIntervalSetting.Any(setting => setting.Interval < 0))
                {
                    actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }
        }
    }
}
