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
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.RA.Common.Security
{
    public class RMCertificateHelper
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RMCertificateHelper));

        private static readonly Dictionary<String, X509Certificate2> cachedCertificates
            = new Dictionary<String, X509Certificate2>();

        private static X509Certificate2 _masterCert;
        /// <summary>
        /// 只在开发的local环境下使用
        /// </summary>
        public static X509Certificate2 MasterCert
        {
            get
            {
                try
                {
                    if (_masterCert == null)
                    {
                        string thumbprint = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.MASTER_CERTIFICATE_THUMBPRINT];
                        if (!string.IsNullOrEmpty(thumbprint))
                        {
                            _masterCert = GetCertFromLocalByThumbprint(thumbprint);
                            logger.Info($"Set master key thumbprint {_masterCert?.Thumbprint}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"init master cert error:{ex.ToString()}");
                }
               
                return _masterCert;
            }
        }

        private static Dictionary<string, Tuple<string, string>> certMappings = new Dictionary<string, Tuple<string, string>>()
        {
            { RMCertNames.AvePointRecords, Tuple.Create(
                RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.PRODUCT_CERTIFICATE_IDENTIFIER], 
                RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.PRODUCT_CERTIFICATE_THUMBPRINT]) 
            }
        };


        public static void InitCerts()
        {
            try
            {
                logger.Info("Certs Init.");
                foreach (var item in certMappings)
                {
                    var certificate = LoadCertificate(item.Key);
                    if (certificate != null)
                    {
                        cachedCertificates[item.Key] = certificate;
                        logger.Info($"Set certifcate {item.Key} | {certificate?.Thumbprint == null}");
                    }
                    else
                    {
                        logger.Error($"Failed to set certifcate {item.Key}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info($"Failed to set cert for product, error message: {ex.ToString()}");
            }
        }

        private static X509Certificate2 LoadCertificate(String type)
        {
            try
            {
                X509Certificate2 certificate = null;
                var certInfo = certMappings[type];
                var certName = certInfo.Item1;
                var certThumbprint = certInfo.Item2;
                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    certificate = GetCertFromLocalByThumbprint(certThumbprint);
                }
                else
                {
                    logger.Info($"LoadCertificate, Start");
                    var certificateSettingFromxml = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.PRODUCT_CERTIFICATE_IDENTIFIER];
                    var content = CipherEncryptionUtil.CipherDecrypt(certificateSettingFromxml);
                    var hasPkey = content.IndexOf("-----BEGIN PRIVATE KEY-----") > -1;
                    if (hasPkey)
                    {
                        certificate = X509Certificate2.CreateFromPem(content, content);
                    }
                    else
                    {
                        certificate = X509Certificate2.CreateFromPem(content);
                    }
                }
                return certificate;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed get KV cert: {ex.ToString()}");
                return null;
            }
        }

        public static X509Certificate2 GetCertificate(string type)
        {
            logger.Info($"GetCertificate, certificate: {type}");
            if (!cachedCertificates.TryGetValue(type, out var certificate))
            {
                throw new NotSupportedException(string.Format("Certificate type:{0} is not supported.", type));
            }
            return certificate;
        }

        private static X509Certificate2 GetCertFromLocalByThumbprint(string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint)) return null;

            var certificate = Get509Cert(StoreLocation.LocalMachine, thumbprint);
            if (certificate == null)
            {
                certificate = Get509Cert(StoreLocation.CurrentUser, thumbprint);
            }
            if (certificate == null)
            {
                throw new Exception(string.Format("Can't find certificate by thumbprint {0}.", thumbprint));
            }
            else
            {
                return certificate;
            }
        }

        private static X509Certificate2 Get509Cert(StoreLocation location, string thumbprint)
        {
            var store = new X509Store(StoreName.My, location);
            store.Open(OpenFlags.ReadOnly);
            var x509cerCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (x509cerCollection.Count == 0)
            {
                return null;
            }
            X509Certificate2 cer = x509cerCollection[0];
            store.Close();
            return cer;
        }

        public static X509Certificate2 GetCertificateByManagedIdentity(string keyVaultUrl, string certificateName)
        {
            var secretValue = GetSecretByManagedIdentity(keyVaultUrl, certificateName);
            var certByte = System.Convert.FromBase64String(secretValue);
            return new X509Certificate2(certByte, string.Empty, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        }

        public static string GetSecretByManagedIdentity(string keyVaultUrl, string certificateName)
        {
            logger.Info($"Get KV cert: {certificateName}");
            return KeyVaultUtil.GetSecretAsync(certificateName, keyVaultUrl).Result;
        }

        /*public static string GetSecretByClient(string keyVaultUrl, string clientId, string secretName, int retryCount = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(keyVaultUrl))
                {
                    logger.Warn($"keyVaultUrl or clientId not found: {keyVaultUrl}");
                    return string.Empty;
                }
                var keyClient = new KeyVaultClient((authority, resource, scope) =>
                {
                    return GetTokenFromCert(authority, resource, clientId);
                });
                var credential = keyClient.GetSecretAsync(keyVaultUrl, secretName).GetAwaiter().GetResult();
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
                    * Sink Details:  this position
                    * Ignore Reason: random用于ThreadSleep 
                    *//*
                    int sleepTime = r.Next(100, 1000);  //随机Sleep时间， 避免并发阻塞
                    logger.Info("Sleep for {0} ms, and retry, retry count {1}", sleepTime, retryCount);
                    System.Threading.Thread.Sleep(sleepTime);
                    return GetSecretByClient(keyVaultUrl, clientId, secretName, retryCount);
                }
                return string.Empty;
            }
        }

        private static async Task<string> GetTokenFromCert(string authority, string resource, string clientId)
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
    }
}
