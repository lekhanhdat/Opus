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
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.SecurityTrimming.Model;

namespace AvePoint.RA.Web.Controllers.Common
{
    [RMAuthorize(Contract.RoleAssignments.RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
    public class RootController : BaseController
    {
        // GET: Root/Home
        public ActionResult Home()
        {
            ViewBag.IsDebug = GetDebugState();
            var nonce = HttpContext.Items["Nonce"] as string;
            ViewBag.Nonce = nonce;
            return View();
        }
        public ActionResult Index()
        {
            ViewBag.IsDebug = GetDebugState();
            var nonce = HttpContext.Items["Nonce"] as string;
            ViewBag.Nonce = nonce;
            return View("Home");
        }

        /// <summary>
        /// 进入网站，在浏览器控制台执行：RM.Cookie.debugMode(true)，可以开启Debug模式
        /// 执行RM.Cookie.debugMode(false)，可关闭Debug模式
        /// </summary>
        /// <returns></returns>
        public bool GetDebugState()
        {
            bool isDebug = false;
#if DEBUG
            isDebug = true;
            var cookie = HttpContext.Request.Cookies["RM_IsDebug"];
            if (cookie != null)
            {
                isDebug = cookie.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
#endif
            return isDebug;
        }
    }
}