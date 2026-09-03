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

namespace AvePoint.GCommon.Utility.Cryptography
{
    public static class DataEncryptionUtil
    {
        public static DataEncryptionInfo ProcessDataEncryptionProfile(DataEncryptionProfile profile, ref string key, ref EncryptionAlgorithm alg, bool reuse)
        {
            byte[] result;
            if (reuse)
            {
                result = CspCommunicationWrapper.UnWrapKey(key);

            }
            else
            {
                result = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(profile.KeyLength);

            }
            DataEncryptionInfo info = new DataEncryptionInfo();
            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
            info.EncryptionType = profile.AlgorithmType;
            info.ProfileGuid = profile.Guid;
            info.ProfileName = profile.Name;
            info.PromptMessage = profile.PromptMessage;
            info.ProtectionAlgorithmType = profile.CurrentProtectionAlgorithm.Type;
            info.ProtectionGuid = profile.CurrentProtectionAlgorithm.Guid;
            alg = (EncryptionAlgorithm)profile.AlgorithmType;
            if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.Generate)
            {
                IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, profile.CurrentProtectionAlgorithm.ProtectionKey);
                info.EncryptedKey = encryption.EncryptBinary(result);
            }
            else if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.Password)
            {
                throw new NotImplementedException();
            }
            if (!reuse)
            {
                key = CspCommunicationWrapper.WrapKeyToBase64String(result);
            }
            return info;
        }

        public static string GetDataDecryptionKey(DataEncryptionInfo info, DataEncryptionProfile profile, ref EncryptionAlgorithm alg)
        {
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
            byte[] result = encryption.DecryptBinary(info.EncryptedKey);

            if (!CryptoUtil.KeyHashVerify(result, info.Checksum))
            {
                throw new Exception("Checksum error.");

            }
            alg = (EncryptionAlgorithm)info.EncryptionType;
            return CspCommunicationWrapper.WrapKeyToBase64String(result);
        }

        private static ProtectionAlgorithm GetProtectionAlgorithm(DataEncryptionInfo info, DataEncryptionProfile profile) {
            if (profile.CurrentProtectionAlgorithm.Guid.Equals(info.ProtectionGuid) && profile.CurrentProtectionAlgorithm.Type == info.ProtectionAlgorithmType)
            {
                return profile.CurrentProtectionAlgorithm;

            }

            foreach (ProtectionAlgorithm alg in profile.ProtectionAlgorithmHistory)
            {

                if (alg.Guid.Equals(info.ProtectionGuid) && alg.Type == info.ProtectionAlgorithmType)
                {
                    return profile.CurrentProtectionAlgorithm;

                }
            }

            return null;
        }

    }
}
