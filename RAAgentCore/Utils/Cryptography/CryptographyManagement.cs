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


using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;

namespace AvePoint.Hybrid.Utility.Cryptography
{
    public class CryptographyManagement
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static byte[] publicKey = { 0, 36, 0, 0, 4, 128, 0, 0, 148, 0, 0, 0, 6, 2, 0, 0, 0, 36, 0, 0, 82, 83, 65, 49, 0, 4, 0, 0, 1, 0, 1, 0, 173, 110, 54, 252, 3, 111, 176, 246, 85, 101, 141, 204, 149, 157, 22, 169, 18, 176, 129, 236, 156, 158, 55, 30, 69, 30, 253, 190, 193, 27, 194, 166, 167, 240, 19, 31, 8, 88, 153, 229, 126, 240, 47, 54, 159, 7, 75, 205, 187, 194, 21, 248, 82, 74, 27, 195, 37, 223, 42, 251, 93, 170, 53, 7, 34, 130, 192, 191, 70, 76, 187, 168, 241, 187, 192, 70, 41, 236, 231, 244, 126, 49, 126, 216, 83, 206, 37, 155, 42, 77, 250, 38, 93, 251, 252, 100, 241, 129, 218, 123, 68, 84, 155, 37, 192, 19, 115, 244, 74, 118, 250, 147, 155, 8, 246, 81, 220, 122, 79, 53, 154, 100, 77, 74, 209, 153, 207, 139 };

        private static CryptoMode cryptoMode = CryptoMode.NoneFIPS;
        public static CryptoMode CryptoMode
        {
            get { return cryptoMode; }
            set { cryptoMode = value; }
        }


        private CryptographyManagement()
        {


        }




        public static void CryptoInit()
        {
            lock (logger)
            {
                if (CryptoModuleStateMachine.GetState() != CryptoState.PowerOn)
                {
                    return;
                }

                CryptoModuleStateMachine.Process(CryptoEvent.InitSuccess);
                PerformSelfTests();
            }
        }

        public static void CryptoReTest()
        {
            CryptoModuleStateMachine.Process(CryptoEvent.CryptoReSelfTest);
            PerformSelfTests();
        }

        public static void PerformSelfTests()
        {
            try
            {
                Assembly assambly = Assembly.GetExecutingAssembly();
                if (assambly.GetName().GetPublicKey() == null || assambly.GetName().GetPublicKeyToken() == null || !ArraysEqual<byte>(publicKey, assambly.GetName().GetPublicKey()))
                {
                    logger.Error("Power-up test failed");
                    CryptoModuleStateMachine.Process(CryptoEvent.PowerOnSelfTestFailed);
                    return;
                }


                foreach (EncryptionAlgorithm type in Enum.GetValues(typeof(EncryptionAlgorithm)))
                {
                    if (type == EncryptionAlgorithm.NONE)
                    {
                        continue;
                    }
                    IEncryption encryption = EncryptionFactory.GetEncryption(type);
                    //encryption.Key = KeyGenerateProviderFactory.CreateProvider().GenerateKeyString(encryption.CurrentKeySize);

                    bool result = TestEncryption(encryption);

                    if (result == false)
                    {
                        logger.Error("Power-up test failed");
                        CryptoModuleStateMachine.Process(CryptoEvent.PowerOnSelfTestFailed);
                        return;
                    }
                }

                //foreach (AsymmetricEncryptionAlgorithm type in Enum.GetValues(typeof(AsymmetricEncryptionAlgorithm)))
                //{
                //    bool result = TestAsymmetricEncryption(AsymmetricEncryptionFactory.GetAsymmetricEncryption(type));
                //    if (result == false)
                //    {
                //        CryptoModuleStateMachine.Process(CryptoEvent.PowerOnSelfTestFailed);
                //        return;
                //    }
                //}

                foreach (HashAlgorithm type in Enum.GetValues(typeof(HashAlgorithm)))
                {
                    bool result = TestHash(HashAlgorithmFactory.CreateHashAlgorithm(type));
                    if (result == false)
                    {
                        logger.Error("Power-up test failed");
                        CryptoModuleStateMachine.Process(CryptoEvent.PowerOnSelfTestFailed);
                        return;
                    }
                }

            }
            catch (Exception e)
            {
                logger.Error("Power-up test failed. {0}", e.ToString());
                CryptoModuleStateMachine.Process(CryptoEvent.PowerOnSelfTestFailed);
                return;

            }
            logger.Info("Power-up test success");
            CryptoModuleStateMachine.Process(CryptoEvent.PowerOnSelfTestSuccess);

        }

