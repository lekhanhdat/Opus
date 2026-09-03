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
using AvePoint.GCommon.Utility.Synchronization;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Win32;
using AvePoint.GCommon.Contract.Server.ControlPanel.Passphrase;
using AvePoint.GCommon.Utility.Cryptography.Encryption;
using System.Configuration;

namespace AvePoint.GCommon.Utility.Cryptography
{
    public static class DatabaseEncryptionHelper
    {
        private const string DatabaseEncryptionKeyName = "DocAveDatabaseEncryptionKey";
        private const string PassphraseName = "Passphrase";

        private static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        public const string Reg_Key = "SOFTWARE\\AvePoint\\DocAve6\\Manager";
        private const string Reg_Key_NetApp = "SOFTWARE\\Network Appliance\\SnapManager for SharePoint 7\\Manager";

        public const string Reg_key_NetAppNew_Path = "SOFTWARE\\Network Appliance\\SnapManager for SharePoint 8\\Manager";

        public const string Reg_key_IBMNew_Path = "SOFTWARE\\IBM\\SnapManager for SharePoint 8\\Manager";

        public static ReaderWriterLockSlim Locker
        {
            get { return locker; }
        }


        public static byte[] GenerateMasterKey(SecureString passphrase)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA512, new byte[0]);
            byte[] masterKey = hash.ComputeHash(CryptoUtil.ConvertSecureStringToBytes(passphrase));

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
            return Encoding.UTF8.GetBytes(sBuilder.ToString());
        }
        public static byte[] GenerateMasterKeyForSha1(SecureString passphrase)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]);
            byte[] masterKey = hash.ComputeHash(CryptoUtil.ConvertSecureStringToBytes(passphrase));

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
            return Encoding.UTF8.GetBytes(sBuilder.ToString());
        }

        public static byte[] GenerateDatabaseEncryptionKey()
        {
            return KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(32);
        }

        public static byte[] EncryptDatabaseEncryptionKeyByMasterKey(byte[] databaseEncryptionKey, byte[] masterKey, EncryptionAlgorithm alg)
        {
            IEncryption encryption = EncryptionFactory.GetEncryption(alg, masterKey, HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(masterKey));
            return encryption.EncryptBinary(databaseEncryptionKey);

        }

        private static bool ExsitLocalMachine(String subKey, String valueName)
        {

            using (var key = Registry.LocalMachine.OpenSubKey(subKey))
            {
                if (key != null)
                {
                    if (key.GetValue(valueName) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }



        public static void SaveMasterKeyToRegistry(byte[] masterKey)
        {

            string result = ProtectMasterKey(masterKey);
            string key = null;
            if (Registry.LocalMachine.OpenSubKey(Reg_Key) == null)
            {
                Registry.LocalMachine.CreateSubKey(Reg_Key);

            }
            if (ExsitLocalMachine(Reg_Key, "MasterKey"))
            {
                key = RegistryManager.ReadLocalMachine(Reg_Key, "MasterKey");
            }

            if (key != null)
            {
                RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, Reg_Key, "MasterKey_temp", key);
            }
            RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, Reg_Key, "MasterKey", result);
        }

        public static void DeleteMasterKeyInRegistry()
        {
            string key = null;
            if (ExsitLocalMachine(Reg_Key, "MasterKey"))
            {
                key = RegistryManager.ReadLocalMachine(Reg_Key, "MasterKey");
            }

            if (key != null)
            {
                RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, Reg_Key, "MasterKey_temp", key);
            }
            RegistryManager.RemoveValueFromRegKey(Reg_Key, "MasterKey");
        }

        public static void RollBackMasterKeyInRegistry()
        {
            if (!ExsitLocalMachine(Reg_Key, "MasterKey_temp"))
            {
                return;
            }
            string key = RegistryManager.ReadLocalMachine(Reg_Key, "MasterKey_temp");
            if (key != null)
            {
                RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, Reg_Key, "MasterKey", key);
                RegistryManager.RemoveValueFromRegKey(BaseKey.LocalMachine, Reg_Key, "MasterKey_temp");
            }
        }

        public static void CommitMasterKeyInRegistry()
        {
            if (!ExsitLocalMachine(Reg_Key, "MasterKey_temp"))
            {
                return;
            }

            string key = RegistryManager.ReadLocalMachine(Reg_Key, "MasterKey_temp");
            if (key != null)
            {
                RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, Reg_Key, "MasterKey_History_" + DateTime.Now.Ticks, key);
                RegistryManager.RemoveValueFromRegKey(BaseKey.LocalMachine, Reg_Key, "MasterKey_temp");
            }
        }

        public static byte[] LoadMasterKeyFromRegistry()
        {
            if (!ExsitLocalMachine(Reg_Key, "MasterKey"))
            {
                return null;
            }

            string key = RegistryManager.ReadLocalMachine(Reg_Key, "MasterKey");
            byte[] result = null;
            if (string.IsNullOrEmpty(key))
            {
                result = null;

            }
            else
            {
                result = UnProtectMasterKey(key);

            }
            return result;
        }

        private static string ProtectMasterKey(byte[] masterKey)
        {

            string reuslt = AveProtectedData.ProtectWithBase64(masterKey);
            return reuslt;
        }


        private static byte[] UnProtectMasterKey(string protectedMasterKey)
        {
            return AveProtectedData.UnProtectWithBase64(protectedMasterKey);
        }



        public static byte[] DecryptDatabaseEncryptionKeyByMasterKey(byte[] savedDatabaseEncryptionKey, byte[] masterKey, EncryptionAlgorithm alg)
        {
            IEncryption encryption = EncryptionFactory.GetEncryption(alg, masterKey, HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(masterKey));
            return encryption.DecryptBinary(savedDatabaseEncryptionKey);
        }

        public static SecureString RandomPassphrase(int len)
        {
            IKeyGenerate gen = KeyGenerateProviderFactory.CreateProvider();
            return gen.GenerateVisibleKeyString(len);

        }




        public static bool IsSystemSettingTableExist(SqlConnection conn)
        {

            SqlCommand command = new SqlCommand("if objectProperty(object_id('SystemSetting'), 'IsUserTable ')   =   1   select 1 else select 0 ", conn);
            return (bool)command.ExecuteScalar();

        }

        public static void CreateSystemTable(SqlConnection conn)
        {
            SqlCommand command = new SqlCommand("CREATE TABLE SystemSetting (" +
            "[Id] nvarchar(255)  NOT NULL," +
            "[name] nvarchar(255)  NULL," +
            "[type] int  NULL," +
            "[setting] nvarchar(max)  NULL," +
            "[binaryData] varbinary(max)  NULL);", conn);

            command.ExecuteNonQuery();

            SqlCommand commandAlter = new SqlCommand("ALTER TABLE [SystemSetting] ADD CONSTRAINT [PK_SystemSetting] PRIMARY KEY CLUSTERED ([Id] ASC);", conn);
            commandAlter.ExecuteNonQuery();

        }
        public static int SaveDatabaseEncryptionKeyToDB(SqlConnection conn, string xml)
        {
            return SaveDatabaseEncryptionInfoToDB(conn, xml, DatabaseEncryptionKeyName);
        }

        public static int SavePassphraseToDB(SqlConnection conn, string xml)
        {
            return SaveDatabaseEncryptionInfoToDB(conn, xml, PassphraseName);
        }

        private static int SaveDatabaseEncryptionInfoToDB(SqlConnection conn, string xml, string name)
        {

            using (SqlCommand command = new SqlCommand("insert into SystemSetting values(@Param1, @Param2, @Param3, @Param4, null) ", conn))
            {
                SqlParameter param1 = new SqlParameter("@Param1", SqlDbType.NVarChar, 255);
                SqlParameter param2 = new SqlParameter("@Param2", SqlDbType.NVarChar, 255);
                SqlParameter param3 = new SqlParameter("@Param3", SqlDbType.Int, 11);
                SqlParameter param4 = new SqlParameter("@Param4", SqlDbType.NVarChar);



                param1.Value = Guid.NewGuid().ToString();
                param2.Value = name;
                param3.Value = 5;
                param4.Value = xml;

                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);
                command.Parameters.Add(param4);
                int result = command.ExecuteNonQuery();
                return result;
            }
        }


        public static string GetDatabaseEncryptionKeyFromDB(SqlConnection conn)
        {
            return GetDatabaseEncryptionInfoFromDB(conn, DatabaseEncryptionKeyName);
        }

        public static string GetPassphraseFromDB(SqlConnection conn)
        {
            return GetDatabaseEncryptionInfoFromDB(conn, PassphraseName);
        }

        private static string GetDatabaseEncryptionInfoFromDB(SqlConnection conn, string name)
        {

            using (SqlCommand command = new SqlCommand("select * from SystemSetting where name = @Param1 and type= @Param2", conn))
            {
                SqlParameter param1 = new SqlParameter("@Param1", SqlDbType.NVarChar, 255);
                SqlParameter param2 = new SqlParameter("@Param2", SqlDbType.Int, 11);
                param1.Value = name;
                param2.Value = 5;
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string result = reader["setting"].ToString();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            //ConfigurationManager.
            return null;
        }

        private static DatabaseEncryptionInfo GernerateEncryptionInfo(byte[] masterKey)
        {
            if (masterKey != null && masterKey.Length > 0)
            {
                //生成DatabbaseEncryptionKey
                byte[] databaseEncryptionKey = GenerateDatabaseEncryptionKey();
                //通过Masterkey
                byte[] encryptedKey = EncryptDatabaseEncryptionKeyByMasterKey(databaseEncryptionKey, masterKey, EncryptionAlgorithm.AES_ENCRYPTION);

                IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);

                byte[] result = hash.ComputeHash(databaseEncryptionKey);
                DatabaseEncryptionInfo info = new DatabaseEncryptionInfo()
                {
                    Value = encryptedKey,
                    CheckSum = result,
                    EncryptionType = (int)EncryptionAlgorithm.AES_ENCRYPTION
                };
                return info;
            }
            return null;
        }

        public static void SaveDatabaseEncryptionInfoToDB(SqlConnection conn, byte[] masterKey)
        {
            DatabaseEncryptionInfo info = GernerateEncryptionInfo(masterKey);
            if (info != null)
            {

                string xml = SerializerHelper.SerializeByDataContractSerializer(info);
                SaveDatabaseEncryptionInfoToDB(conn, xml, DatabaseEncryptionKeyName);
            }
        }

        public static void SavePassphraseInfo(SqlConnection conn, SecureString passphrase, byte[] masterKey)
        {
            PassphraseInfo passphraseInfo = new PassphraseInfo();
            //获取加密的DatabseEncryptionKey，并解密
            String datakeyxml = GetDatabaseEncryptionInfoFromDB(conn, DatabaseEncryptionKeyName);
            if (datakeyxml != null)
            {
                DatabaseEncryptionInfo dataEncrypted = SerializerHelper.DeserializeByDataContractSerializer<DatabaseEncryptionInfo>(datakeyxml);

                byte[] databaseEncryptionKey = DecryptDatabaseEncryptionKeyByMasterKey(dataEncrypted.Value, masterKey, EncryptionAlgorithm.AES_ENCRYPTION);

                //加密passphrase
                IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, databaseEncryptionKey, HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(databaseEncryptionKey));
                byte[] encryptedPassphrase = encryption.EncryptBinary(CryptoUtil.ConvertSecureStringToBytes(passphrase));
                //计算Hash
                IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
                byte[] result = hash.ComputeHash(CryptoUtil.ConvertSecureStringToBytes(passphrase));
                //创建dto
                AvePasswordDto dto = new AvePasswordDto()
                {
                    Value = encryptedPassphrase,
                    CheckSum = result,
                    EncryptionType = (int)EncryptionAlgorithm.AES_ENCRYPTION,
                    Version = 0
                };

                passphraseInfo.Passphrase = dto;
                string xml = SerializerHelper.SerializeByDataContractSerializer(passphraseInfo);
                SaveDatabaseEncryptionInfoToDB(conn, xml, PassphraseName);
            }
        }

        public static string GetPassphraseStrFromDB(SqlConnection conn, byte[] masterKey)
        {
            string passphraseXmlStr = GetDatabaseEncryptionInfoFromDB(conn, PassphraseName);
            string dataEncryptionXmlStr = GetDatabaseEncryptionInfoFromDB(conn, DatabaseEncryptionKeyName);
            string retValue = null;
            if (passphraseXmlStr != null && passphraseXmlStr.Length > 0 && dataEncryptionXmlStr != null && dataEncryptionXmlStr.Length > 0)
            {
                PassphraseInfo passphraseInfo = SerializerHelper.DeserializeByDataContractSerializer<PassphraseInfo>(passphraseXmlStr);
                DatabaseEncryptionInfo databaseEncryptionInfo = SerializerHelper.DeserializeByDataContractSerializer<DatabaseEncryptionInfo>(dataEncryptionXmlStr);
                byte[] passphraseKey = passphraseInfo.Passphrase.Value;
                byte[] databaseEncrypedKey = databaseEncryptionInfo.Value;
                byte[] databaseEncryptionKey = DecryptDatabaseEncryptionKeyByMasterKey(databaseEncrypedKey, masterKey, EncryptionAlgorithm.AES_ENCRYPTION);
                IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, databaseEncryptionKey, HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(databaseEncryptionKey));
                SecureString passphrase = encryption.DecryptString(passphraseKey);
                char[] passphraseChars = CryptoUtil.ConvertSecureStringToChars(passphrase);
                retValue = new string(passphraseChars);
            }
            return retValue;
        }

        public static bool ValidateMasterKey(SqlConnection conn, byte[] masterKey)
        {
            string dataEncryptionXmlStr = GetDatabaseEncryptionInfoFromDB(conn, DatabaseEncryptionKeyName);
            if (dataEncryptionXmlStr != null && dataEncryptionXmlStr.Length > 0)
            {
                DatabaseEncryptionInfo databaseEncryptionInfo = SerializerHelper.DeserializeByDataContractSerializer<DatabaseEncryptionInfo>(dataEncryptionXmlStr);
                if (databaseEncryptionInfo != null)
                {
                    byte[] key = DecryptDatabaseEncryptionKeyByMasterKey(databaseEncryptionInfo.Value, masterKey, EncryptionAlgorithm.AES_ENCRYPTION);
                    IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
                    byte[] result = hash.ComputeHash(key);

                    if (!CryptographyManagement.ArraysEqual<byte>(result, databaseEncryptionInfo.CheckSum))
                    {
                        throw new Exception("Database key corrupt");
                    }
                    return true;
                }

            }
            return false;
        }

        public static byte[] ComputePasspraseHash(SecureString passphrase)
        {
            if (passphrase != null)
            {
                IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]);
                byte[] passphraseByte = CryptoUtil.ConvertSecureStringToBytes(passphrase);
                for (int i = 0; i < 10; i++)
                {
                    passphraseByte = hash.ComputeHash(passphraseByte);
                }
                return passphraseByte;
            }
            return null;
        }

        public static byte[] ComputePasspraseHash(byte[] passphraseByte)
        {
            if (passphraseByte != null && passphraseByte.Length > 0)
            {
                IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]);
                for (int i = 0; i < 10; i++)
                {
                    passphraseByte = hash.ComputeHash(passphraseByte);
                }
                return passphraseByte;
            }
            return null;
        }
        //public static bool ValidateAgentPassprase(SqlConnection conn, byte[] passphraseHash, byte[] masterKey)
        //{
        //    string passphraseXmlStr = GetDatabaseEncryptionInfoFromDB(conn, PassphraseName);
        //    string dataEncryptionXmlStr = GetDatabaseEncryptionInfoFromDB(conn, DatabaseEncryptionKeyName);
        //    if (passphraseXmlStr != null && passphraseXmlStr.Length > 0 && dataEncryptionXmlStr != null && dataEncryptionXmlStr.Length > 0)
        //    {
        //        PassphraseInfo passphraseInfo = SerializerHelper.DeserializeFromXmlString<PassphraseInfo>(passphraseXmlStr);
        //        DatabaseEncryptionInfo databaseEncryptionInfo = SerializerHelper.DeserializeFromXmlString<DatabaseEncryptionInfo>(dataEncryptionXmlStr);
        //        byte[] passphraseKey = passphraseInfo.Passphrase.Value;
        //        byte[] databaseEncrypedKey = databaseEncryptionInfo.Value;
        //        byte[] databaseEncryptionKey = DecryptDatabaseEncryptionKeyByMasterKey(databaseEncrypedKey, masterKey, EncryptionAlgorithm.AES_ENCRYPTION);
        //        IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, databaseEncryptionKey, HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, null).ComputeHash(databaseEncryptionKey));
        //        byte[] passphrase = encryption.DecryptBinary(passphraseKey);
        //        IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
        //        byte[] result = hash.ComputeHash(passphrase);
        //        if (!CryptographyManagement.ArraysEqual<byte>(result, passphraseHash))
        //        {
        //            throw new Exception("Database key corrupt");
        //        }
        //        return true;

        //    }
        //    return false;
        //}

        public static byte[] GenerateMasterKeyForOldVersion(SecureString passphrase)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]);
            byte[] masterKey = hash.ComputeHash(CryptoUtil.ConvertSecureStringToBytes(passphrase));

            for (int i = 0; i < 4; i++)
            {
                masterKey = hash.ComputeHash(masterKey);

            }

            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.MD5, new byte[0]);
            byte[] data = md5.ComputeHash(masterKey);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return Encoding.UTF8.GetBytes(sBuilder.ToString());
        }

        #region
        public static void SaveMasterKeyToRegistry(byte[] masterKey, string registerPath)
        {
            string result = ProtectMasterKey(masterKey);
            string key = null;
            if (Registry.LocalMachine.OpenSubKey(registerPath) == null)
            {
                Registry.LocalMachine.CreateSubKey(registerPath);

            }
            if (ExsitLocalMachine(registerPath, "MasterKey"))
            {
                key = RegistryManager.ReadLocalMachine(registerPath, "MasterKey");
            }

            if (key != null)
            {
                RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, registerPath, "MasterKey_temp", key);
            }
            RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, registerPath, "MasterKey", result);
        }

        public static void DeleteMasterKeyInRegistry(string registerPath)
        {
            string key = null;
            if (ExsitLocalMachine(registerPath, "MasterKey"))
            {
                key = RegistryManager.ReadLocalMachine(registerPath, "MasterKey");
            }

            if (key != null)
            {
                RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, registerPath, "MasterKey_temp", key);
            }
            RegistryManager.RemoveValueFromRegKey(registerPath, "MasterKey");
        }

        public static void RollBackMasterKeyInRegistry(string registerPath)
        {
            if (!ExsitLocalMachine(registerPath, "MasterKey_temp"))
            {
                return;
            }
            string key = RegistryManager.ReadLocalMachine(registerPath, "MasterKey_temp");
            if (key != null)
            {
                RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, registerPath, "MasterKey", key);
                RegistryManager.RemoveValueFromRegKey(BaseKey.LocalMachine, registerPath, "MasterKey_temp");
            }
        }

        public static void CommitMasterKeyInRegistry(string registerPath)
        {
            if (!ExsitLocalMachine(registerPath, "MasterKey_temp"))
            {
                return;
            }

            string key = RegistryManager.ReadLocalMachine(registerPath, "MasterKey_temp");
            if (key != null)
            {
                RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, registerPath, "MasterKey_History_" + DateTime.Now.Ticks, key);
                RegistryManager.RemoveValueFromRegKey(BaseKey.LocalMachine, registerPath, "MasterKey_temp");
            }
        }

        public static byte[] LoadMasterKeyFromRegistry(string registerPath)
        {
            if (!ExsitLocalMachine(registerPath, "MasterKey"))
            {
                return null;
            }

            string key = RegistryManager.ReadLocalMachine(registerPath, "MasterKey");
            byte[] result = null;
            if (string.IsNullOrEmpty(key))
            {
                result = null;

            }
            else
            {
                result = UnProtectMasterKey(key);

            }
            return result;
        }

        public static string GetRegisterPathByProductType(int type)
        {
            string path = Reg_Key;
            switch (type)
            {
                case 0:
                    path = Reg_Key;
                    break;
                case 1:
                    path = Reg_key_NetAppNew_Path;
                    break;
                case 2:
                    path = Reg_key_IBMNew_Path;
                    break;
                default:
                    break;
            }
            return path;
        }

        //Note : call in cmdlet 
        public static int GetProductTypeByReg()
        {
            Exception exception = null;

            string path = Reg_Key.Substring(0, Reg_Key.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase));
            if (IsExsitProductInfoInReg(path, out exception))
            {
                return 0;
            }

            path = Reg_key_NetAppNew_Path.Substring(0, Reg_key_NetAppNew_Path.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase));
            if (IsExsitProductInfoInReg(path, out exception))
            {
                return 1;
            }

            path = Reg_key_IBMNew_Path.Substring(0, Reg_key_IBMNew_Path.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase));

            if (IsExsitProductInfoInReg(path, out exception))
            {
                return 2;
            }

            if (exception != null)
            {
                throw exception;
            }

            throw new Exception("Can not find installation path in registry.");

        }

        private static bool IsExsitProductInfoInReg(string regPath, out Exception e)
        {
            string path = null;
            Exception outE = null;
            try
            {
                path = RegistryManager.ReadLocalMachine(regPath, "InstallPath");
            }
            catch (Exception ex)
            {
                outE = ex;
                path = null;
            }
            e = outE;
            return path != null && path.Length != 0;
        }
        #endregion
    }
}
