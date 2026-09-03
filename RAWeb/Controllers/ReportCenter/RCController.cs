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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RACodeReview("Allen yin", comment: "两个show report方法先悟要重构一下")]
    public class RCController : BaseController
    {
        private ISPSettingTreeService _SPSettingTreeService;
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);

        #region DashBoard
        //[RMAuthorize(RMPermissionMasks.ReportCenterAdmin)]
        //public ActionResult DashBoard()
        //{
        //    SetSiteMapLinks(new List<SiteMapLink>() {
        //        new SiteMapLink() { text = I18NEntity.GetString("RM_Home_PageTitle"), href = "/Root/Home" },
        //        new SiteMapLink() { text = I18NEntity.GetString("RM_DSB_PageTitle") },
        //    });
        //    return View();
        //}

        #endregion
      
        [RMAuthorize(Contract.RoleAssignments.RMPermissionMasks.CommonModuleAccess, Contract.RoleAssignments.RMSOPermissionMasks.CommonModuleAccess)]
        public ViewResult TreeTest()
        {
            var exchangeRoot = SPSettingTreeService.LoadExchangeRoot()[0];
            if (exchangeRoot == null || exchangeRoot.Id.Equals(System.Guid.Empty))
            {
                //mlogger.Warn("exchage farm node is null.Please refresh page.");
            }
            else
            {
                if (exchangeRoot.Children != null)
                {
                    exchangeRoot.Children = null;
                }
            }
            ViewData["ExchangeRoot"] = JsonConvert.SerializeObject(exchangeRoot);
            return View();
        }
    }
}