        private static bool TestHash(IHashAlgorithm hash)
        {
            if (CryptoMode == CryptoMode.FIPS && hash.FipsMode == CryptoMode.NoneFIPS)
            {
                logger.Info("In FIPS environment,the hash method that does not match the FIPS requirement will be skipped in the platform self test.");
                return true;
            }
            byte[] test = hash.GetTestData();
            byte[] result = hash.ComputeHash(test);

            if (!ArraysEqual<byte>(result, hash.GetTestResult()))
            {
                return false;
            }

            return true;

        }

        private static bool TestKeyGenerate(IKeyGenerate keyGenerate)
        {

            keyGenerate.GenerateKeyBytes(64);
            return true;

        }

        private static bool TestAsymmetricEncryption(IAsymmetricEncryption alg)
        {
            byte[] test = Encoding.UTF8.GetBytes("AvePoint test");
            byte[] result = alg.Encrypt(test, false);
            byte[] source = alg.Decrypt(result, false);
            if (!ArraysEqual<byte>(test, source))
            {
                return false;

            }


            //byte[] resultSign = alg.SignData(test, null);
            //if (!alg.VerifyData(test, null, resultSign))
            //{
            //    return false;
            //}

            return true;

        }



        private static bool TestEncryption(IEncryption encryption)
        {
            if (CryptoMode == CryptoMode.FIPS && encryption.FipsMode == CryptoMode.NoneFIPS)
            {
                logger.Info("In FIPS environment,the encryption method that does not match the FIPS requirement will be skipped in the platform self test.");
                return true;
            }
            byte[] testBytes = encryption.GetTestData();
            SecureString testString = CryptoUtil.ConvertBytesToSecureString(testBytes);


            byte[] encrypted = encryption.EncryptBinary(testBytes);
            if (ArraysEqual<byte>(encrypted, testBytes))
            {
                return false;
            }

            byte[] result = encryption.DecryptBinary(encrypted);

            if (!ArraysEqual<byte>(testBytes, result))
            {

                return false;
            }

            string encryptedString = encryption.EncryptStringWithBase64(testString);

            if (encryptedString.Equals(testString))
            {
                return false;
            }
            SecureString resultString = encryption.DecryptString(encryptedString);

            if (!ArraysEqual<byte>(CryptoUtil.ConvertSecureStringToBytes(resultString), CryptoUtil.ConvertSecureStringToBytes(testString)))
            {
                return false;

            }

            return true;
        }

        public static CryptoState ShowState()
        {
            return CryptoModuleStateMachine.GetState();

        }

        public static bool CanAccess
        {
            get
            {
                CryptoInit();
                CryptoState state = CryptoModuleStateMachine.GetState();
                if (state == CryptoState.Public || state == CryptoState.CryptoOfficer || state == CryptoState.User)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public static void CheckAccess()
        {

            if (!CanAccess)
            {
                throw new Exception("Access Denied");

            }

        }


        public static void CheckFipsCompatible(ICryptography crypto)
        {

            if (CryptoMode == CryptoMode.FIPS && CryptoMode != crypto.FipsMode)
            {
                throw new Exception("FIPS Exception");

            }

        }


        public static void PerformApprovedSecurityFunction()
        {


        }

        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }


        public static EncryptionAlgorithm[] ListAllEncryptionAlgorithm(CryptoMode fipsMode)
        {
            List<EncryptionAlgorithm> result = new List<EncryptionAlgorithm>();
            foreach (EncryptionAlgorithm alg in Enum.GetValues(typeof(EncryptionAlgorithm)))
            {
                IEncryption encryption = EncryptionFactory.GetEncryption(alg);
                if (encryption.FipsMode == fipsMode)
                {
                    result.Add(alg);
                }
            }

            return result.ToArray();


        }

        public static void KeyHashVerifyCheck(byte[] key, byte[] hashValue)
        {
            if (CryptoUtil.KeyHashVerify(key, hashValue) == false)
            {
                CryptoModuleStateMachine.Process(CryptoEvent.ConditionalSelfTestFailed);
                throw new Exception("Key Entry test failed");
            }

        }

    }
}
