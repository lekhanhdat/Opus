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

using System.Net;
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Core.Common
{
    /// <summary>
    /// Wrapper Config
    /// </summary>
    internal class WrapperConfig
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(WrapperConfig), false);

        private static WrapperConfig instance;
        public static WrapperConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (logger)
                    {
                        instance = new WrapperConfig(Path.Combine(WrapperEnv.RootFolder, Constants.WrapperConfigFileName));
                    }
                }

                return instance;
            }
        }

        public WrapperCommonConfig Common { get; private set; }

        public WrapperRestoreConfig Restore { get; private set; }

        public O365RestoreConfig O365 { get; private set; }

        private WrapperConfig(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            var document = new XmlDocument();
            document.Load(fileName);

            foreach (XmlNode item in document.DocumentElement.ChildNodes)
            {
                if (item.NodeType == XmlNodeType.Element)
                {
                    AnalyzeXmlNode((XmlElement)item);
                }
            }
        }

        private void AnalyzeXmlNode(XmlElement element)
        {
            switch (element.Name)
            {
                case "restore":
                    Restore = new WrapperRestoreConfig(element);
                    break;
                case "common":
                    Common = new WrapperCommonConfig(element);
                    break;
                case "o365":
                    O365 = new O365RestoreConfig(element);
                    break;

            }
        }

        public T GetConfig<T>(string xPath)
        {
            throw new NotImplementedException();
        }
    }

    internal abstract class WrapperBaseConfig
    {
        internal WrapperBaseConfig() { }

        internal WrapperBaseConfig(XmlElement element)
        {
            if (element != null && element.ChildNodes.Count > 0)
            {
                foreach (XmlNode item in element.ChildNodes)
                {
                    if (item.NodeType == XmlNodeType.Element)
                    {
                        AnalyzeXmlNode((XmlElement)item);
                    }
                }
            }
        }

        protected abstract void AnalyzeXmlNode(XmlElement element);

        protected bool GetAttribute(XmlElement element, string name, bool defaultValue)
        {
            var currentValue = element.GetAttribute(name);

            if (!string.IsNullOrEmpty(currentValue))
            {
                return bool.Parse(currentValue);
            }

            return defaultValue;
        }

        protected int GetAttribute(XmlElement element, string name, int defaultValue)
        {
            var currentValue = element.GetAttribute(name);

            if (!string.IsNullOrEmpty(currentValue))
            {
                return int.Parse(currentValue);
            }

            return defaultValue;
        }

        protected string GetAttribute(XmlElement element, string name)
        {
            return element.GetAttribute(name);
        }

        protected List<string> GetArrayList(XmlElement element, string subElementName, string attributeName)
        {
            return (from XmlElement subElement in element.ChildNodes where subElement.Name.Equals(subElementName, StringComparison.OrdinalIgnoreCase) select subElement.GetAttribute(attributeName) into attributeValue where !string.IsNullOrEmpty(attributeValue) select attributeValue).ToList();
        }

        protected List<T> GetArrayList<T>(XmlElement element, string subElementName, string attributeName, Func<string, T> converter)
        {
            var lists = new List<T>();

            foreach (XmlElement subElement in element.ChildElements())
            {
                if (subElement.Name.Equals(subElementName, StringComparison.OrdinalIgnoreCase))
                {
                    var attributeValue = subElement.GetAttribute(attributeName);

                    if (!string.IsNullOrEmpty(attributeValue))
                    {
                        lists.Add(converter(attributeValue));
                    }
                }
            }

            return lists;
        }
    }

    internal class WrapperCommonConfig : WrapperBaseConfig
    {
        public WrapperDefaultProxyConfig DefaultProxy { get; private set; }

        public int DefaultConnectionLimit { get; private set; }

        internal WrapperCommonConfig(XmlElement element)
            : base(element)
        {
            DefaultConnectionLimit = GetAttribute(element, "defaultConnectionLimit", 80);
        }

        protected override void AnalyzeXmlNode(XmlElement element)
        {
            switch (element.Name)
            {
                case "defaultProxy":
                    DefaultProxy = new WrapperDefaultProxyConfig(element);
                    break;
            }
        }
    }

    internal class WrapperDefaultProxyConfig : WrapperBaseConfig
    {
        public bool Enabled { get; private set; }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public string Address { get; private set; }

        public bool BypassProxyOnLocal { get; private set; }

        public string[] BypassList { get; private set; }

        internal WrapperDefaultProxyConfig(XmlElement element)
        {
            Enabled = GetAttribute(element, "enabled", false);
            UserName = GetAttribute(element, "userName");
            Password = GetAttribute(element, "password");
            Address = GetAttribute(element, "address");
            BypassProxyOnLocal = GetAttribute(element, "bypassProxyOnLocal", false);

            var list = GetArrayList(element, "bypassList", "address");

            if (list != null)
            {
                BypassList = list.ToArray();
            }
        }

        protected override void AnalyzeXmlNode(XmlElement element)
        {
            throw new NotImplementedException();
        }

        internal IWebProxy CreateWebProxy()
        {
            WebProxy proxy = null;

            if (Enabled)
            {
                if (string.IsNullOrEmpty(UserName))
                {
                    proxy = new WebProxy(Address, BypassProxyOnLocal, BypassList) { UseDefaultCredentials = true };
                }
                else
                {
                    var index = UserName.IndexOf('/');
                    if (index > 0)
                    {
                        proxy = new WebProxy(Address, BypassProxyOnLocal, BypassList, new NetworkCredential(UserName.Substring(index + 1), Password, UserName.Substring(0, index)));
                    }
                    else
                    {
                        proxy = new WebProxy(Address, BypassProxyOnLocal, BypassList, new NetworkCredential(UserName, Password));
                    }
                }
            }

            return proxy;
        }
    }

    internal class UserProfileRestoreConfig : WrapperBaseConfig
    {
        private List<string> userProfileIgnoredProperties;

        public int MaxUserprofileDetailCount { get; private set; }

        public bool IsUserProfileIgnoredProperty(string name)
        {
            return userProfileIgnoredProperties != null && userProfileIgnoredProperties.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        public UserProfileRestoreConfig(XmlElement element)
        {
            AnalyzeXmlNode(element);
        }

        protected override void AnalyzeXmlNode(XmlElement element)
        {
            MaxUserprofileDetailCount = GetAttribute(element, "maxCacheCount", 10);
            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "userProfileIgnoredProperties":
                        {
                            var text = item.InnerText;

                            if (!string.IsNullOrEmpty(text))
                            {
                                userProfileIgnoredProperties = new List<string>(text.Split(';'));
                            }
                        }
                        break;
                }
            }
        }
    }

    internal class MetadataRestoreConfig : WrapperBaseConfig
    {
        public int MaxCacheMetadataInfoCount { get; private set; }

        public MetadataRestoreConfig(XmlElement element)
        {
            AnalyzeXmlNode(element);
        }

        protected override void AnalyzeXmlNode(XmlElement element)
        {
            MaxCacheMetadataInfoCount = GetAttribute(element, "maxCacheCount", 20);
        }
    }
    internal class WrapperRestoreConfig : WrapperBaseConfig
    {

        public Version DefaultVersion { get; set; }

        public UserProfileRestoreConfig UserProfileRestoreConfig { get; private set; }

        public MetadataRestoreConfig MetadataRestoreConfig { get; private set; }

        internal WrapperRestoreConfig(XmlElement element)
            : base(element)
        {
            DefaultVersion = new Version(GetAttribute(element, "defaultVersion"));
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "userprofile")]
        protected override void AnalyzeXmlNode(XmlElement element)
        {
            switch (element.Name)
            {
                case "metadataService":
                    MetadataRestoreConfig = new Common.MetadataRestoreConfig(element);
                    break;
                case "userprofileService":
                    UserProfileRestoreConfig = new Common.UserProfileRestoreConfig(element);
                    break;
            }
        }
    }

    internal class O365RestoreConfig : WrapperBaseConfig
    {
        public int TimeoutSeconds { get; private set; }

        internal O365RestoreConfig(XmlElement element)
            : base(element)
        {
            TimeoutSeconds = GetAttribute(element, "timeoutSeconds", 27000);
        }

        protected override void AnalyzeXmlNode(XmlElement element)
        {
        }
    }
}
