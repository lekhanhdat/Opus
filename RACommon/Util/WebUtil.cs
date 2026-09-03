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
using System.Linq;
using System.Net;
using AvePoint.RA.CommonUtil;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using System.Text;
using Newtonsoft.Json.Linq;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Contract.Tenant;
using System.Security.Claims;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using System.Collections.Generic;
using System.Net.Sockets;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.Wrapper.Common;
using AvePoint.RA.Common;
using System.Web;
using Microsoft365.Common.HttpUtil;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Aos;
using System.Text.RegularExpressions;

namespace AvePoint.RA.Common.Util
{
    public static class WebUtil
    {
        private readonly static RALogger logger = RALogger.GetInstance(typeof(WebUtil));
        public const string WEBSITE_REGEDIT_REGISTRYKEY = @"SOFTWARE/Microsoft/Windows/CurrentVersion/Uninstall/AvePointRevIM";
        public const string ROOTFOLDER = "Records";
        public const int ListenerPort = 14018;

        private static Regex TenantNameRegex = new Regex(@"https://([^/]+?)(-my|-admin)?\.sharepoint.*", RegexOptions.IgnoreCase);

        #region Cookie Operation

        //public static void SetCookie(string key, string value)
        //{
        //    try
        //    {
        //        if (CheckCookieKey(key))
        //        {
        //            HttpContext.Current.Response.Cookies[key].Value = value;
        //        }
        //        else
        //        {
        //            var co = new HttpCookie(key, value);
        //            HttpContext.Current.Response.Cookies.Add(co);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        //no need
        //    }
        //}

        //public static string GetCookieValue(string key)
        //{
        //    var value = string.Empty;
        //    try
        //    {
        //        if (CheckCookieKey(key))
        //        {
        //            value = HttpContext.Current.Request.Cookies[key].Value;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn($"An error has occurred when GetCookieValue, message:{e.Message}");
        //    }
        //    return value;
        //}

        //public static bool CheckCookieKey(string key)
        //{
        //    var isExist = false;
        //    try
        //    {
        //        if (HttpContext.Current.Response.Cookies.AllKeys.Contains(key))
        //        {
        //            isExist = true;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn($"An error has occurred when CheckCookieKey, message:{e.Message}");
        //    }
        //    return isExist;
        //}

        #endregion Cookie Operation
        public static string LogOnUserName
        {
            get
            {
                return TenantLocalValue.LogonUserEmail;
                //if (!string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail))
                //{
                //    return TenantLocalValue.LogonUserEmail;
                //}
                //if (HttpContext.Current != null && HttpContext.Current.User != null)
                //{
                //    var userName = HttpContext.Current.User.Identity.Name;
                //    if (string.IsNullOrEmpty(userName))
                //    {
                //        logger.Debug("Current logon user is empty");
                //        return "DocAve System";
                //    }
                //    else
                //    {
                //        return userName;
                //    }
                //}
                //else if (Thread.CurrentPrincipal != null)
                //{
                //    var userName = Thread.CurrentPrincipal.Identity.Name;
                //    return userName;
                //}
                //else
                //{
                //    logger.Debug("Current context is empty");
                //    return "RA System";
                //}
            }
        }

        public static string DataCenter
        {
            get
            {
                return DataCenterManagent.GetDataCenter(TenantId);
            }
        }

        public static string LogonUserId
        {
            get
            {
                if(!string.IsNullOrEmpty(TenantLocalValue.LogonUserId))
                {
                    return TenantLocalValue.LogonUserId;
                }
                return null;
            }
        }

        public static string LogonGroupId
        {
            get
            {
                if(string.IsNullOrEmpty(TenantLocalValue.LogonGroupId))
                {
                    return null;
                }

                return TenantLocalValue.LogonGroupId;
            }
        }

        public static string Company
        {
            get
            {
                if (string.IsNullOrEmpty(TenantLocalValue.Company))
                {
                    return null;
                }

                return TenantLocalValue.Company;
            }
        }

        public static string AccountNumber
        {
            get
            {
                if (string.IsNullOrEmpty(TenantLocalValue.AccountNumber))
                {
                    return null;
                }

                return TenantLocalValue.AccountNumber;
            }
        }

        public static string LogonUserDisplayName
        {
            get
            {
                string userName = string.Empty;
                if (!string.IsNullOrEmpty(TenantLocalValue.DisplayName))
                {
                    userName = TenantLocalValue.DisplayName;

                }
                if (string.IsNullOrEmpty(userName.Trim()) && !string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail))
                {
                    var email = TenantLocalValue.LogonUserEmail;
                    userName = email.Substring(0, email.IndexOf("@"));
                }
                return HttpUtility.HtmlEncode(userName);
            }
        }

