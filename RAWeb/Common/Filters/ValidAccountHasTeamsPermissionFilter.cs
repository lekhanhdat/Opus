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
using System.Net;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidAccountHasTeamsPermissionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidAccountHasTeamsPermissionFilter));

        public IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        public ValidAccountHasTeamsPermissionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (!RMKeyValueDao.EnableTeamsFeature())
            {
                logger.Info("Access denied for teams");
                actionContext.Result = new ObjectResult("Access  Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
        }
    }
}
