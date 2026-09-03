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
using System;
using M365.Wrapper.Backup.Auth.TokenService.TokenProvider.Nested;

namespace Microsoft365.Authentication.TokenProvider.TokenService;

internal static class TsTokenProviderParameterExtension
{
    private static RALogger logger = RALogger.GetInstance(typeof(TsTokenProviderParameterExtension));

    internal static INestedTokenProvider CreateProviderByType(this TsTokenProviderParameter parameter, NestedTokenProviderType providerType)
    {
        return providerType switch
        {
            NestedTokenProviderType.ApplicationBear => parameter.CreateAppOnlyBearTokenProvider(),
            NestedTokenProviderType.ApplicationDelegateBear => parameter.CreateAppDelegateBearTokenProvider(),
            NestedTokenProviderType.DelegateBear => parameter.CreateDelegateBearTokenProvider(false),
            NestedTokenProviderType.AccountPoolDelegateBear => parameter.CreateDelegateBearTokenProvider(true),
            NestedTokenProviderType.IDCLR => parameter.CreateIDCLRTokenProvider(false),
            NestedTokenProviderType.AccountPoolIDCLR => parameter.CreateIDCLRTokenProvider(true),
            NestedTokenProviderType.MicrosoftDelegate => parameter.CreateMicrosoftDelegateProvider(),
            NestedTokenProviderType.MicrosoftDelegateCombineServiceAccount => parameter.CreateMicrosoftDelegateTokenCombineServiceAccountProvider(),
            NestedTokenProviderType.VivaEngage => parameter.CreateVivaEngageTokenProvider(),
            _ => throw new ArgumentException($"Invalid token provider type {providerType}"),
        };
    }

    private static INestedTokenProvider CreateIDCLRTokenProvider(this TsTokenProviderParameter parameter, bool pooUser)
    {
        CheckArgs(parameter);
        string userName = pooUser ? parameter.AccountPoolUserName : parameter.ServiceAccountUserName;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var mfaEnabled = pooUser ? parameter.AccountPoolIsMFA : parameter.ServiceAccountIsMFA;
        if (mfaEnabled)
        {
            return new TsNestedIDCLRTokenProvider(parameter.TokenService, parameter.CustomerId, parameter.TenantId, userName);
        }
        return new TsNestedModernIDCLRTokenProvider(parameter.TokenService, parameter.CustomerId, parameter.TenantId, userName);
    }

    private static INestedTokenProvider CreateMicrosoftDelegateProvider(this TsTokenProviderParameter parameter)
    {
        CheckArgs(parameter);
        if (string.IsNullOrWhiteSpace(parameter.MicrosoftDelegateId))
        {
            return null;
        }
        return new TsNestedMicrosoftDelegateProvider(parameter.TokenService, parameter.CustomerId, parameter.TenantId, parameter.MicrosoftDelegateId);
    }

    private static INestedTokenProvider CreateMicrosoftDelegateTokenCombineServiceAccountProvider(this TsTokenProviderParameter parameter)
    {
        CheckArgs(parameter);
        if (string.IsNullOrWhiteSpace(parameter.MicrosoftDelegateAppUsername))
        {
            return null;
        }
        return new TsNestedMicrosoftDelegateCombineServiceAccountTokenProvider(parameter.TokenService, parameter.CustomerId, parameter.TenantId, parameter.MicrosoftDelegateAppUsername);
    }

    private static INestedTokenProvider CreateDelegateBearTokenProvider(this TsTokenProviderParameter parameter, bool pooUser, string appId = "")
    {
        CheckArgs(parameter);
        string userName = pooUser ? parameter.AccountPoolUserName : parameter.ServiceAccountUserName;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var mfaEnabled = pooUser ? parameter.AccountPoolIsMFA : parameter.ServiceAccountIsMFA;
        if (mfaEnabled)
        {
            return null;
        }
        return new TsNestedDelegateTokenProvider(parameter.TokenService, parameter.CustomerId, parameter.TenantId, userName);
    }

    private static INestedTokenProvider CreateAppOnlyBearTokenProvider(this TsTokenProviderParameter parameter)
    {
        CheckArgs(parameter);
        if (!IsAppParameterValid(parameter))
        {
            return null;
        }
        return new TsNestedAppOnlyTokenProvider(parameter.TokenService, parameter.CustomerId, parameter.TenantId, parameter.AppType.Value, parameter.AppId);
    }
    
    private static INestedTokenProvider CreateAppDelegateBearTokenProvider(this TsTokenProviderParameter parameter)
    {
        CheckArgs(parameter);
        if (!IsAppParameterValid(parameter))
        {
            return null;
        }
        return new TsNestedAppDelegateTokenProvider(parameter.TokenService, parameter.CustomerId, parameter.TenantId, parameter.AppType.Value, parameter.AppId);
    }

    private static INestedTokenProvider CreateVivaEngageTokenProvider(this TsTokenProviderParameter parameter)
    {
        CheckArgs(parameter);
        if (parameter.VivaEngageId.IsNullOrEmpty())
        {
            return null;
        }
        return new TsNestedAppOnlyTokenProvider(parameter.TokenService, parameter.CustomerId, parameter.TenantId, IdentityProviderType.Yammer, parameter.VivaEngageId);
    }

    private static bool IsAppParameterValid(TsTokenProviderParameter parameter)
    {
        if (parameter.AppType == null
            || parameter.AppType == IdentityProviderType.MicrosoftDelegate
            || parameter.AppType == IdentityProviderType.Local
            || (parameter.AppType == IdentityProviderType.CustomAzureApp && string.IsNullOrWhiteSpace(parameter.AppId)))
        {
            return false;
        }
        return true;
    }

    internal static bool IsValid(this TsTokenProviderParameter parameter)
    {
        try
        {
            CheckArgs(parameter);
            return true;
        }
        catch (ArgumentNullException ex)
        {
            logger.Warn($"TsTokenProviderParameter is not valid.Error:{ex.Message}");
        }
        return false;
    }

    private static void CheckArgs(TsTokenProviderParameter parameter)
    {
        ArgumentNullCheck(parameter, nameof(parameter));
        ArgumentNullCheck(parameter.CustomerId, nameof(parameter.CustomerId));
        ArgumentNullCheck(parameter.TenantId, nameof(parameter.TenantId));
        ArgumentNullCheck(parameter.TokenService, nameof(parameter.TokenService));
    }

    private static void ArgumentNullCheck<T>(T arg, string argName)
    {
        if (Equals(arg, default(T)))
        {
            throw new ArgumentNullException($"{typeof(T).FullName} - Argument Name:{argName}");
        }
    }

    private static void ArgumentNullCheck(string arg, string argName)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            throw new ArgumentNullException($"Argument Name:{argName}");
        }
    }
}