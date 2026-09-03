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
    using Microsoft365.Common.Logger;

    using System;
    using System.Net;

    internal static class NativeTokenProviderParameterExtension
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(NativeTokenProviderParameterExtension));

        internal static INestedTokenProvider CreateProviderByType(this NativeTokenProviderParameter parameter, NestedTokenProviderType providerType)
        {
            switch (providerType)
            {
                case NestedTokenProviderType.ApplicationBear:
                    return parameter.CreateAppOnlyBearTokenProvider();
                case NestedTokenProviderType.DelegateBear:
                    return parameter.CreateDelegateBearTokenProvider(false);
                case NestedTokenProviderType.AccountPoolDelegateBear:
                    return parameter.CreateDelegateBearTokenProvider(true);
                case NestedTokenProviderType.IDCLR:
                    return parameter.CreateIDCLRTokenProvider(false);
                case NestedTokenProviderType.AccountPoolIDCLR:
                    return parameter.CreateIDCLRTokenProvider(true);
                case NestedTokenProviderType.MicrosoftDelegate:
                    return parameter.CreateMicrosoftDelegateBearTokenProvider();
            }
            throw new ArgumentException($"Invalid token provider type {providerType}");
        }
        private static INestedTokenProvider CreateIDCLRTokenProvider(this NativeTokenProviderParameter parameter, bool pooUser)
        {
            var result = GetUserNetworkCredential(parameter, pooUser);
            var credential = result?.Item2;
            if (credential==null)
            {
                return null;
            }
            return new NativeNestedIDCLRTokenProvider(credential.UserName, credential.SecurePassword,parameter.EnvironmentType);
        }

        private static Tuple<bool,NetworkCredential> GetUserNetworkCredential(NativeTokenProviderParameter parameter, bool pooUser)
        {
            CheckArgs(parameter);
            bool isMfa = pooUser ? parameter.AccountPoolIsMFA : parameter.ServiceAccountIsMFA;
            if (string.IsNullOrWhiteSpace(pooUser ? parameter.AccountPoolUserName : parameter.ServiceAccountUserName) || string.IsNullOrWhiteSpace(pooUser ? parameter.AccountPoolPassword : parameter.ServiceAccountPassword))
            {
                return new Tuple<bool, NetworkCredential>(isMfa,null);
            }
            return new Tuple<bool, NetworkCredential>(isMfa,new NetworkCredential(pooUser ? parameter.AccountPoolUserName : parameter.ServiceAccountUserName, pooUser ? parameter.AccountPoolPassword : parameter.ServiceAccountPassword));
        }

        private static INestedTokenProvider CreateMicrosoftDelegateBearTokenProvider(this NativeTokenProviderParameter parameter)
        {
            var result = GetUserNetworkCredential(parameter, false);
            if (result != null && result.Item1)
            {
                logger.Warn($"Skip CreateMicrosoftDelegateBearTokenProvider for MFA account {result?.Item2?.UserName}");
                return null;
            }
            var credential = result?.Item2;
            if (credential == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(parameter.MicrosoftDelegateAppId))
            {
                return null;
            }
            return new NativeNestedMicrosoftDelegateTokenProvider(credential.UserName, credential.SecurePassword,parameter.MicrosoftDelegateAppId, parameter.EnvironmentType);
        }

        private static INestedTokenProvider CreateDelegateBearTokenProvider(this NativeTokenProviderParameter parameter, bool pooUser)
        {
            var result = GetUserNetworkCredential(parameter, pooUser);
            if (result != null && result.Item1)
            {
                logger.Warn($"Skip CreateDelegateBearTokenProvider for MFA account {result?.Item2?.UserName}");
                return null;
            }
            var credential = result?.Item2;
            if (credential == null)
            {
                return null;
            }
            return new NativeNestedDelegateTokenProvider(credential.UserName, credential.SecurePassword, parameter.EnvironmentType);
        }

        private static INestedTokenProvider CreateAppOnlyBearTokenProvider(this NativeTokenProviderParameter parameter)
        {
            CheckArgs(parameter);
            if (!IsAppParameterValid(parameter))
            {
                return null;
            }
            return new NativeNestedAppOnlyTokenProvider(parameter.TenantId, parameter.AppId, parameter.AppCertificate,parameter.EnvironmentType);
        }

        private static bool IsAppParameterValid(NativeTokenProviderParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.TenantId) || string.IsNullOrWhiteSpace(parameter.AppId) || parameter.AppCertificate == null)
            {
                return false;
            }
            return true;
        }

        internal static bool IsValid(this NativeTokenProviderParameter parameter)
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

        private static void CheckArgs(NativeTokenProviderParameter parameter)
        {
            ArgumentNullCheck(parameter, nameof(parameter));
            ArgumentObjetCheck(parameter.EnvironmentType, nameof(parameter.EnvironmentType), (AveAzureEnvironment env) => { return env != AveAzureEnvironment.None; });
            ArgumentNullCheck(parameter.TenantId, nameof(parameter.TenantId));
        }

        private static void ArgumentObjetCheck<T>(T arg, string argName, Func<T,bool> expectedExpression)
        {
            if (!expectedExpression(arg))
            {
                throw new ArgumentNullException($"{typeof(T).FullName} - Argument Name:{argName},Value:{arg}");
            }
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
}