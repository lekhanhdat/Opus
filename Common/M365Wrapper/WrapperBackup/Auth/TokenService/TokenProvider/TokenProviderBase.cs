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

namespace Microsoft365.Authentication.TokenProvider;
using AvePoint.RA.CommonUtil;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public abstract class TokenProviderBase : IATokenProviderBase
{
    protected RALogger logger = RALogger.GetInstance(typeof(TokenProviderBase));
    protected ConcurrentDictionary<NestedTokenProviderType, INestedTokenProvider> ProviderList = new();
    public TokenProviderBase()
    { }

    public abstract ValueTask<AccessTokenResult> GetEwsTokenAsync(EwsTokenType tokenType = EwsTokenType.Adaptation, CancellationToken cancellationToken = default);

    public abstract ValueTask<AccessTokenResult> GetGraphTokenAsync(MSGraphTokenType tokenType = MSGraphTokenType.Adaptation, CancellationToken cancellationToken = default);

    public abstract ValueTask<AccessTokenResult> GetPowerAppsTokenAsync(PowerAppsTokenType tokenType = PowerAppsTokenType.Adaptation, CancellationToken cancellationToken = default);

    public abstract ValueTask<AccessTokenResult> GetPowerBITokenAsync(PowerBITokenType tokenType = PowerBITokenType.Adaptation, CancellationToken cancellationToken = default);

    public abstract ValueTask<AccessTokenResult> GetOutlookTokenAsync(OutlookTokenType tokenType = OutlookTokenType.Adaptation, CancellationToken cancellationToken = default);

    public abstract ValueTask<AccessTokenResult> GetVivaEngageTokenAsync(VivaEngageTokenType tokenType = VivaEngageTokenType.Adaptation, CancellationToken cancellationToken = default);

    public abstract ValueTask<AccessTokenResult> GetTeamsSkypeTokenAsync(TeamsSkypeTokenType tokenType = TeamsSkypeTokenType.Adaptation, CancellationToken cancellationToken = default);

    public virtual async ValueTask<AccessTokenResult> GetSharePointTokenAsync(string siteUrl, SPTokenType tokenType = SPTokenType.Adaptation, SPUserType userType = SPUserType.Adaptation, CancellationToken cancellationToken = default)
    {
        return await GetAvaliableTokenAsync(GetSharePointNestedTokenProviderType(tokenType, userType), siteUrl, TokenResourceType.SharePoint, cancellationToken);
    }

    protected virtual async ValueTask<AccessTokenResult> GetAvaliableTokenAsync(List<NestedTokenProviderType> source, string resource, TokenResourceType resourceType, CancellationToken cancellationToken)
    {
        AccessTokenResult token = default;
        foreach (var providerType in source)
        {
            token = await ProviderList.TryGetAccessTokenAsync(providerType, resource, resourceType, cancellationToken);
            if (token.IsValid())
            {
                return token;
            }
        }
        return token;
    }

    protected virtual List<NestedTokenProviderType> GetSharePointNestedTokenProviderType(SPTokenType tokenType, SPUserType userType) => (tokenType, userType) switch
    {
        (SPTokenType.DelegateBear, SPUserType.AccountPoolUser) => [NestedTokenProviderType.AccountPoolDelegateBear],
        (SPTokenType.DelegateBear, SPUserType.ServiceAccount) => [NestedTokenProviderType.DelegateBear],
        (SPTokenType.DelegateBear, SPUserType.Adaptation) => [NestedTokenProviderType.AccountPoolDelegateBear, NestedTokenProviderType.DelegateBear],
        (SPTokenType.ApplicationBear, _) => [NestedTokenProviderType.ApplicationBear],
        (SPTokenType.IDCLR, SPUserType.AccountPoolUser) => [NestedTokenProviderType.AccountPoolIDCLR],
        (SPTokenType.IDCLR, SPUserType.ServiceAccount) => [NestedTokenProviderType.IDCLR],
        (SPTokenType.IDCLR, SPUserType.Adaptation) => [NestedTokenProviderType.AccountPoolIDCLR, NestedTokenProviderType.IDCLR],
        (SPTokenType.Adaptation, SPUserType.AccountPoolUser) => [NestedTokenProviderType.ApplicationBear, NestedTokenProviderType.AccountPoolDelegateBear, NestedTokenProviderType.AccountPoolIDCLR],
        (SPTokenType.Adaptation, SPUserType.ServiceAccount) => [NestedTokenProviderType.ApplicationBear, NestedTokenProviderType.DelegateBear, NestedTokenProviderType.IDCLR],
        (SPTokenType.Adaptation, SPUserType.Adaptation) => [NestedTokenProviderType.ApplicationBear, NestedTokenProviderType.AccountPoolDelegateBear, NestedTokenProviderType.AccountPoolIDCLR, NestedTokenProviderType.DelegateBear, NestedTokenProviderType.IDCLR],
        _ => throw new NotSupportedException($"{tokenType} - {userType} is not supported.")
    };
}