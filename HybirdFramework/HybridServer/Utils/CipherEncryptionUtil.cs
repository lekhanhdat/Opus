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
using AvePoint.RA.Contract.Configurations;
using Cloud.Sdk.Cipher;
using Cloud.Sdk.Data.Cipher;
using Cloud.Sdk.Data.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using Util;
using Util.Security;

namespace HybridServer.Utils
{
    public class CipherEncryptionUtil
    {
        // donot add RALogger. HybridServer or other projects may be not use log4net.
        //private static readonly IRALogger logger = RALogger.GetInstance(typeof(CipherEncryptionUtil));
        private static readonly ICloudSdkCipherClientFactory cipherClientFactory;
        private static readonly LocalCache cache = new LocalCache(TimeSpan.FromDays(30));

        static CipherEncryptionUtil()
        {
            var services = new ServiceCollection();
            services
                .AddCloudSdk(CallerType.CloudRecords, null)
                .AddCloudSdkCipherApi();
            var serviceProvider = services.BuildServiceProvider();
            cipherClientFactory = serviceProvider.GetService<ICloudSdkCipherClientFactory>();
        }

        private static byte[] CipherKey
        {
            get
            {
                return cache.Get<Byte[]>(
                    "cipherKey",
                    GetCipherKey,
                    TimeSpan.FromDays(30));
            }
        }

        private static byte[] GetCipherKey()
        {
            try
            {
                var cipherServerUrl = ConfigurationSetting.GetValue(RMEnvSettingKey.CIPHER_SERVICE_URL.ToString());     //Infra Cipher Api Address
                var cipherKey = ConfigurationSetting.GetValue(RMEnvSettingKey.INFRA_CIPHER_KEY.ToString());     //Vault为各产品配置的加密key，各产品唯一

                var client = cipherClientFactory.CreateCipherApiClient(cipherServerUrl);
                var decryptedResult = client.CipherService.Decrypt(cipherKey).GetAwaiter().GetResult();      //获取解密后的Infra Cipher Key
                if (decryptedResult.Status == DecryptStatus.Failed)
                {
                    throw new Exception("Decrypt infra cipher key failed. Error message: " + decryptedResult.ErrorMsg);
                }
                return decryptedResult.CipherKey;
            }
            catch (Exception e)
            {
                throw new Exception("CipherEncryptionUtil GetCipherKey . Error message: " + e);
            }
        }

        public static string CipherDecrypt(string plainText)
        {
            try
            {
                var secret = plainText.Trim('*');   //需要解密的配置项 (Vault中会对加密配置项前后加上**符号，所以需要trim去除)
                var aes = new AesGcm(CipherKey);   //Util.Security -> AesGcm
                var decryptedSecret = aes.Decrypt(secret);  //解密secret
                return decryptedSecret;
            }
            catch (Exception e)
            {
                throw new Exception("CipherEncryptionUtil CipherDecrypt . Error message: " + e);
            }

        }
    }
}
