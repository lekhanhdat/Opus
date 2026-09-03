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
using System.Text;
using System.Globalization;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Cloud;
using System.Collections.Specialized;
using System.Web;

namespace AvePoint.Wrapper.Common
{
    public class AveUrlUtility
    {
        private static readonly string[] s_rgstrAllowedProtocols = new string[] { "http://", "https://", "file://", @"file:\\", "ftp://", "mailto:", "news:", "nntp:", "RTSP://".ToLower(), "TEL:".ToLower(), "pnm://", "mms://" };
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveUrlUtility));

        //this one is thread safe since it's private and no write operations
        private static readonly bool[] s_LegalUrlChars = new bool[] { 
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
            true, true, false, false, true, false, false, true, true, true, false, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, false, true, false, true, false, false, 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, true, true, false, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false, 
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false
         };


        public static string CombineUrl(string baseUrlPath, string additionalNodes)
        {
            if (baseUrlPath == null)
            {
                throw new ArgumentNullException("baseUrlPath");
            }
            if (baseUrlPath.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("baseUrlPath");
            }
            if (additionalNodes == null)
            {
                throw new ArgumentNullException("additionalNodes");
            }
            if (additionalNodes.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("additionalNodes");
            }
            bool flag = baseUrlPath.EndsWith("/", StringComparison.OrdinalIgnoreCase);
            bool flag2 = additionalNodes.StartsWith("/", StringComparison.OrdinalIgnoreCase);
            if (flag && flag2)
            {
                return (baseUrlPath + additionalNodes.Substring(1));
            }
            if ((flag || !flag2) && (!flag || flag2))
            {
                return (baseUrlPath + "/" + additionalNodes);
            }
            return (baseUrlPath + additionalNodes);
        }

        public static int IndexOfIllegalCharInUrlLeafName(string strLeafName)
        {
            if (strLeafName == null)
            {
                throw new ArgumentNullException();
            }
            if (strLeafName.Length > 0)
            {
                if (strLeafName[0] == '.')
                {
                    return 0;
                }
                if (strLeafName[strLeafName.Length - 1] == '.')
                {
                    return (strLeafName.Length - 1);
                }
                char ch = '\0';
                for (int i = 0; i < strLeafName.Length; i++)
                {
                    if (!IsLegalCharInUrl(strLeafName[i], false) || ((strLeafName[i] == '.') && (ch == '.')))
                    {
                        return i;
                    }
                    ch = strLeafName[i];
                }
            }
            return -1;

        }

        internal static bool IsLegalCharInUrl(char ch, bool fAllowSlash)
        {
            bool flag = (ch >= '\x00a0') || s_LegalUrlChars[ch];
            if (!fAllowSlash && (ch == '/'))
            {
                flag = false;
            }
            return flag;
        }

        public static bool IsLegalCharInUrl(char character)
        {
            return IsLegalCharInUrl(character, false);
        }

        public static bool IsLegalFileName(string name)
        {
            return IsLegalFileName(name, false);
        }

        internal static bool IsLegalFileName(string name, bool fAllowSlash)
        {
            int num = -1;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                if (!IsLegalCharInUrl(name[i], fAllowSlash))
                {
                    return false;
                }
                if (name[i] == '.')
                {
                    if ((i == 0) || (i == (name.Length - 1)))
                    {
                        return false;
                    }
                    if ((num != -1) && (num == (i - 1)))
                    {
                        return false;
                    }
                    num = i;
                }
            }
            return true;
        }

        public static bool IsProtocolAllowed(string fullOrRelativeUrl)
        {
            return IsProtocolAllowed(fullOrRelativeUrl, true);
        }

        public static bool IsProtocolAllowed(string fullOrRelativeUrl, bool allowRelativeUrl)
        {
            if ((fullOrRelativeUrl == null) || (fullOrRelativeUrl.Length <= 0))
            {
                return allowRelativeUrl;
            }
            fullOrRelativeUrl = fullOrRelativeUrl.Split(new char[] { '?' })[0];
            if (fullOrRelativeUrl.IndexOf(':') == -1)
            {
                return allowRelativeUrl;
            }
            if (s_rgstrAllowedProtocols != null)
            {
                fullOrRelativeUrl = fullOrRelativeUrl.ToLower(CultureInfo.InvariantCulture).TrimStart(new char[0]);
                foreach (string str in s_rgstrAllowedProtocols)
                {
                    if (fullOrRelativeUrl.StartsWith(str, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsUrlFull(string url)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(url) && (url[0] != '/'))
            {
                try
                {
                    var tempURI = new Uri(url);
                    flag = true;
                }
                catch (UriFormatException e) 
                { 
                    mLogger.Warn($"An error occurred while checking if the url is full,e:{e}.");
                }//need not to log
            }
            return flag;
        }

        public static bool IsUrlRelative(string url)
        {
            Uri uri;
            try
            {
                uri = new Uri(url, UriKind.RelativeOrAbsolute);
            }
            catch (UriFormatException)
            {
                return false;
            }
            return !uri.IsAbsoluteUri;
        }
        /// <summary>
        /// like: /sites/Joey_Group/_layouts/15/groupstatus.aspx?id=40b48de0-b481-49eb-8c00-6e92e9dcb89a&target=notebook
        /// </summary>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="url"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static string ReplaceGroupId(AveSiteInfo sourceSiteInfo, string url, string groupId)
        {
            try
            {
                if (!string.Equals(sourceSiteInfo.WebTemplate, "GROUP#0", StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrEmpty(url)
                    || string.IsNullOrEmpty(groupId))
                {
                    return url;
                }
                if (url.IndexOf("groupstatus.aspx?", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    NameValueCollection paramsFromUrlString = GetParamsFromUrlString(url);
                    var idString = paramsFromUrlString[("id")];
                    if (AveTypeHelper.IsGuid(idString))
                    {
                        return url.Replace(idString, groupId);
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while replacing group id. Url:  {0}, Error: {1}", url, e);
            }
            return url;
        }
        private static NameValueCollection GetParamsFromUrlString(string url)
        {
            NameValueCollection values = new NameValueCollection();
            int index = url.IndexOf('?');
            if (index > -1 && index + 1 < url.Length)
            {
                values = HttpUtility.ParseQueryString(url.Substring(index + 1));
            }
            return values;
        }

        public static bool IsSameTenant(string srcUrl, string desUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(srcUrl) || string.IsNullOrEmpty(desUrl))
                {
                    return false;
                }
                Uri srcUri = new Uri(srcUrl);
                Uri desUri = new Uri(desUrl);
                if (string.Equals(srcUri.Host, desUri.Host))
                {
                    return true;
                }
                return false;
            }
            catch(Exception)
            {
                return false;
            }
        }

        // Properties
        public static string[] AllowedProtocols
        {
            get
            {
                return (s_rgstrAllowedProtocols.Clone() as string[]);
            }
        }

        public static string GetServerUrl(string siteUrl)
        {
            int indexOfSlash = siteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
            string webAppName = siteUrl;
            if (indexOfSlash != -1)
            {
                webAppName = siteUrl.Substring(0, siteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase));
            }
            if (!webAppName.EndsWith("/", StringComparison.Ordinal))
                webAppName = webAppName + "/";
            return webAppName;
        }

        public static string GetServerRelativeUrl(string webUrl)
        {
            string webappname = GetServerUrl(webUrl);
            return (webUrl.TrimEnd('/')).Substring(webappname.TrimEnd('/').Length);
        }

        public static string GetSiteServerRelativeUrl(string siteUrl)
        {
            if (!siteUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !siteUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }
            else
            {
                siteUrl = siteUrl.TrimEnd('/');
                string siteServerRelativeUrl = string.Empty;
                string webAppName = GetServerUrl(siteUrl).TrimEnd('/');
                if (siteUrl.Equals(webAppName))
                {
                    siteServerRelativeUrl = "/";
                }
                else
                {
                    siteServerRelativeUrl = siteUrl.Substring(webAppName.Length);
                }
                return siteServerRelativeUrl;
            }
        }

        public static string GetRelativeUrl(string parentUrl, string childUrl)
        {
            parentUrl = (string.IsNullOrEmpty(parentUrl)||"/".Equals(parentUrl))? "/" : ('/' + parentUrl.Trim('/') + '/');
            childUrl = string.IsNullOrEmpty(childUrl)? "/" : ('/' + childUrl.Trim('/'));
            if (childUrl.StartsWith(parentUrl))
            {
                return childUrl.Substring(parentUrl.Length);
            }
            else
            {
                return childUrl;
            }
        }

        public static string GetParentUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            int index = url.TrimEnd('/').LastIndexOf('/');
            if (index > 0)
            {
                return url.Substring(0, index);
            }
            else
            {
                return null;
            }
        }

        public static bool IsAspx(string relativeUrl, bool allowMasterPage)
        {
            if (relativeUrl == null)
            {
                throw new ArgumentNullException();
            }
            string extension = Path.GetExtension(relativeUrl);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Substring(1);
            }
            if (((extension == null) || (extension.Length == 0)) || (!StsCompareStrings(extension, "aspx") && (!allowMasterPage || (!StsCompareStrings(extension, "master") && !StsCompareStrings(extension, "ascx")))))
            {
                return false;
            }
            return true;
        }

        public static bool StsCompareStrings(string str1, string str2)
        {
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            return (0 == compareInfo.Compare(str1, str2, CompareOptions.IgnoreCase));
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sitequota is a part of url")]
        public static string GetSolutionUrl(string url, Guid id, ViewUrlKind kind)
        {
            var builder = new StringBuilder();
            var siteFactory = AveObjectModelFactory.CreateObjectModelFactory(url, null);
            IAveAdministrationWebApplication adminWebApp = siteFactory.CreateAdministrationWebApplication();
            builder.Append(adminWebApp.Local.GetResponseUri(AveUrlZone.Default).ToString().TrimEnd('/'));
            builder.Append("/_admin/");
            switch (kind)
            {
                case ViewUrlKind.SiteQuotaView:
                    builder.Append("sitequota.aspx?SiteId=");
                    break;
                case ViewUrlKind.DataBaseView:
                    builder.Append("cntdbadm.aspx?WebApplicationId=");
                    break;
                default:
                    break;
            }
            var idstr = id == Guid.Empty ? "" : id.ToString();
            builder.Append(idstr);
            return builder.ToString();
        }

        public static string ReplaceWebApplicationForPRItem(string result, string sourceWebApplicationUrl)
        {
            if (!IsUrlFull(result) || string.IsNullOrEmpty(sourceWebApplicationUrl))
            {
                return result;
            }            
            string oldServerRelativeUrl = GetServerRelativeUrl(result);
            if (sourceWebApplicationUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                sourceWebApplicationUrl = sourceWebApplicationUrl.TrimEnd('/');
            }
            return sourceWebApplicationUrl + oldServerRelativeUrl;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Pattern used in regex.")]
        public static string ReplaceJSUrl(string url)
        {
            string pattern = "\\\\u[0-9A-Fa-f]{4}";
            return Regex.Replace(url, pattern, ReplaceByMatch);
        }
        private static string ReplaceByMatch(Match m)
        {
            string code = m.Groups[0].Value.Trim('\\').TrimStart('u');
            return ((char)(ushort.Parse(code, NumberStyles.HexNumber))).ToString();
        }

        private static string GetSPOAdminUrl(string siteUrl)
        {
            mLogger.Info("start to get admin url by site url {0}", siteUrl);

            Uri siteUri = new Uri(siteUrl);
            int firstDotIndex = siteUri.Host.IndexOf('.');
            string domainPrefix = siteUri.Host.Substring(0, firstDotIndex);
            if (domainPrefix.EndsWith("-my", StringComparison.OrdinalIgnoreCase)
                || domainPrefix.EndsWith("-public", StringComparison.OrdinalIgnoreCase)
                || domainPrefix.EndsWith("-admin", StringComparison.OrdinalIgnoreCase))
            {
                domainPrefix = domainPrefix.Remove(domainPrefix.LastIndexOf('-'));
            }
            string domainSuffix = siteUri.Host.Substring(firstDotIndex, siteUri.Host.Length - firstDotIndex);
            return string.Format("https://{0}-admin{1}", domainPrefix, domainSuffix);
        }

        public static string GetSPOAdminUrlBySiteUrl(AveBPOSAccountInfo account, string siteUrl)
        {
            if (account != null && !string.IsNullOrEmpty(account.AdminUrl))
            {
                mLogger.Info($"GetSPOAdminUrlBySiteUrl.GetByAdminURL:{account.AdminUrl}.");
                return account.AdminUrl;
            }
            else
            {
                mLogger.Info($"GetSPOAdminUrlBySiteUrl.GetBySiteURL:{siteUrl}.");
                return GetSPOAdminUrl(siteUrl);
            }
        }

        public static bool IsTenantAdminSite(string siteUrl)
        {
            Uri siteUri = new Uri(siteUrl);
            string domainPrefix = siteUri.Host.Substring(0, siteUri.Host.IndexOf('.'));
            return domainPrefix.EndsWith("-admin", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSameUrl(string url1, string url2)
        {
            url1 = url1.TrimEnd('/');
            url2 = url2.TrimEnd('/');
            Uri uri1 = null;
            if (Uri.TryCreate(url1, UriKind.RelativeOrAbsolute, out uri1))
            {
                Uri uri2 = null;
                if (Uri.TryCreate(url2, UriKind.RelativeOrAbsolute, out uri2))
                {
                    return uri1 == uri2;
                }
            }
            return false;
        }

        public static bool IsTenantMySite(string siteUrl)
        {
            if (Regex.IsMatch(siteUrl, @"^(https://)[^/]+/personal/.", RegexOptions.IgnoreCase))   //有些客户的onedrive url比较特殊,domain host中并不是以—my结尾的 SAAS-25851
            {
                return true;
            }
            Uri siteUri = new Uri(siteUrl);
            if (siteUri.Host.IndexOf('.') > 0)
            {
                string domainPrefix = siteUri.Host.Substring(0, siteUri.Host.IndexOf('.'));
                return domainPrefix.EndsWith("-my", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        public static bool IsParentUrl(string parentUrl, string childUrl)
        {
            if (string.IsNullOrEmpty(parentUrl)
                || string.IsNullOrEmpty(childUrl))
            {
                return false;
            }

            parentUrl = parentUrl.TrimEnd('/');
            childUrl = childUrl.TrimEnd('/');

            if (childUrl.StartsWith(parentUrl))
            {
                childUrl = childUrl.Substring(0, childUrl.LastIndexOf('/'));
                return parentUrl.Equals(childUrl);
            }
            return false;
        }

        public static string GetSiteUrl(string fullUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fullUrl))
                {
                    return "";
                }
                if (Uri.TryCreate(fullUrl, UriKind.Absolute, out Uri result))
                {
                    var rootUrl = result.GetLeftPart(UriPartial.Authority);
                    var absolutePath = result.AbsolutePath;

                    if (absolutePath.StartsWith("/sites/", StringComparison.OrdinalIgnoreCase) || absolutePath.StartsWith("/teams/", StringComparison.OrdinalIgnoreCase)
                        || absolutePath.StartsWith("/personal/", StringComparison.OrdinalIgnoreCase) || absolutePath.StartsWith("/portals/", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = absolutePath.Split('/');
                        return rootUrl + "/" + path[1] + "/" + path[2];
                    }
                    else if (absolutePath.StartsWith("/search/", StringComparison.OrdinalIgnoreCase) || absolutePath.Equals("/search", StringComparison.OrdinalIgnoreCase))
                    {
                        return rootUrl + "/search";
                    }
                    return rootUrl;
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn($@"fail get site url,ex:{ex}");
            }
            return "";
        }
    }
}
