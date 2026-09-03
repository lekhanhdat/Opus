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


using AvePoint.RA.CommonUtil;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Token.Services;
using Microsoft365.Authentication.Token.Modern;
using Microsoft365.Authentication.Token.ModernToken;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.TokenProvider.TokenService;

class TsNestedModernIDCLRTokenProvider : TsNestedTokenProviderBase
{
    private static RALogger logger = RALogger.GetInstance(typeof(TsNestedModernIDCLRTokenProvider));
    public override NestedTokenProviderType TokenFactoryType
    {
        get { return NestedTokenProviderType.IDCLR; }
    }

    protected TsNestedDelegateTokenProvider InnerDelegateBearTokenProvider { get; set; }
    protected ITokenTypeConverter TokenTypeConverter { get; set; }
    public string UserName { get; set; }

    protected override string Identity
    {
        get { return UserName; }
    }

    public TsNestedModernIDCLRTokenProvider(IModernTokenService tokenService, string customerId, string tenantId, string userName)
        : base(tokenService, customerId, tenantId)
    {
        UserName = userName;
        TokenTypeConverter = DefaultTokenTypeConverter.Instance;
        InnerDelegateBearTokenProvider = new TsNestedDelegateTokenProvider(tokenService, customerId, tenantId, userName);
    }

    public override async ValueTask<AccessTokenResult> GetAccessTokenAsync(string resource, TokenResourceType resourceType, CancellationToken cancellationToken)
    {
        return await ProcessTokenResultAsync(
           async () =>
           {
               var token = await InnerDelegateBearTokenProvider.GetAccessTokenAsync(resource, resourceType, cancellationToken);
               if (token.IsValid())
               {
                   var result = TokenTypeConverter.ConvertBearToCookie(new Uri(resource), token.AccessToken, false);
                   return new AccessTokenResult(result, string.Empty, DateTimeOffset.UtcNow.AddHours(1), TokenType.IDCLR);
               }
               else
               {
                   logger.Error($"Get modern token failed because Delegate token is not valid.ExpiresOn:{token?.ExpiresOn},TokenType:{token?.TokenType},Error:{token?.Error}");
               }
               return token;
           },
           GenerateTokenCacheKey(resource, resourceType));
    }
}