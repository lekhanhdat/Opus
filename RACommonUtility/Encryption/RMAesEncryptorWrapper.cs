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

namespace AvePoint.RA.RACommonUtility.Encryption
{
    public class RMAesEncryptorWrapper
    {
        private readonly IRMAesEncryptor _instance;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rmAesEncryptor">默认使用AesGcm</param>
        /// <param name="key">默认使用加密DB中数据的DB key</param>
        public RMAesEncryptorWrapper(byte[] key = null)
        {
            var encryptkey = key ?? RMAesEncryptorKeyManager.GetDefaultEncryptKey(TenantLocalValue.LogonGroupId);
            _instance = new RMAesGcmEncryptor(encryptkey);
        }

        public string Encrypt(string plainText)
        {
            return _instance.Encrypt(plainText);
        }

        public string Decrypt(string cipher)
        {
            return _instance.Decrypt(cipher);
        }

        public byte[] Encrypt(byte[] plain)
        {
            return _instance.Encrypt(plain);
        }

        public byte[] Decrypt(byte[] cipher)
        {
            return _instance.Decrypt(cipher);
        }
    }
}
