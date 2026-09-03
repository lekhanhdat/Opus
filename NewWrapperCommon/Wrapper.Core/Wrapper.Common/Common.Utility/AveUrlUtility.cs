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
using System.Reflection;
using AvePoint.Wrapper.Common.Common.Utility;
using System.Collections.Specialized;
using System.Web;

[module: SuppressMessage("CheckHardCode", "Z100009:CheckString", Scope = "member", Target = "AvePoint.Wrapper.Common.AveUrlUtility.#.cctor()")]
namespace AvePoint.Wrapper.Common
{
    public class AveUrlUtility
    {
        private static readonly string[] s_rgstrAllowedProtocols = new string[] { "http://", "https://", "file://", @"file:\\", "ftp://", "mailto:", "news:", "nntp:", "rtsp://", "tel:", "pnm://", "mms://" };

        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

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
            if (additionalNodes.Length < 0)
            {
                throw new ArgumentOutOfRangeException("additionalNodes");
            }
            if (additionalNodes.Length == 0)
            {
                return baseUrlPath;
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
        public static string GetSPOAdminUrlBySiteUrl(AveBPOSAccountInfo account, string siteUrl)
        {
            if (account != null && !string.IsNullOrEmpty(account.AdminUrl))
            {
                logger.Info("current user name:{0}, admin url:{1}", account.UserName, account.AdminUrl);
                return account.AdminUrl;
            }
            else
            {
                return GetSPOAdminUrl(siteUrl);
            }
        }
        private static string GetSPOAdminUrl(string siteUrl)
        {
            logger.Info("start to get admin url by site url {0}", siteUrl);

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
                    new Uri(url);
                    flag = true;
                }
                catch (UriFormatException) { }//need not to log
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
            if (string.IsNullOrEmpty(siteUrl) || !siteUrl.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                return siteUrl;
            }
            else
            {
                return new Uri(siteUrl).GetLeftPart(UriPartial.Authority);
            }
        }

        public static string GetRelativeUrl(string parentUrl, string childUrl)
        {
            var parentUrlTemp = (string.IsNullOrEmpty(parentUrl) || "/".Equals(parentUrl)) ? "/" : ('/' + parentUrl.Trim('/') + '/');
            var childUrlTemp = string.IsNullOrEmpty(childUrl) ? "/" : ('/' + childUrl.Trim('/'));
            if (childUrlTemp.StartsWith(parentUrlTemp, StringComparison.OrdinalIgnoreCase))
            {
                return childUrlTemp.Substring(parentUrlTemp.Length);
            }
            return childUrlTemp;
        }

        public static string GetServerRelativeUrl(string webUrl)
        {
            if (string.IsNullOrEmpty(webUrl) || !webUrl.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                return webUrl;
            }
            else
            {
                if (webUrl.Contains("+"))//ADO-110767, UrlDecode will change '+' to space.
                {
                    webUrl = webUrl.Replace("+", "%2b");
                }
                return System.Web.HttpUtility.UrlDecode(new Uri(webUrl).PathAndQuery);
            }
        }

