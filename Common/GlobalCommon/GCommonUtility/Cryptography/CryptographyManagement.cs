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
using System.Reflection;
using System.Security;
using System.Text;

namespace AvePoint.GCommon.Utility.Cryptography
{
    public class CryptographyManagement
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static CryptoMode? cryptoMode = null;
        private static readonly object cryptoModeLock = new object();
        public static CryptoMode CryptoMode
        {
            get
            {
                if (!cryptoMode.HasValue)
                {
                    lock (cryptoModeLock)
                    {
                        if (!cryptoMode.HasValue)
                        {
                            cryptoMode = FipsModeUtil.GetCryptoModeFromRegistry();
                        }
                    }
                }
                return cryptoMode.Value;
            }
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
                //Assembly assambly = Assembly.GetExecutingAssembly();
                //if (assambly.GetName().GetPublicKey() == null || assambly.GetName().GetPublicKeyToken() == null || !ArraysEqual<byte>(publicKey, assambly.GetName().GetPublicKey()))
                //{
                //    logger.Error("Power-up test failed");
                //    CryptoModuleStateMachine.Process(CryptoEvent.PowerOnSelfTestFailed);
                //    return;
                //}

                foreach (EncryptionAlgorithm type in Enum.GetValues(typeof(EncryptionAlgorithm)))
                {
                    if (type == EncryptionAlgorithm.NONE)
                    {
                        continue;
                    }
                    IEncryption encryption = EncryptionFactory.GetEncryption(type);

                    bool result = TestEncryption(encryption);

                    if (result == false)
                    {
                        logger.Error("Power-up test failed");
                        CryptoModuleStateMachine.Process(CryptoEvent.PowerOnSelfTestFailed);
                        return;
                    }
                }

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



        //private static bool TestAsymmetricEncryption(IAsymmetricEncryption alg)
        //{
        //    byte[] test = Encoding.UTF8.GetBytes("AvePoint test");
        //    byte[] result = alg.Encrypt(test, false);
        //    byte[] source = alg.Decrypt(result, false);
        //    if (!ArraysEqual<byte>(test, source))
        //    {
        //        return false;

        //    }


        //    //byte[] resultSign = alg.SignData(test, null);
        //    //if (!alg.VerifyData(test, null, resultSign))
        //    //{
        //    //    return false;
        //    //}

        //    return true;

        //}



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
