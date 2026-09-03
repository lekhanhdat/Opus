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



using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Office.InfoPath.Server.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOAlternateAccessMapping : IAveOAlternateAccessMapping
    {
        #region IAveOAlternateAccessMapping Members

        private string mAccessType = "Microsoft.Office.Server.SocialData.AlternateAccessMapping";

        public AveOAlternateAccessMapping()
        {

        }
        
        /// <summary>
        /// copy from SP API,2016 January CU 修改了这个internal 方法参数,但是内部实现是一样的,所以封装时重写了13的方法
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="DefaultZone"></param>
        /// <returns></returns>
        public Uri GetDeserializedUrl(Uri Url, SPUrlZone DefaultZone = SPUrlZone.Default)
        {
            bool flag = Url != null && (HttpContext.Current != null || DefaultZone != SPUrlZone.Default);
            if (flag)
            {
                SPSite sPSite = null;
                if (HttpContext.Current != null)
                {
                    sPSite = SPControl.GetContextSite(HttpContext.Current);
                }
                Uri uri = (SPFarm.Local.AlternateUrlCollections.DeserializeUrlFromStorage(Url.AbsoluteUri, (sPSite != null) ? sPSite.Zone : DefaultZone) == null) ? Url : new Uri(SPFarm.Local.AlternateUrlCollections.DeserializeUrlFromStorage(Url.AbsoluteUri, (sPSite != null) ? sPSite.Zone : DefaultZone));
                if (uri != null)
                {
                    Url = uri;
                }
            }
            return Url;
        }

        public Uri GetSerializedUrl(Uri Url)
        {
            return (Uri)AveAssemblyUtility.InvokeStaticMethod(mAccessType, "GetSerializedUrl", new object[]{ Url });
        }

        #endregion
    }
}
