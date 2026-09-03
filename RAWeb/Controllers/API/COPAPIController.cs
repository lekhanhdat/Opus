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
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace AvePoint.RA.Web.Controllers.API
{
    public class COPAPIController : COPPortalApiController
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private ITenantService _TenantService = null;
        public ITenantService TenantService
        {
            get
            {
                if (_TenantService == null)
                {
                    _TenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return _TenantService;
            }
        }

        [HttpPost]
        public bool DeleteTenant([FromBody]string tenantId)
        {
            try
            {
                Logger.Info("access api delete tenant:{0}", tenantId);
                var success = TenantService.DeleteTenant(tenantId);
                
            }
            catch (Exception ex)
            {
                ApiMessageUtil.SetResponseErrorMsg(RestStateCode.GetTermColumnInfo, ex);
                Logger.Error("error occurred while delete tenant{0}", ex.ToString());
                return false;
            }
            return true;
        }
    }
}
