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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/sharepointonprembrowser/[action]")]
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    public class SharePointOnPremBrowserController : RAWebApiBase
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SharePointOnPremBrowserController));

        private IFileSystemTreeCacheDao _FileSystemTreeCacheDao;

        public IFileSystemTreeCacheDao FileSystemTreeCacheDao => PlatformWindsorManager.GetService(ref _FileSystemTreeCacheDao);

        [HttpPost]
        public bool AddBrowserCache([FromBody] HybridBrowserCache cache)
        {
            try
            {
                Logger.Info($"Hybrid browser cache node info: {cache.CacheData}");
                return TenantUtil.RunUnderTenant(cache.TenantId, null, () =>
                {
                    var info = new FileSystemTreeCache
                    {
                        BatchId = cache.BatchId,
                        TreeData = cache.CacheData ?? string.Empty,
                    };
                    return FileSystemTreeCacheDao.SaveTreeNodeInfo(info) > 0;
                });
            }
            catch(Exception e)
            {
                Logger.Error($"An error occur while add hybrid browser cache. Error: {e}");
                return false;
            }
        }
    }
}
