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
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Web.Extentions.Authorize;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RMContract.SharePoint;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common
{
    public class BaseController : Controller
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected CurrentUserInfo CurrentUser;
       

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            InitContextInfo();

            CurrentUser = await HttpContext.Request.GetCurrentUserInfoAsync();

            if (CurrentUser != null)
            {
                ITenantService tenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                if (tenantService.CheckTenantExist(CurrentUser.TenantGroupId))
                {
                    TenantLocalValue.LogonGroupId = CurrentUser.TenantGroupId;
                    TenantLocalValue.LogonUserId = CurrentUser.AccountId;
                    TenantLocalValue.LogonUserEmail = CurrentUser.RegisterEmail;
                    TenantLocalValue.PartnerUser = CurrentUser.PartnarUser;
                    TenantLocalValue.Company = CurrentUser.Company;
                    TenantLocalValue.AccountNumber = CurrentUser.AccountNumber;
                }
               
            }
            await base.OnActionExecutionAsync(context, next);
        }

        public virtual void InitContextInfo() { }

        //public async Task InitLoginUserInfoAsync()
        //{
          
        //}
    }
}