        public static string TenantId
        {
            get
            {
                //if (TenantLocalValue.LogonGroupId == null)
                //{
                //    if (HttpContext.Current != null && HttpContext.Current.User != null)
                //    {
                //        var ci = HttpContext.Current.User.Identity as ClaimsIdentity;
                //        var tClaim = ci.Claims.Where(c => c.Type == RMClaimTypes.TenantGroupId).FirstOrDefault();
                //        TenantLocalValue.LogonGroupId = tClaim == null ? null : tClaim.Value;

                //    }
                //}
                return TenantLocalValue.LogonGroupId;
            }
        }
        public static string RegisterEMail
        {
            get
            {
                //if (TenantLocalValue.LogonUserEmail == null)
                //{
                //    if (HttpContext.Current != null && HttpContext.Current.User != null)
                //    {
                //        var ci = HttpContext.Current.User.Identity as ClaimsIdentity;

                //        var eClaim = ci.Claims.Where(c => c.Type == RMClaimTypes.RegisterEmail).FirstOrDefault();
                //        TenantLocalValue.LogonUserEmail = eClaim == null ? null : eClaim.Value;

                //    }
                //}
                return TenantLocalValue.LogonUserEmail;
            }
        }

        public static string GetIPAddress()
        {
            string host = Dns.GetHostName();
            var ipList = GetIPAddressWithHandleException(host);
            for (int i = 0; i < ipList.Length; i++)
            {
                //从IP地址列表中筛选出IPv4类型的IP地址
                //AddressFamily.InterNetwork表示此IP为IPv4,
                //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                if (ipList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipList[i].ToString();
                }
            }
            return string.Empty;
        }

        public static string GetRecordsHomePageUrl()
        {
            ThrowUtil.ThrowIfNullOrEmpty(AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "forwardto/target?product=ReportCenter"), "redirect home url empty");
            var redirectUrl = AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "forwardto/target?product=ReportCenter").Substring(0, AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "forwardto/target?product=ReportCenter").LastIndexOf("=") + 1) + RecordsConstants.RECORDS_APPLICATION_NAME;
            return redirectUrl;
        }

        public static string GetAOSHomePageUrl()
        {
            ThrowUtil.ThrowIfNullOrEmpty(AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "account/LogOff"), "redirect aso url empty");
            var redirectUrl = AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "account/LogOff").Substring(0, AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "account/LogOff").LastIndexOf('/') + 1) + "login";
            return redirectUrl;
        }
        public static string GetRedirectRecodsSSOLoginUrl()
        {
            ThrowUtil.ThrowIfNullOrEmpty(RMGlobalConfiguration.AppConfig[RMAppSettingKey.RECO_SSO_LOGIN_URL], "redirect sso url empty");
            var redirectUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.RECO_SSO_LOGIN_URL];
            return redirectUrl;
        }

        public static string GetRedirectRecodsSSOLoginUrlForRelateApp()
        {
            ThrowUtil.ThrowIfNullOrEmpty(RMGlobalConfiguration.AppConfig[RMAppSettingKey.RECO_DOMAIN_URL], "redirect RECO_DOMAIN_URL empty");
            var redirectUrl = $"{RMGlobalConfiguration.AppConfig[RMAppSettingKey.RECO_DOMAIN_URL]}/sso";
            return redirectUrl;
        }

        private static IPAddress[] GetIPAddress(string host)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(host, out ip))
            {
                return Dns.GetHostEntry(host).AddressList;
            }
            else
            {
                return new IPAddress[] { ip };
            }
        }

        private static IPAddress[] GetIPAddressWithHandleException(string host)
        {
            IPAddress[] ip = null;
            try
            {
                ip = GetIPAddress(host);
            }
            catch (Exception ex)
            {
                logger.Error("GetIPAddressWithHandleException: " + ex.Message + ex.StackTrace);

                try
                {
                    IPHostEntry dnstoip = Dns.GetHostEntry(host);
                    return dnstoip.AddressList;
                }
                catch (Exception e)
                {
                    logger.Error("Use Dns.Resolve to get address: " + e.Message + e.StackTrace);
                }
            }

            return ip;
        }

        /// <summary> Get web install path. </summary>
        /// <returns></returns>
        public static string GetInstallPath()
        {
            return Global.Util.WebUtil.GetInstallPath();
        }

        public static Configuration GetWebConfiguration()
        {
            return Global.Util.WebUtil.GetWebConfiguration();
        }

        public static string GetTempPath()
        {
            try
            {
                if (RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING] != null)
                {
                    return RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER];
                }
            }
            catch (Exception ex)
            {
                logger.Error("Get local resource path from role failed: {0}", ex.ToString());
            }
            return Path.Combine(GetInstallPath(), @"../Temp");
        }

        public static string GetProductVersionConfigPath()
        {
            string installPath = GetInstallPath();
            string filePath = Path.Combine(installPath, "Config/ServiceVersion/ServiceVersion.config");
#if DEBUG
            if (!File.Exists(filePath))
            {
                // 开发环境找不到配置文件的话，从Code 里的RACommon里取
                var index = installPath.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}");
                if (index > 0)
                {
                    filePath = Path.Combine(installPath.Substring(0, index), "../RACommon/ServiceVersion/ServiceVersion.config");
                }
            }
