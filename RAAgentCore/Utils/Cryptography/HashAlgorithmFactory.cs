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


using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using AvePoint.Hybrid.Utility.Cryptography.Hash;

namespace AvePoint.Hybrid.Utility.Cryptography
{
    public static class HashAlgorithmFactory
    {
        public static IHashAlgorithm CreateHashAlgorithm(HashAlgorithm type, SecureString key)
        {
            return CreateHashAlgorithm(type, CryptoUtil.ConvertSecureStringToBytes(key));
            
        }

        public static IHashAlgorithm CreateHashAlgorithm(HashAlgorithm type, byte[] key)
        {
            CryptographyManagement.CheckAccess();

            switch (type)
            {
                case HashAlgorithm.SHA1: return new AveSha1();
                case HashAlgorithm.HMACSHA1: return new HmacSha1(key);
                case HashAlgorithm.MD5:
                    if (CryptographyManagement.CryptoMode == CryptoMode.FIPS)
                    {
                        return new AveMD5Provider();
                    }
                    return new AveMd5();
                case HashAlgorithm.HMASHA256:
                    if (CryptographyManagement.CryptoMode == CryptoMode.FIPS)
                    {
                        return new AveHMACSHA256Provider(key);
                    }
                    return new HmacSHA256(key);
                case HashAlgorithm.SHA256: return new AveSha256();
                case HashAlgorithm.SHA384: return new AveSha384();
                case HashAlgorithm.SHA512: return new AveSha512();
            }
            return null;

        }


        public static IHashAlgorithm CreateHashAlgorithm(HashAlgorithm type)
        {

            switch (type)
            {
                case HashAlgorithm.SHA1: return new AveSha1();
                case HashAlgorithm.HMACSHA1: return new HmacSha1(Encoding.UTF8.GetBytes("AvePoint Test Key"));
                case HashAlgorithm.MD5: 
                    if (CryptographyManagement.CryptoMode == CryptoMode.FIPS)
                    {
                        return new AveMD5Provider();
                    }
                    return new AveMd5();
                case HashAlgorithm.HMASHA256:
                    if (CryptographyManagement.CryptoMode == CryptoMode.FIPS)
                    {
                        return new AveHMACSHA256Provider(Encoding.UTF8.GetBytes("AvePoint Test Key"));
                    }
                    return new HmacSHA256(Encoding.UTF8.GetBytes("AvePoint Test Key"));
                case HashAlgorithm.SHA256: return new AveSha256();
                case HashAlgorithm.SHA384: return new AveSha384();
                case HashAlgorithm.SHA512: return new AveSha512();
            }
            return null;

        }
    }
}
