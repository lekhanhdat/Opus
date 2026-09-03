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



namespace AvePoint.Hybrid.Utility.Cryptography.Registry
{
    #region using directives
    using System;
    using Microsoft.Win32;
    #endregion

    /// <summary>
    /// Base key.
    /// </summary>
    public enum BaseKey
    {
        /// <summary>
        /// Default, current user.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Local machine.
        /// </summary>
        LocalMachine
    }

    /// <summary>
    /// Provider a common way to access the Registry.
    /// </summary>
    public sealed class RegistryManager
    {
        /// <summary>
        /// read a special subkey value
        /// </summary>
        /// <param name="subKey">subkey</param>
        /// <param name="subKeyName">subkey name</param>
        /// <param name="valueName">value name</param>
        /// <returns>result string value</returns>
        public static String ReadSubkeyValue(RegistryKey subKey, String subKeyName, String valueName)
        {
            var result = default(String);
            using (var key = subKey.OpenSubKey(subKeyName))
            {
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    result = value == null ? String.Empty : value.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Get all sub key names in a registry key
        /// </summary>
        /// <param name="subKey">subkey</param>
        /// <returns>the name array</returns>
        public static String[] GetAllSunKeyNames(RegistryKey subKey)
        {
            return subKey.GetSubKeyNames();
        }

        /// <summary>
        /// read sub key from local machine registry sub key
        /// </summary>
        /// <param name="subKey">subkey name</param>
        /// <param name="valueName">value name</param>
        /// <returns>result string value</returns>
        public static String ReadLocalMachine(String subKey, String valueName)
        {
            var result = String.Empty;
            using (var key = Registry.LocalMachine.OpenSubKey(subKey))
            {
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    result = value == null ? String.Empty : value.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Read the value from the class root
        /// </summary>
        /// <param name="subKey">sub key name </param>
        /// <param name="valueName">value name </param>
        /// <returns>result string value</returns>
        public static String ReadClassRoot(String subKey, String valueName)
        {
            var result = String.Empty;
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(subKey))
            {
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    result = value == null ? String.Empty : value.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Get the value form a WOW application in a 64 bit registry
        /// </summary>
        /// <param name="subKey">the subkey name</param>
        /// <param name="valueName">the value name</param>
        /// <returns>result string value</returns>
        public static String ReadLocalMachine64(String subKey, String valueName)
        {
            return Registry64.LocalMachine.GetValue(subKey, valueName);
        }

        /// <summary>
        /// Gets value from a registry key under base key.
        /// </summary>
        /// <param name="keyBase">Base key</param>
        /// <param name="keyName">Key name</param>
        /// <param name="valueName">Value name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns></returns>
        public static Object GetValueFromRegKey(BaseKey keyBase, String keyName, String valueName, Object defaultValue)
        {
            var key = (keyBase == BaseKey.Default) ?
                Registry.CurrentUser.OpenSubKey(keyName) : Registry.LocalMachine.OpenSubKey(keyName);
            if (key != null && Array.IndexOf(key.GetValueNames(), valueName) > -1)
            {
                var value = key.GetValue(valueName);
                if (value != null) return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets value from a registry key under CurrentUser.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="valueName">Value name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns></returns>
        public static Object GetValueFromRegKey(String keyName, String valueName, Object defaultValue)
        {
            return GetValueFromRegKey(BaseKey.Default, keyName, valueName, defaultValue);
        }

        /// <summary>
        /// Sets value to a registry key under base key.
        /// </summary>
        /// <param name="keyBase">Base key</param>
        /// <param name="keyName">Key name</param>
        /// <param name="valueName">Value name</param>
        /// <param name="newValue">New value</param>
        public static void SetValueToRegKey(BaseKey keyBase, String keyName, String valueName, Object newValue)
        {
            if (null == newValue)
                throw new ArgumentException("new value cannot be null");
            var key = (keyBase == BaseKey.Default) ?
                Registry.CurrentUser.OpenSubKey(keyName, true) : Registry.LocalMachine.OpenSubKey(keyName, true);

            if (key == null)
                key = (keyBase == BaseKey.Default) ?
                    Registry.CurrentUser.CreateSubKey(keyName) : Registry.LocalMachine.CreateSubKey(keyName);
            key.SetValue(valueName, newValue);
        }

        /// <summary>
        /// Sets value to a registry key under CurrentUser.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="valueName">Value name</param>
        /// <param name="newValue">New value</param>
        public static void SetValueToRegKey(String keyName, String valueName, String newValue)
        {
            SetValueToRegKey(BaseKey.Default, keyName, valueName, newValue);
        }

        /// <summary>
        /// Removes a value from a registry key under base key.
        /// </summary>
        /// <param name="keyBase">Base key</param>
        /// <param name="keyName">Key name</param>
        /// <param name="valueName">Value name</param>
        public static void RemoveValueFromRegKey(BaseKey keyBase, String keyName, String valueName)
        {
            var key = (keyBase == BaseKey.Default) ?
                Registry.CurrentUser.OpenSubKey(keyName, true) : Registry.LocalMachine.OpenSubKey(keyName, true);
            if (key != null && Array.IndexOf(key.GetValueNames(), valueName) > -1)
                key.DeleteValue(valueName, false);
        }

        /// <summary>
        /// Removes a value from a registry key under CurrentUser.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="valueName">Value name</param>
        public static void RemoveValueFromRegKey(String keyName, String valueName)
        {
            RemoveValueFromRegKey(BaseKey.Default, keyName, valueName);
        }
    }
}