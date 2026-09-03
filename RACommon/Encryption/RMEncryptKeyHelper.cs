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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Util.Security;

namespace AvePoint.RA.Common.Encryption
{
    public static class RMEncryptKeyHelper
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly string db_key_tenantPreNonce = $"{KeyNameCollection.AES_Encrypt}{RMNameValueDto.Seprator}{RMNameValueType.AesEncryptTenantPreNonce}";
        public static readonly string db_key_defaultEncryptKey = $"{KeyNameCollection.AES_Encrypt}{RMNameValueDto.Seprator}{RMNameValueType.AesEncryptDefaultKey}";
        
        /// <summary>
        /// 生成PreNonce(32字节随机数)的Base64字符串, 每个tenant对应一个PreNonce, PreNonce是用来去Aos获取MasterKey传递的参数
        /// </summary>
        /// <returns></returns>
        public static string GenTenantPreNonce()
        {
            return GetRandomKey();
        }

        /// <summary>
        /// 获取Prodcut Master Key
        /// </summary>
        /// <param name="tenantPreNonceBase64Str">每个tenant对应一个PreNonce</param>
        /// <returns></returns>
        public static byte[] GetTenantProductMasterKey(string tenantPreNonceBase64Str)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            try
            {
                ArgumentCheck.NotNull(tenantPreNonceBase64Str, nameof(tenantPreNonceBase64Str));
                return RMAosApiClient.GetProductMasterKey(tenantId, Convert.FromBase64String(tenantPreNonceBase64Str));
            }
            catch (Exception e)
            {
                logger.Error($"An error while get master key, tenantid:{tenantId}, {e}");
                return null;
            }
        }

        /// <summary>
        /// 生成随机秘钥，并用Product Master key加密此秘钥, 此秘钥在加密解密数据时使用 
        /// </summary>
        /// <param name="tenantPreNonceBase64Str"></param>
        /// <returns></returns>
        public static string GenEncryptKey(string tenantPreNonceBase64Str)
        {
            try
            {
                var key = GetRandomKey();
                var masterKey = GetTenantProductMasterKey(tenantPreNonceBase64Str);
                return new RMAesGcmEncryptor(masterKey).Encrypt(key);
            }
            catch (Exception e)
            {
                logger.Error($"An error while GenEncryptKey, {e}");
                throw;
            }
        }

        /// <summary>
        /// 获取随机生成的秘钥
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns>返回base64字符串</returns>
        private static string GetRandomKey(int keySize = 32)
        {
            try
            {
                return Convert.ToBase64String(KeyGenerator.Get(keySize));
            }
            catch (Exception e)
            {
                logger.Error($"An error while GetRandomKey, {e}");
                throw;
            }
        }
    }
}
