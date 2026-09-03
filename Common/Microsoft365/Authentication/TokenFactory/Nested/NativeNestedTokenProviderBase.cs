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

namespace Microsoft365.Authentication
{
    using Microsoft365.Authentication.TokenProvider;
    using System;
    using Microsoft365.Authentication.Token;
    using Microsoft365.Common.Utility;
    using System.Threading.Tasks;

    public abstract class NativeNestedTokenProviderBase
    {
        protected virtual string GenerateTokenCacheKey(string resource, AuthenticationResourceType resourceType)
        {
            return $"{ToString()}|{GetType().Name}|{resource}|{resourceType}";
        }

        protected virtual AccessTokenResult ProcessTokenResult(Func<Task<AccessTokenResult>> action, string cacheKey)
        {
            return MemoryTokenCache.RunWithCache(() =>
            {
                try
                {
                    return RetryExecutor.Execute(() =>
                            {
                                return action.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
                            }, $"ProcessTokenResult - {ToString()}");
                }
                catch (Exception ex)
                {
                    return new AccessTokenResult(ex);
                }
            }, cacheKey);
        }

        protected virtual Task<AccessTokenResult> ProcessTokenResultAsync(string resource, AuthenticationResourceType resourceType)
        {
            throw new NotImplementedException(typeof(NativeNestedTokenProviderBase).Name);
        }

        public virtual AccessTokenResult GetAccessToken(string resource, AuthenticationResourceType resourceType)
        {
            return ProcessTokenResult(async () =>
           {
               return await ProcessTokenResultAsync(resource, resourceType);
           }, GenerateTokenCacheKey(resource, resourceType));
        }
    }
}