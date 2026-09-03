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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility.Cryptography.Encryption;
using AvePoint.Hybrid.Utility.Cryptography.Registry;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using HybridCommonModel.DataModel.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AvePoint.Hybrid.Utility.ConfigurationFile
{
    public class AgentConfigurationFileHelper
    {
        //private static string SubKeyName = @"SYSTEM\CurrentControlSet\Control\Lsa";
        ////private static string subKeyName = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa";
        //private static string InstallationCodeKey = @"installationCode";
        //private static string AgentConfigurationFileKey = @"agentConfiguration";
        //private const string ProxySettingKey = @"aveAgentProxy";
        //private const string AgentAccountKey = @"aveAgentAccount";

        //private const string DefaultEncryptionKey = "a57fb058-acbd-41c9-8d80-c96553f2c82a"; //used as aes key to encrypt/decrypt proxy setting data


        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(AgentConfigurationFileHelper));


        public static AgentConfigurtion ReadFromLocalPath(string filePath, string installationCode)
        {
            if (!ExistInLocalPath(filePath))
            {
                logger.Warn($"Can't find the file : {filePath}");
                return null;
            }

            var encryptBytes = File.ReadAllBytes(filePath);

            return ReadFromEncryptBytes(encryptBytes, installationCode);
        }

        public static AgentConfigurtion ReadFromEncryptBytes(byte[] encryptBytes, string installationCode)
        {
            try
            {
                var decryptBytes = AESEncriptionHelper.Decrypt(encryptBytes, GetEncryptKey(installationCode));
                var plainText = System.Text.Encoding.UTF8.GetString(decryptBytes);
                var config = JsonConvert.DeserializeObject<AvePoint.Hybrid.Contract.Object.AgentConfigurtion>(plainText);

                return config;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to read encrypt bytes: {e.ToString()}");
                return null;
            }
        }

        public static AgentConfigurtion ReadFromRegistry()
        {
            var configStr = ReadConfigBase64String();
            if (string.IsNullOrEmpty(configStr)) return null;

            var installationCode = ReadInstallationCode();
            if (string.IsNullOrEmpty(installationCode)) return null;

            var bytes = Convert.FromBase64String(configStr);

            var result = ReadFromEncryptBytes(bytes, installationCode);
            return result;
        }

        

        public static bool ExistInLocalPath(string filePath)
        {
            return File.Exists(filePath);
        }

        public static string ReadConfigBase64String()
        {
            var value = RegistryManager.ReadLocalMachine(RegistryConstants.SubKeyName, RegistryConstants.AgentConfigurationFileKey);
            return value;
        }

        public static void WriteConfig(string filePath, string installationCode)
        {
            var encryptBytes = File.ReadAllBytes(filePath);
            WriteConfig2Registry(encryptBytes);
        }

        public static void WriteConfig2Registry(AgentConfigurtion conf, string installationCode)
        {
            var confJson = JsonConvert.SerializeObject(conf);
            var confBytes = System.Text.Encoding.UTF8.GetBytes(confJson);
            var encryptBytes = AESEncriptionHelper.Encrypt(confBytes, GetEncryptKey(installationCode));
            WriteConfig2Registry(encryptBytes);
        }

        private static void WriteConfig2Registry(byte[] encryptBytes)
        {
            var base64ConfigString = Convert.ToBase64String(encryptBytes);
            WriteConfig2Registry(base64ConfigString);
        }

        public static void WriteConfig2Registry(string base64ConfigString)
        {
            RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, RegistryConstants.SubKeyName, RegistryConstants.AgentConfigurationFileKey, base64ConfigString);

        }

        public static void WriteInstallationCode(string installationCode)
        {
            RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, RegistryConstants.SubKeyName, RegistryConstants.InstallationCodeKey, installationCode);
        }

        public static string ReadInstallationCode()
        {
            var value = RegistryManager.ReadLocalMachine(RegistryConstants.SubKeyName, RegistryConstants.InstallationCodeKey);
            return value;
        }

        private static string GetEncryptKey(string installationCode)
        {
            return $"{installationCode}{AvePoint.Hybrid.Contract.Object.AgentConfigurtion.Salt}";
        }

        /// <summary>
        /// write proxy setting to registry
        /// </summary>
        /// <param name="options"></param>
        //public static void WriteProxySetting(AveWebProxyOptions options)
        //{
        //    var jsonStr = JsonConvert.SerializeObject(options);
        //    var encryptBase64Str = Convert.ToBase64String(AESEncriptionHelper.Encrypt(Encoding.UTF8.GetBytes(jsonStr), RegistryConstants.DefaultEncryptionKey));
        //    RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, RegistryConstants.SubKeyName, RegistryConstants.ProxySettingKey, encryptBase64Str);
        //}

        /// <summary>
        /// get proxy setting from registry
        /// </summary>
        /// <returns></returns>
        //public static AveWebProxyOptions ReadProxySetting()
        //{
        //    var encryptBase64Str = RegistryManager.ReadLocalMachine(RegistryConstants.SubKeyName, RegistryConstants.ProxySettingKey);
        //    if (string.IsNullOrEmpty(encryptBase64Str)) return null;

        //    var decryptBase64Str = Encoding.UTF8.GetString(AESEncriptionHelper.Decrypt(Convert.FromBase64String(encryptBase64Str), RegistryConstants.DefaultEncryptionKey));
        //    return JsonConvert.DeserializeObject<AveWebProxyOptions>(decryptBase64Str);
        //}

        /// <summary>
        /// remove proxy setting from registry
        /// </summary>
        //public static void RemoveProxySetting()
        //{
        //    RegistryManager.RemoveValueFromRegKey(BaseKey.LocalMachine, RegistryConstants.SubKeyName, RegistryConstants.ProxySettingKey);
        //}

    }
}
