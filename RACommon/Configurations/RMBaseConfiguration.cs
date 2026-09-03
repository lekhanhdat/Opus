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
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.CommonUtil;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Util;

namespace AvePoint.RA.Common.Configurations
{
    public abstract class RMBaseConfiguration<T> : IDisposable where T : struct, Enum
    {
        protected RALogger logger;
        private ConcurrentDictionary<T, string> settings;
        private Timer refreshTimer;
        // cache 10 min
        protected virtual int RefreshConfigInterval => 1000 * 60 * 10;
        protected virtual bool EnableRefreshTimer => false;
        /// <summary>
        /// 需要从环境变量中获取的配置项，用 Environment.GetEnvironmentVirable 获取
        /// </summary>
        protected virtual HashSet<T> EnvVirableItems => null;
        /// <summary>
        /// 需要加密的配置项
        /// </summary>
        protected virtual Dictionary<T, RMEncryptType> EncryptedItems => null;

        public RMBaseConfiguration()
        {
            logger = RALogger.GetInstance(this.GetType());
            RefreshSettings();
            if (EnableRefreshTimer)
            {
                refreshTimer = new Timer(RefreshSettings, null, RefreshConfigInterval, RefreshConfigInterval);
            }
        }

        public string this[T settingKey]
        {
            get
            {
                string value = null;
                if (settings.TryGetValue(settingKey, out value))
                {
                    return value;
                }
                return null;
            }
        }

        private void RefreshSettings(object objArgs = null)
        {
            try
            {
                var tempDic = new ConcurrentDictionary<T, string>();
                foreach (T key in Enum.GetValues(typeof(T)))
                {
                    try
                    {
                        tempDic[key] = GetValueFromConfigFile(key);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error occurred while get vaule from config: {ex}");
                    }
                    
                }

                this.settings = tempDic;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while load AppSettings: {ex}");
            }
        }

        protected string Decrypt(string key, string plainText, RMEncryptType type)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText)) return plainText;
                switch (type)
                {
                    case RMEncryptType.Cipher:
                        plainText = CipherEncryptionUtil.CipherDecrypt(plainText);
                        break;
                    case RMEncryptType.Base64:
                        plainText = ConvertFormBase64(plainText);
                        break;
                    default:
                        throw new NotSupportedException($"invalid encrypt type:{type}");
                }
                return plainText;
            }
            catch (Exception ex)
            {
                logger.Info($"Decrypt setting: {key} failed: {ex.ToString()}");
                return plainText;
            }
        }

        protected string AnalysePwd(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                Regex regex = new Regex("^(plaintext|pt){(.*)}$");
                Match match = regex.Match(password);
                if (match.Success)
                {
                    return match.Groups[2].Value;
                }
            }
            
            return password;
        }

        protected string ConvertFormBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                return base64String;
            }
            return Regex.Replace(Encoding.UTF8.GetString(Convert.FromBase64String(base64String)), "[\\s\t\n\r]", " ", RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);
        }

        protected virtual string GetValueFromConfigFile(T key)
        {
            string value = null;
            try
            {
                value = ConfigurationSetting.GetValue(key.ToString());

                if (EnvVirableItems?.Contains(key) == true)
                {
                    var envValue = Environment.GetEnvironmentVariable(key.ToString());
                    if (!string.IsNullOrEmpty(envValue))
                    {
                        value = envValue;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Get config from local configuration failed. " + e);
            }

            RMEncryptType encryptType = RMEncryptType.Cipher;
            //log does not output sensitive information RECO-13410
            if (EncryptedItems?.TryGetValue(key, out encryptType) == true)
            {
                value = Decrypt(key.ToString(), value, encryptType);
            }
            if (!IsSensitiveValue(key, value))
            {
                logger.Info($"AppSetting key: {key}, value: { value }");
            }

            return value;

        }

        public IEnumerable<IConfigurationSection> GetSectionValueFromCongfigFile(string key)
        {
            return ConfigurationSetting.GetValue<IEnumerable<IConfigurationSection>>(key);
        }

        private bool IsSensitiveValue(T key, string value)
        {
            if (!string.IsNullOrEmpty(value)) 
            {
                return EncryptedItems?.ContainsKey(key) == true 
                    || new Regex("password|key|database|storage|blob|table|servicebus", RegexOptions.IgnoreCase).IsMatch(value) == true
                    || key.ToString().ToLower().Contains("client") || key.ToString().ToLower().Contains("report_temp_folder")
                    || key.ToString().ToLower().Contains("infra_cipher_key") || key.ToString().ToLower().Contains("product_certificate_identifier")
                    || key.ToString().ToLower().Contains("encryption_fscertificate_key") || key.ToString().ToLower().Contains("db_default_encryption_key") || key.ToString().ToLower().Contains("SQL_SERVER_FOR_ELASTICPOOL")
                  ;
            }
            return false;
        }

        protected bool ContainsValue(T key)
        {
            return settings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value);
        }

        public int GetNumberValue(T key, int defaultValue)
        {
            if (int.TryParse(this[key], out var number))
            {
                return number;
            }
            else
            {
                return defaultValue;
            }
        }

        public bool GetBooleanValue(T key, bool defaultValue)
        {
            if (bool.TryParse(this[key], out var value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public void SetEnvironmentVariable(T key, string value)
        {
            Environment.SetEnvironmentVariable(key.ToString(), value);
            settings[key] = value;
        }

        public void Dispose()
        {
            this.refreshTimer?.Dispose();
        }
    }

    class RMConfigItem
    {
        public string Name { get; set; }
        public RMEncryptType EncryptType { get; set; }
    }

    public enum RMEncryptType 
    {
        Cert = 66,
        Base64 = 67,
        Cipher = 68,
    }
}
