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
namespace AvePoint.Wrapper.Common
{
    using System;
    using AvePoint.GCommon.Utility.Cloud;
    using Microsoft365.Authentication;
    using Microsoft365.Authentication.Token.BearToken;

    public static class AveBPOSAccountInfoExtension
    {
        public static ITokenProvider Convert2TokenProvider(this AveBPOSAccountInfo info)
        {
            return Convert3TokenProvider(info);
        }

        private static ITokenProvider Convert3TokenProvider(AveBPOSAccountInfo info)
        {
            ITokenProvider provider = null;

            if (info != null)
            {
                if (info.TokenProvider != null)
                {
                    provider = info.TokenProvider;
                }
                else if (!string.IsNullOrEmpty(GCommonRoleConfiguration.AosTokenApiURL))
                {
                    //use token service default
                    provider = TokenProviderFactory.GetInstance().Get(info);
                }
                else
                {
                    //no longer get token locally
                    throw new Exception("Token api url is null");
                }
            }
            return provider;
        }
    }
}
