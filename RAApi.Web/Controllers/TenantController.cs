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
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AvePoint.RA.Api.Web.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
    public class TenantController : RAWebApiBase
    {
        private static readonly RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(TenantController));

        private ITenantService _TenantService;

        public ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);

        [HttpPost]
        public bool CheckHardDeletionStatus([FromBody]string customerId) 
        {
            try
            {
                var isExist = TenantService.CheckTenantExist(customerId);
                logger.Info($"customer Id:{customerId} exist: {isExist}");
                //不存在说明已经删除成功, 返回True
                return !isExist;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while check tenant:{ex.ToString()}");
                return false;
            }       
            
        }

        [HttpGet("{customerid}/harddeletion")]
        public bool HardDeletion([FromBody] string customerid)
        {
            try
            {
                var isExist = TenantService.CheckTenantExist(customerid);
                logger.Info($"customer Id:{customerid} exist: {isExist}");
                //不存在说明已经删除成功, 返回True
                return !isExist;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while check tenant:{ex.ToString()}");
                return false;
            }

        }

    }
}
