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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AvePoint.Hybrid.Utility.Cryptography.AsymmetricEncryption
{
    public class RSAEncryption : IAsymmetricEncryption, IDisposable
    {

        RSACryptoServiceProvider rsa;
        public RSAEncryption()
        {
            rsa = new RSACryptoServiceProvider();

        }

        public RSAEncryption(X509Certificate2 cert)
        {

            AsymmetricAlgorithm privateKey = cert.PrivateKey;
            AsymmetricAlgorithm publicKey = cert.PublicKey.Key;

            if (privateKey != null)
            {
                rsa = (RSACryptoServiceProvider)privateKey;

            }
            else if (publicKey != null)
            {

                rsa = (RSACryptoServiceProvider)publicKey;

            }
            else
            {

                throw new Exception("Key should not be null");
            }


        }


        #region IAsymmetricEncryption Members

        public byte[] Encrypt(byte[] rgb, bool fOAEP)
        {
            return rsa.Encrypt(rgb, fOAEP);
        }

        public byte[] Decrypt(byte[] rgb, bool fOAEP)
        {
            return rsa.Decrypt(rgb, fOAEP);
        }

        public byte[] SignData(System.IO.Stream inputStream, object halg)
        {
            return rsa.SignData(inputStream, halg);
        }

        public byte[] SignData(byte[] buffer, object halg)
        {
            return rsa.SignData(buffer, halg);
        }

        public byte[] SignData(byte[] buffer, int offset, int count, object halg)
        {
            return rsa.SignData(buffer, offset, count, halg);
        }

        public byte[] SignHash(byte[] rgbHash, string str)
        {
            return rsa.SignHash(rgbHash, str);
        }

        public bool VerifyData(byte[] buffer, object halg, byte[] signature)
        {
            return rsa.VerifyData(buffer, halg, signature);
        }

        public bool VerifyHash(byte[] rgbHash, string str, byte[] rgbSignature)
        {
            return rsa.VerifyHash(rgbHash, str, rgbSignature);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            rsa.Clear();
        }

        #endregion
    }
}
