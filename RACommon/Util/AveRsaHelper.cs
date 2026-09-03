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
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class AveRsaHelper
    {

        readonly X509Certificate2 certificate2;
        public AveRsaHelper(X509Certificate2 certificate2)
        {
            this.certificate2 = certificate2;
        }

        /// <summary>
        /// Using the certificate public key to encrypt a plain text from string
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public String Encrypt(String plainText)
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            var encryptedData = Encrypt(data);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Using the certificate public key to encrypt a plain text from byte
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] plainData)
        {
            var rsa = this.certificate2.GetRSAPublicKey();
            var encryptedData = rsa.Encrypt(plainData, RSAEncryptionPadding.OaepSHA1);
            return encryptedData;
        }

        /// <summary>
        /// Decrypt the cipher text using the certificate private key
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <returns></returns>
        public String Decrypt(String encryptedText)
        {
            var cipherData = Convert.FromBase64String(encryptedText);
            var data = Decrypt(cipherData);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Decrypt the cipher text using the certificate private key
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] encryptedData)
        {
            var rsa = this.certificate2.GetRSAPrivateKey();
            var data = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA1);
            return data;
        }

        /// <summary>
        /// Sign data using the certificate
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        //public String SignData(String plainText)
        //{
        //    //AveSha1 sha1 = new AveSha1();
        //    //result = sha1.ComputeHash(data);
        //    //RSAEncryption RSA = new RSAEncryption(cer);
        //    //return RSA.VerifyHash(result, CryptoConfig.MapNameToOID("SHA1"), SignData);

        //    var data = Encoding.UTF8.GetBytes(plainText);
        //    var sha1 = new GCommon.Utility.Cryptography.Hash.AveSha1();
        //    var hashbytes = sha1.ComputeHash(data);
        //    var signatrueFormatter = new RSAPKCS1SignatureFormatter(this.certificate2.PrivateKey);
        //    signatrueFormatter.SetHashAlgorithm("SHA1");
        //    var signature = signatrueFormatter.CreateSignature(hashbytes);
        //    return Convert.ToBase64String(signature);
        //}

        /// <summary>
        /// verify the data using the signature
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public Boolean VerifyData(String plainText, String signature)
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            var signatureData = Convert.FromBase64String(signature);
            var rsa = (RSACryptoServiceProvider)this.certificate2.PublicKey.Key;
            return rsa.VerifyData(data, "SHA1", signatureData);
        }

    }
}
