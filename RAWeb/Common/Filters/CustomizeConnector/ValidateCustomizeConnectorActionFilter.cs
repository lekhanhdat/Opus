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
using Amazon.S3.Model;
using Amazon.S3.Model.Internal.MarshallTransformations;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager.BuildIn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters.CustomizeConnector
{
    public class ValidateCustomizeConnectorActionFilter : BaseActionFilterAsync
    {

        private static IRMCustomizeConnectorService CustomizeConnectorService => PlatformWindsorManager.GetService<IRMCustomizeConnectorService>();

        public CustomizeConnectorAction Action { get; set; }

        public ValidateCustomizeConnectorActionFilter(CustomizeConnectorAction action)
        {
            Action = action;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
        {
            var validateRes = true;
            switch(Action)
            {
                case CustomizeConnectorAction.Add:
                    validateRes = AddValidate(actionContext);
                    break;
                case CustomizeConnectorAction.Update:
                    validateRes = await UpdateValidate(actionContext);
                    break;
            }

            if(validateRes)
            {
                await next();
            }
        }

        private static bool AddValidate(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not CustomizeConnectorInfo connectorInfo || connectorInfo.ColumnInfoes?.Count == 0 || string.IsNullOrWhiteSpace(connectorInfo.Name))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var customizeColumnManager = new ConnectorColumnManager(new List<CustomizeConnectorColumnInfo>());
            if (!(customizeColumnManager.ColumnListValidate(connectorInfo.ColumnInfoes) && ConnectorBuildInColumnManager.ColumnListValidate(connectorInfo.ColumnInfoes)))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if(!ColumnInfoesValidate(connectorInfo.ColumnInfoes))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static async Task<bool> UpdateValidate(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not CustomizeConnectorInfo connectorInfo || 
                connectorInfo.ColumnInfoes?.Count == 0 || 
                string.IsNullOrWhiteSpace(connectorInfo.Name) ||
                connectorInfo.Id == Guid.Empty)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var existConnectorInfo = await CustomizeConnectorService.GetAsync(connectorInfo.Id);
            var customizeColumnManager = new ConnectorColumnManager(existConnectorInfo.ColumnInfoes);
            if (!(customizeColumnManager.ColumnListValidate(connectorInfo.ColumnInfoes) && ConnectorBuildInColumnManager.ColumnListValidate(connectorInfo.ColumnInfoes)))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if (!ColumnInfoesValidate(connectorInfo.ColumnInfoes))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static bool ColumnInfoesValidate(List<CustomizeConnectorColumnInfo> columnInfoes)
        {

            var columnOrigins = Enum.GetValues<CustomizeConnectorOrigin>().Where(item => item != CustomizeConnectorOrigin.None).ToHashSet();
            if(columnInfoes.Any(item => !columnOrigins.Contains(item.Origin)))
            {
                return false;
            }

            var columnScope = Enum.GetValues<CustomizeConnectorColumnScope>().Where(item => item != CustomizeConnectorColumnScope.None).ToHashSet();
            if (columnInfoes.Any(item => !columnScope.Contains(item.Scope)))
            {
                return false;
            }

            var hasDuplicateOrder = columnInfoes.GroupBy(item => item.Order).ToDictionary(item => item.Key, item => item.Count()).Values.Any(item => item > 1);
            if(hasDuplicateOrder)
            {
                return false;
            }

            var orders = columnInfoes.Where(item => item.Id != CustomizeConnectorBuildColumnIds.RowKey).Select(item => item.Order).ToList();
            var orderIslegal = columnInfoes.First(item => item.Id == CustomizeConnectorBuildColumnIds.RowKey).Order == -1 &&
                orders.Min() == 1 && orders.Max() == columnInfoes.Count - 1;
            if(!orderIslegal)
            {
                return false;
            }

            return true;
        }
    }

    public enum CustomizeConnectorAction
    {
        Add = 1,
        Update = 2,
        Delete = 3
    }
}
