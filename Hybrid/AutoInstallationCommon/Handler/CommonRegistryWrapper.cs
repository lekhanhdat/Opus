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
using System.Globalization;
using Microsoft.Win32;

namespace AutoInstallationCommon.Utility
{
    public class CommonRegistryWrapper
    {
        private static CommonRegistryWrapper _thisInstance;

        private CommonRegistryWrapper()
        {
        }

        public static CommonRegistryWrapper GetInstance()
        {
            return _thisInstance ?? (_thisInstance = new CommonRegistryWrapper());
        }

        /// <summary>
        ///     删除键
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        public void Delete(string keyName)
        {
            if (Exists(keyName)) GetRootKey(ref keyName).DeleteSubKeyTree(keyName); // .DeleteSubKey(path, true)
        }

        /// <summary>
        ///     删除键值项
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        /// <param name="valueName">键值项的名称</param>
        public void Delete(string keyName, string valueName)
        {
            RegistryKey registryKet = null;
            try
            {
                if (Exists(keyName))
                {
                    registryKet = GetRootKey(ref keyName).OpenSubKey(keyName, true);
                    if (registryKet.GetValue(valueName) != null) registryKet.DeleteValue(valueName);
                }
            }
            finally
            {
                if (registryKet != null) registryKet.Close();
            }
        }

        /// <summary>
        ///     添加键
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        public void Create(string keyName)
        {
            if (!Exists(keyName))
                GetRootKey(ref keyName).CreateSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
        }

        /// <summary>
        ///     判断是否存在指定的项
        /// </summary>
        /// <param name="regRoot">判断的根名称</param>
        /// <param name="regItem">指定的项名称</param>
        /// <returns></returns>
        public bool IsRegistryItemExist(RegistryKey regRoot, string regItem)
        {
            try
            {
                string[] subkeyNames;
                subkeyNames = regRoot.GetSubKeyNames();
                foreach (var keyName in subkeyNames)
                    if (keyName != null)
                        if (keyName.Equals(regItem))
                            return true;
                return false;
            }
            catch
            {
                //to do?
                return false;
            }
        }


        /// <summary>
        ///     设置键值项的值
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        /// <param name="valueName">键值项的名称</param>
        /// <param name="value">键值项的值</param>
        public void Create(string keyName, string valueName, string value)
        {
            RegistryKey registryKey = null;
            try
            {
                if (!Exists(keyName))
                    registryKey = GetRootKey(ref keyName).CreateSubKey(keyName,
                        RegistryKeyPermissionCheck.ReadWriteSubTree);
                else
                    registryKey = GetRootKey(ref keyName).OpenSubKey(keyName, true);
                if (registryKey != null) registryKey.SetValue(valueName, value);
            }
            finally
            {
                if (registryKey != null) registryKey.Close();
            }
        }

        /// <summary>
        ///     创建DWord型
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        public void CreateDWord(string keyName, string valueName, string value)
        {
            RegistryKey registryKey = null;
            try
            {
                if (!Exists(keyName))
                    registryKey = GetRootKey(ref keyName).CreateSubKey(keyName,
                        RegistryKeyPermissionCheck.ReadWriteSubTree);
                else
                    registryKey = GetRootKey(ref keyName).OpenSubKey(keyName, true);
                if (registryKey != null) registryKey.SetValue(valueName, value, RegistryValueKind.DWord);
            }
            finally
            {
                if (registryKey != null) registryKey.Close();
            }
        }

        /// <summary>
        ///     判断键是否存在
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        /// <returns>true:存在;false:不存在或发生异常</returns>
        public bool Exists(string keyName)
        {
            RegistryKey registryKey = null;
            try
            {
                registryKey = GetRootKey(ref keyName).OpenSubKey(keyName);
                if (registryKey != null)
                    return true;
                else
                    return false;
            }
            finally
            {
                if (registryKey != null) registryKey.Close();
            }
        }

