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
using AvePoint.RA.Common.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class AveSecurityKeyEncryptor
    {
        private static readonly AveRsaHelper rsaHelper = null;

        static AveSecurityKeyEncryptor()
        {
            rsaHelper = new AveRsaHelper(RMCertificateHelper.GetX509Certificate2(RMCertNames.AvePointRecords));
        }

        public static String Encrypt(String plainKey)
        {
            return rsaHelper.Encrypt(plainKey);
        }

        public static byte[] Encrypt(Byte[] keys)
        {
            return rsaHelper.Encrypt(keys);
        }

        public static string EncryptToBase64String(Byte[] key)
        {
            return Convert.ToBase64String(rsaHelper.Encrypt(key));
        }

        public static String Decrypt(String cipherKey)
        {
            return rsaHelper.Decrypt(cipherKey);
        }

        public static Byte[] DecryptFromBase64String(string cipherText)
        {
            return (rsaHelper.Decrypt2(Convert.FromBase64String(cipherText)));
        }

        public static Byte[] Decrypt(byte[] cipherKey)
        {
            return (rsaHelper.Decrypt2(cipherKey));
        }

        public static string SignData(string plainText)
        {
            return rsaHelper.SignData(plainText);
        }

        public static bool VerifyData(string plainText, string signature)
        {
            return rsaHelper.VerifyData(plainText, signature);
        }

        public static bool VerifyData(string thumbprint, string plainText, string signature)
        {
            var myRsaHelper = new AveRsaHelper(AveCertificateHelper.GetCertificate(thumbprint));
            return myRsaHelper.VerifyData(plainText, signature);
        }
    }
}
