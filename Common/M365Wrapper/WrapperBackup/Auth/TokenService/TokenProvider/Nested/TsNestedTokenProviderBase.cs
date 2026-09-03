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

using Microsoft365.Authentication.TokenService;
using Polly;
using AvePoint.RA.CommonUtil;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cloud.Sdk.Token.Services;
using Microsoft365.Authentication.Token.Idclr;
using Microsoft365.Authentication.Token;
using Cloud.Sdk.Core;
namespace Microsoft365.Authentication.TokenProvider.TokenService;

public abstract class TsNestedTokenProviderBase : INestedTokenProvider
{
    private static RALogger logger = RALogger.GetInstance(typeof(TsNestedTokenProviderBase));
    public abstract NestedTokenProviderType TokenFactoryType { get; }
    public virtual IModernTokenService TokenService { get; protected set; }
    public virtual string CustomerId { get; protected set; }
    public virtual string TenantId { get; protected set; }
    protected abstract string Identity { get; }

    public TsNestedTokenProviderBase(IModernTokenService tokenService, string customerId, string tenantId)
    {
        TokenService = tokenService;
        CustomerId = customerId;
        TenantId = tenantId;
    }

    protected virtual async ValueTask<AccessTokenResult> ProcessTokenResultAsync(Func<ValueTask<AccessTokenResult>> action, string cacheKey)
    {
        return await MemoryTokenCache.RunWithCacheAsync(action, cacheKey);
    }

    protected virtual async ValueTask<AccessTokenResult> ProcessTokenResultAsync(Func<ValueTask<TokenResult>> action, string cacheKey)
    {
        var tokenResult = await MemoryTokenCache.RunWithCacheAsync(async () =>
        {
            try
            {
                var invalidClientPolicy = Policy
                    .Handle<CloudApiException>(ex => ex.ErrorCode == (int)IdentityServerErrorCode.InvalidClient)
                    .WaitAndRetryAsync(
                        10,
                        _ => TimeSpan.FromMilliseconds(1000),
                        async (ex, _, _, _) =>
                        {
                            logger.Error($"Request token failed with {ex}, try to reload certificate.");
                            await Task.Delay(TimeSpan.FromMinutes(15));
                            await TokenServiceContext.ReloadCommunicationCertificateAsync(default);
                        }
                    );

                var defaultPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(1000));

                var policies = Policy.WrapAsync(invalidClientPolicy, defaultPolicy);
                return await policies.ExecuteAsync(async () =>
                {
                    var result = await action();
                    if (!string.IsNullOrEmpty(result?.Error))
                    {
                        logger.Error($"Request token failed.Error:{result?.Error}");
                    }
                    return result?.ConvertToAccessTokenResult(TokenFactoryType.GetTokenType());
                });
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new AccessTokenResult(ex));
            }
        }, cacheKey);
        if (tokenResult != null && tokenResult.Exception != null)
        {
            if (tokenResult.Exception is Microsoft.Identity.Client.MsalException or AuthenticationIdclrException or ADAL.AdalException)
            {
                throw tokenResult.Exception;
            }
        }
        return tokenResult;
    }

    protected virtual string GenerateTokenCacheKey(string resource, TokenResourceType resourceType)
    {
        return $"{ToString()}|{resource}|{resourceType}";
    }

    public override string ToString()
    {
        return $"{CustomerId}|{TenantId}|{TokenFactoryType}|{Identity}";
    }

    public abstract ValueTask<AccessTokenResult> GetAccessTokenAsync(string resource, TokenResourceType resourceType, CancellationToken cancellationToken);
}