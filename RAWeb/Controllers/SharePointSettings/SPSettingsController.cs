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
using AvePoint.GCommon;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AvePoint.RA.Web.Controllers.SharePointSettings
{
    public class SPSettingsController : BaseController
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(SPSettingsController));
        //
        // GET: /SPSettings/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GlobalSettings() 
        {
            SetSiteMapLinks(new List<SiteMapLink>() {
                new SiteMapLink() { text = ResourceManager.GetString("RM_SPS_SharePointSettings"), href="/SPSettings/Index" },
                new SiteMapLink(){text =ResourceManager.GetString("RM_SPS_GS_GlobalSettings")}
            });
            return View();
        }
	}
}