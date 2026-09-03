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


using System.Collections.Generic;
using AutoInstallation.Contract;
using Microsoft.Win32;

namespace AutoInstallationCommon.Utility
{
    public class CommonCreateRegisterHandler
    {
        private readonly RegistryKey root = Registry.LocalMachine;

        /// <summary>
        ///     创建注册表
        /// </summary>
        /// <param name="registerDict"></param>
        public void CreateRegister(Dictionary<string, Dictionary<string, string>> registerDict)
        {
            foreach (var subKeyName in registerDict.Keys)
                if (!ExistSubKey(subKeyName))
                {
                    var thisSubKey = CreateRegisterSubKeyWorker(subKeyName);
                    var dict = registerDict[subKeyName];
                    foreach (var key in dict.Keys) CreateRegisterKeyValueWorker(thisSubKey, key, dict[key]);
                }
        }

        public void CreateRegister(Dictionary<string, List<RegeditKey>> registerDict)
        {
            foreach (var subKeyName in registerDict.Keys)
                if (!ExistSubKey(subKeyName))
                {
                    var thisSubKey = CreateRegisterSubKeyWorker(subKeyName);
                    var dict = registerDict[subKeyName];
                    foreach (var key in dict) CreateRegisterKeyValueWorker(thisSubKey, key);
                }
        }

        /// <summary>
        ///     创建一个SubKey
        /// </summary>
        /// <param name="subKeyName">全名</param>
        /// <returns>建好的SubKey</returns>
        private RegistryKey CreateRegisterSubKeyWorker(string subKeyName)
        {
            var resultKey = root.CreateSubKey(subKeyName, RegistryKeyPermissionCheck.Default);
            return resultKey;
        }

        /// <summary>
        ///     存值
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        private void CreateRegisterKeyValueWorker(RegistryKey subKey, string keyName, string value)
        {
            subKey.SetValue(keyName, value);
        }

        private void CreateRegisterKeyValueWorker(RegistryKey subKey, RegeditKey key)
        {
            subKey.SetValue(key.Name, key.Value, key.ValueKind);
        }

        /// <summary>
        ///     检查SubKey是否存在
        /// </summary>
        /// <param name="subKeyName">全名</param>
        /// <returns>是否存在</returns>
        private bool ExistSubKey(string subKeyName)
        {
            var subKey = root.OpenSubKey(subKeyName);
            if (subKey != null) return true;
            return false;
        }
    }
}