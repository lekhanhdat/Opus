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



namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.PlatformRecovery;
    using AvePoint.GCommon.Contract.Server.ControlPanel;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Cryptography;
    using System.Xml;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;

    #endregion

    public class AgentCacheManager
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static readonly string cachedIDMutexLockName = "CachedIDOperateFileLockMutex";

        public static void PersistAgentCredential(string domain, string username, string password, bool passwordEncrypted)
        {
            Tuple<string, string, string> credential = null;
            try
            {
                credential = AgentCacheManager.GetCachedAgentCredential();
            }
            catch (Exception ee)
            {
                logger.Warn("Get Cached Agent Credential failed. {0}", ee.ToString());
            }
            string oldPassword = string.Empty;
            if (credential != null)
            {
                oldPassword = credential.Item3;
            }
            if (passwordEncrypted)
            {
                password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(password));
                if (!string.IsNullOrEmpty(oldPassword))
                {
                    try
                    {
                        oldPassword = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(oldPassword));
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                    }
                }
            }
            if (credential == null || !domain.Equals(credential.Item1, StringComparison.OrdinalIgnoreCase) || !username.Equals(credential.Item2, StringComparison.OrdinalIgnoreCase) || !password.Equals(oldPassword, StringComparison.Ordinal))
            {
                SaveCachedAgentCredential(domain, username, password);
            }
        }

        public static void ClearCachedAgentCredential()
        {
            using (AveMutex mutex = new AveMutex(cachedIDMutexLockName, false))
            {
                try
                {
                    mutex.WaitLocked();
                    string credentialFile = Path.Combine(AveEnv.AgentDataFolder, "CachedID.dat");

                    while (File.Exists(credentialFile))
                    {
                        File.Delete(credentialFile);
                    }
                }
                finally
                {
                    mutex.ReleaseLock();
                }
            }
        }

        private static void SaveCachedAgentCredential(string domain, string username, string password)
        {
            using (AveMutex mutex = new AveMutex(cachedIDMutexLockName, false))
            {
                try
                {
                    mutex.WaitLocked();
                    string credentialFile = Path.Combine(AveEnv.AgentDataFolder, "CachedID.dat");
                    XmlDocument xDoc = new XmlDocument();
                    XmlElement element = xDoc.CreateElement("Credential");
                    element.SetAttribute("domain", domain);
                    element.SetAttribute("username", username);
                    element.SetAttribute("password", password);
                    string encryptedString = ConfigurationProtectionUtil.ProtectWithBase64(CryptoUtil.ConvertStringToBytes(element.OuterXml));
                    File.WriteAllText(credentialFile, encryptedString);
                }
                finally
                {
                    mutex.ReleaseLock();
                }
            }
        }

        public static Tuple<string, string, string> GetCachedAgentCredential(bool encryptPasswordUsingCommunicationKey = true)
        {
            using (AveMutex mutex = new AveMutex(cachedIDMutexLockName, false))
            {
                try
                {
                    mutex.WaitLocked();
                    string credentialFile = Path.Combine(AveEnv.AgentDataFolder, "CachedID.dat");
                    if (File.Exists(credentialFile))
                    {
                        logger.Info("Reading ID from cache.");
                        string encryptedString = File.ReadAllText(credentialFile);
                        string xml = CryptoUtil.ConvertBytesToString(ConfigurationProtectionUtil.UnProtectWithBase64(encryptedString));
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(xml);
                        XmlElement element = xDoc.DocumentElement;
                        string domain = element.GetAttribute("domain");
                        string username = element.GetAttribute("username");
                        string password = element.GetAttribute("password");
                        if (encryptPasswordUsingCommunicationKey)
                        {
                            password = CspCommunicationWrapper.WrapKeyToBase64String(CryptoUtil.ConvertStringToBytes(password));
                        }
                        return new Tuple<string, string, string>(domain, username, password);
                    }
                    else
                    {
                        throw new FileNotFoundException(credentialFile);
                    }
                }
                finally
                {
                    mutex.ReleaseLock();
                }
            }
        }

        internal static void PersistRegisterResult(byte[] communicationEncryptionKey, int cryptoMode)
        {
            string credentialFile = Path.Combine(AveEnv.AgentDataFolder, "CachedRI.dat");
            XmlDocument xDoc = new XmlDocument();
            XmlElement element = xDoc.CreateElement("RegisterResult");
            element.SetAttribute("communicationEncryptionKey", Convert.ToBase64String(communicationEncryptionKey));
            element.SetAttribute("cryptoMode", cryptoMode.ToString());
            string encryptedString = ConfigurationProtectionUtil.ProtectWithBase64(CryptoUtil.ConvertStringToBytes(element.OuterXml));
            File.WriteAllText(credentialFile, encryptedString);
        }

        internal static Tuple<byte[], int> GetCachedRegisterResult()
        {
            string keyFile = Path.Combine(AveEnv.AgentDataFolder, "CachedRI.dat");
            if (File.Exists(keyFile))
            {
                logger.Debug("Reading EKS from cache.");
                string encryptedString = File.ReadAllText(keyFile);
                string xml = CryptoUtil.ConvertBytesToString(ConfigurationProtectionUtil.UnProtectWithBase64(encryptedString));
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                XmlElement element = xDoc.DocumentElement;
                byte[] communicationEncryptionKey = Convert.FromBase64String(element.GetAttribute("communicationEncryptionKey"));
                int cryptoMode = int.Parse(element.GetAttribute("cryptoMode"));
                return new Tuple<byte[], int>(communicationEncryptionKey, cryptoMode);
            }
            else
            {
                logger.Debug("cannot read EKS from cache.");
                return null;
            }
        }

    }

}

