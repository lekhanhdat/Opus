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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HSMAzureCommon
{
    public class WinAzure
    {
        public Boolean AzureIused;
        public string AzureContainerSourceUri;
        public string AzureContainerManifestUri;
        public string AzureQueueReportUri;
        public string AzureSourceContainerName;
        public string AzureManifestContainerName;
        public string AzureQueueReportContainerName;
        public string AccountName;
        public string EndPointSuffixm;
        public string AccountKey;
        public string AccessPoint;

        public WinAzure() { }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special word are included")]
        public WinAzure(string AzureInfo)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(AzureInfo);
            XmlNode cache = doc.FirstChild;
            foreach (XmlNode node in cache.ChildNodes)
            {
                if (node is XmlElement)
                {
                    XmlElement ele = (XmlElement)node;
                    if (ele.Name.Equals("AzureIused") && ele.HasAttribute("Values")) { AzureIused = Boolean.Parse(ele.Attributes["Values"].Value); }
                    else if (ele.Name.Equals("AzureContainerSourceUri") && ele.HasAttribute("Values")) { AzureContainerSourceUri = ele.Attributes["Values"].Value; }
                    else if (ele.Name.Equals("AzureContainerManifestUri") && ele.HasAttribute("Values")) { AzureContainerManifestUri = ele.Attributes["Values"].Value; }
                    else if (ele.Name.Equals("AzureQueueReportUri") && ele.HasAttribute("Values")) { AzureQueueReportUri = ele.Attributes["Values"].Value; }
                    else if (ele.Name.Equals("AzureSourceContainerName") && ele.HasAttribute("Values")) { AzureSourceContainerName = ele.Attributes["Values"].Value; }
                    else if (ele.Name.Equals("AzureManifestContainerName") && ele.HasAttribute("Values")) { AzureManifestContainerName = ele.Attributes["Values"].Value; }
                    else if (ele.Name.Equals("AzureQueueReportContainerName") && ele.HasAttribute("Values")) { AzureQueueReportContainerName = ele.Attributes["Values"].Value; }

                    else if (ele.Name.Equals("AccountName") && ele.HasAttribute("Values")) { AccountName = ele.Attributes["Values"].Value; }
                    else if (ele.Name.Equals("EndPointSuffixm") && ele.HasAttribute("Values")) { EndPointSuffixm = ele.Attributes["Values"].Value; }
                    else if (ele.Name.Equals("AccountKey") && ele.HasAttribute("Values")) { AccountKey = ele.Attributes["Values"].Value; }
                    else if (ele.Name.Equals("AccessPoint") && ele.HasAttribute("Values")) { AccessPoint = ele.Attributes["Values"].Value; }

                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special word are included")]
        public override string ToString()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<Node></Node>");
            XmlNode cache = doc.FirstChild;
            XmlNode AzureUsed = doc.CreateElement("AzureIused");
            XmlAttribute att = doc.CreateAttribute("Values");
            att.Value = AzureIused.ToString();
            AzureUsed.Attributes.Append(att);
            cache.AppendChild(AzureUsed);

            XmlNode nodeAzureContainerSourceUri = doc.CreateElement("AzureContainerSourceUri");
            XmlAttribute attAzureContainerSourceUri = doc.CreateAttribute("Values");
            attAzureContainerSourceUri.Value = AzureContainerSourceUri;
            nodeAzureContainerSourceUri.Attributes.Append(attAzureContainerSourceUri);
            cache.AppendChild(nodeAzureContainerSourceUri);

            XmlNode nodeAccountName = doc.CreateElement("AccountName");
            XmlAttribute attAzureAccountName = doc.CreateAttribute("Values");
            attAzureAccountName.Value = AccountName;
            nodeAccountName.Attributes.Append(attAzureAccountName);
            cache.AppendChild(nodeAccountName);

            XmlNode nodeEndPointSuffixm = doc.CreateElement("EndPointSuffixm");
            XmlAttribute attAzureEndPointSuffixm = doc.CreateAttribute("Values");
            attAzureEndPointSuffixm.Value = EndPointSuffixm;
            nodeEndPointSuffixm.Attributes.Append(attAzureEndPointSuffixm);
            cache.AppendChild(nodeEndPointSuffixm);

            XmlNode nodeAccountKey = doc.CreateElement("AccountKey");
            XmlAttribute attAzureAccountKey = doc.CreateAttribute("Values");
            attAzureAccountKey.Value = AccountKey;
            nodeAccountKey.Attributes.Append(attAzureAccountKey);
            cache.AppendChild(nodeAccountKey);


            XmlNode nodeAccessPoint = doc.CreateElement("AccessPoint");
            XmlAttribute attAzureAccessPoint = doc.CreateAttribute("Values");
            attAzureAccessPoint.Value = AccessPoint;
            nodeAccessPoint.Attributes.Append(attAzureAccessPoint);
            cache.AppendChild(nodeAccessPoint);

            XmlNode nodeAzureContainerManifestUri = doc.CreateElement("AzureContainerManifestUri");
            XmlAttribute attAzureContainerManifestUri = doc.CreateAttribute("Values");
            attAzureContainerManifestUri.Value = AzureContainerManifestUri;
            nodeAzureContainerManifestUri.Attributes.Append(attAzureContainerManifestUri);
            cache.AppendChild(nodeAzureContainerManifestUri);

            XmlNode nodeAzureQueueReportUri = doc.CreateElement("AzureQueueReportUri");
            XmlAttribute attAzureQueueReportUri = doc.CreateAttribute("Values");
            attAzureQueueReportUri.Value = AzureQueueReportUri;
            nodeAzureQueueReportUri.Attributes.Append(attAzureQueueReportUri);
            cache.AppendChild(nodeAzureQueueReportUri);

            XmlNode nodeAzureSourceContainerName = doc.CreateElement("AzureSourceContainerName");
            XmlAttribute attAzureSourceContainerName = doc.CreateAttribute("Values");
            attAzureSourceContainerName.Value = AzureSourceContainerName;
            nodeAzureSourceContainerName.Attributes.Append(attAzureSourceContainerName);
            cache.AppendChild(nodeAzureSourceContainerName);


            XmlNode nodeAzureManifestContainerName = doc.CreateElement("AzureManifestContainerName");
            XmlAttribute attAzureManifestContainerName = doc.CreateAttribute("Values");
            attAzureManifestContainerName.Value = AzureManifestContainerName;
            nodeAzureManifestContainerName.Attributes.Append(attAzureManifestContainerName);
            cache.AppendChild(nodeAzureManifestContainerName);

            XmlNode nodeAzureQueueReportContainerName = doc.CreateElement("AzureQueueReportContainerName");
            XmlAttribute attAzureQueueReportContainerName = doc.CreateAttribute("Values");
            attAzureQueueReportContainerName.Value = AzureQueueReportContainerName;
            nodeAzureQueueReportContainerName.Attributes.Append(attAzureQueueReportContainerName);
            cache.AppendChild(nodeAzureQueueReportContainerName);

            return doc.OuterXml;
        }

        public static WinAzure Clone(AzureResult result)
        {
            return new WinAzure()
            {
                AzureIused = result.AzureIused,
                AzureContainerManifestUri = result.AzureContainerManifestUri,
                AzureContainerSourceUri = result.AzureContainerSourceUri,
                AzureQueueReportUri = result.AzureQueueReportUri,
                AzureManifestContainerName = result.AzureManifestContainerName,
                AzureQueueReportContainerName = result.AzureQueueReportContainerName,
                AzureSourceContainerName = result.AzureSourceContainerName
            };
        }
    }

    public class ObjectSetting
    {
        public string sourcweburl = string.Empty;
        public string destweburl = string.Empty;
        public string sourcelisttitle = string.Empty;
        public string destlisttitle = string.Empty;
    }

    public static class StringExtension
    {
        public static Guid ToHashGuid(this string value)
        {
            if (value.IsNullOrEmpty())
            {
                throw new Exception("Null Or Empty");
            }
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            var hash = AvePoint.GCommon.Utility.Cryptography.HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.MD5);
            byte[] hashBytes = hash.ComputeHash(bytes);
            return new Guid(hashBytes);
        }

        public static String GetParentUrl(this String serverRelativeUrl)
        {
            if (serverRelativeUrl.IsNullOrEmpty())
            {
                return serverRelativeUrl;
            }
            Int32 lastIndex = serverRelativeUrl.LastIndexOf('/');
            if (lastIndex > 0)
            {
                return serverRelativeUrl.Substring(0, lastIndex);
            }
            return serverRelativeUrl;
        }

        public static int LastIdxOf(this string str, string value)
        {
            return str.LastIndexOf(value, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetListUrl(this string str)
        {
            string value = "";
            if (str.Contains("/"))
            {
                value = str.Substring(str.LastIdxOf("/"));
            }
            return value;
        }
    }
    public enum ContainerType
    {
        None = 0,
        SourceUri,
        MainFestUri
    }

    public enum DownloadFileType
    {
        None = 0,
        XML,
        Logs,
        Warn,
        Err
    }
}
