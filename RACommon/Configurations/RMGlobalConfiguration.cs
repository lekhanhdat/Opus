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
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.Contract.Configurations;
using Azure;
using System;
using System.IO;
using System.Net;

namespace AvePoint.RA.Common.Configurations
{
    public class RMGlobalConfiguration
    {
        private static RMDatabaseConfiguration _dbConfig;
        private static RMAppConfiguration _appConfig;
        private static RMEnvConfiguration _envSetting;
        private static RMCommonEncryptConfiguration _encryptConfig;
        private static RMStorageConfiguration _storageConfig;

        static RMGlobalConfiguration()
        {
            _envSetting = new RMEnvConfiguration();
            RMCertificateHelper.InitCerts();
            _dbConfig = new RMDatabaseConfiguration();
            _appConfig = new RMAppConfiguration();
            _encryptConfig = new RMCommonEncryptConfiguration();
            _storageConfig = new RMStorageConfiguration();
        }

        public static RMEnvConfiguration EnvSetting
        {
            get
            {
                return _envSetting;
            }
        }

        public static RMDatabaseConfiguration DBConfig
        {
            get
            {
                return _dbConfig;
            }
        }

        public static RMAppConfiguration AppConfig
        {
            get
            {
                return _appConfig;
            }
        }

        public static RMCommonEncryptConfiguration EncryptConfig
        {
            get
            {
                return _encryptConfig;
            }
        }

        public static RMStorageConfiguration StorageConfig
        {
            get
            {
                return _storageConfig;
            }
        }

        public static void Init()
        {
#if DEBUG
            while (File.Exists("C:\\InitGlobalConfig.sleep"))
            {
                System.Threading.Thread.Sleep(2000);
            }
#endif
        }

        /*private static string GetCertFromKV(string certName)
        {
            try
            {
                logger.Info($"Get KV cert: {certName}");
                var keyVaultUrl = EnvSetting[RMEnvSettingKey.KEY_VAULT_URL];
                if (string.IsNullOrEmpty(keyVaultUrl))
                {
                    logger.Warn("KEY_VAULT_URL not found.");
                    return null;
                }

                string secretValue = KeyVaultUtil.GetSecretAsync(certName, keyVaultUrl).Result;
                
                logger.Info($"Got KV cert: {certName}");
                return secretValue;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed get KV cert: {ex.ToString()}");
                return null;
            }
        }*/

       /* public static string GetSecretValue(string secretName, int retryCount = 0)
        {

            //AveRetryPolicy retryPolicy = new AveRetryPolicy(new ConfigurationTransientErrorStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(10)));
            //return retryPolicy.ExecuteAction(() =>
            //{
            try
            {
                var clientId = _envSetting[RMEnvSettingKey.KEY_VAULT_CLIENT_ID];
                var KeyVaultUrl = _envSetting[RMEnvSettingKey.KEY_VAULT_URL];
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(KeyVaultUrl))
                {
                    logger.Warn("clientId and Url not found.");
                    return string.Empty;
                }
                var secretClient = new SecretClient(new Uri(KeyVaultUrl), new DefaultAzureCredential());
                var credential = secretClient.GetSecretAsync(secretName).GetAwaiter().GetResult().Value;
                logger.Info($"get value from keyvault success, key:{secretName}.");
                return credential.Value;
            }
            catch (Exception ex)
            {
                logger.Error($"get secret value error,{secretName}:{ex.ToString()}");
                if (retryCount < 3)
                {
                    retryCount++;
                    Random r = new Random();
                    *//* Fortify Issue Type: Insecure Randomness 
                       Sink Details:  this position
                       Ignore Reason: random用于ThreadSleep 
                    *//*
                    int sleepTime = r.Next(100, 1000);  //随机Sleep时间， 避免并发阻塞
                    logger.Info("Sleep for {0} ms, and retry, retry count {1}", sleepTime, retryCount);
                    System.Threading.Thread.Sleep(sleepTime);
                    return GetSecretValue(secretName, retryCount); 
                }
                return string.Empty;
            }
               
            //});
        }*/


       /*public static async Task<string> GetTokenFromCert(string authority, string resource, string clientId)
        {
            //AuthenticationContext authenticationContext = new AuthenticationContext(authority, false);
            //ClientAssertionCertificate cac = new ClientAssertionCertificate(clientId, RMCertificateHelper.MasterCert);
            //var authenticationResult = await authenticationContext.AcquireTokenAsync(resource, cac);

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithCertificate(RMCertificateHelper.MasterCert)
            .WithAuthority(authority)
            //.WithRedirectUri(resourceUrl.ToString())
            .WithLegacyCacheCompatibility(false)
            .Build();

            // Add a token cache. For details about other serialization
            // see https://aka.ms/msal-net-cca-token-cache-serialization
            //app.AddInMemoryTokenCache();

            var authResult = await app.AcquireTokenForClient(new[] { new Uri(resource).GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/.default" }).ExecuteAsync();

            return authResult.AccessToken;
        }*/

        public sealed class ConfigurationTransientErrorStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                if (ex is RequestFailedException)
                {
                    var kvEx = ex as RequestFailedException;
                    if (kvEx != null && kvEx.Status == (int)HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static long GetDCDownloadFileSizeLimit()
        {
            var fileSizeLimit = AppConfig[RMAppSettingKey.DOWNLOADCENTER_DOWNLOAD_FILESIZE_LIMIT];
            var defaultMaxFileSize = 100 * 1024 * 1024;
            if (string.IsNullOrEmpty(fileSizeLimit))
            {
                return defaultMaxFileSize;
            }
            return long.Parse(fileSizeLimit);
        }
    }
}
