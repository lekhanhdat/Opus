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
using Microsoft.AspNetCore.Mvc;
using System;

namespace RASPAppWeb.Controllers
{
    public class DefaultController : Controller
    {
        private static RALogger logger = RALogger.GetInstance(typeof(DefaultController));

        public IActionResult Index()
        {
            try
            {
                var spLanguage = this.HttpContext.Request.Query[SharePointContext.SPLanguageKey];
                logger.Info($"Preinit Current thread language {spLanguage}");
                I18nUtil.SetLanguage(spLanguage);
            }
            catch (Exception ex)
            {
                logger.Info($"Preinit init language {ex}");
            }
            return View();
        }
    }
}