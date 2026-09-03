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
using Azure.Identity;
using Microsoft.Azure.StackExchangeRedis;
using StackExchange.Redis;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AvePoint.RA.RedisCache
{
    public static class RedisConnectionFactory
    {
        private static readonly DefaultAzureCredential s_credential = new DefaultAzureCredential();

        public static ConnectionMultiplexer Connect(string connectionString, bool ignoreCertificateValidation = false, bool isDevelopmentEnvironment = false)
        {
            ArgumentCheck.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            return ConnectAsync(connectionString, ignoreCertificateValidation, isDevelopmentEnvironment).GetAwaiter().GetResult();
        }

        public static async Task<ConnectionMultiplexer> ConnectAsync(string connectionString, bool ignoreCertificateValidation = false, bool isDevelopmentEnvironment = false)
        {
            ArgumentCheck.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            var configurationOptions = ConfigurationOptions.Parse(connectionString);
            return await ConnectAsync(configurationOptions, ignoreCertificateValidation, isDevelopmentEnvironment).ConfigureAwait(false);
        }

        public static async Task<ConnectionMultiplexer> ConnectAsync(ConfigurationOptions configurationOptions, bool ignoreCertificateValidation = false, bool isDevelopmentEnvironment = false)
        {
            ArgumentCheck.NotNull(configurationOptions, nameof(configurationOptions));

            if (string.IsNullOrWhiteSpace(configurationOptions.Password) && !isDevelopmentEnvironment)
            {
                configurationOptions = await configurationOptions.ConfigureForAzureWithTokenCredentialAsync(s_credential).ConfigureAwait(false);
            }

            if (ignoreCertificateValidation)
            {
                configurationOptions.CertificateValidation += IgnoreCertificateValidation;
            }

            return await ConnectionMultiplexer.ConnectAsync(configurationOptions).ConfigureAwait(false);
        }

        private static bool IgnoreCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
