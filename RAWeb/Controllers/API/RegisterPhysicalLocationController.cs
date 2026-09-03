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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Models.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AvePoint.RA.Web.Controllers.API
{
    public class RegisterPhysicalLocationController : RAWebApiBase
    {

        //public IAosApiService AOSApiService { get; set; }


        //[HttpPost]
        //public int Post(APIPhysicalLocationModel model)
        //{
        //    //1.      Sample Function Signatures
        //    //RegisterPhysicalRecordStore (string SharePointUrl) {}

        //    //2.      Execution Logic
        //    //If (SharePointUrl exists in RevIM) {
        //    //                Mark this SharePointUrl as a Physical Records Location
        //    //                Run any required RevIM job to update SharePoint (if any)
        //    //}

        //    if (model == null || model.Url == null || model.SiteCollectionUrl == null)
        //    {
        //        logger.Warn("param is null. return 1");
        //        return 1;
        //    }
        //    logger.Debug("register physical location {0} {1}", model.SiteCollectionUrl, model.Url);
        //    //加参 Site Collection Url, 用于取用户密码, 使用ClientAPI
        //    DAOAPIClientV1 test = new DAOAPIClientV1();
        //    RemoteSiteCollection site = test.GetRemoteSiteCollectionByUrl(model.SiteCollectionUrl);

        //    if (site == null)
        //    {
        //        logger.Warn("no remote site collection match the site collection url in DA. return 102");
        //        return 102;
        //    }
        //    RemoteWebApplication remoteSiteGroup = test.GetWebApplicationById(site.parentId);
        //    return SharePointSiteService.MarkPhysicalLocation(remoteSiteGroup, site, model.Url);
        //}
    }
}