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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.CommonUtil;
using Cloud.Sdk.Data.Aos.SecurityProfile;
using System;
using System.Text;

namespace AvePoint.RA.Common.Encryption
{
    public class RMDatabaseDefaultEncryptor
    {
        private readonly static IRMDatabaseEncryption defaultEncryption;


        static RMDatabaseDefaultEncryptor()
        {
            defaultEncryption = new RMDatabaseAESEncryption();
        }

        /// <summary>
        ///解包装并加密需要保存在数据库中的敏感数据
        /// </summary>
        public static string UnWrapAndEncryptToString(string base64Password)
        {
            if (!string.IsNullOrEmpty(base64Password))
            {
                return defaultEncryption.EncryptPasswordDtoToXmlString(CspCommunicationWrapper.UnWrapKey(base64Password));
            }
            return string.Empty;
        }
        /// <summary>
        ///加密需要保存在数据库中的敏感数据
        /// </summary>
        public static string EncryptToString(string data)
        {
            if (data != null)
            {
                return defaultEncryption.EncryptPasswordDtoToXmlString(CryptoUtil.ConvertStringToBytes(data));
            }
            return null;
        }

        /// <summary>
        ///解密保存在数据库中的敏感数据并包装
        /// </summary>
        public static string DecryptAndWrapToBase64(String cipherText)
        {
            if (!string.IsNullOrEmpty(cipherText))
            {
                byte[] bytes = null;
                bytes = defaultEncryption.DecryptPasswordXmlToByte(cipherText);
                return CspCommunicationWrapper.WrapKeyToBase64String(bytes);
            }
            return null;
        }
        /// <summary>
        ///解密保存在数据库中的敏感数据
        /// </summary>
        public static string DecryptToString(string cipherText)
        {
            if (!string.IsNullOrEmpty(cipherText))
            {
                
                if (cipherText.Contains("RMPasswordDto"))
                {
                    return CryptoUtil.ConvertBytesToString(defaultEncryption.DecryptPasswordXmlToByte(cipherText));
                }
                else
                {
                    return cipherText;
                }
            }
            return null;
        }
    }
}
