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
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft365.SharePoint;
using Microsoft365.Authentication;

namespace AvePoint.Wrapper.Common
{
    static class ReliableHttpWebRequestExtension
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(ReliableHttpWebRequestExtension));
        public static void RefreshDigestInfo(this ReliableHttpWebRequest request, string siteUrl, ITokenProvider tokenProvider)
        {
            if (request.Headers["X-RequestDigest"] != null)
            {
                request.RequestFailed += (r, e) =>
                {
                    try
                    {
                        mLogger.Info("Refresh form digest");
                        request.Headers["X-RequestDigest"] = SharePointContext.GetFormDigestProvider().GetFormDigest(siteUrl, tokenProvider).DigestValue;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Refresh digest at request excutor.error:{0}", ex);
                    }
                };
            }
        }
    }
}