        public static string GetLocalPath(string webUrl)
        {
            if (string.IsNullOrEmpty(webUrl) || !webUrl.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                return webUrl;
            }
            else
            {
                return new Uri(webUrl).LocalPath;
            }

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

        public static bool IsTenantMySite(string siteUrl)
        {
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

        public static void SplitUrl(string fullOrRelativeUri, out string dirName, out string leafName)
        {
            if (fullOrRelativeUri == null)
            {
                dirName = string.Empty;
                leafName = string.Empty;
            }
            else
            {
                if ((fullOrRelativeUri.Length > 0) && ('/' == fullOrRelativeUri[0]))
                {
                    fullOrRelativeUri = fullOrRelativeUri.Substring(1);
                }
                int length = fullOrRelativeUri.LastIndexOf('/');
                if (-1 == length)
                {
                    dirName = string.Empty;
                    if (fullOrRelativeUri.Length > 0)
                    {
                        leafName = ('/' == fullOrRelativeUri[0]) ? fullOrRelativeUri.Substring(1) : fullOrRelativeUri;
                    }
                    else
                    {
                        leafName = string.Empty;
                    }
                }
                else
                {
                    dirName = fullOrRelativeUri.Substring(0, length);
                    leafName = fullOrRelativeUri.Substring(length + 1);
                }
            }
        }

        /// <summary>
        /// 通过 url截取出leafname和Dirname
        /// </summary>
        /// <param name="fullUrl">List Releated Url</param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        public static void GetDirNameAndLeafName(string fullUrl, out string dirName, out string leafName)
        {
            var index = fullUrl.LastIndexOf('/');
            if (index >= 0)
            {
                dirName = fullUrl.Substring(0, index);
                leafName = fullUrl.Substring(index + 1);
            }
            else
            {
                dirName = "";
                leafName = fullUrl;
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

        /// <summary>
        /// 检查一个site collection url在webApp中是否是可以利用的，通过分析webApp可利用的managedPath
        /// </summary>
        public static bool CheckManagedPath(IAveWebApplication webApp, string siteUrl, bool isHostHeader)
        {
            return CheckManagedPath(webApp, null, siteUrl, isHostHeader);
        }

        public static bool CheckManagedPath(IAveWebApplication webApp, AveObjectModelFactory factory, string siteUrl, bool isHostHeader)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSite.CheckManagedPath"))
            {

                bool result = false;
                try
                {
                    siteUrl = siteUrl.TrimEnd('/');
                    if (isHostHeader)
                    {
                        result = true;
                        if (factory != null)
                        {
                            result = CheckHostHeaderManagedPath(siteUrl, factory);
                        }
                    }
                    else
                    {
                        string webAppUrl = webApp.GetResponseUri(AveUrlZone.Default).AbsoluteUri.ToString();
                        foreach (IAvePrefix prefix in webApp.Prefixes)
                        {
                            SetResultValueByEachPrefix(prefix, webAppUrl, siteUrl, ref result);
                            if (result)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.ToString());
                }
                return result;

            }

        }

        //these methods copy from SP
        private static bool CheckHostHeaderManagedPath(string siteUrl, AveObjectModelFactory factory)
        {
            string absolutePath = AveHttpUtility.UrlPathDecode(new Uri(siteUrl).AbsolutePath, false);
            string result = FindSiteRoot(absolutePath, factory.CreateFarm().Local.Servers.GetValue<IAveWebService>());
            if (result != null)
            {
                if (absolutePath.Trim(new char[] { '/' }).Equals(result.Trim(new char[] { '/' })))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool StsStartsWith(string strMain, string strBegining)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(strMain, strBegining, CompareOptions.IgnoreCase);
        }

        private static IAvePrefix[] GetStortedHostHeaderProfixes(IAvePrefixCollection hostHeaderPrefixes)
        {
            int count = hostHeaderPrefixes.Count;
            SortedList<string, IAvePrefix> list = new SortedList<string, IAvePrefix>(count, new AveProfixComparer());
            IAvePrefix[] array = new IAvePrefix[count];
            foreach (IAvePrefix prefix in hostHeaderPrefixes)
            {
                list.Add(prefix.Name, prefix);
            }
            list.Values.CopyTo(array, 0);
            return array;
        }

        private static string FindSiteRoot(string serverRelativeRequestPath, IAveWebService webService)
        {
            string str = null;
            string strMain = serverRelativeRequestPath.TrimStart(new char[] { '/' });
            var storedPrefixes = GetStortedHostHeaderProfixes(webService.HostHeaderPrefixes);
            foreach (var prefix in storedPrefixes)
            {
                int length;
                int num2;
                if (StsStartsWith(strMain, prefix.Name))
                {
                    length = prefix.Name.Length;
                    num2 = length + 1;
                    switch (prefix.PrefixType)
                    {
                        case AvePrefixType.ExplicitInclusion:
                            if ((serverRelativeRequestPath.Length < num2 + 1 || serverRelativeRequestPath[num2] != '/') && length != 0)
                            {
                                strMain = strMain.TrimEnd(new char[] { '/' });
                                if (!string.Equals(strMain, prefix.Name, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    break;
                                }
                            }
                            if (serverRelativeRequestPath.Length > length)
                            {
                                str = serverRelativeRequestPath.Substring(0, length + 1).Trim();
                                if (str.Length > 1) str = str.TrimEnd(new char[] { '/' });
                            }
                            return str;

                        case AvePrefixType.WildcardInclusion:
                            if (serverRelativeRequestPath.Length > num2 + 1 && serverRelativeRequestPath[num2] == '/' || string.IsNullOrEmpty(prefix.Name) && serverRelativeRequestPath.Length != 1)
                            {
                                int index = serverRelativeRequestPath.IndexOf('/', num2 + 1);
                                if (index < 0) return serverRelativeRequestPath;
                                return serverRelativeRequestPath.Substring(0, index);
                            }
                            break;
                        case AvePrefixType.Exclusion:
                            break;
                    }
                }
            }
            return null;
        }

        public static string GetMySiteUrl(AveObjectModelFactory objectModel, AveBPOSAccountInfo accountInfo)
        {
            using (var azureShell = objectModel.CreateAzurePowerShellRequest(accountInfo))
            {
                var domain = azureShell.GetOffice365Domain();
                string mySiteHostUrl = "https://{0}-my.sharepoint.com";
                if (domain.EndsWith(".partner.onmschina.cn", StringComparison.OrdinalIgnoreCase))
                {
                    mySiteHostUrl = "https://{0}-my.sharepoint.cn";
                }
                else if (domain.EndsWith(".onmicrosoft.de", StringComparison.OrdinalIgnoreCase))
                {
                    mySiteHostUrl = "https://{0}-my.sharepoint.de";
                }
                else if (domain.EndsWith(".onmicrosoft.us", StringComparison.OrdinalIgnoreCase))
                {
                    mySiteHostUrl = "https://{0}-my.sharepoint.us";
                }
                return string.IsNullOrEmpty(domain) ? string.Empty : string.Format(mySiteHostUrl, domain.Substring(0, domain.IndexOf('.')));
            }
        }

        public static string GetTenantAdminSiteUrl(IAveAzurePowerShellRequest azureShell)
        {
            return azureShell.GetOffice365AdminSiteCollectionUrl();
        }

        public static string GetTenantAdminSiteUrl(AveObjectModelFactory objectModel, AveBPOSAccountInfo accountInfo)
        {
            using (var azureShell = objectModel.CreateAzurePowerShellRequest(accountInfo))
            {
                return GetTenantAdminSiteUrl(azureShell);
            }
        }

        [Obsolete("Don't use , url can end with .cn")]
        public static string GetTenantAdminSiteUrl(string siteUrl)
        {
            Uri siteUri = new Uri(siteUrl);
            string domainPrefix = siteUri.Host.Substring(0, siteUri.Host.IndexOf('.'));
            string suffix = ".com";
            if (siteUri.Host.EndsWith(".cn", StringComparison.OrdinalIgnoreCase))
            {
                suffix = ".cn";
            }
            else if (siteUri.Host.EndsWith(".us", StringComparison.OrdinalIgnoreCase))
            {
                suffix = ".us";
            }
            else if (siteUri.Host.EndsWith(".de", StringComparison.OrdinalIgnoreCase))
            {
                suffix = ".de";
            }

            if (domainPrefix.EndsWith("-my", StringComparison.OrdinalIgnoreCase))
            {
                domainPrefix = domainPrefix.Remove(domainPrefix.LastIndexOf('-'));
            }
            else if (domainPrefix.EndsWith("-public", StringComparison.OrdinalIgnoreCase))
            {
                domainPrefix = domainPrefix.Remove(domainPrefix.LastIndexOf('-'));
            }
            else if (domainPrefix.EndsWith("-admin", StringComparison.OrdinalIgnoreCase))
            {
                domainPrefix = domainPrefix.Remove(domainPrefix.LastIndexOf('-'));
            }
            return string.Format("https://{0}-admin.sharepoint{1}", domainPrefix, suffix);
        }
        public static bool IsDurableLink(string url, out Guid sourceItemId)
        {
            return IsNormalDurableLink(url, out sourceItemId) || IsWopiFrameDurableLink(url, out sourceItemId);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "WopiFrame and sourcedoc is a part of url")]
        /// <summary>
        /// like: http://jackie2016:100/sites/Test1/_layouts/15/WopiFrame.aspx?sourcedoc={DC26E9EA-5E0A-41C4-95F5-4F7AFEF30CAD}&file={AA}.docx&action=default
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sourceItemId"></param>
        /// <returns></returns>
        private static bool IsWopiFrameDurableLink(string url, out Guid sourceItemId)
        {
            sourceItemId = Guid.Empty;
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            try
            {
                if (url.IndexOf("WopiFrame.aspx?", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    NameValueCollection paramsFromUrlString = GetParamsFromUrlString(url);
                    string idString = paramsFromUrlString["sourcedoc"];
                    if (!string.IsNullOrEmpty(idString))
                    {
                        sourceItemId = new Guid(idString);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while judging the url if durable link. Url:  {0}, Error: {1}", url, e);
            }
            return false;
        }

        /// <summary>
        /// Like : http://jackie2016:100/sites/Test1/Shared%20Documents/%7BAA%7D.docx?d=wdc26e9ea5e0a41c495f54f7afef30cad
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sourceItemId"></param>
        /// <returns></returns>
        private static bool IsNormalDurableLink(string url, out Guid sourceItemId)
        {
            sourceItemId = Guid.Empty;
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            try
            {
                NameValueCollection paramsFromUrlString = GetParamsFromUrlString(url);
                string str = paramsFromUrlString["d"];
                if (!string.IsNullOrEmpty(str) && str.Length == 0x21 && str.Substring(0, 1).ToLowerInvariant() == "w")
                {
                    sourceItemId = new Guid(str.Substring(1));
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while judging the url if durable link. Url:  {0}, Error: {1}", url, e);
            }
            return false;
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

        private static void SetResultValueByEachPrefix(IAvePrefix prefix, string webAppUrl, string siteUrl, ref bool result)
        {
            string avaliableManagedPathUrl = string.IsNullOrEmpty(prefix.Name) ? webAppUrl.TrimEnd('/') : webAppUrl.TrimEnd('/') + "/" + prefix.Name;
            if (prefix.PrefixType == AvePrefixType.ExplicitInclusion)
            {
                if (string.Compare(avaliableManagedPathUrl, siteUrl, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = true;
                    return;
                }
            }
            if (prefix.PrefixType == AvePrefixType.WildcardInclusion)
            {
                if (siteUrl.StartsWith(avaliableManagedPathUrl, StringComparison.OrdinalIgnoreCase) &&
                    !siteUrl.Equals(avaliableManagedPathUrl, StringComparison.OrdinalIgnoreCase))
                {
                    string siteRelatedUrl = siteUrl.Remove(0, avaliableManagedPathUrl.Length + 1);
                    if (siteRelatedUrl.IndexOf('/') == -1)
                    {
                        result = true;
                        return;
                    }
                }
            }
        }
    }
}
