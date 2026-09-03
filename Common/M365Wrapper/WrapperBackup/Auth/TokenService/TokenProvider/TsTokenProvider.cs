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

using Cloud.Sdk.Data.AosModern;
using Microsoft365.Common.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Util.MSAzure;

internal class TsTokenProvider : TokenProviderBase, IATokenProviderBase
{
    private const string Skype_Resource_URL = "https://api.spaces.skype.com";
    private readonly EwsTokenType[] ewsTokenTypeOrders = [EwsTokenType.ApplicationBear, EwsTokenType.DelegateBear];
    private readonly MSGraphTokenType[] graphTokenTypeOrders = [MSGraphTokenType.ApplicationBear, MSGraphTokenType.DelegateGroupTeamBear, MSGraphTokenType.MicrosoftDelegate, MSGraphTokenType.DelegateUserBear, MSGraphTokenType.MicrosoftDelegateCombineServiceAccount];
    private readonly PowerAppsTokenType[] powerAppsTokenTypeOrders = [PowerAppsTokenType.MicrosoftDelegate, PowerAppsTokenType.DelegateUserBear];
    private readonly PowerBITokenType[] powerBITokenTypeOrders = [PowerBITokenType.MicrosoftDelegate, PowerBITokenType.DelegateUserBear];
    private readonly OutlookTokenType[] outlookTokenTypeOrders = [OutlookTokenType.ApplicationBear, OutlookTokenType.DelegateBear];

    protected TsTokenProviderParameter Parameter { get; set; }

    public TsTokenProvider(TsTokenProviderParameter parameter)
    {
        Parameter = parameter;
        var factoryTypes = Enum.GetValues(typeof(NestedTokenProviderType))
                         .Cast<NestedTokenProviderType>()
                         .ToList();
        foreach (var type in factoryTypes)
        {
            var provider = parameter.CreateProviderByType(type);
            if (provider != null && ProviderList.TryAdd(type, provider))
            {
                logger.Info($"Add {type} provider {provider.GetType().FullName} to ProviderList.");
            }
        }
    }

