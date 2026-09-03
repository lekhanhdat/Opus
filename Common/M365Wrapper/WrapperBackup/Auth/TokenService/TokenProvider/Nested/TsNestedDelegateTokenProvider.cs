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

namespace Microsoft365.Authentication.TokenProvider.TokenService;
using AvePoint.RA.CommonUtil;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Token.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class TsNestedDelegateTokenProvider : TsNestedTokenProviderBase
{
    public override NestedTokenProviderType TokenFactoryType
    {
        get { return NestedTokenProviderType.DelegateBear; }
    }
    public string UserName { get; set; }

    protected override string Identity
    {
        get { return $"{UserName}"; }
    }

    public TsNestedDelegateTokenProvider(IModernTokenService tokenService, string customerId, string tenantId, string userName)
        : base(tokenService, customerId, tenantId)
    {
        UserName = userName;
    }

    public override async ValueTask<AccessTokenResult> GetAccessTokenAsync(string resource, TokenResourceType resourceType, CancellationToken cancellationToken)
    {
        string cacheKey = GenerateTokenCacheKey(resource, resourceType);
        return await ProcessTokenResultAsync(async () =>
        {
            if (resourceType == TokenResourceType.SharePoint)
            {
                return await TokenService.GetSharePointTokenAsync(UserName, resource, SharePointTokenType.Bearer);
            }
            return await TokenService.GetTokenByServiceAccountAsync(resourceType, UserName);
        }, cacheKey);

    }
}