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
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.RA.CommonUtil;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;
using AvePoint.RA.Common.Cache;
using System.IO;

namespace AvePoint.RA.Common.Util
{
    public class RASecurityKeyEncryptor
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RASecurityKeyEncryptor));
        readonly AveRsaHelper rsaHelper;

        public static SecurityKeyProfile SystemKeyVault { get; set; }
        public RASecurityKeyEncryptor()
            : this(AveCertificateHelper.GetCertificate(CommonRoleConfiguration.CertificateThumbprint))
        { }

        public RASecurityKeyEncryptor(X509Certificate2 certificate2)
        {
            this.rsaHelper = new AveRsaHelper(certificate2);
        }

        private SecurityKeyProfile keyVault { get; set; }
        public RASecurityKeyEncryptor(SecurityKeyProfile keyVault)
            : this(AveCertificateHelper.GetCertificate(CommonRoleConfiguration.CertificateThumbprint))
        {
            this.keyVault = keyVault;
        }

        public String Encrypt(String plainKey)
        {
            return this.rsaHelper.Encrypt(plainKey);
        }

        public String Encrypt(Byte[] keys)
        {
            var plainKey = Convert.ToBase64String(keys);
            return Encrypt(plainKey);
        }

        public String Decrypt(String cipherKey)
        {
            return this.rsaHelper.Decrypt(cipherKey);
        }

        public Byte[] DecryptToBytes(String cipherKey)
        {
            return Convert.FromBase64String(this.Decrypt(cipherKey));
        }

        public string EncryptWithKeyVault(string plainString)
        {
            if (this.keyVault == null || string.IsNullOrEmpty(plainString))
            {
                return plainString;
            }
            AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5.0)));
            return retryPolicy.ExecuteAction<string>(() =>
            {
                var keyClient = new KeyVaultClient((authority, resource, scope) =>
                {
                    return GetAccessTokenByCredential(authority, resource, this.keyVault.ClientId, this.keyVault.ClientSecret);
                });
                var encryBytes = keyClient.EncryptAsync(this.keyVault.KeyIdentity, JsonWebKeyEncryptionAlgorithm.RSAOAEP, Convert.FromBase64String(EncodeBase64(plainString))).GetAwaiter().GetResult().Result;
                return Convert.ToBase64String(encryBytes);
            });
        }
        public string DecryptWithKeyVault(string encryptedString)
        {
            if (this.keyVault == null || string.IsNullOrEmpty(encryptedString))
            {
                return encryptedString;
            }
            AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5.0)));
            return retryPolicy.ExecuteAction<string>(() =>
            {
                try
                {
                    var keyClient = new KeyVaultClient((authority, resource, scope) =>
                    {
                        return GetAccessTokenByCredential(authority, resource, this.keyVault.ClientId, this.keyVault.ClientSecret);
                    });
                    var encryBytes = keyClient.DecryptAsync(this.keyVault.KeyIdentity, JsonWebKeyEncryptionAlgorithm.RSAOAEP, Convert.FromBase64String(encryptedString)).GetAwaiter().GetResult().Result;
                    return DecodeBase64(Convert.ToBase64String(encryBytes));
                }
                catch (Exception ex)
                {
                    logger.Info("Decrypt string with key vault failed, client id: {0}, encrypted string: {1}, exception: {2}.", this.keyVault.ClientId, encryptedString, ex.ToString());
                    throw ex;
                }
            });
        }

        public string EncryptWithKeyVault2(SecurityKeyProfile profile, string plainString)
        {
            if (profile == null || string.IsNullOrEmpty(plainString))
            {
                return plainString;
            }
            var keyClient = new KeyVaultClient((authority, resource, scope) =>
            {
                return GetAccessTokenByCredential(authority, resource, profile.ClientId, profile.ClientSecret);
            });
            var encryBytes = keyClient.EncryptAsync(profile.KeyIdentity, JsonWebKeyEncryptionAlgorithm.RSAOAEP, Convert.FromBase64String(EncodeBase64(plainString))).GetAwaiter().GetResult().Result;
            return Convert.ToBase64String(encryBytes);
        }
        public string DecryptWithKeyVault2(SecurityKeyProfile profile, string encryptedString)
        {
            if (profile == null || string.IsNullOrEmpty(encryptedString))
            {
                return encryptedString;
            }
            var keyClient = new KeyVaultClient((authority, resource, scope) =>
            {
                return GetAccessTokenByCredential(authority, resource, profile.ClientId, profile.ClientSecret);
            });
            var encryBytes = keyClient.DecryptAsync(profile.KeyIdentity, JsonWebKeyEncryptionAlgorithm.RSAOAEP, Convert.FromBase64String(encryptedString)).GetAwaiter().GetResult().Result;
            return DecodeBase64(Convert.ToBase64String(encryBytes));
        }
        public string EncryptWithCert(string plainString)
        {
            if (this.keyVault == null || string.IsNullOrEmpty(plainString))
            {
                return plainString;
            }
            AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5.0)));
            return retryPolicy.ExecuteAction<string>(() =>
            {
                var keyClient = new KeyVaultClient((authority, resource, scope) =>
                {
                    return GetTokenFromCert(authority, resource, this.keyVault.ClientId);
                });
                var encryBytes = keyClient.EncryptAsync(this.keyVault.KeyIdentity, JsonWebKeyEncryptionAlgorithm.RSAOAEP, Convert.FromBase64String(EncodeBase64(plainString))).GetAwaiter().GetResult().Result;
                return Convert.ToBase64String(encryBytes);
            });
        }
        public string DecryptWithCert(string encryptedString)
        {
            if (this.keyVault == null || string.IsNullOrEmpty(encryptedString))
            {
                return encryptedString;
            }
            AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5.0)));
            return retryPolicy.ExecuteAction<string>(() =>
            {
                try
                {
                    var keyClient = new KeyVaultClient((authority, resource, scope) =>
                    {
                        return GetTokenFromCert(authority, resource, this.keyVault.ClientId);
                    });
                    var encryBytes = keyClient.DecryptAsync(this.keyVault.KeyIdentity, JsonWebKeyEncryptionAlgorithm.RSAOAEP, Convert.FromBase64String(encryptedString)).GetAwaiter().GetResult().Result;
                    return DecodeBase64(Convert.ToBase64String(encryBytes));
                }
                catch (Exception ex)
                {
                    logger.Info("Decrypt string with cert failed, client id: {0}, encrypted string: {1}, exception: {2}.", this.keyVault.ClientId, encryptedString, ex.ToString());
                    throw ex;
                }
            });
        }

        public string SetSecret(string vault, string secretName, string value)
        {
            if (this.keyVault == null || string.IsNullOrEmpty(value))
            {
                return value;
            }
            AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5.0)));
            return retryPolicy.ExecuteAction<string>(() =>
            {
                try
                {
                    var keyClient = new KeyVaultClient((authority, resource, scope) =>
                    {
                        return GetTokenFromCert(authority, resource, this.keyVault.ClientId);
                    });
                    var secretIdentifier = keyClient.SetSecretAsync(vault, secretName, value).GetAwaiter().GetResult().SecretIdentifier;
                    return secretIdentifier.Vault;
                }
                catch (Exception ex)
                {
                    logger.Info("Set secret failed, client id: {0}, vault: {1}, secret name: {2}, value: {3}, exception: {4}.", this.keyVault.ClientId, vault, secretName, value, ex.ToString());
                    throw ex;
                }
            });
        }
        public string GetSecret(string secretIdentifierVault)
        {
            if (this.keyVault == null || string.IsNullOrEmpty(secretIdentifierVault))
            {
                return string.Empty;
            }
            AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5.0)));
            return retryPolicy.ExecuteAction<string>(() =>
            {
                try
                {
                    var keyClient = new KeyVaultClient((authority, resource, scope) =>
                    {
                        return GetTokenFromCert(authority, resource, this.keyVault.ClientId);
                    });
                    var value = keyClient.GetSecretAsync(secretIdentifierVault).GetAwaiter().GetResult().Value;
                    return value;
                }
                catch (Exception ex)
                {
                    logger.Info("Get secret by identifier failed, client id: {0}, secretIdentifierVault: {1}, exception: {2}.", this.keyVault.ClientId, secretIdentifierVault, ex.ToString());
                    return string.Empty;
                }
            });
        }

        public string GetSecret(string vault, string secretName)
        {
            if (this.keyVault == null || string.IsNullOrEmpty(secretName))
            {
                return string.Empty;
            }
            AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5.0)));
            return retryPolicy.ExecuteAction<string>(() =>
            {
                try
                {
                    var keyClient = new KeyVaultClient((authority, resource, scope) =>
                    {
                        return GetTokenFromCert(authority, resource, this.keyVault.ClientId);
                    });
                    var value = keyClient.GetSecretAsync(vault, secretName).GetAwaiter().GetResult().Value;
                    return value;
                }
                catch (Exception ex)
                {
                    logger.Info("Get secret by name failed, client id: {0}, vault: {1}, secret name: {2}, exception: {3}.", this.keyVault.ClientId, vault, secretName, ex.ToString());
                    throw ex;
                }
            });
        }

        public async System.Threading.Tasks.Task<string> GetTokenFromCert(string authority, string resource, string clientId)
        {
            return GetAppAccessTokenFromCert(authority, resource, clientId);
        }

        private async Task<string> GetAccessTokenByCredential(string authority, string resource, string clientId, string clientSecret)
        {
            logger.Info("Get access token by credential for client: {0}.", clientId);
            var adCredential = new ClientCredential(clientId, clientSecret);
            var authenticationContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = authenticationContext.AcquireToken(resource, adCredential);
            logger.Info("End get access token by credential for client: {0}.", clientId);
            return result.AccessToken;
        }

        private string SecurityOnCache(SecurityKeyProfile profile, Func<KeyVaultClient, string> encryptOrDecrypt)
        {
            try
            {
                return encryptOrDecrypt(SecurityClientCache.GetSecurityClient(profile));
            }
            catch (Exception ex)
            {
                logger.Error("Security key vault error: {0}.", ex.ToString());
                return encryptOrDecrypt(SecurityClientCache.GetSecurityClient(profile, true));
            }
        }

        private string EncodeBase64(string source)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            var encode = source;
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = source;
            }
            return encode;
        }
        private string DecodeBase64(string result)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }

        public static bool CheckKeyVault(SecurityKeyProfile profile, bool isApplied)
        {
            if (profile.IsDefault)
            {
                return true;
            }
            var keyVaultEncryptor = new RASecurityKeyEncryptor(profile);
            try
            {
                if (isApplied)
                {
                    keyVaultEncryptor.DecryptWithKeyVault(Convert.ToBase64String(Encoding.UTF8.GetBytes("TestDecryptedValue")));
                }
                else
                {
                    keyVaultEncryptor.EncryptWithKeyVault("TestEncryptedValue");
                    keyVaultEncryptor.DecryptWithKeyVault(Convert.ToBase64String(Encoding.UTF8.GetBytes("TestDecryptedValue")));
                }
            }
            catch (KeyVaultClientException ex)
            {
                logger.Error("Check key vault client failed, client id: {0}, key identiy: {1}, error: {2}.", profile.ClientId, profile.KeyIdentity, ex.ToString());
                if (ex.Message.Contains("Operation encrypt is not permitted on this key")
                    || ex.Message.Contains("Operation decrypt is not permitted on this key")
                    || ex.Message.Contains("not permitted"))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Check key vault failed, client id: {0}, key identiy: {1}, error: {2}.", profile.ClientId, profile.KeyIdentity, ex.ToString());
                return false;
            }
            return true;
        }

        private static string GetAppAccessTokenFromCert(string authority, string resource, string clientId)
        {
            AuthenticationContext authenticationContext = new AuthenticationContext(authority, false);
            var certPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "t-sp2.pfx");
            if (!System.IO.File.Exists(certPath))
            {
                certPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin\\", "t-sp2.pfx");
            }
            var certfile = System.IO.File.OpenRead(certPath);
            var certificateBytes = new byte[certfile.Length];
            certfile.Read(certificateBytes, 0, (int)certfile.Length);
            string secret = System.Text.Encoding.Default.GetString(Convert.FromBase64String("QW9sMzQlXg=="));
            var cert = new X509Certificate2(
                certificateBytes,
                secret,
                X509KeyStorageFlags.Exportable |
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet);
            ClientAssertionCertificate cac = new ClientAssertionCertificate(clientId, cert);
            var authenticationResult = authenticationContext.AcquireToken(resource, cac);
            return authenticationResult.AccessToken;
        }
    }
    public class SecurityKeyProfile
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string KeyIdentity { get; set; }

        public bool IsDefault { get; set; }
    }
    public class SecurityClientCache
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SecurityClientCache));
        private static object obj = new object();
        private static Dictionary<string, ClientTokenCache> ClientTokens = new Dictionary<string, ClientTokenCache>();
        internal const int Cache_Interval_Minutes = 30;
        static SecurityClientCache()
        {
            ClientTokens = new Dictionary<string, ClientTokenCache>();
        }

        internal static KeyVaultClient GetSecurityClient(SecurityKeyProfile profile, bool refresh = false)
        {
            var key = profile.ClientId + profile.ClientSecret;
            if (!ClientTokens.ContainsKey(key) || refresh)
            {
                logger.Info("Start lock for key {0}", key);
                lock (obj)
                {
                    logger.Info("lock begin.");
                    if (refresh)
                    {
                        logger.Info("Refresh key {0}.", key);
                        ClientTokens.Remove(key);
                    }
                    if (!ClientTokens.ContainsKey(key))
                    {
                        logger.Info("Add client cache for key {0}.", key);
                        ClientTokens.Add(key, new ClientTokenCache(profile));
                    }
                    logger.Info("lock end.");
                }
                logger.Info("Finish lock for key {0}.", key);
            }
            return ClientTokens[key].GetClient();
        }
    }
    public class ClientTokenCache
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ClientTokenCache));
        private string accessToken;
        private long lastRefreshTime;
        private string clientId;
        private string clientSecret;
        public ClientTokenCache(SecurityKeyProfile profile)
        {
            logger.Info("Token cache for client id: {0}.", profile.ClientId);
            this.clientId = profile.ClientId;
            this.clientSecret = profile.ClientSecret;
            this.lastRefreshTime = default(long);
        }

        public KeyVaultClient GetClient()
        {
            return new KeyVaultClient((authority, resource, scope) =>
            {
                return GetAccessToken(authority, resource);
            });
        }

        private async Task<string> GetAccessToken(string authority, string resource)
        {
            if (lastRefreshTime == default(long)
                   || (DateTime.UtcNow - new DateTime(lastRefreshTime, DateTimeKind.Utc)).TotalMinutes > SecurityClientCache.Cache_Interval_Minutes
            || string.IsNullOrEmpty(this.accessToken))
            {
                var key = this.clientId + this.clientSecret;
                logger.Info("Refresh access token for {0}.", key);
                this.accessToken = GetAccessToken(authority, resource, this.clientId, this.clientSecret);
                logger.Info("End refresh access token for {0}.", key);
                this.lastRefreshTime = DateTime.UtcNow.Ticks;
            }
            return this.accessToken;
        }

        private static string GetAccessToken(string authority, string resource, string clientId, string clientSecret)
        {
            var adCredential = new ClientCredential(clientId, clientSecret);
            var authenticationContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
            return authenticationContext.AcquireTokenAsync(resource, adCredential).GetAwaiter().GetResult().AccessToken;
        }

        
    }
}
