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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;

namespace AvePoint.RA.Web.Common.WIF
{
    /// <summary>
    /// 文件下载过滤器:关联jquery.filedownload.js
    /// </summary>
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    //public class FileDownloadFilterAttribute : ActionFilterAttribute
    //{
    //    protected RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
    //    public FileDownloadFilterAttribute(string cookieName = "fileDownload", string cookiePath = "/")
    //    {
    //        CookieName = cookieName;
    //        CookiePath = cookiePath;
    //    }
    //    public string CookieName { get; set; }
    //    public string CookiePath { get; set; }
    //    public override void OnActionExecuted(ActionExecutedContext filterContext)
    //    {
    //        CheckAndHandleFileResult(filterContext);
    //        base.OnActionExecuted(filterContext);
    //    }

    //    private void CheckAndHandleFileResult(ActionExecutedContext filterContext)
    //    {
    //        var response = filterContext.HttpContext.Response;

    //        try
    //        {
    //            if (response.Headers.ContentType == "application/octet-stream")
    //            {
    //                response.Cookies.Append(CookieName, "true", new CookieOptions() { Path = CookiePath });
    //            }
    //            else
    //            {
    //                response.Cookies.Append(CookieName, "true", new CookieOptions() { Expires = DateTime.Now.AddDays(-1) });
    //            }
    //        }
    //        catch(Exception ex)
    //        {
    //            logger.Warn($"fileDownloadFilter occured exception:message:{ex.Message},stackTrack:{ex.StackTrace}");
    //        }
            
    //    } 
    //}
}