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

namespace AvePoint.Media.Storage.Box
{
    #region using directives
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Storage.Util;
    using GCommon;
    using System;
    using System.IO;
    using System.Threading;
    using System.Xml;
    #endregion

    class BoxConfigFileHandler
    {
        private string connectionString;
        private string clientId;
        private IXSystem configLocationSystem;
        private StorageInfo info;
        private StorageInfo tempFileInfo;
        private XmlHandler xmlHandler;
        private StorageOpenValidResult validateResult;
        private String originalEmailAddress;
        private String emailAddress;
        private static Object locker = new Object();
        private AveLogger logger = AveLogger.GetInstance(typeof(BoxConfigFileHandler));

        public StorageOpenValidResult ValidateResult
        {
            get { return validateResult; }
            set { validateResult = value; }
        }
        public BoxConfigFileHandler(string location, string username, string password, string emailAddress, string originalEmailAddress)
        {
            this.logger.Info(String.Format("Box Email address is {0}, original email address is {1}.", emailAddress, originalEmailAddress));
            //this.clientId = clientId;
            connectionString = string.Format(@"docave-xam://fs_vim?location={0}&Advanced=false&name={1}&secret={2}&culture={3}", location, username, XRI.ValueEncode(SecretUtil.EncryptPassword(password)), AbstractXSystem.Culture);
            this.originalEmailAddress = originalEmailAddress;
            this.emailAddress = emailAddress;
            info = new StorageInfo();
            tempFileInfo = new StorageInfo();
            xmlHandler = new XmlHandler();
            info.LowName = HashCodeHelper.ToMD5HashCode(this.emailAddress) + ".config";
            tempFileInfo.LowName = info.LowName + "_temp";
            configLocationSystem = XFactory.InstanceSystem(connectionString);
            configLocationSystem.Open();
            validateResult = configLocationSystem.Validate();
            this.ConfigFileExist();
        }

        public bool ConfigFileExist()
        {
            this.logger.Info(String.Format("Box config file name is : {0}", info.LowName));
            var result = configLocationSystem.FileExists(info) || configLocationSystem.FileExists(tempFileInfo);
            if (result)
            {
                return true;
            }
            else
            {
                //如果配置文件不存在, 尝试升级
                try
                {
                    this.UpgradeConfigFile();
                }
                catch (PathNotFoundException)
                {
                    return false;
                }
                result = configLocationSystem.FileExists(info) || configLocationSystem.FileExists(tempFileInfo);
                return result;
            }
        }

        public void Close()
        {
            configLocationSystem.Close();
        }

        public void CreateOrUpdateConfigFile(string refreshToken, string accessToken, string time, string clientSecret, string clientId)
        {
            WaitConfigHandle();
            lock (locker)
            {
                configLocationSystem.MoveFile(info, tempFileInfo, true);
                using (var fileSteam = configLocationSystem.OpenStream(tempFileInfo, FileMode.Create))
                {
                    xmlHandler.CreateConfig(fileSteam, refreshToken, accessToken, time, clientSecret, clientId);
                }
                configLocationSystem.MoveFile(tempFileInfo, info, true);
            }
        }

        private void WaitConfigHandle()
        {
            int count = 0;
            while (configLocationSystem.FileExists(tempFileInfo))
            {
                count++;
                if (count > 6)
                {
                    throw new IOException("The config file has been processed.");
                }
                Thread.Sleep(1000);
            }
        }

        public BoxAuthInfo GetAuthInfo()
        {
            WaitConfigHandle();
            lock (locker)
            {
                BoxAuthInfo boxInfo;
                using (var fileSteam = configLocationSystem.OpenStream(info, FileMode.Open))
                {
                    boxInfo = xmlHandler.LoadConfig(fileSteam);
                }
                return boxInfo;
            }
        }

