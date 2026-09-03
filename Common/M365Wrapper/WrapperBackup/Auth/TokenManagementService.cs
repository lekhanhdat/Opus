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
using Cloud.Sdk.Token;
using Microsoft365.Authentication.TokenProvider.TokenService;
using Microsoft365.Authentication.TokenProvider;
using Microsoft365.Authentication.TokenService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.CommonUtil;
using Util.MSAzure;
using AvePoint.Application.TokenManager.TokenManagement;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Tenant;

namespace ExchangeUtility.Graph
{
    public class TokenManagementService 
    {
        private static RALogger logger = RALogger.GetInstance(typeof(TokenManagementService));

        public IATokenProviderBase CreateTokenProvider(BposInfo bposInfo)
        {
            return CreateTsTokenProvider(bposInfo);
        }


        private static void SetMicrosoftDelegateAppInfo(BposInfo bposInfo, TsTokenProviderParameter parameter)
        {
            var delegateApp = bposInfo?.ExternalBposInfos?.FirstOrDefault(t => t.AppType == AppType.MicrosoftDelegate)
                ?? (bposInfo?.AppType == AppType.MicrosoftDelegate ? bposInfo.UserAccountInfo : null);
            if (delegateApp is not null)
            {
                parameter.MicrosoftDelegateId = delegateApp.CustomerAppId;
            }
        }

        private static void SetMicrosoftDelegateAppInfo4ServiceAccount(BposInfo bposInfo, TsTokenProviderParameter parameter)
        {
            var delegateApp = bposInfo?.ExternalBposInfos?.FirstOrDefault(t => t.AppType == AppType.MicrosoftDelegate);
            if (delegateApp is not null)
            {
                parameter.MicrosoftDelegateAppUsername = bposInfo.UserAccountInfo.ServiceAccountUsername;
            }
        }

        private static void SetVivaEngageAppInfo(BposInfo bposInfo, TsTokenProviderParameter parameter)
        {
            var yammerApp = bposInfo?.ExternalBposInfos?.FirstOrDefault(t => t.AppType == AppType.YammerApp)
                ?? (bposInfo?.AppType == AppType.YammerApp ? bposInfo.UserAccountInfo : null);
            if (yammerApp is not null)
            {
                parameter.VivaEngageId = yammerApp.CustomerAppId;
            }
        }

        private IATokenProviderBase CreateTsTokenProvider(BposInfo bposInfo)
        {
            try
            {
                var parameter = new TsTokenProviderParameter
                {
                    CustomerId = bposInfo.CustomerId,
                    TenantId = bposInfo.UserAccountInfo.TenantId
                };

                switch (bposInfo.ConnectionType)
                {
                    case BposConnectionType.AppToken:
                        SetAppInfo(bposInfo, parameter);
                        SetAppEnvironment(bposInfo, parameter);
                        SetMicrosoftDelegateAppInfo(bposInfo, parameter);
                        SetVivaEngageAppInfo(bposInfo, parameter);
                        break;
                    case BposConnectionType.ServiceAccount:
                        SetAccountInfo(bposInfo, parameter);
                        SetEnvironment(bposInfo, parameter);
                        SetMicrosoftDelegateAppInfo4ServiceAccount(bposInfo, parameter);
                        break;
                    case BposConnectionType.Modern:
                        SetAppInfo(bposInfo, parameter);
                        SetAccountInfo(bposInfo, parameter);
                        SetEnvironment(bposInfo, parameter);
                        SetMicrosoftDelegateAppInfo(bposInfo, parameter);
                        SetVivaEngageAppInfo(bposInfo, parameter);
                        break;
                }
                return CreateTsTokenProvider(parameter) ?? throw new ArgumentException("Token Servie is not available to perform authentication.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when trying to create a token provider.ConnectionType:{bposInfo?.ConnectionType},Error:{ex}");
                return null;
            }
        }

        private IATokenProviderBase CreateTsTokenProvider(TsTokenProviderParameter parameter)
        {
            parameter.TokenService = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId).ModernTokenService;
            
            return new TsTokenProvider(parameter);
        }

        private static void SetEnvironment(BposInfo bposInfo, TsTokenProviderParameter parameter)
        {
            parameter.EnvironmentType = Convert(bposInfo.UserAccountInfo.AADEnvironment);
        }

        public static AzureEnvironment Convert(AADEnvironment environment)
        {
            return environment switch
            {
                AADEnvironment.AzureChinaCloud => AzureEnvironment.China,
                AADEnvironment.AzureGermanyCloud => AzureEnvironment.Germany,
                AADEnvironment.USGovernment => AzureEnvironment.USGovGCCHigh,
                //AADEnvironment.USDODCloud => AzureEnvironment.USGovDoD,
                _ => AzureEnvironment.Worldwide
            };
        }

        private static void SetAppEnvironment(BposInfo bposInfo, TsTokenProviderParameter parameter)
        {
            parameter.EnvironmentType = Convert(bposInfo.UserAccountInfo.AADEnvironment);
        }

        private static void SetAccountInfo(BposInfo bposInfo, TsTokenProviderParameter parameter)
        {
            bool isSecondarySAUnAvailable = string.IsNullOrEmpty(bposInfo.UserAccountInfo.SecondarySAUsername);
            bool isServiceAccountUnAvailable = string.IsNullOrEmpty(bposInfo.UserAccountInfo.ServiceAccountUsername);

            if (isSecondarySAUnAvailable)
            {
                if (isServiceAccountUnAvailable)
                {
                    //no user credential
                    return;
                }
                else
                {
                    parameter.ServiceAccountUserName = bposInfo.UserAccountInfo.ServiceAccountUsername;
                    parameter.ServiceAccountIsMFA = bposInfo.UserAccountInfo.ServiceAccountIsMFA;
                }
            }
            else
            {
                if (isServiceAccountUnAvailable)
                {
                    //no SA, but have seconary SA, assume credential have problem
                    throw new ArgumentException($"ServiceAccountUsername is valid, but SecondarySAUsername {bposInfo.UserAccountInfo.SecondarySAUsername} is not.");
                }
                else
                {
                    parameter.AccountPoolUserName = bposInfo.UserAccountInfo.ServiceAccountUsername;
                    parameter.ServiceAccountUserName = bposInfo.UserAccountInfo.SecondarySAUsername;
                    parameter.AccountPoolIsMFA = bposInfo.UserAccountInfo.ServiceAccountIsMFA;
                    parameter.ServiceAccountIsMFA = bposInfo.UserAccountInfo.SecondarySAIsMFA;
                }
            }
        }

        private static void SetAppInfo(BposInfo bposInfo, TsTokenProviderParameter parameter)
        {
            parameter.AppId = bposInfo.UserAccountInfo.AppId;
            parameter.AppType = bposInfo.AppType.ConvertToCloudSdkTokenAppType();
        }
    }
}
