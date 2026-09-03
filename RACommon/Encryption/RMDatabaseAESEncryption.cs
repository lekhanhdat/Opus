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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Contract.Configurations;
using System;
using System.Linq;
using System.Security;
using System.Text;

namespace AvePoint.RA.Common.Encryption
{
    public class RMDatabaseAESEncryption : IRMDatabaseEncryption
    {
        private readonly  string dbDefaultEncryptKey;      
        private uint keyCrc;
        
        public RMDatabaseAESEncryption()
        {
            var certificateSettingFromxml = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.DB_DEFAULT_ENCRYPTION_KEY];

            dbDefaultEncryptKey = RMGlobalConfiguration.EnvSetting.IsDevEnvironment
                ? certificateSettingFromxml
                : CipherEncryptionUtil.CipherDecrypt(certificateSettingFromxml);
           
            byte[] result = DatabaseEncryptionHelper.ComputePasspraseHash(AESEncriptionHelper.GetAESKey(dbDefaultEncryptKey));
            AveCRC32 crc = new AveCRC32();
            crc.Update(result, 0, result.Length);
            keyCrc = crc.Value;
        }

        private byte[] Decrypt(byte[] cipherTextBytes)
        {
            return AESEncriptionHelper.Decrypt(cipherTextBytes, dbDefaultEncryptKey);
        }
     
        public byte[] DecryptPasswordDto(RMPasswordDto dto)
        {
            byte[] decryptedKey = Decrypt(dto.Value);

            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
            byte[] result = hash.ComputeHash(decryptedKey);
            if (!result.SequenceEqual<byte>(dto.CheckSum))
            {
                throw new Exception("Password Broken");
            }

            return decryptedKey;
        }

        public SecureString DecryptPasswordDtoToSecureString(RMPasswordDto dto)
        {
            return CryptoUtil.ConvertBytesToSecureString(this.DecryptPasswordDto(dto));
        }

        public byte[] DecryptPasswordXmlToByte(string xml)
        {
            RMPasswordDto passwordDto = SerializerHelper.DeserializeByDataContractSerializer<RMPasswordDto>(xml);
            return this.DecryptPasswordDto(passwordDto);
        }

        public SecureString DecryptPasswordXmlToSecureString(string xml)
        {
            RMPasswordDto passwordDto = SerializerHelper.DeserializeByDataContractSerializer<RMPasswordDto>(xml);
            return this.DecryptPasswordDtoToSecureString(passwordDto);
        }

        private byte[] Encrypt(byte[] plainTextBytes)
        {
            return AESEncriptionHelper.Encrypt(plainTextBytes, dbDefaultEncryptKey);
        }

        public RMPasswordDto EncryptPasswordDto(byte[] key)
        {
            RMPasswordDto dto = new RMPasswordDto();

            byte[] encryptedKey = Encrypt(key);

            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
            byte[] result = hash.ComputeHash(key);

            dto.CheckSum = result;
            dto.Value = encryptedKey;
            dto.Version = 0;
            dto.EncryptionType = (int)EncryptionAlgorithm.AES_ENCRYPTION;
            dto.KeyCrc = keyCrc;
            return dto;
        }
        public RMPasswordDto EncryptPasswordDto(SecureString key)
        {
            return this.EncryptPasswordDto(CryptoUtil.ConvertSecureStringToBytes(key));
        }
        public string EncryptPasswordDtoToXmlString(SecureString key)
        {
            RMPasswordDto passwordDto = this.EncryptPasswordDto(key);
            return SerializerHelper.SerializeByDataContractSerializer(passwordDto);
        }
        public string EncryptPasswordDtoToXmlString(byte[] key)
        {
            RMPasswordDto passwordDto = this.EncryptPasswordDto(key);
            return SerializerHelper.SerializeByDataContractSerializer(passwordDto);
        }

    }
}
