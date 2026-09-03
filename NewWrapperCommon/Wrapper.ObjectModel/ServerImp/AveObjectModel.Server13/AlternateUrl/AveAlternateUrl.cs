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
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveAlternateUrl : AveAutoSerializingObject, IAveAlternateUrl
    {
        private SPAlternateUrl mAlternateUrl;

        public AveAlternateUrl(SPAlternateUrl alternateUrl)
            : base(alternateUrl)
        {
            mAlternateUrl = alternateUrl;
        }

        public AveAlternateUrl(string incomingUrl, AveUrlZone urlZone)
            : this(new SPAlternateUrl(incomingUrl, (SPUrlZone)urlZone))
        { }

        public AveAlternateUrl(Uri requestUri, AveUrlZone zone)
            : this(new SPAlternateUrl(requestUri, (SPUrlZone)zone))
        { }

        internal SPAlternateUrl AlternateUrl
        {
            get
            {
                return mAlternateUrl;
            }
        }

        #region IAveSPAlternateUrl Members

        public Uri Uri
        {
            get
            {
                return mAlternateUrl.Uri;
            }
        }

        public string IncomingUrl
        {
            get
            {
                return mAlternateUrl.IncomingUrl;
            }
        }

        public AveUrlZone UrlZone
        {
            get
            {
                return (AveUrlZone)mAlternateUrl.UrlZone;
            }
        }

        public override bool Equals(object obj)
        {
            return mAlternateUrl.Equals((obj as AveAlternateUrl).AlternateUrl);
        }
        #endregion
    }
}
