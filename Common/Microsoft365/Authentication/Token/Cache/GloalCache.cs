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
namespace Microsoft365.Authentication.Token
{
    using Microsoft365.Authentication.TokenProvider;
    using Microsoft365.Common.Cache;
    using Microsoft365.Configuration;
    using System;
    public static class MemoryTokenCache
    {
        private static IKeyValueCache<string, AccessTokenResult> TokenCache = new KeyValueCache<string, AccessTokenResult>(Microsoft365Configuration.AuthenticationConfiguration?.TokenSetting?.MaxCacheInstance, Microsoft365Configuration.AuthenticationConfiguration.TokenSetting.CacheInstanceLifeCycleEdge, int.MaxValue);
        public static AccessTokenResult RunWithCache(this Func<AccessTokenResult> func,string key)
        {
            var entry = TokenCache?.Get(key);
            if (entry != null && entry.IsValid())
            {
                return entry;
            }
            var tokenResult = func();
            if (tokenResult.IsValid())
            {
                TokenCache?.AddOrUpdate(key, tokenResult, tokenResult.ExpiresOn);
            }
            return tokenResult;
        }
    }
}