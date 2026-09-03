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
using AvePoint.Hybrid.Utility;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AvePoint.Hybrid.AgentService.Utils
{
    public static class RsaEncryptedPayloadHelper
    {
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                throw new ArgumentException("Encrypted text cannot be null or whitespace.", nameof(encryptedText));
            }

            var certificate = CommonConfiguration.getAppCert();
            if (certificate == null)
            {
                throw new InvalidOperationException("The application certificate is not initialized.");
            }

            using (var rsa = certificate.GetRSAPrivateKey())
            {
                if (rsa == null)
                {
                    throw new InvalidOperationException("The certificate does not contain an RSA private key.");
                }

                var cipherData = Convert.FromBase64String(encryptedText);
                var plainData = rsa.Decrypt(cipherData, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(plainData);
            }
        }
    }
}
