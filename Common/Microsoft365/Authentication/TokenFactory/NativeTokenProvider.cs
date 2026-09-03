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

using Microsoft365.Authentication.ServiceEndPoint;
using Microsoft365.Authentication.TokenProvider;
using Microsoft365.Common.Extension;
using Microsoft365.Common.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft365.Authentication
{
    internal class NativeTokenProvider : TokenProviderBase, IATokenProvider
    {
        protected NativeTokenProviderParameter Parameter { get; set; }
        public NativeTokenProvider(NativeTokenProviderParameter parameter)
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
                    logger.Trace($"Add provider {provider.GetType().FullName} to ProviderList.");
                }
            }
        }
        public override AccessTokenResult GetEwsToken(EWSTokenType tokenType)
        {
            switch (tokenType)
            {
                case EWSTokenType.DelegateBear:
                    return ProviderList.TryGetAccessToken(NestedTokenProviderType.DelegateBear, "", AuthenticationResourceType.ExchangeWebService);
                case EWSTokenType.ApplicationBear:
                    return ProviderList.TryGetAccessToken(NestedTokenProviderType.ApplicationBear, "", AuthenticationResourceType.ExchangeWebService);
                case EWSTokenType.Adaptation:
                default:
                    return GetEwsToken(EWSTokenType.ApplicationBear) ?? GetEwsToken(EWSTokenType.DelegateBear);
            }
            throw new NotSupportedException($"EWSTokenType {tokenType} is not supported.");
        }

        public override ValueTask<AccessTokenResult> GetGraphTokenAsync(MSGraphTokenType tokenType = MSGraphTokenType.Adaptation, CancellationToken cancellationToken = default)
        {
            return new ValueTask<AccessTokenResult>(GetGraphToken(tokenType));
        }

        public override AccessTokenResult GetGraphToken(MSGraphTokenType tokenType)
        {
            var env = Parameter.EnvironmentType.GetMsoInstance();
            switch (tokenType)
            {
                case MSGraphTokenType.DelegateUserBear:
                    return ProviderList.TryGetAccessToken(NestedTokenProviderType.DelegateBear, env.AdalMsGraphServiceResource, AuthenticationResourceType.ExchangeGraph);
                case MSGraphTokenType.DelegateGroupTeamBear:
                    return ProviderList.TryGetAccessToken(NestedTokenProviderType.DelegateBear, env.AdalMsGraphServiceResource, AuthenticationResourceType.Teams);
                case MSGraphTokenType.ApplicationBear:
                case MSGraphTokenType.Adaptation:
                    return ProviderList.TryGetAccessToken(NestedTokenProviderType.ApplicationBear, env.AdalMsGraphServiceResource, AuthenticationResourceType.Graph);
                case MSGraphTokenType.MicrosoftDelegate:
                    return ProviderList.TryGetAccessToken(NestedTokenProviderType.MicrosoftDelegate, env.AdalMsGraphServiceResource, AuthenticationResourceType.Teams);
            }
            throw new NotSupportedException($"MSGraphTokenType {tokenType} is not supported.");
        }

    }
}