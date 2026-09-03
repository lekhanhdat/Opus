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


namespace AvePoint.Common
{
    using AvePoint.GCommon;
    #region using directives
    using System;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    #endregion

    public class RsaHelper
    {
        readonly X509Certificate2 certificate2;
        public RsaHelper(X509Certificate2 certificate2)
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
            var rsa = this.certificate2.GetRSAPrivateKey();
            var data = rsa.Decrypt(cipherData, RSAEncryptionPadding.OaepSHA1);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Decrypt the cipher text using the certificate private key
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <returns></returns>
        public byte[] Decrypt2(byte[] encryptedData)
        {
            var rsa = this.certificate2.GetRSAPrivateKey();
            var data = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA1);
            return data;
        }
        public String SingDataWithPss(string plainText)
        {
            using (RSA rsa = this.certificate2.GetRSAPrivateKey())
            {
                var messageBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] signatureBytes = rsa.SignData(messageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss); ;
                byte[] signature = rsa.SignData(messageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                return Convert.ToBase64String(signature);
            }
        }
        public Boolean VerifyDataWithPss(String plainText, String signature)
        {
            using (RSA rsa = this.certificate2.GetRSAPublicKey())
            {
                var messageBytes = Encoding.UTF8.GetBytes(plainText);
                var signatureData = Convert.FromBase64String(signature);
                bool isValid = rsa.VerifyData(messageBytes, signatureData, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                return isValid;
            }
        }

        /// <summary>
        /// Sign data using the certificate
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        //public String SignData(String plainText)
        //{
        //    var data = Encoding.UTF8.GetBytes(plainText);
        //    //Fortify scan : [RECO-20916] Privacy Violation:Heap Inspection
        //    using (var sha1 = new SHA1CryptoServiceProvider())
        //    {
        //        //var hashbytes = sha1.ComputeHash(data);
        //        var signatrueFormatter = new RSAPKCS1SignatureFormatter(this.certificate2.PrivateKey);
        //        signatrueFormatter.SetHashAlgorithm("SHA1");
        //        var signature = signatrueFormatter.CreateSignature(sha1.ComputeHash(data));
        //        return Convert.ToBase64String(signature);
        //    }
        //}

        /// <summary>
        /// verify the data using the signature
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        //public Boolean VerifyData(String plainText, String signature)
        //{
        //    var data = Encoding.UTF8.GetBytes(plainText);
        //    //var signatureData = Convert.FromBase64String(signature);
        //    //var rsa = (RSACryptoServiceProvider)this.certificate2.PublicKey.Key;
        //    //TO DO .net6 debug
        //    //Fortify scan : [RECO-20916] Privacy Violation:Heap Inspection
        //    using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider(2048))
        //    {
        //        rsaCryptoServiceProvider.FromXmlString(this.certificate2.PublicKey.Key.ToXmlString(false));

        //        return rsaCryptoServiceProvider.VerifyData(data, Convert.FromBase64String(signature), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        //    }

        //}

    }
}
