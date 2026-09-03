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
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Service.Services.MachineLearningManualApproval.Queriers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters.MachineLearning
{
    public class ValidMLManualApprovalParameterFilter : BaseActionFilterAsync
    {
        private static readonly int MaxProcessItemLimit = 50;

        public MLManualApprovalActionType ActionType { get; set; }

        public ValidMLManualApprovalParameterFilter(MLManualApprovalActionType actionType)
        {
            ActionType = actionType;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
        {
            bool validateRes;
            switch (ActionType)
            {
                case MLManualApprovalActionType.Reassign:
                    validateRes = await ReassignValid(actionContext);
                    break;
                default:
                    actionContext.Result = new ObjectResult("Illegal Operation") { StatusCode = (int)HttpStatusCode.Forbidden };
                    validateRes = false;
                    break;
            }

            if (validateRes)
            {
                await next();
            }
        }

        private async Task<bool> ReassignValid(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not ManualAprovalEscalateDefinition definition || definition.ItemIds?.Count == 0 || definition.ToUsers?.Count == 0)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var itemIds = definition.ItemIds;
            if (itemIds.Count > MaxProcessItemLimit)
            {
                actionContext.Result = new ObjectResult("Limit Exceeded") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var queryDefinition = new ManualApprovalQueryDefinition
            {
                PageSize = 50,
                NeedCalculationCount = false,
            };
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.MLApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<RMMLApprovalStatus> { RMMLApprovalStatus.WaitingApprove })
            });

            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ItemId,
                Value = JsonConvert.SerializeObject(itemIds)
            });
            var count = await MLManualApprovalQuerier.Count(queryDefinition);
            if (count != itemIds.Count)
            {
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }
    }
}
