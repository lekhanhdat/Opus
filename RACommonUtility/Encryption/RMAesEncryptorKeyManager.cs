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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Encryption
{
    public static class RMAesEncryptorKeyManager
    {
        private static readonly ConcurrentDictionary<string, CacheEncryptorKey> _productMasterkeyCache = new();
        private static readonly ConcurrentDictionary<string, CacheEncryptorKey> _defaultEncryptKeyCache = new();
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static IRMEncryptKeyValueDao RMEncryptKeyValueDao => PlatformWindsorManager.GetService<IRMEncryptKeyValueDao>();

        private record CacheEncryptorKey(byte[] Key, DateTime Expired);

        public static byte[] GetDefaultEncryptKey(string tenantId)
        {
            var hasCache = _defaultEncryptKeyCache.TryGetValue(tenantId, out CacheEncryptorKey keyCache);
            var isCacheExpired = hasCache && keyCache.Expired < DateTime.UtcNow;
            if(isCacheExpired)
            {
                logger.Warn($"Default EncryptKey cache expired, tenantId: {tenantId}");
            }

            if (!hasCache || isCacheExpired)
            {
                keyCache = new CacheEncryptorKey(GetEncryptDataDBKey(tenantId), DateTime.UtcNow.AddDays(1));
                logger.Info($"Get Default EncryptKey for tenantId: {tenantId}");
                if (!_defaultEncryptKeyCache.TryAdd(tenantId, keyCache))
                {
                    logger.Warn($"failed to add default encrypt key to cache, tenantId: {tenantId}");
                }
            }
            return keyCache.Key;
        }

        private static byte[] GetProductMasterKey(string tenantId)
        {
            var hasCache = _productMasterkeyCache.TryGetValue(tenantId, out CacheEncryptorKey keyCache);
            var isCacheExpired = hasCache && keyCache.Expired < DateTime.UtcNow;
            if (isCacheExpired)
            {
                logger.Warn($"Product MasterKey cache expired, tenantId: {tenantId}");
            }

            if (!hasCache || isCacheExpired)
            {
                var key = RMAosApiClient.GetProductMasterKey(tenantId, GetPreNonce());
                keyCache = new CacheEncryptorKey(key, DateTime.UtcNow.AddDays(1));
                if (!_productMasterkeyCache.TryAdd(tenantId, keyCache))
                {
                    logger.Warn($"failed to add master key to cache, tenantId: {tenantId}");
                }
            }
            return keyCache.Key;
        }

        /// <summary>
        /// 从数据库中读取每个Tenan对应的PreNonce, 用于从Aos获取Master key
        /// </summary>
        /// <returns></returns>
        private static byte[] GetPreNonce()
        {
            try
            {
                var dbValue = RMEncryptKeyValueDao.GetValue(RMEncryptKeyHelper.db_key_tenantPreNonce);
                return Convert.FromBase64String(dbValue);
            }
            catch (Exception e)
            {
                logger.Error($"An error while get pre nonce, {e}");
                throw;
            }
        }

        /// <summary>
        /// 从数据库中读取用于加密DB中数据的DB key，因为它是通过MasterKey加密后存储的，所以取出后需要用MasterKey解密
        /// </summary>
        /// <returns></returns>
        private static byte[] GetEncryptDataDBKey(string tenantId)
        {
            try
            {
                var dbValue = RMEncryptKeyValueDao.GetValue(RMEncryptKeyHelper.db_key_defaultEncryptKey);
                var masterKey = GetProductMasterKey(tenantId);
                var encryptDbKeyStr = new RMAesGcmEncryptor(masterKey).Decrypt(dbValue);
                return Convert.FromBase64String(encryptDbKeyStr);
            }
            catch (Exception e)
            {
                logger.Error($"An error while get encrypt data db key, {e}");
                throw;
            }
        }
    }
}