        private void UpgradeConfigFile()
        {
            //当邮箱地址存在大写时才升级
            if (!this.emailAddress.Equals(this.originalEmailAddress))
            {
                lock (locker)
                {
                    var newInfo = new StorageInfo { LowName = HashCodeHelper.ToMD5HashCode(this.emailAddress) + ".config" };
                    var originalInfo = new StorageInfo { LowName = HashCodeHelper.ToMD5HashCode(this.originalEmailAddress) + ".config" };
                    this.logger.Info(String.Format("Original Box config file name is {0}.", originalInfo.LowName));
                    if (!configLocationSystem.FileExists(originalInfo))
                    {
                        //如果新旧配置文件找不到, 抛异常
                        this.logger.Warn("Can not found the original config file.");
                        throw new PathNotFoundException(String.Format("Can not find original config file, email address : {0} , config file name : {1} .", this.originalEmailAddress, originalInfo.LowName));
                    }
                    else
                    {
                        //旧的配置文件可以找到, 新的配置文件不存在, 升级
                        this.logger.Info("Try to upgrade config file.");
                        var boxInfo = default(BoxAuthInfo);
                        using (var originalConfigFileSteam = configLocationSystem.OpenStream(originalInfo, FileMode.Open))
                        {
                            boxInfo = xmlHandler.LoadConfig(originalConfigFileSteam);
                        }
                        using (var newConfigFileStream = configLocationSystem.OpenStream(newInfo, FileMode.Create))
                        {
                            xmlHandler.CreateConfig(newConfigFileStream, boxInfo.RefreshToken, boxInfo.AccessToken, DateTime.Now.Ticks.ToString(), boxInfo.ClientSecret, boxInfo.ClientId);
                        }
                        this.logger.Info("Upgrade config file success.");
                        //删除旧的配置文件
                        configLocationSystem.DeleteFile(originalInfo);
                        this.logger.Info("Delete original config file.");
                    }
                }
            }
        }
    }

    class XmlHandler
    {
        public BoxAuthInfo LoadConfig(Stream fileStream)
        {
            var info = new BoxAuthInfo();
            var myXmlDoc = new XmlDocument();
            myXmlDoc.Load(fileStream);
            var rootNode = myXmlDoc.SelectSingleNode("AuthInfo");
            var nodeList = rootNode.ChildNodes;
            foreach (XmlNode node in nodeList)
            {
                XmlAttributeCollection attributeCol = node.Attributes;
                if ("RefreshToken".Equals(node.Name))
                {
                    foreach (XmlAttribute attri in attributeCol)
                    {
                        info.RefreshToken = SecretUtil.DescryptPassword(attri.Value);
                    }
                }
                else if ("UpgradeTime".Equals(node.Name))
                {
                    foreach (XmlAttribute attri in attributeCol)
                    {
                        info.Time = attri.Value;
                    }
                }
                else if ("ClientSecret".Equals(node.Name))
                {
                    foreach (XmlAttribute attri in attributeCol)
                    {
                        info.ClientSecret = SecretUtil.DescryptPassword(attri.Value);
                    }
                }
                else if ("ClientID".Equals(node.Name))
                {
                    foreach (XmlAttribute attri in attributeCol)
                    {
                        info.ClientId = attri.Value;
                    }
                }
                else
                {
                    foreach (XmlAttribute attri in attributeCol)
                    {
                        info.AccessToken = SecretUtil.DescryptPassword(attri.Value);
                    }
                }
            }
            return info;
        }

        public void CreateConfig(Stream fileStream, string refreshToken, string accessToken, string time, string clientSecret, string clientId)
        {
            var myXmlDoc = new XmlDocument();
            var rootElement = myXmlDoc.CreateElement("AuthInfo");
            myXmlDoc.AppendChild(rootElement);
            var refreshTokenElement = myXmlDoc.CreateElement("RefreshToken");
            refreshTokenElement.SetAttribute("Token", SecretUtil.EncryptPassword(refreshToken));
            rootElement.AppendChild(refreshTokenElement);
            var accessTokenElement = myXmlDoc.CreateElement("AccessToken");
            accessTokenElement.SetAttribute("Token", SecretUtil.EncryptPassword(accessToken));
            rootElement.AppendChild(accessTokenElement);
            var upgradeTimeElement = myXmlDoc.CreateElement("UpgradeTime");
            upgradeTimeElement.SetAttribute("Time", time);
            rootElement.AppendChild(upgradeTimeElement);
            var clientSecretElement = myXmlDoc.CreateElement("ClientSecret");
            clientSecretElement.SetAttribute("Secret", SecretUtil.EncryptPassword(clientSecret));
            rootElement.AppendChild(clientSecretElement);
            var clientIdElement = myXmlDoc.CreateElement("ClientID");
            clientIdElement.SetAttribute("ID", clientId);
            rootElement.AppendChild(clientIdElement);
            myXmlDoc.Save(fileStream);
        }
    }
}
