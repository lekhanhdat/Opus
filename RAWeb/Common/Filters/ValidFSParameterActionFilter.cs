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
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.ControlPanel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidFSParameterActionFilter : BaseActionFilter
    {
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();

        private string action;
        public ValidFSParameterActionFilter()
        {
        }
        public ValidFSParameterActionFilter(string type)
        {
            action = type;
        }


        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (action.Equals("ValidateSaveFSTermSetting"))
            {
                var treeNode = actionContext.ActionArguments.Values.First() as RMFSTreeNode;
                if(treeNode != null)
                {
                    var isEnableManualApproval = treeNode.ApprovalType == (int)ApprovalType.ApprovalProcess;
                    if (isEnableManualApproval)
                    {
                        var workflowId = treeNode.WorkflowReferenceId;
                        if (workflowId == null || string.IsNullOrEmpty(workflowId.Trim()))
                        {
                            actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                            return;
                        }
                        var result = Guid.TryParse(workflowId, out var referenceId);
                        if (result)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                            if (workflow == null)
                            {
                                actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                                return;
                            }
                        }
                        else
                        {
                            actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                            return;
                        }
                    }
                }
            }
            else if(action.Equals("ValidateSavePRTermSetting"))
            {
                var treeNode = actionContext.ActionArguments.Values.First() as RMPRSaveRecordOwnerDto;
                if (treeNode != null)
                {
                    var isEnableManualApproval = treeNode.ApprovalType == (int)ApprovalType.ApprovalProcess;
                    if (isEnableManualApproval)
                    {
                        var workflowId = treeNode.WorkflowReferenceId;
                        if (workflowId == null || string.IsNullOrEmpty(workflowId.Trim()))
                        {
                            actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                            return;
                        }
                        var result = Guid.TryParse(workflowId, out var referenceId);
                        if (result)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                            if (workflow == null)
                            {
                                actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                                return;
                            }
                        }
                        else
                        {
                            actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                            return;
                        }
                    }
                }
            }         
        }
    }
}