    public override async ValueTask<AccessTokenResult> GetEwsTokenAsync(EwsTokenType tokenType = EwsTokenType.Adaptation, CancellationToken cancellationToken = default)
    {
        return tokenType switch
        {
            EwsTokenType.DelegateBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.DelegateBear, null, TokenResourceType.ExchangeWebService, cancellationToken),
            EwsTokenType.ApplicationBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.ApplicationBear, null, TokenResourceType.ExchangeWebService, cancellationToken),
            EwsTokenType.Adaptation => await AdaptiveGetTokenAsync(ewsTokenTypeOrders, GetEwsTokenAsync, cancellationToken),
            _ => throw new NotSupportedException($"{nameof(EwsTokenType)} {tokenType} is not supported.")
        };
    }

    public override async ValueTask<AccessTokenResult> GetGraphTokenAsync(MSGraphTokenType tokenType = MSGraphTokenType.Adaptation, CancellationToken cancellationToken = default)
    {
        var serviceUrl = Endpoints.GetEndpoints(Parameter.EnvironmentType).MicrosoftGraph;
        return tokenType switch
        {
            MSGraphTokenType.DelegateUserBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.DelegateBear, serviceUrl, TokenResourceType.ExchangeGraph, cancellationToken),
            MSGraphTokenType.DelegateGroupTeamBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.DelegateBear, serviceUrl, TokenResourceType.Teams, cancellationToken),
            MSGraphTokenType.MicrosoftDelegate => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.MicrosoftDelegate, serviceUrl, TokenResourceType.None, cancellationToken),
            MSGraphTokenType.MicrosoftDelegateCombineServiceAccount => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.MicrosoftDelegateCombineServiceAccount, serviceUrl, TokenResourceType.Teams, cancellationToken),
            MSGraphTokenType.ApplicationBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.ApplicationBear, serviceUrl, TokenResourceType.Graph, cancellationToken),
            MSGraphTokenType.ApplicationDelegateBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.ApplicationDelegateBear, serviceUrl, TokenResourceType.Graph, cancellationToken),
            MSGraphTokenType.Adaptation => await AdaptiveGetTokenAsync(graphTokenTypeOrders, GetGraphTokenAsync, cancellationToken),
            _ => throw new NotSupportedException($"{nameof(EwsTokenType)}  {tokenType} is not supported.")
        };
    }

    public override async ValueTask<AccessTokenResult> GetSharePointTokenAsync(string siteUrl, SPTokenType tokenType = SPTokenType.Adaptation, SPUserType userType = SPUserType.Adaptation, CancellationToken cancellationToken = default)
    {
        return tokenType switch
        {
            SPTokenType.DelegateBear => await FromUserTypeAsync(siteUrl, userType, NestedTokenProviderType.DelegateBear, NestedTokenProviderType.AccountPoolDelegateBear, cancellationToken),
            SPTokenType.ApplicationBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.ApplicationBear, siteUrl, TokenResourceType.SharePoint, cancellationToken),
            SPTokenType.IDCLR => await FromUserTypeAsync(siteUrl, userType, NestedTokenProviderType.IDCLR, NestedTokenProviderType.AccountPoolIDCLR, cancellationToken),
            SPTokenType.Adaptation => await AdaptiveGetSharePointTokenAsync(siteUrl, userType, cancellationToken),
            _ => throw new NotSupportedException($"{nameof(EwsTokenType)}  {tokenType} is not supported.")
        };
    }

    public override async ValueTask<AccessTokenResult> GetPowerAppsTokenAsync(PowerAppsTokenType tokenType = PowerAppsTokenType.Adaptation, CancellationToken cancellationToken = default)
    {
        var serviceUrl = Endpoints.GetEndpoints(Parameter.EnvironmentType).MicrosoftGraph;
        return tokenType switch
        {
            PowerAppsTokenType.MicrosoftDelegate => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.MicrosoftDelegate, serviceUrl, TokenResourceType.PowerApps, cancellationToken),
            PowerAppsTokenType.DelegateUserBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.DelegateBear, serviceUrl, TokenResourceType.PowerApps, cancellationToken),
            PowerAppsTokenType.Adaptation => await AdaptiveGetTokenAsync(powerAppsTokenTypeOrders, GetPowerAppsTokenAsync, cancellationToken),
            _ => throw new NotSupportedException($"{nameof(PowerAppsTokenType)} {tokenType} is not supported.")
        };
    }

    public override async ValueTask<AccessTokenResult> GetPowerBITokenAsync(PowerBITokenType tokenType = PowerBITokenType.Adaptation, CancellationToken cancellationToken = default)
    {
        var serviceUrl = Endpoints.GetEndpoints(Parameter.EnvironmentType).PowerBIResource;
        return tokenType switch
        {
            PowerBITokenType.MicrosoftDelegate => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.MicrosoftDelegate, serviceUrl, TokenResourceType.PowerBI, cancellationToken),
            PowerBITokenType.DelegateUserBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.DelegateBear, serviceUrl, TokenResourceType.PowerBI, cancellationToken),
            PowerBITokenType.Adaptation => await AdaptiveGetTokenAsync(powerBITokenTypeOrders, GetPowerBITokenAsync, cancellationToken),
            _ => throw new NotSupportedException($"{nameof(PowerBITokenType)} {tokenType} is not supported.")
        };
    }

    public override async ValueTask<AccessTokenResult> GetOutlookTokenAsync(OutlookTokenType tokenType = OutlookTokenType.Adaptation, CancellationToken cancellationToken = default)
    {
        return tokenType switch
        {
            OutlookTokenType.DelegateBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.DelegateBear, null, TokenResourceType.Outlook, cancellationToken),
            OutlookTokenType.ApplicationBear => await ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.ApplicationBear, null, TokenResourceType.Outlook, cancellationToken),
            OutlookTokenType.Adaptation => await AdaptiveGetTokenAsync(outlookTokenTypeOrders, GetOutlookTokenAsync, cancellationToken),
            _ => throw new NotSupportedException($"{nameof(OutlookTokenType)} {tokenType} is not supported.")
        };
    }

    public override ValueTask<AccessTokenResult> GetVivaEngageTokenAsync(VivaEngageTokenType tokenType = VivaEngageTokenType.Adaptation, CancellationToken cancellationToken = default)
    {
        return tokenType switch
        {
            VivaEngageTokenType.ApplicationBear or VivaEngageTokenType.Adaptation => ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.VivaEngage, null, TokenResourceType.None, cancellationToken),
            _ => throw new NotSupportedException($"{nameof(VivaEngageTokenType)} {tokenType} is not supported.")
        };
    }

    public override ValueTask<AccessTokenResult> GetTeamsSkypeTokenAsync(TeamsSkypeTokenType tokenType = TeamsSkypeTokenType.Adaptation, CancellationToken cancellationToken = default)
    {
        return tokenType switch
        {
            TeamsSkypeTokenType.DelegateUserBear or TeamsSkypeTokenType.Adaptation => ProviderList.TryGetAccessTokenAsync(NestedTokenProviderType.DelegateBear, Skype_Resource_URL, TokenResourceType.TeamsSkype, cancellationToken),
            _ => throw new NotSupportedException($"{nameof(TeamsSkypeTokenType)}  {tokenType} is not supported.")
        };
    }

    private async ValueTask<AccessTokenResult> AdaptiveGetTokenAsync<T>(T[] tokenTypeOders, Func<T, CancellationToken, ValueTask<AccessTokenResult>> func, CancellationToken cancellationToken) where T : Enum
    {
        AccessTokenResult tokenResult = null;
        foreach (var tokenType in tokenTypeOders)
        {
            tokenResult = await func(tokenType, cancellationToken);
            if (tokenResult.IsValid())
            {
                return tokenResult;
            }
        }

        return tokenResult;
    }

    private async ValueTask<AccessTokenResult> AdaptiveGetSharePointTokenAsync(string siteUrl, SPUserType userType, CancellationToken cancellationToken)
    {
        var tokenTypeOrders = new Dictionary<SPTokenType, SPUserType>
        {
            [SPTokenType.ApplicationBear] = SPUserType.Adaptation,
            [SPTokenType.DelegateBear] = userType,
            [SPTokenType.IDCLR] = userType
        };

        AccessTokenResult tokenResult = null;
        foreach (var typeInfo in tokenTypeOrders)
        {
            tokenResult = await GetSharePointTokenAsync(siteUrl, typeInfo.Key, typeInfo.Value, cancellationToken);
            if (tokenResult.IsValid())
            {
                return tokenResult;
            }
        }

        return tokenResult;
    }

    private async ValueTask<AccessTokenResult> FromUserTypeAsync(
        string siteUrl,
        SPUserType userType,
        NestedTokenProviderType defaultServiceAccountType,
        NestedTokenProviderType defaultPoolUserType,
        CancellationToken cancellationToken)
    {
        INestedTokenProvider nestedTokenProvider = userType switch
        {
            SPUserType.ServiceAccount => ProviderList.TryGetValue(defaultServiceAccountType),
            SPUserType.AccountPoolUser => ProviderList.TryGetValue(defaultPoolUserType),
            _ => ProviderList.TryGetValue(defaultPoolUserType) ?? ProviderList.TryGetValue(defaultServiceAccountType),
        };
        if (nestedTokenProvider is null)
        {
            return null;
        }
        return await nestedTokenProvider.GetAccessTokenAsync(siteUrl, TokenResourceType.SharePoint, cancellationToken);
    }
}