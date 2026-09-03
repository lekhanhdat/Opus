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

using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Web.ActionResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvePoint.RA.Web.Controllers.Common
{
    [RACodeReview("Allen Yin")]
    public class RMWebController : Controller
    { 
        // GET: RMWeb
        /// <summary>
        /// 此Controller 返回资源文件 缓存360天
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 30 * 60 * 24 * 60 * 12)]
        public JsResourceResult JsResx()
        {
            return new JsResourceResult();
        }


    }
}