        /// <summary>
        ///     判断键值项是否存在
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        /// <param name="valueName">键值项的名称</param>
        /// <returns>true:存在;false:不存在或发生异常</returns>
        public bool Exists(string keyName, string valueName)
        {
            RegistryKey registryKey = null;
            try
            {
                registryKey = GetRootKey(ref keyName).OpenSubKey(keyName);
                if (registryKey != null)
                {
                    if (registryKey.GetValue(valueName) != null)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                if (registryKey != null) registryKey.Close();
            }
        }

        /// <summary>
        ///     获取键值项的值，如果不存在该值并且是64位系统，会自动检测对应的32位的值
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        /// <param name="valueName">键值项的名称</param>
        /// <returns></returns>
        public string GetValue(string keyName, string valueName)
        {
            var keyValue = string.Empty;
            keyValue = GetKeyValue(keyName, valueName);
            if (string.IsNullOrEmpty(keyValue))
            {
                var systemInfo = new SYSTEM_INFO();
                Win32Wrapper.GetSystemInfo(ref systemInfo);
                if (systemInfo.dwOemId == 9) keyValue = GetKeyValue(Generate6432NodeKeyName(keyName), valueName);
            }

            return keyValue;
        }

        /// <summary>
        ///     获取键值项的值
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        /// <param name="valueName">键值项的名称</param>
        /// <returns>null:不存在该键值项或发生异常</returns>
        private string GetKeyValue(string keyName, string valueName)
        {
            RegistryKey registryKey = null;
            try
            {
                if (Exists(keyName))
                {
                    registryKey = GetRootKey(ref keyName).OpenSubKey(keyName);
                    if (registryKey != null && registryKey.GetValue(valueName) != null)
                        return Convert.ToString(registryKey.GetValue(valueName));
                    else
                        return null;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                if (registryKey != null) registryKey.Close();
            }
        }

        private string Generate6432NodeKeyName(string keyName)
        {
            if (keyName.ToUpper(CultureInfo.CurrentCulture).Contains("HKEY_LOCAL_MACHINE\\SOFTWARE\\") &&
                !keyName.ToUpper(CultureInfo.CurrentCulture).Contains("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432NODE\\"))
                return keyName.ToUpper(CultureInfo.CurrentCulture).Replace("HKEY_LOCAL_MACHINE\\SOFTWARE\\",
                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432NODE\\");
            if (keyName.ToUpper(CultureInfo.CurrentCulture).Contains("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432NODE\\"))
                return keyName;
            return string.Empty;
        }

        /// <summary>
        ///     获取主键
        /// </summary>
        /// <param name="keyName">键的完整名称</param>
        /// <returns>null:发生异常</returns>
        private RegistryKey GetRootKey(ref string keyName)
        {
            //HKEY_LOCAL_MACHINE\SOFTWARE\AvePoint\DocAve6

            if (keyName.Contains("\\"))
                return GetRootKeyHandler(ref keyName);
            return null;
        }

        private static RegistryKey GetRootKeyHandler(ref string keyName)
        {
            var rootPath = keyName.Substring(0, keyName.IndexOf('\\')).ToUpper(CultureInfo.CurrentCulture);
            keyName = keyName.Substring(keyName.IndexOf('\\') + 1);
            switch (rootPath)
            {
                case "HKEY_LOCAL_MACHINE":
                    return Registry.LocalMachine;
                case "HKEY_CLASSES_ROOT":
                    return Registry.ClassesRoot;
                case "HKEY_CURRENT_USER":
                    return Registry.CurrentUser;
                case "HKEY_USERS":
                    return Registry.Users;
                case "HKEY_CURRENT_CONFIG":
                    return Registry.CurrentConfig;
                default:
                    return null;
            }
        }

        public bool SetValue(string keyName, string valueName, string value)
        {
            RegistryKey registryKet = null;
            try
            {
                registryKet = GetRootKey(ref keyName).OpenSubKey(keyName, true);
                if (registryKet != null) registryKet.SetValue(valueName, value, RegistryValueKind.String);
                return true;
            }
            finally
            {
                if (registryKet != null) registryKet.Close();
            }
        }

        /// <summary>
        ///     Sets value to a registry key under base key.
        /// </summary>
        /// <param name="keyBase">Base key</param>
        /// <param name="keyName">Key name</param>
        /// <param name="valueName">Value name</param>
        /// <param name="newValue">New value</param>
        public void SetValueToRegKey(BaseKey keyBase, string keyName, string valueName, object newValue)
        {
            if (null == newValue) throw new ArgumentException("new value cannot be null");
            var key = keyBase == BaseKey.Default
                ? Registry.CurrentUser.OpenSubKey(keyName, true)
                : Registry.LocalMachine.OpenSubKey(keyName, true);

            if (key == null)
                key = keyBase == BaseKey.Default
                    ? Registry.CurrentUser.CreateSubKey(keyName)
                    : Registry.LocalMachine.CreateSubKey(keyName);
            key.SetValue(valueName, newValue);
        }

        /// <summary>
        ///     Removes a value from a registry key under base key.
        /// </summary>
        /// <param name="keyBase">Base key</param>
        /// <param name="keyName">Key name</param>
        /// <param name="valueName">Value name</param>
        public void RemoveValueFromRegKey(BaseKey keyBase, string keyName, string valueName)
        {
            var key = keyBase == BaseKey.Default
                ? Registry.CurrentUser.OpenSubKey(keyName, true)
                : Registry.LocalMachine.OpenSubKey(keyName, true);
            if (key != null && Array.IndexOf(key.GetValueNames(), valueName) > -1) key.DeleteValue(valueName, false);
        }

        /// <summary>
        ///     read subkey from local machine registry subkey
        /// </summary>
        /// <param name="subKey">subkey name</param>
        /// <param name="valueName">value name</param>
        /// <returns>result string value</returns>
        public string ReadLocalMachine(string subKey, string valueName)
        {
            var result = string.Empty;
            using (var key = Registry.LocalMachine.OpenSubKey(subKey))
            {
                if (key != null) result = key.GetValue(valueName).ToString();
            }

            return result;
        }
    }

    public enum BaseKey
    {
        /// <summary>
        ///     Default, current user.
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Local machine.
        /// </summary>
        LocalMachine
    }
}