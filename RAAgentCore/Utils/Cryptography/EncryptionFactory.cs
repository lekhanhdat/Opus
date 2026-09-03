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
using AvePoint.Hybrid.Utility.Cryptography.Encryption;

namespace AvePoint.Hybrid.Utility.Cryptography
{
    public class EncryptionFactory
    {


        private static readonly Byte[] AES_KEY = 
        {
            (Byte) 201, (Byte) 219, (Byte) 55, (Byte) 183, (Byte) 156, (Byte) 64, (Byte) 85, (Byte) 204,
            (Byte) 201, (Byte) 219, (Byte) 55, (Byte) 183, (Byte) 156, (Byte) 64, (Byte) 85, (Byte) 204
        };

        private static readonly Byte[] BLOWFISH_KEY = 
        {
            (Byte) 201, (Byte) 219, (Byte) 55, (Byte) 183, (Byte) 156, (Byte) 64, (Byte) 85, (Byte) 204,
        };

        public static IEncryption GetDefaultKeyEncryption(EncryptionAlgorithm alg)
        {
            
            if (alg == EncryptionAlgorithm.BLOWFISH_ENCRYPTION)
            {
                byte[] sha1ValueBlowfish = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(BLOWFISH_KEY);
                return GetEncryption(alg, BLOWFISH_KEY, sha1ValueBlowfish);
            }

            byte[] sha1ValueAes = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(AES_KEY);
            return GetEncryption(alg, AES_KEY, sha1ValueAes);
        }
        public static IEncryption GetEncryption(EncryptionAlgorithm alg, SecureString key, byte[] sha1Value)
        {
            //CryptoModuleStateMachine.Process
            return GetEncryption(alg, CryptoUtil.ConvertSecureStringToBytes(key), sha1Value);
        }

        public static IEncryption GetEncryption(EncryptionAlgorithm alg, string encryptedkey)
        {
            //CryptoModuleStateMachine.Process
            byte[] key = CspCommunicationWrapper.UnWrapKey(encryptedkey);
            byte[] keyHash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(key);
            return GetEncryption(alg, key, keyHash);
        }

        public static IEncryption GetEncryption(EncryptionAlgorithm alg, byte[] key, byte[] sha1Value)
        {
            IEncryption encryption = mGetEncryption(alg, key, sha1Value);
            if (CryptographyManagement.CryptoMode == CryptoMode.FIPS && encryption.FipsMode == CryptoMode.NoneFIPS)
            {
                throw new Exception(String.Format("The {0} encryption method does not match the FIPS requirement.",alg));
            }
            return encryption;
        }

        private static IEncryption mGetEncryption(EncryptionAlgorithm alg, byte[] key, byte[] sha1Value)
        {
            //CryptoModuleStateMachine.Process(CryptoEvent.EnterKeyBegin);

            CryptographyManagement.CheckAccess();

            CryptographyManagement.KeyHashVerifyCheck(key, sha1Value);

            try
            {
                switch (alg)
                {
                    case EncryptionAlgorithm.AES_ENCRYPTION:
                        return new AESEncryption(key);
                    case EncryptionAlgorithm.DES_ENCRYPTION:
                        return new DESEncryption(key);
                    case EncryptionAlgorithm.BLOWFISH_ENCRYPTION:
                        return new BlowfishEncryption(key);

                }
            }
            finally
            {
                //CryptoModuleStateMachine.Process(CryptoEvent.EnterKeySuccess);

            }

            //CryptoModuleStateMachine.Process(CryptoEvent.EnterKeyFailed);
            return null;
        }

        internal static IEncryption GetEncryption(EncryptionAlgorithm alg)
        {
            //CryptoModuleStateMachine.Process
            switch (alg)
            {
                case EncryptionAlgorithm.AES_ENCRYPTION:
                    return new AESEncryption();
                case EncryptionAlgorithm.DES_ENCRYPTION:
                    return new DESEncryption();
                case EncryptionAlgorithm.BLOWFISH_ENCRYPTION:
                    return new BlowfishEncryption();

            }
            return null;
        }

    }
}
