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
using System.Windows.Forms;
using AutoInstallation.Contract;
using Microsoft.Win32;
using LOGRESX = AutoInstallation.Records.App.Resources.LogResource;
using GUIRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallationCommon.Utility.Handler
{
    public class RegeditKeyHandler
    {
        private static readonly RegistryKey root = Registry.LocalMachine;
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static List<RegeditKey> GetRegeditKey(SoftWareRegeditInfo info)
        {
            var keys = new List<RegeditKey>();
            keys.Add(info.DisplayIcon);
            keys.Add(info.DisplayName);
            keys.Add(info.DisplayVersion);
            keys.Add(info.EstimatedSize);
            keys.Add(info.HelpLink);
            keys.Add(info.InstallDate);
            keys.Add(info.InstalledLocation);
            keys.Add(info.InstallSource);
            keys.Add(info.Publisher);
            keys.Add(info.UninstallString);
            keys.Add(info.URLInfoAbout);
            keys.Add(info.URLUpdateInfo);
            return keys;
        }

        public static void DeleteRegeditKey(string subKeyName)
        {
            try
            {
                var regUninstall = root.OpenSubKey(subKeyName, true);
                if (regUninstall != null)
                    root.DeleteSubKey(subKeyName);
                else
                    logger.Info(LOGRESX.COMMONLOG_REGISTRYKEYNOTEXIST, subKeyName);
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONUTILITYLOG_DELETEREGISTRYKEYERROR, subKeyName, ex.ToString());
            }
        }

        public static void ChangeRegister(SoftWareRegeditInfo info, bool throwException)
        {
            var subKey = root.OpenSubKey(info.RegistryKeyPath, true);
            if (subKey == null)
            {
                if (throwException)
                {
                    logger.Error(LOGRESX.COMMONLOG_REGISTRYKEYNOTEXIST, info.RegistryKeyPath);
                    MessageBox.Show("GUIRESX.COMMONUTILITY_GETREGISTRYKEYERROR",
                        "GUIRESX.COMMON_TEXT_AVEPOINTRELATEDRECORDS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
            else
            {
                foreach (var key in GetRegeditKey(info)) CreateRegisterKeyValueWorker(subKey, key);
            }
        }

        public static void CreateRegister(SoftWareRegeditInfo info)
        {
            try
            {
                if (!ExistSubKey(info.RegistryKeyPath))
                {
                    var thisSubKey = CreateRegisterSubKeyWorker(info.RegistryKeyPath);

                    foreach (var key in GetRegeditKey(info)) CreateRegisterKeyValueWorker(thisSubKey, key);
                }
                else
                {
                    logger.Warn(LOGRESX.COMMONUTILITYLOG_REGEDITKEYEXIST);
                }
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONUTILITYLOG_REGISTERREGISTRYKEYERROR, ex.ToString());
            }
        }

        /// <summary>
        ///     检查SubKey是否存在
        /// </summary>
        /// <param name="subKeyName">全名</param>
        /// <returns>是否存在</returns>
        public static bool ExistSubKey(string subKeyName)
        {
            var subKey = root.OpenSubKey(subKeyName);
            if (subKey != null) return true;
            return false;
        }

        private static void CreateRegisterKeyValueWorker(RegistryKey subKey, RegeditKey key)
        {
            try
            {
                subKey.SetValue(key.Name, key.Value, key.ValueKind);
            }
            catch (Exception ex)
            {
                logger.Warn(LOGRESX.COMMONUTILITYLOG_MODIFYREGISTRYKEYERROR, key.Name, key.Value,
                    key.ValueKind.ToString(), ex.ToString());
            }
        }

        /// <summary>
        ///     创建一个SubKey
        /// </summary>
        /// <param name="subKeyName">全名</param>
        /// <returns>建好的SubKey</returns>
        private static RegistryKey CreateRegisterSubKeyWorker(string subKeyName)
        {
            var resultKey = root.CreateSubKey(subKeyName, RegistryKeyPermissionCheck.Default);
            return resultKey;
        }

        /// <summary>
        ///     抛错
        /// </summary>
        /// <param name="info"></param>
        public static void GetSoftWareRegeditInfo(ref SoftWareRegeditInfo info, bool throwException)
        {
            var subKey = root.OpenSubKey(info.RegistryKeyPath);
            if (subKey == null)
            {
                if (throwException)
                {
                    logger.Error(LOGRESX.COMMONLOG_REGISTRYKEYNOTEXIST, info.RegistryKeyPath);
                    MessageBox.Show("GUIRESX.COMMONUTILITY_GETREGISTRYKEYERROR",
                        "GUIRESX.COMMON_TEXT_AVEPOINTRELATEDRECORDS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
            else
            {
                foreach (var key in GetRegeditKey(info))
                    try
                    {
                        key.Value = subKey.GetValue(key.Name);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(LOGRESX.COMMONUTILITYLOG_GETSUBREGISTRYKEYVALUEFAILED, key.Name, ex.ToString());
                    }
            }
        }

        public static bool GetSoftWareRegeditInfo(ref SoftWareRegeditInfo info, bool throwException,
            List<string> requirements)
        {
            var subKey = root.OpenSubKey(info.RegistryKeyPath);
            if (subKey == null)
            {
                if (throwException)
                {
                    logger.Error(LOGRESX.COMMONLOG_REGISTRYKEYNOTEXIST, info.RegistryKeyPath);
                    MessageBox.Show("GUIRESX.COMMONUTILITY_GETREGISTRYKEYERROR",
                        "GUIRESX.COMMON_TEXT_AVEPOINTRELATEDRECORDS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                else
                {
                    logger.Info(LOGRESX.COMMONLOG_REGISTRYKEYNOTEXIST, info.RegistryKeyPath);
                }

                return false;
            }

            foreach (var key in GetRegeditKey(info))
                try
                {
                    key.Value = subKey.GetValue(key.Name);
                }
                catch (Exception ex)
                {
                    logger.Warn(LOGRESX.COMMONUTILITYLOG_GETSUBREGISTRYKEYVALUEFAILED, key.Name, ex.ToString());
                    if (requirements.Contains(key.Name)) // == "UninstallString")
                    {
                        MessageBox.Show("GUIRESX.COMMONUTILITY_GETREGISTRYKEYERROR",
                            "GUIRESX.COMMON_TEXT_AVEPOINTRELATEDRECORDS",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                }

            return true;
        }
    }
}