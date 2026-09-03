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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Cryptography;

namespace AvePoint.RA.Web.Common.WIF
{
    public class AESSessionSecurityTokenHandler : SessionSecurityTokenHandler
    {
        public AESSessionSecurityTokenHandler() : base()
        {
            ConfigTransforms();
        }

        private void ConfigTransforms()
        {
            var trans = new List<CookieTransform>();
            trans.Add(new DeflateCookieTransform());
            trans.Add(new AESCookieTransform());
            base.SetTransforms(trans);
        }
    }

    /// <summary>
    /// 使用AES对称算法来加密、解密cookie
    /// </summary>
    public class AESCookieTransform : System.IdentityModel.CookieTransform
    {
        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public override byte[] Decode(byte[] encoded)
        {
            var key = GetKey();
            try
            {
                return AuthenticatedEncryption.Decrypt(AESEncriptionHelper.GetAESKey(key), encoded);
                //return AESEncriptionHelper.Decrypt(encoded, key);
            }
            catch (CryptographicException e) //compatible with old data
            {
                logger.Warn("Use compatible mode to decrypt cookie");
                //return AESEncriptionHelper.DecryptWithFIPSDisabled(encoded, key);
                return AESEncriptionHelper.Decrypt(encoded, key);
            }
        }

        public override byte[] Encode(byte[] value)
        {
            return AuthenticatedEncryption.Encrypt(AESEncriptionHelper.GetAESKey(GetKey()), value);
            //return AESEncriptionHelper.Encrypt(value, GetKey());
        }

        private string GetKey()
        {
            //var key = CommonRoleConfiguration.SessionKey;
            //ThrowUtil.ThrowIfNullOrEmpty(key, ConfigKey.SessionKey);
            //return key;

            //简单起见，目前直接拿db的connection string作为key
            return RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING];
        }
    }

}