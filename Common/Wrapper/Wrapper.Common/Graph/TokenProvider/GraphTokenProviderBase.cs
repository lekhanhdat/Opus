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

namespace AvePoint.Wrapper.Common.Graph
{
    internal class GraphTokenProviderBase: IGraphTokenProvider
    {
        private static readonly TimeSpan Token_EXPIRES_EDGE = new TimeSpan(0, 5, 0);
        public GraphTokenProviderBase(string resourceUrl)
        {
            ResourceUrl = resourceUrl;
        }
        public string ResourceUrl { get; private set; }

        protected TokenItem CachedTokenItem { get; set; }
        public virtual string GetToken()
        {
            if (CachedTokenItem == null || CachedTokenItem.ExpiresOn - DateTimeOffset.Now <= Token_EXPIRES_EDGE)
            {
                RefreshToken(); 
            }
            return CachedTokenItem?.AccessToken;
        }
        protected virtual void RefreshToken()
        { }
    }
}
