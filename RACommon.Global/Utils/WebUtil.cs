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

using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Xml;

namespace AvePoint.RA.Common.Global.Util
{
    public static class WebUtil
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(WebUtil));

        public const string WEBSITE_REGEDIT_REGISTRYKEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\AvePointRevIM";
        public const string ROOTFOLDER = "Records";
        public const int ListenerPort = 14018;

        /// <summary> Get web install path. </summary>
        /// <returns></returns>
        public static string GetInstallPath()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (path.EndsWith($"bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - 4);
            }
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            return path;
        }      

        public static Configuration GetWebConfiguration()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Path.Combine(GetInstallPath(), "Web.config");
            return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        }

        public static List<string> GetDesignLists()
        {
            List<string> results = new List<string>();
            try
            {
                string configFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Config/DesignLists/DesignLists.config";
                XmlDocument doc = new XmlDocument();
                doc.Load(configFilePath);
                foreach (var node in doc.GetElementsByTagName("List"))
                {
                    XmlElement xe = (XmlElement)node;
                    results.Add(xe.GetAttribute("url") + xe.GetAttribute("serverTemplate"));
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Get Design Lists config file error {0}", ex.ToString());
            }
            return results;
        }

        /// <summary>
        /// for sp make full url
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        public static string MakeFullUrl(string siteUrl, string strUrl)
        {
            if (siteUrl == null || strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }
            if (siteUrl == strUrl)
            {
                return siteUrl;
            }
            if (strUrl.StartsWith("http:")|| strUrl.StartsWith("https:"))
            {
                return strUrl;
            }
            strUrl = strUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (strUrl.StartsWith("/"))
            {
                var siteUri = new Uri(siteUrl);
                var protocol = siteUri.Scheme + ":";
                stringBuilder.Append(protocol);
                stringBuilder.Append("//");
                stringBuilder.Append(siteUri.Host);
                if ((StsCompareStrings(protocol, "http:") && siteUri.Port != 80) || (StsCompareStrings(protocol, "https:") && siteUri.Port != 443))
                {
                    stringBuilder.Append(":");
                    stringBuilder.Append(siteUri.Port);
                }
                stringBuilder.Append(strUrl);
            }
            else
            {
                stringBuilder.Append(siteUrl);
                if (strUrl != "")
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(strUrl);
                }
            }
            if (stringBuilder[stringBuilder.Length - 1] == '/')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        public static string MakeServerRelativeUrl(string strUrl)
        {
            if (strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }

            strUrl = strUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (!strUrl.StartsWith("/"))
            {
                var siteUri = new Uri(strUrl);
                var protocol = siteUri.Scheme + ":";
                stringBuilder.Append(protocol);
                stringBuilder.Append("//");
                stringBuilder.Append(siteUri.Host);
                if ((StsCompareStrings(protocol, "http:") && siteUri.Port != 80) || (StsCompareStrings(protocol, "https:") && siteUri.Port != 443))
                {
                    stringBuilder.Append(":");
                    stringBuilder.Append(siteUri.Port);
                }
                strUrl = strUrl.Replace(stringBuilder.ToString(), "");
            }
            else
            {
                return strUrl;
            }
            return strUrl.ToString();
        }
        public static bool StsCompareStrings(string str1, string str2)
        {
            System.Globalization.CompareInfo compareInfo = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
            return 0 == compareInfo.Compare(str1, str2, System.Globalization.CompareOptions.IgnoreCase);
        }
      
    }

    public enum CheckMode : int
    {
        /// <summary>
        /// 如果在用Dns.GetHostEntry获取AddressList时出错, 会捕获异常并尝试使用Dns.Resolve来获取AddressList. 
        /// 如果仍然出错, 则判定无法找到AddressList, 两个host所对应的机器不相同
        /// 这个模式下, 不会因无法找到AddressList而抛出异常
        /// </summary>
        LooseMode = 0,

        /// <summary>
        /// 只使用微软建议的Dns.GetHostEntry获取AddressList, 
        /// 如果出现异常, 会抛出
        /// </summary>
        StrictMode = 1,

        /// <summary>
        /// 对方法的第一个参数使用LooseMode, 对第二个参数使用StrictMode
        /// </summary>
        FirstParamStrictMode = 2,
    }
   
    public enum FileExtension
    {
        ZIP,
        XLSX,
        CSV
    }

}
