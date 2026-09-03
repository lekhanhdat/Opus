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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using System.Security;

namespace AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement
{
    public static class DataEncryptionUtil
    {
        public static DataEncryptionInfoWrapper CreateDataEncryptionInfoWrapper(DataEncryptionProfile profile)
        {
            byte[] result;
            result = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(profile.KeyLength / 8);
            DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
            DataEncryptionInfo info = new DataEncryptionInfo();
            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
            info.EncryptionType = profile.CurrentProtectionAlgorithm.AlgorithmType;
            info.ProfileGuid = profile.Guid;
            //info.ProfileName = profile.Name;
            //info.PromptMessage = profile.PromptMessage;
            //info.ProtectionAlgorithmType = profile.CurrentProtectionAlgorithm.Type;
            info.ProtectionGuid = profile.CurrentProtectionAlgorithm.Guid;
            info.ProfileName = profile.Name;
            if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.Generate)
            {
                //IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, profile.CurrentProtectionAlgorithm.ProtectionKey);
                //info.EncryptedDynamicKey = encryption.EncryptBinary(result);
                info.EncryptedDynamicKey = null;
                info.Checksum = null;
                Array.Copy(CspCommunicationWrapper.UnWrapKey(profile.CurrentProtectionAlgorithm.ProtectionKey), result, result.Length);
            }

            else if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.NoDynamicKey)
            {
                
                info.EncryptedDynamicKey = null;
                info.Checksum = null;
                IHashAlgorithm sha1 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]);
                Array.Copy(CspCommunicationWrapper.UnWrapKey(profile.CurrentProtectionAlgorithm.ProtectionKey), result, result.Length);
            }
            else if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.Password)
            {
                info.EncryptedDynamicKey = null;
                info.Checksum = null;
                //byte[] protectionKey = CspCommunicationWrapper.UnWrapKey(profile.CurrentProtectionAlgorithm.ProtectionKey);
                Array.Copy(CspCommunicationWrapper.UnWrapKey(profile.CurrentProtectionAlgorithm.ProtectionKey), result, result.Length);
            }
            wrapper.EncryptionInfo = info;
            wrapper.DynamicKey = CspCommunicationWrapper.WrapKeyToBase64String(result);
            return wrapper;
        }

        public static DataEncryptionInfoWrapper ResolveDataEncryptionInfo(DataEncryptionInfo info, DataEncryptionProfile profile)
        {
            DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
            if (!info.ProfileGuid.Equals(profile.Guid))
            {
                throw new Exception("Guid Not match");
            }

            ProtectionAlgorithm protectionAlg = GetProtectionAlgorithm(info, profile);
            if (protectionAlg == null)
            {
                throw new Exception("Can't find protection profile");

            }
            IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, protectionAlg.ProtectionKey);
            byte[] result = encryption.DecryptBinary(info.EncryptedDynamicKey);

            if (!CryptoUtil.KeyHashVerify(result, info.Checksum))
            {
                throw new Exception("Checksum error.");

            }
            wrapper.DynamicKey = CspCommunicationWrapper.WrapKeyToBase64String(result);
            wrapper.EncryptionInfo = info;
            return wrapper;
        }

        private static ProtectionAlgorithm GetProtectionAlgorithm(DataEncryptionInfo info, DataEncryptionProfile profile) {
            if (profile.CurrentProtectionAlgorithm.Guid.Equals(info.ProtectionGuid))
            {
                return profile.CurrentProtectionAlgorithm;

            }

            foreach (ProtectionAlgorithm alg in profile.ProtectionAlgorithmHistory)
            {

                if (alg.Guid.Equals(info.ProtectionGuid))
                {
                    return profile.CurrentProtectionAlgorithm;

                }
            }

            return null;
        }

        public static string GenerateProtectionKeyHashForCheck(byte[] key, int keyLength)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]);
            byte[] masterKey = hash.ComputeHash(key);

            for (int i = 0; i < 4; i++)
            {
                masterKey = hash.ComputeHash(masterKey);
            }

            //IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.MD5, new byte[0]);
            //byte[] data = md5.ComputeHash(masterKey);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < masterKey.Length && i < 16; i++)
            {
                sBuilder.Append(masterKey[i].ToString("x2"));
            }
            byte[] protectionKey = Encoding.UTF8.GetBytes(sBuilder.ToString());
            byte[] keyForStore = new byte[keyLength / 8];
            Array.Copy(protectionKey, 0, keyForStore, 0, keyForStore.Length);
            keyForStore = hash.ComputeHash(keyForStore);
            return Convert.ToBase64String(keyForStore);
        }
        #region For Dynamic key
        public static AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper CreateDataEncryptionInfoWrapperForDynamic(DataEncryptionProfile profile)
        {
            byte[] result;
            result = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(profile.KeyLength / 8);
            AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper wrapper = new
                AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper();
            DataEncryptionInfo info = new DataEncryptionInfo();
            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
            info.EncryptionType = profile.CurrentProtectionAlgorithm.AlgorithmType;
            info.ProfileGuid = profile.Guid;
            //info.ProfileName = profile.Name;
            //info.PromptMessage = profile.PromptMessage;
            //info.ProtectionAlgorithmType = profile.CurrentProtectionAlgorithm.Type;
            info.ProtectionGuid = profile.CurrentProtectionAlgorithm.Guid;
            info.ProfileName = profile.Name;

            IEncryption encryption = EncryptionFactory.GetEncryption((EncryptionAlgorithm)profile.CurrentProtectionAlgorithm.AlgorithmType, Convert.FromBase64String(profile.CurrentProtectionAlgorithm.ProtectionKey), HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(Convert.FromBase64String(profile.CurrentProtectionAlgorithm.ProtectionKey)));
            info.EncryptedDynamicKey = encryption.EncryptBinary(result);
            wrapper.EncryptionInfo = info;
            wrapper.DynamicKey = CspCommunicationWrapper.WrapKeyToBase64String(result);
            return wrapper;
        }
        public static AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper CreateDataEncryptionInfoWrapperForDynamic(DataEncryptionProfile profile,string encryptDynamicKey)
        {
            var enc = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION);
            byte[] result = enc.DecryptBytesWithBase64(encryptDynamicKey);
            AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper wrapper = new
                AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper();
            DataEncryptionInfo info = new DataEncryptionInfo();
            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
            info.EncryptionType = profile.CurrentProtectionAlgorithm.AlgorithmType;
            info.ProfileGuid = profile.Guid;
            info.ProtectionGuid = profile.CurrentProtectionAlgorithm.Guid;
            info.ProfileName = profile.Name;
            IEncryption encryption = EncryptionFactory.GetEncryption((EncryptionAlgorithm)profile.CurrentProtectionAlgorithm.AlgorithmType, Convert.FromBase64String(profile.CurrentProtectionAlgorithm.ProtectionKey), HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(Convert.FromBase64String(profile.CurrentProtectionAlgorithm.ProtectionKey)));
            info.EncryptedDynamicKey = encryption.EncryptBinary(result);
            wrapper.EncryptionInfo = info;
            wrapper.DynamicKey = CspCommunicationWrapper.WrapKeyToBase64String(result);
            return wrapper;
        }
        #endregion
    }
}
