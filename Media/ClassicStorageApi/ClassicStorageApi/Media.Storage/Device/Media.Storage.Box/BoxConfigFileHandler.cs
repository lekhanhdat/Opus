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

using AvePoint.Media.ClassicStorage.Util;

namespace AvePoint.Media.ClassicStorage.Box
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Xml;
    using AvePoint.Media.ClassicStorage;
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.Contract.Configurations;
    using AvePoint.GCommon.Utility;
    using System.ComponentModel;
    using AvePoint.Media.StorageApi;
    using System.Globalization;
    using AvePoint.GCommon;

    class BoxConfigFileHandler
    {
        private IXSystemCommon configLocationSystem;
        private StorageInfo fileInfo;
        private StorageOpenValidResult validateResult;
        private List<String> orgRefreshTokens = new List<string>();
        private String orgRefreshToken;
        private bool isOldConfig = false;
        private string clientId;
        private AveLogger logger = AveLogger.GetInstance(typeof(BoxConfigFileHandler));
        internal StorageOpenValidResult ValidateResult
        {
            get { return this.validateResult; }
            set { this.validateResult = value; }
        }
        internal BoxConfigFileHandler(String clientId, String emailAddress)
        {
            var setting = ParseStringIntoSettings(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]);
            string xriString=$"docave-xam://azure_vim?accessPoint={setting["DefaultEndpointsProtocol"]}&containerName=opus-online-box-config-location&name={setting["AccountName"]}&secretWithOutEncrypt={setting["AccountKey"].Replace("=", "%3D")}&creation=true";
            XRI xri = XRI.ValueOf(xriString);
            //xri.Params[XRIParameterKeys.ContainerKey.ToLower()] = "RECO-box-config-location";
            this.configLocationSystem = XFactory.InstanceSystem(xri.ToString());
            this.configLocationSystem.Open();
            this.validateResult = configLocationSystem.Validate();
            this.fileInfo = new StorageInfo();
            this.clientId = clientId;
            //if (clientId.Equals("6wlvcp6l8tujowomdwrbjtqlwhdxzqfq", StringComparison.OrdinalIgnoreCase))
            //{
                this.fileInfo.LowName = HashCodeHelper.ToMD5HashCode(emailAddress.ToLowerInvariant()) + ".config";
            //}
            //else
            //{
            //    this.fileInfo.LowName = clientId + ".config";
            //    this.isOldConfig = true;
            //}
        }
        internal BoxConfigFileHandler(String clientId, String emailAddress, String orgRefreshToken)
        {
            var setting = ParseStringIntoSettings(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]);
            string xriString = $"docave-xam://azure_vim?accessPoint={setting["DefaultEndpointsProtocol"]}&containerName=opus-online-box-config-location&name={setting["AccountName"]}&secretWithOutEncrypt={setting["AccountKey"].Replace("=", "%3D")}&creation=true";
            XRI xri = XRI.ValueOf(xriString);
            //xri.Params[XRIParameterKeys.ContainerKey.ToLower()] = "RECO-box-config-location";
            this.configLocationSystem = XFactory.InstanceSystem(xri.ToString());
            this.configLocationSystem.Open();
            this.validateResult = configLocationSystem.Validate();
            this.fileInfo = new StorageInfo();
            this.clientId = clientId;
            //if (clientId.Equals("6wlvcp6l8tujowomdwrbjtqlwhdxzqfq", StringComparison.OrdinalIgnoreCase))
            //{
                this.fileInfo.LowName = HashCodeHelper.ToMD5HashCode(emailAddress.ToLowerInvariant()) + ".config";
            //}
            //else
            //{
            //    this.fileInfo.LowName = clientId + ".config";
            //    this.isOldConfig = true;
            //}
            this.orgRefreshToken = orgRefreshToken;
        }

        internal Boolean ConfigFileExist()
        {
            return this.configLocationSystem.FileExists(fileInfo);
        }

        internal Boolean OriginalTokenExist()
        {
            if (string.IsNullOrEmpty(this.orgRefreshToken) || this.isOldConfig)
            {
                return true;
            }
            GetAuthInfo();
            return this.orgRefreshTokens.Contains(this.orgRefreshToken);
        }

        internal void UpdateEmailAddress(string emailAddress)
        {
            if (this.clientId.Equals("6wlvcp6l8tujowomdwrbjtqlwhdxzqfq", StringComparison.OrdinalIgnoreCase))
            {
                this.fileInfo.LowName = HashCodeHelper.ToMD5HashCode(emailAddress.ToLowerInvariant()) + ".config";
            }
            else
            {
                this.fileInfo.LowName = this.clientId + ".config";
            }
        }

        internal void Close()
        {
            this.configLocationSystem.Close();
        }

        internal void ConfigOrUpdateConfigFile(String refreshToken, String accessToken, String time)
        {
            if (!ConfigFileExist())
            {
                this.orgRefreshTokens.Add(this.orgRefreshToken);
            }
            else
            {
                if (!OriginalTokenExist())
                {
                    this.orgRefreshTokens.Add(this.orgRefreshToken);
                }
            }
            using (var memStream = new MemoryStream())
            {
                this.CreateConfig(memStream, SecretUtil.EncryptPassword(refreshToken), SecretUtil.EncryptPassword(accessToken), time);
                fileInfo.Length = memStream.Length;
                this.configLocationSystem.CommitStream(memStream, fileInfo);
            }
        }

        internal BoxAuthInfo GetAuthInfo()
        {
            var boxAuthInfo = new BoxAuthInfo();
            using (var stream = this.configLocationSystem.OpenStream(this.fileInfo, FileMode.Open))
            {
                boxAuthInfo = this.LoadConfig(stream);
            }
            return boxAuthInfo;
        }

        internal Boolean DeleteConfig()
        {
            Boolean result;
            this.configLocationSystem.DeleteFile(fileInfo);
            if (this.configLocationSystem.FileExists(fileInfo))
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        private BoxAuthInfo LoadConfig(Stream fileStream)
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
                        info.RefreshToken = SecretUtil.Decrypt(attri.Value);
                    }
                }
                else if ("UpgradeTime".Equals(node.Name))
                {
                    foreach (XmlAttribute attri in attributeCol)
                    {
                        info.Time = attri.Value;
                    }
                }
                else if ("OriginalRefreshToken".Equals(node.Name))
                {
                    foreach (XmlAttribute attri in attributeCol)
                    {
                        this.orgRefreshTokens = attri.Value.Split(',').ToList();
                    }
                }
                else
                {
                    foreach (XmlAttribute attri in attributeCol)
                    {
                        info.AccessToken = SecretUtil.Decrypt(attri.Value);
                    }
                }
            }
            return info;
        }

        internal void CreateConfig(Stream fileStream, string refreshToken, string accessToken, string time)
        {
            var myXmlDoc = new XmlDocument();
            var rootElement = myXmlDoc.CreateElement("AuthInfo");
            myXmlDoc.AppendChild(rootElement);
            var refreshTokenElement = myXmlDoc.CreateElement("RefreshToken");
            refreshTokenElement.SetAttribute("Token", refreshToken);
            rootElement.AppendChild(refreshTokenElement);
            var accessTokenElement = myXmlDoc.CreateElement("AccessToken");
            accessTokenElement.SetAttribute("Token", accessToken);
            rootElement.AppendChild(accessTokenElement);
            var upgradeTimeElement = myXmlDoc.CreateElement("UpgradeTime");
            upgradeTimeElement.SetAttribute("Time", time);
            rootElement.AppendChild(upgradeTimeElement);
            if (!this.isOldConfig)
            {
                var originalTokenElement = myXmlDoc.CreateElement("OriginalRefreshToken");
                originalTokenElement.SetAttribute("Token", string.Join(",", this.orgRefreshTokens.ToArray()));
                rootElement.AppendChild(originalTokenElement);
            }
            myXmlDoc.Save(fileStream);
        }
        private IDictionary<string, string> ParseStringIntoSettings(string connectionString)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            string[] array = connectionString.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = array[i].Split(new char[1] { '=' }, 2);
                if (array2.Length != 2)
                {
                    logger.Warn("Settings must be of the form \"name=value\".");
                    return null;
                }

                if (dictionary.ContainsKey(array2[0]))
                {
                    logger.Warn(string.Format(CultureInfo.InvariantCulture, "Duplicate setting '{0}' found.", array2[0]));
                    return null;
                }

                dictionary.Add(array2[0], array2[1]);
            }

            return dictionary;
        }
    }
}