#endif
            return filePath;
        }

        public static string GetProductVersion()
        {
            string strVersion = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetProductVersionConfigPath());
                //foreach (var node in doc.GetElementsByTagName("version"))
                //{
                //    XmlElement xe = (XmlElement)node;
                //    strVersion = xe.InnerText;
                //    break;
                //}
                XmlElement xe = (XmlElement)doc.GetElementsByTagName("version")[0];
                strVersion = xe.InnerText;
            }
            catch (Exception e)
            {
                logger.Error("get product verison Error:{0}", e.ToString());
            }
            return strVersion;
        }


        public static string GetProductDisplayVersion()
        {
            string strVersion = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetProductVersionConfigPath());
                //foreach (var node in doc.GetElementsByTagName("DisplayVersion"))
                //{
                //    XmlElement xe = (XmlElement)node;
                //    strVersion = xe.InnerText;
                //    break;
                //}
                XmlElement xe = (XmlElement)doc.GetElementsByTagName("DisplayVersion")[0];
                strVersion = xe.InnerText;
            }
            catch (Exception e)
            {
                logger.Error("get product verison Error:{0}", e.ToString());
            }
            return strVersion;
        }


        public static string UrlDecode(string url)
        {
            return HttpUtility.UrlDecode(url);
        }

        public static List<string> GetDesignLists(bool isCSDTenant = false)
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
                    if (isCSDTenant && Convert.ToInt32(xe.GetAttribute("serverTemplate")) == (int)AveListTemplateType.WebPageLibrary)
                    {
                        //For the CSD tenant, the WebPageLibrary is not a design list.
                        continue;
                    }
                    else
                    {
                        results.Add(xe.GetAttribute("url") + xe.GetAttribute("serverTemplate"));
                    }
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
            if (strUrl.StartsWith("http:") || strUrl.StartsWith("https:"))
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
        public static string GetOffice365tenantIdByUserName(String userName)
        {
            try
            {
                string result = "";
                var domain = userName.Split('@').Last();
                String office365tenantId;
                using (var client = HttpClientFactory.CreateHttpClient("Office365TenantDiscover"))
                {
                    result = client.GetStringAsync($"https://login.windows.net/{domain}/.well-known/openid-configuration").ConfigureAwait(false).GetAwaiter().GetResult();
                    var info = SerializerHelper.DeserializeByJsonSerializer<Dictionary<String, Object>>(result);
                    var endpoint = info["userinfo_endpoint"] as String;
                    office365tenantId = endpoint.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[2];
                    return office365tenantId;
                }
               
            }
            catch (Exception ex)
            {
                logger.Error("Get user tenant id failed: {0}.", ex.ToString());
                return string.Empty;
            }
        }

        public static string GetOffice365tenantIdByDomain(String domain)
        {
            try
            {
                string result = "";
                String office365tenantId;
                using (var client = HttpClientFactory.CreateHttpClient("Office365TenantDiscover"))
                {
                    result = client.GetStringAsync($"https://login.windows.net/{domain}/.well-known/openid-configuration").ConfigureAwait(false).GetAwaiter().GetResult();
                    var info = SerializerHelper.DeserializeByJsonSerializer<Dictionary<String, Object>>(result);
                    var endpoint = info["userinfo_endpoint"] as String;
                    office365tenantId = endpoint.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[2];
                    return office365tenantId;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Get user tenant id failed: {0}.", ex.ToString());
                return string.Empty;
            }
        }

        public static string GetTenantName(string webUrl)
        {
            /*
            SharePoint URL : https://tenant_name.sharepoint.com
            SPO Site URL: https://tenant_name.sharepoint.com/sites/site_name
            SPO Admin Site URL : https://tenant_name-admin.sharepoint.com/
            OneDrive URL : https://tenant_name-my.sharepoint.com/personal/username_domain_com/_layouts/15/onedrive.aspx
            */
            if (webUrl != null)
            {
                var matchs = TenantNameRegex.Match(webUrl);
                if (matchs.Success)
                {
                    var tenantName = matchs.Groups[1].Value;
                    return tenantName;
                }
            }

            return null;
        }
        public static string GetTenantDomainName(string webUrl)
        {
            var tenantName = GetTenantName(webUrl);
            var uri = new Uri(webUrl);
            if (uri.Host.EndsWith("sharepoint.cn", StringComparison.OrdinalIgnoreCase))
            {
                return $"{tenantName}.partner.onmschina.cn";
            }
            else
            {
                return $"{tenantName}.onmicrosoft.com";
            }
        }

        public static string GetListItemRealPath(string itemUrl)
        {
            if (string.IsNullOrEmpty(itemUrl))
            {
                throw new ArgumentNullException("itemUrl");
            }
            if (itemUrl.StartsWith("http:") || itemUrl.StartsWith("https:"))
            {
                var splitArrary = itemUrl.Split(new string[] { "/Lists/" }, StringSplitOptions.None);
                if (splitArrary.Length > 1)
                {
                    var webUrl = splitArrary[0];
                    var listName = splitArrary[1].Split('/')[0];
                    var itemName = itemUrl.Substring(itemUrl.LastIndexOf("/") + 1).Split('_')[0];
                    //eg:https://m365b310744.sharepoint.com/sites/yySite01/Lists/list1/f3/f31/13_.000
                    return $"{webUrl}/Lists/{listName}/DispForm.aspx?ID={itemName}";
                }

            }
            return itemUrl;
        }
        public static string GetListItemRealPath(string webUrl, string listServerUrl, string itemPath)
        {
            if (string.IsNullOrEmpty(webUrl))
            {
                throw new ArgumentNullException("webUrl");
            }
            if (string.IsNullOrEmpty(listServerUrl))
            {
                throw new ArgumentNullException("listServerUrl");
            }
            if (string.IsNullOrEmpty(itemPath))
            {
                throw new ArgumentNullException("itemPath");
            }
            string itemName = "";
            if (!itemPath.Contains("/"))
            {
                itemName = itemPath;
            }
            else
            {
                itemName = itemPath.Substring(itemPath.LastIndexOf("/") + 1);
            }
            return MakeFullUrl(webUrl, listServerUrl) + $"/DispForm.aspx?ID={itemName.Split('_')[0]}";
        }

        /// <summary>
        /// /检查上次文件扩展名
        /// </summary>
        /// <param name="fileExt">上传文件名</param>
        /// <param name="extWhiteList">允许上传的文件类型(白名单)</param>
        public static void CheckFileExtension(string fileExt, List<FileExtension> extWhiteList)
        {
            var extWhiteStrList = extWhiteList.ConvertAll((o) => { return o.ToString().ToLower(); });
            if (!extWhiteStrList.Contains(fileExt))
            {
                throw new Exception($"Please make sure the file format is {string.Join(",", extWhiteStrList)}.");
            }
        }
        /// <summary>
        /// 检查文件大小是否超过maxSize
        /// </summary>
        /// <param name="fileSize">B</param>
        /// <param name="maxSize">MB</param>
        /// <returns></returns>
        public static void CheckFileSize(long fileSize, int maxSize)
        {
            if (fileSize / (1024 * 1024) > maxSize)
            {
                throw new Exception($"Please make sure the file size is less than {maxSize} MB.");
            }
        }
        /// <summary>
        /// 检查文件二进制头，来确认此类文件是否允许上传
        /// </summary>
        /// <param name="file"></param>
        /// <param name="allowFileExts">白名单</param>
        public static void CheckFileHeadCode(Stream inputStream, List<FileExtension> allowFileExts)
        {
            if (inputStream == null || inputStream.Length == 0 || allowFileExts == null || allowFileExts.Count == 0)
            {
                throw new Exception("An error occurred while checking the file header.");
            }
            //从文件流中读取两个字节
            var fileTypeCode = "";
            var outputStream = new MemoryStream();
            inputStream.CopyTo(outputStream);
            var fileBytes = outputStream.ToArray();
            fileTypeCode = fileBytes != null && fileBytes.Length > 0 ? $"{fileBytes[0].ToString()}{fileBytes[1].ToString()}" : "";
            inputStream.Position = 0;
            outputStream.Close();

            //和白名单中做比较
            var fileTypes = GetFileTypeCodes(allowFileExts);
            if (fileTypes.Count > 0 && !fileTypes.Contains(fileTypeCode))
            {
                throw new Exception($"Please make sure that the uploaded file type is allowed.");
            }
        }

        private static List<string> GetFileTypeCodes(List<FileExtension> fileExts)
        {
            var fileTypeCodes = new List<string>();
            foreach (var ext in fileExts)
            {
                switch (ext)
                {
                    case FileExtension.ZIP:
                    case FileExtension.XLSX:
                        fileTypeCodes.Add("8075");
                        break;
                    case FileExtension.CSV:
                        //fileTypeCodes.Add("239187"); 
                        //fileTypeCodes.Add("4944");
                        //fileTypeCodes.Add("4949");
                        break;
                    default:
                        break;
                }
            }
            return fileTypeCodes;
        }

        [Obsolete("use GetSPAdminUrl(string siteUrl, string tenantId) instead")]
        public static string GetSPAdminUrl(string siteUrl)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    var domain = siteUrl.Split('/').Where(s => s.Contains(".sharepoint.")).FirstOrDefault();
                    domain = domain?.Replace(".sharepoint.", "-admin.sharepoint.");
                    result = string.IsNullOrEmpty(domain) ? domain : $"https://{domain}";
                    logger.Info($"try to get site adminUrl by site url:{siteUrl}, adminUrl : {result}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"get admin url error:{ex.ToString()}");
            }

            return result;
        }

        public static string GetSPAdminUrl(string siteUrl, string tenantId)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    var domain = siteUrl.Split('/').Where(s => s.Contains(".sharepoint.")).FirstOrDefault();
                    domain = domain?.Replace(".sharepoint.", "-admin.sharepoint.");
                    result = string.IsNullOrEmpty(domain) ? domain : $"https://{domain}";
                    logger.Info($"try to get site adminUrl by site url:{siteUrl}, adminUrl : {result}");

                    if (string.IsNullOrWhiteSpace(result) && !string.IsNullOrEmpty(tenantId))
                    {
                        logger.Info($"Will use TenantId :{tenantId} to get admin url.");
                        result = GetSPAdminUrlByTenantId(tenantId);
                    }
                }
                else
                {
                    logger.Info($"siteUrl is empty, will use TenantId :{tenantId} to get admin url.");
                    result = GetSPAdminUrlByTenantId(tenantId);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"get admin url error:{ex.ToString()}");
            }

            return result;
        }

        private static string GetSPAdminUrlByTenantId(string tenantId)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(tenantId))
                {
                    var tenantInfo = RMAosApiClient.GetO365TenantInfoByIdAsync(tenantId).GetAwaiter().GetResult();
                    if (tenantInfo != null)
                    {
                        result = tenantInfo.AdminUrl;
                    }
                    else
                    {
                        logger.Error($"Can not get admin url, tenantInfo is null.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"get admin url error:{ex.ToString()}");
            }
            return result;
        }

        /// <summary>
        /// 解决不同浏览器导出Excel文件吗乱码问题
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ConvertFileName(string fileName)
        {
            string outputFileName = null;
            outputFileName = HttpUtility.UrlEncode(fileName);
            outputFileName = outputFileName.Replace("+", "%20");
            //if (browser.Contains("FIREFOX") == true)
            //{
            //    //outputFileName = "/"" + fileName + "/"";
            //}
            //else
            //{
            //    //其它浏览器需要对文件名编码
            //    outputFileName = HttpUtility.UrlEncode(fileName);
            //    outputFileName = outputFileName.Replace("+", "%20");    //处理空格转为加号的问题
            //}
            return outputFileName;
        }

        public static void CheckStringLen(string sourceStr, int allowLength)
        {
            if (!string.IsNullOrEmpty(sourceStr))
            {
                sourceStr = sourceStr.Trim();
                if (sourceStr.Length > allowLength)
                {
                    throw new Exception($"Please make sure the content length is less than {allowLength}");
                }
            }
        }

        //public static string GetClientIP()
        //{
        //    var request = HttpContext.Current?.Request;
        //    string str = string.Empty;
        //    if (request != null)
        //    {
        //        str = request.ServerVariables["HTTP_X_FORWARDED_FOR"]?.ToString().Split(',')[0].Trim();
        //        if (string.IsNullOrEmpty(str))
        //        {
        //            str = request.ServerVariables["REMOTE_ADDR"];
        //        }
        //        if (string.IsNullOrEmpty(str))
        //        {
        //            str = request.UserHostAddress;
        //        }
        //    }
        //    return str;
        //}
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
        CSV,
        XML
    }
}
