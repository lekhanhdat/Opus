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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.RA.Contract.Tenant;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Identity.Client;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.Cryptography.Encryption.KeyVault
{
    public class KeyVaultServiceProvider : IAOSEncryptionServiceProvider
    {
        public static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(3)));

        private static AveLogger logger = AveLogger.GetInstance(typeof(KeyVaultServiceProvider));
        private string profileId;
        private string clientId;
        private string clientSecret;
        private string keyIdentifier;
       // private KeyVaultClient keyClient;
        private KeyClient keyClient;
        private CryptographyClient cryptoClient;
        private SecretClient secretClient;
        public bool isSystemKeyVaultNull = false;//判断当前环境的systemsecurityprofile是否存在 为空时使用证书加解密
        private static Dictionary<string, string> fakeKeyValut = new Dictionary<string, string>();//当systemsecurityprofile不存在时 模拟keyvault缓存加解密相关信息
        private static Dictionary<string, string> cache = new Dictionary<string, string>();
        private string keyVaultUrl
        {
            get
            {
                return new Uri(keyIdentifier).GetLeftPart(UriPartial.Authority);
            }
        }


        public KeyVaultServiceProvider(AOSSecurityProfile profile)
        {
            this.profileId = profile.Id;
            if (profile.Id == Common.Portal.PortalUtil.IdSystemKeyVault &&
                string.IsNullOrEmpty(profile.ClientId) &&
                string.IsNullOrEmpty(profile.ClientSecret) &&
                string.IsNullOrEmpty(profile.KeyIdentity))// 2019 8月21日 不使用假的profile获取keyvaultclient 在后续加解密中兼容使用证书加解密
            {
                logger.Warn("Current enviorment dont have system security profile.");
                isSystemKeyVaultNull = true;
                return;
            }
            this.clientId = profile.ClientId;
            this.clientSecret = profile.ClientSecret;
            this.keyIdentifier = profile.KeyIdentity;

            var credential = new ClientSecretCredential(TenantLocalValue.LogonGroupId, clientId, this.clientSecret);
            keyClient = new KeyClient(new Uri(keyVaultUrl), credential);
            secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
            KeyVaultKey key = keyClient.GetKeyAsync(keyIdentifier).GetAwaiter().GetResult();
            cryptoClient = new CryptographyClient(key.Id, credential);
        }

        public byte[] EncryptBinary(byte[] binary)
        {
            try
            {
                if (isSystemKeyVaultNull)
                {
                    var encryptor = new SecurityKeyEncryptor(CertificateHelper.DocAveOnlineCertificate);
                    return encryptor.Encrypt(binary);
                }
                else
                {
                    return cryptoClient.EncryptAsync(Azure.Security.KeyVault.Keys.Cryptography.EncryptionAlgorithm.RsaOaep, binary).GetAwaiter().GetResult().Ciphertext; 
                }
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }

        }

        public byte[] DecryptBinary(byte[] binary)
        {
            try
            {
                var str = Convert.ToBase64String(binary);
                string value = null;
                if (cache.TryGetValue(str, out value))
                {
                    return Convert.FromBase64String(value);
                }
                else
                {

                    var result = isSystemKeyVaultNull ? new SecurityKeyEncryptor(CertificateHelper.DocAveOnlineCertificate).DecryptToBytes(binary)
                        :  cryptoClient.DecryptAsync(Azure.Security.KeyVault.Keys.Cryptography.EncryptionAlgorithm.RsaOaep, binary).GetAwaiter().GetResult().Plaintext;
                    lock (cache)
                    {
                        cache[str] = Convert.ToBase64String(result);
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }

        }

        public string EncryptStringWithBase64(string content)
        {
            try
            {
                if (isSystemKeyVaultNull)
                {
                    return new SecurityKeyEncryptor(CertificateHelper.DocAveOnlineCertificate).Encrypt(content);
                }
                else
                {
                    var result = cryptoClient.EncryptAsync(Azure.Security.KeyVault.Keys.Cryptography.EncryptionAlgorithm.RsaOaep, Convert.FromBase64String(content)).GetAwaiter().GetResult().Ciphertext;
                    var wrapContent = Convert.ToBase64String(result);
                    return wrapContent;
                }
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }

        }

        public string DecryptStringWithBase64(string wrapKey)
        {
            try
            {
                if (isSystemKeyVaultNull)
                {
                    return new SecurityKeyEncryptor(CertificateHelper.DocAveOnlineCertificate).Decrypt(wrapKey);
                }
                else
                {
                    var result = cryptoClient.DecryptAsync(Azure.Security.KeyVault.Keys.Cryptography.EncryptionAlgorithm.RsaOaep, Convert.FromBase64String(wrapKey)).GetAwaiter().GetResult().Plaintext;
                    return Convert.ToBase64String(result);
                }
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }

        }

        #region SecurityString
        public string EncryptSecureStringWithBase64(SecureString plainString)
        {
            try
            {
                var wrapContent = Convert.ToBase64String(EncryptSecureString(plainString));
                return wrapContent;
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }
            if (plainString == null || plainString.Length == 0)
            {
                return "";
            }

        }

        public SecureString DecryptSecureStringWithBase64(string encryptedString)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedString))
                {
                    SecureString sString = new SecureString();
                    sString.MakeReadOnly();
                    return sString;
                }

                byte[] base64Decrypted = Convert.FromBase64String(encryptedString);
                var secureString = DecryptSecureString(base64Decrypted);
                AvePoint.GCommon.Utility.Cryptography.CryptoUtil.ZeroBytes(base64Decrypted);
                return secureString;
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }

        }


        public byte[] EncryptSecureString(SecureString plainString)
        {
            try
            {
                if (plainString == null || plainString.Length == 0)
                {
                    return new byte[0];
                }

                byte[] buf = CryptoUtil.ConvertSecureStringToBytes(plainString);
                byte[] encryptBinary = EncryptBinary(buf);
                AvePoint.GCommon.Utility.Cryptography.CryptoUtil.ZeroBytes(buf);
                return encryptBinary;
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }
        }

        public SecureString DecryptSecureString(byte[] encryptedByte)
        {
            try
            {
                SecureString sString = new SecureString();

                if (encryptedByte == null || encryptedByte.Length <= 0)
                {
                    sString.MakeReadOnly();
                    return sString;
                }

                byte[] decrytedBytes = DecryptBinary(encryptedByte);
                char[] decrytedChars = Encoding.UTF8.GetChars(decrytedBytes);
                AvePoint.GCommon.Utility.Cryptography.CryptoUtil.ZeroBytes(decrytedBytes);
                foreach (char decrytedChar in decrytedChars)
                {
                    sString.AppendChar(decrytedChar);
                }

                sString.MakeReadOnly();
                return sString;
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }

        }

        public string GetProfileId()
        {
            return profileId;
        }
        #endregion

        #region Secret
        /// <summary>
        /// 保存key到keyvault中
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SetSecret(string secretName, string value)
        {
            try
            {
                if (isSystemKeyVaultNull)
                {
                    var lowerSecretName = secretName.ToLower();
                    if (fakeKeyValut.ContainsKey(lowerSecretName))
                    {
                        fakeKeyValut[lowerSecretName] = value;
                    }
                    else
                    {
                        fakeKeyValut.Add(lowerSecretName, value);
                    }
                    return string.Empty;
                }
                else
                {
                    var secret = secretClient.SetSecretAsync(secretName, value).GetAwaiter().GetResult().Value.Value;
                    return secret;
                }
            }
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }
        }

        /// <summary>
        /// 获取保存在keyvault 中的key
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public string GetSecret(string secretName)
        {
            try
            {
                return RetryPolicy.ExecuteAction<string>(() =>
                 {
                     if (isSystemKeyVaultNull)
                     {
                         var lowerSecretName = secretName.ToLower();
                         if (fakeKeyValut.ContainsKey(lowerSecretName))
                         {
                             return fakeKeyValut[lowerSecretName];
                         }
                         else
                         {
                             logger.Error("Cant get secret from fake key vault secretname {0}",secretName);
                             return string.Empty;
                         }
                     }
                     else
                     {
                         var value = secretClient.GetSecretAsync(secretName).GetAwaiter().GetResult().Value.Value;
                         return value;
                     }
                 });

            }
            //catch (KeyVaultClientException e)
            //{
            //    if (e.Status == System.Net.HttpStatusCode.NotFound)
            //    {
            //        return null;
            //    }
            //    HandleException(e);
            //    throw e;
            //}
            catch (Exception e)
            {
                HandleException(e);
                throw e;
            }
        }

        #endregion

        private void HandleException(Exception e)
        {
            logger.Error("Fail to opearte encryption or decryption by key vault.Please check if the key vault still exists and the clientId has granted enough permission on the key");
            logger.Error(e.Message, e);
        }

    }
}
