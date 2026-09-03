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
namespace Microsoft365.SharePoint.CSOM
{
    using Microsoft.SharePoint.Client;
    using System;
    using Microsoft365.Authentication;
    using Microsoft365.SharePoint.CSOM.Extension;

    public class SiteCollectionVerification
    {
        private string mUrl;
        private ITokenProvider mTokenProvider;

        public SiteCollectionVerification(string siteUrl, ITokenProvider tokenProvider)
        {
            mUrl = siteUrl;
            mTokenProvider = tokenProvider;
        }

        public bool Verify()
        {
            using (var context = new ClientContext(mUrl))
            {
                try
                {
                    context.SetTokenProvider(mTokenProvider);
                    context.Load(context.Site.RootWeb, w => w.Id);
                    context.ExecuteQuery();
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}