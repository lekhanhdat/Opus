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
namespace Microsoft365.Authentication.TokenProvider
{
    using Microsoft365.Common.Cache;
    using Microsoft365.Common.Extension;
    using Microsoft365.Common.Logger;

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class TokenProviderBase : IATokenProvider
    {
        protected IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(TokenProviderBase));
        protected ConcurrentDictionary<NestedTokenProviderType, INestedTokenProvider> ProviderList = new ConcurrentDictionary<NestedTokenProviderType, INestedTokenProvider>();
        public TokenProviderBase()
        { }
        public abstract AccessTokenResult GetEwsToken(EWSTokenType tokenType);

        public abstract AccessTokenResult GetGraphToken(MSGraphTokenType tokenType);

        public abstract ValueTask<AccessTokenResult> GetGraphTokenAsync(MSGraphTokenType tokenType = MSGraphTokenType.Adaptation, CancellationToken cancellationToken = default);

        public virtual AccessTokenResult GetSharePointToken(string siteUrl, SPTokenType tokenType, SPUserType userType)
        {
            return GetAvaliableToken(GetSharePointNestedTokenProviderType(tokenType, userType), siteUrl, AuthenticationResourceType.SharePoint);
        }

        protected virtual AccessTokenResult GetAvaliableToken(List<NestedTokenProviderType> source, string resource, AuthenticationResourceType resourceType)
        {
            AccessTokenResult token = default;
            foreach (var providerType in source)
            {
                token = ProviderList.TryGetAccessToken(providerType, resource, resourceType);
                if (token.IsValid())
                {
                    return token;
                }
            }
            return token;
        }
        protected virtual List<NestedTokenProviderType> GetSharePointNestedTokenProviderType(SPTokenType tokenType, SPUserType userType) => (tokenType, userType) switch
        {
            (SPTokenType.DelegateBear, SPUserType.AccountPoolUser) => new() { NestedTokenProviderType.AccountPoolDelegateBear },
            (SPTokenType.DelegateBear, SPUserType.ServiceAccount) => new() { NestedTokenProviderType.DelegateBear },
            (SPTokenType.DelegateBear, SPUserType.Adaptation) => new() { NestedTokenProviderType.AccountPoolDelegateBear, NestedTokenProviderType.DelegateBear },
            (SPTokenType.ApplicationBear, _) => new() { NestedTokenProviderType.ApplicationBear },
            (SPTokenType.IDCLR, SPUserType.AccountPoolUser) => new() { NestedTokenProviderType.AccountPoolIDCLR },
            (SPTokenType.IDCLR, SPUserType.ServiceAccount) => new() { NestedTokenProviderType.IDCLR },
            (SPTokenType.IDCLR, SPUserType.Adaptation) => new() { NestedTokenProviderType.AccountPoolIDCLR, NestedTokenProviderType.IDCLR },
            (SPTokenType.Adaptation, SPUserType.AccountPoolUser) => new() { NestedTokenProviderType.ApplicationBear, NestedTokenProviderType.AccountPoolDelegateBear, NestedTokenProviderType.AccountPoolIDCLR },
            (SPTokenType.Adaptation, SPUserType.ServiceAccount) => new() { NestedTokenProviderType.ApplicationBear, NestedTokenProviderType.DelegateBear, NestedTokenProviderType.IDCLR },
            (SPTokenType.Adaptation, SPUserType.Adaptation) => new() { NestedTokenProviderType.ApplicationBear, NestedTokenProviderType.AccountPoolDelegateBear, NestedTokenProviderType.AccountPoolIDCLR, NestedTokenProviderType.DelegateBear, NestedTokenProviderType.IDCLR },
            _ => throw new NotSupportedException($"{tokenType} - {userType} is not supported.")
        };

    }
}