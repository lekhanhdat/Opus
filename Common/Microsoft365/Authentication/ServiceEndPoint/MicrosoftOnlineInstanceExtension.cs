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
namespace Microsoft365.Authentication.ServiceEndPoint
{
    using System;
    using Microsoft365.Authentication;
    using Microsoft365.Common.Logger;

    public static class MicrosoftOnlineInstanceExtension
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(MicrosoftOnlineInstanceExtension));
        private const string AAD_ClientId_AzureChinaCloud = "1b730954-1685-4b74-9bfd-dac224a7b894";
        private const string AAD_ClientId_AzureCloud = "12128f48-ec9e-42f0-b203-ea49fb6af367";
        public static string GetAADPublicClientIdByEnvironment(AveAzureEnvironment environment)
        {
            string clientId = AAD_ClientId_AzureCloud;
            switch (environment)
            {
                case AveAzureEnvironment.AzureChinaCloud:
                    clientId = AAD_ClientId_AzureChinaCloud;
                    break;
                case AveAzureEnvironment.AzureCloud:
                case AveAzureEnvironment.AzureGermanyCloud:
                case AveAzureEnvironment.USGovernment:
                case AveAzureEnvironment.USGovernmentDOD:
                    clientId = AAD_ClientId_AzureCloud;
                    break;
                default:
                    clientId = AAD_ClientId_AzureCloud;
                    break;
            }
            logger.Info($"[MicrosoftOnlineInstanceExtension]GetAADPublicClientIdByEnvironment,Environment:{environment},ClientId:{clientId}");
            return clientId;
        }

        public static MicrosoftOnlineInstanceDetail GetMsoInstance(this AveAzureEnvironment environment)
        {
            switch (environment)
            {
                case AveAzureEnvironment.AzureChinaCloud:
                    return MicrosoftOnlineInstance.AzureChinaCloud;

                case AveAzureEnvironment.AzureGermanyCloud:
                    return MicrosoftOnlineInstance.AzureCloud;
                case AveAzureEnvironment.AzurePPE:
                    return MicrosoftOnlineInstance.AzureCloud;
                case AveAzureEnvironment.USGovernment:
                    return MicrosoftOnlineInstance.AzureUSGovernmentCloud;
                case AveAzureEnvironment.USGovernmentDOD:
                    return MicrosoftOnlineInstance.AzureUSGovernmentDODCloud;
                case AveAzureEnvironment.None:
                case AveAzureEnvironment.AzureCloud:
                default:
                    return MicrosoftOnlineInstance.AzureCloud;

            }
        }

        public static Tuple<AveAzureEnvironment, MicrosoftOnlineInstanceDetail> ResoveEnvironment(string domainOrPrincipalName, AveAzureEnvironment oldEnvironmnet)
        {
            AveAzureEnvironment environment = oldEnvironmnet;
            MicrosoftOnlineInstanceDetail instance = null;
            try
            {
                if (string.IsNullOrEmpty(domainOrPrincipalName))
                {
                    logger.Warn($"The domainOrPrincipalName is null or empty, will keep original enviroment type:{environment}");
                    return new Tuple<AveAzureEnvironment, MicrosoftOnlineInstanceDetail>(environment, instance);
                }
                instance = MicrosoftOnlineInstance.FromDomainOrPrincipalName(domainOrPrincipalName);
                if (instance == null)
                {
                    logger.Warn($"No AAD Environment was resolved by the domain name {domainOrPrincipalName}, will keep original enviroment type:{environment}");
                    return new Tuple<AveAzureEnvironment, MicrosoftOnlineInstanceDetail>(environment, instance);
                }
                if (string.Equals(instance.InitialDomainNameSuffix, MicrosoftOnlineInstance.AzureChinaCloud.InitialDomainNameSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    environment = AveAzureEnvironment.AzureChinaCloud;
                }
                else if (string.Equals(instance.InitialDomainNameSuffix, MicrosoftOnlineInstance.AzureGermanyCloud.InitialDomainNameSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    environment = AveAzureEnvironment.AzureGermanyCloud;
                }
                else if (string.Equals(instance.InitialDomainNameSuffix, MicrosoftOnlineInstance.AzureUSGovernmentCloud.InitialDomainNameSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    environment = AveAzureEnvironment.USGovernment;
                    if (string.Equals(instance.AdalMsGraphServiceResource, MicrosoftOnlineInstance.AzureUSGovernmentDODCloud.AdalMsGraphServiceResource, StringComparison.OrdinalIgnoreCase))
                    {
                        environment = AveAzureEnvironment.USGovernmentDOD;
                    }
                }
                else if (string.Equals(instance.InitialDomainNameSuffix, MicrosoftOnlineInstance.AzurePPE.InitialDomainNameSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    environment = AveAzureEnvironment.AzurePPE;
                }
                else if (string.Equals(instance.InitialDomainNameSuffix, MicrosoftOnlineInstance.AzureCloud.InitialDomainNameSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    environment = AveAzureEnvironment.AzureCloud;
                }
                logger.Info($"Get Real EnvironmentType:{environment} by User:{domainOrPrincipalName},Old One:{oldEnvironmnet}");
            }
            catch (Exception ex)
            {
                logger.Error($"Retrieve environment type failed.User: {domainOrPrincipalName},AveAzureEnvironment: {environment},Error:{ex}");
            }
            return new Tuple<AveAzureEnvironment, MicrosoftOnlineInstanceDetail>(environment, instance);
        }

        public static AveAzureEnvironment FromDomainOrPrincipalName(string domainOrPrincipalName, AveAzureEnvironment old)
        {
            return ResoveEnvironment(domainOrPrincipalName, old).Item1;
        }
    }
}