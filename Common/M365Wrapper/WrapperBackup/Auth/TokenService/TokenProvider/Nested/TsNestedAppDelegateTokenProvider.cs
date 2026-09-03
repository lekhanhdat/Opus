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
using System.Threading;
using System.Threading.Tasks;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Token.Services;
using Microsoft365.Authentication.TokenProvider;
using Microsoft365.Authentication.TokenProvider.TokenService;

namespace M365.Wrapper.Backup.Auth.TokenService.TokenProvider.Nested;

public class TsNestedAppDelegateTokenProvider : TsNestedTokenProviderBase
{
    public override NestedTokenProviderType TokenFactoryType
    {
        get
        {
            return NestedTokenProviderType.ApplicationDelegateBear;
        }
    }

    public IdentityProviderType AppType { get; private set; }
    public string AppId { get; private set; }

    protected override string Identity
    {
        get { return $"{AppType}|{AppId}"; }
    }

    public TsNestedAppDelegateTokenProvider(IModernTokenService tokenService, string customerId, string tenantId, IdentityProviderType appType, string appId)
        : base(tokenService, customerId, tenantId)
    {
        AppType = appType;
        AppId = appId;
    }

    public override async ValueTask<AccessTokenResult> GetAccessTokenAsync(string resource, TokenResourceType resourceType, CancellationToken cancellationToken)
    {
        return await ProcessTokenResultAsync(async () =>
        {
            return await TokenService.GetTokenByAppProfileAsync(AppType,
                resourceType,
                TenantId,
                AppId,
                String.IsNullOrEmpty(resource) ? null : new Uri(resource).GetLeftPart(UriPartial.Authority),
                TokenType.DelegatedToken);
        }, GenerateTokenCacheKey(resource, resourceType));
    }
}