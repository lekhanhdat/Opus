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
using AvePoint.RA.CommonUtil;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.Web.Extentions.Util;

namespace AvePoint.RA.Api.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class APIValidateSaleforceHeadersFilter : Attribute, IAsyncActionFilter
{
    private static readonly AvePoint.RA.Contract.Services.IRALogger s_logger = RALogger.GetInstance(typeof(APIValidateSaleforceHeadersFilter));

    private readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var customerId = TenantLocalValue.LogonGroupId;
        var organizationId = context.HttpContext.Request.GetRequestHeadersParam("AppOrganizationId");
        s_logger.Info($"Get from header OrganizationId: {organizationId}, CustomerId: {customerId}");
        if (customerId.IsNullOrEmpty() || organizationId.IsNullOrEmpty())
        {
            context.Result = new ObjectResult("Invalid data.") { StatusCode = (int)HttpStatusCode.Forbidden };
            return;
        }

        if (RMAosApiClient.GetSalesforceAppProfile(customerId, organizationId, true) == null)
        {
            context.Result = new ObjectResult("Invalid data.") { StatusCode = (int)HttpStatusCode.Forbidden };
            return;
        };
        
        try
        {
            var scopeInfo = await _configurationDao.GetAsync(RMDiscoveryConfigurationType.SalesforceNewlyScope, new RMDiscoverySalesforceScopeInfo());
            var organizationIds = scopeInfo.Organizations.Select(organization => organization.Id);
            if (!organizationIds.Contains(organizationId, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(I18NEntity.GetString("RM_FA_SF_Discovery_Scan_Data_First")) { StatusCode = (int)HttpStatusCode.NotFound };
                return;
            }
        }
        catch
        {
            context.Result = new ObjectResult(I18NEntity.GetString("RM_FA_SF_Discovery_Scan_Data_First")) { StatusCode = (int)HttpStatusCode.NotFound };
            return;
        }
        
        if (context.ActionArguments.TryGetValue("queryParameter", out dynamic queryParameter))
        {
            queryParameter.OrganizationId = organizationId;
        }
        await next();

    }
}