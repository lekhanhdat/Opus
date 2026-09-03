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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.ObjectModel.CompoundRequest;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Common;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.Common
{
    public class AveClientRequest : IAveClientRequest, IDisposable
    {
        private const int DefaultHttpConnectionLimit = 80;
        private static AveLogger Logger = AveLogger.GetInstance(typeof(AveClientRequest));
        protected IAveRequest mRequest;
        private AveBPOSAccountInfo mUserAccountInfo = new AveBPOSAccountInfo();

        protected AveAuthenticationMode mAveAuthMode;

        private AveServerVersion mSPVersion;
        
        public string SiteUrl { get; private set; }

        public AveAuthenticationMode AveAuthMode { get { return mAveAuthMode; } }
        public string WebUrl { get; private set; }
        
        public AveServerVersion SPVersion
        {
            get
            {
                return mSPVersion;
            }
        }
        
        //365 底层有cache，不用再留其他构造方法
        public AveClientRequest(string url, AveBPOSAccountInfo userAccountInfo, AuthenticationModeOption[] AveAuthenticationModeOptions)
        {
            SiteUrl = url;
            InitAccountUser(userAccountInfo);
            InitHttpSettings();
            InitHttpsSettings();
            var result = GetAutheticationResult(AveAuthenticationModeOptions);
            if (result.Status == AutheStatus.Successful)
            {
                mAveAuthMode = result.AutheMode;
                InitSPVersion(result);
                InitRequest(result);
            }
            else
            {
                throw new Exception(result.ToString());
            }
        }

        private void InitAccountUser(AveBPOSAccountInfo accountInfo)
        {
            accountInfo.CopyTo(mUserAccountInfo);
            if (!string.IsNullOrEmpty(accountInfo.UserName))
            {
                string[] account = mUserAccountInfo.UserName.Split(new char[] { '\\' });
                if (account.Length > 1)
                {
                    mUserAccountInfo.Domain = account[0];
                    mUserAccountInfo.UserName = account[1];
                }
            }
        }

        private void InitHttpsSettings()
        {
            if (SiteUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);
            }
        }

        private void InitHttpSettings()
        {
            ServicePointManager.DefaultConnectionLimit = DefaultHttpConnectionLimit;
        }

        private AuthenticationResult GetAutheticationResult(AuthenticationModeOption[] modes)
        {
            string userName = this.mUserAccountInfo.UserName;
            if (!string.IsNullOrEmpty(mUserAccountInfo.Domain))
            {
                userName = this.mUserAccountInfo.Domain + "\\" + this.mUserAccountInfo.UserName;
            }
            Logger.Info(string.Format("Login site collection[URL:{0}], Client Id: {1}, Region: {2}", SiteUrl, mUserAccountInfo.ClientId, mUserAccountInfo.AzureRegion));

            if (WrapperConfiguration.IsProxyEnabled)
            {
                Logger.Debug(string.Format("Login site collection[URL:{0}] with proxy , Proxy Info: {1}", SiteUrl, WrapperConfiguration.ProxyInfo.ToString()));
            }
            string tempWebUrl = string.Empty;
            SiteUrl = TryToRectifyUrl(SiteUrl, out tempWebUrl);
            WebUrl = tempWebUrl;

            AuthenticationHandler auth = new AuthenticationHandler(this.SiteUrl, this.mUserAccountInfo);
            return auth.GetAuthenticationResult(modes);
        }

        //rectify url, for example: convert /sites/site1/SitePages/Home.aspx to /sites/site1
        private string TryToRectifyUrl(string url, out string webUrl)
        {
            webUrl = url;
            if (mUserAccountInfo.ConnectionType != BposConnectionType.AppToken)
            {
                int index = url.LastIndexOf('/');
                string urlHeader = url.StartsWith("https", StringComparison.OrdinalIgnoreCase) ? "https://" : "http://";
                string leafname = url.Substring(index);
                if (index > urlHeader.Length && leafname.Contains('.'))
                {
                    string tempUrl = url.Substring(0, index);
                    if (tempUrl.IndexOf('/', urlHeader.Length) != tempUrl.LastIndexOf('/'))   //Subsite的url中允许存在英文句号，因此需要在这里判断，避免因为Subsite中的英文句号导致判断出错
                    {
                        url = url.Substring(0, index);
                    }
                }
                url = url.Replace("/_layouts/15/start.aspx#", "");//ADO-79272
                if (url.Contains('%'))
                {
                    url = System.Web.HttpUtility.UrlDecode(url);
                }
                while (true)
                {
                    try
                    {
                        AveSiteService siteService1 = new AveSiteService(url + "/_vti_bin/Sites.asmx") { Timeout = 3 * 60 * 1000, Credentials = !string.IsNullOrEmpty(mUserAccountInfo.Domain) ? new NetworkCredential(mUserAccountInfo.UserName, mUserAccountInfo.Password, mUserAccountInfo.Domain) : new NetworkCredential(mUserAccountInfo.UserName, mUserAccountInfo.Password) };

                        string siteInfo = siteService1.GetSite(url);
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(siteInfo);
                        if (doc.DocumentElement.HasAttribute("Url"))
                        {
                            webUrl = url;
                            url = doc.DocumentElement.GetAttribute("Url");
                        }
                    }
                    catch (WebException we)
                    {
                        using (HttpWebResponse response = we.Response as HttpWebResponse)
                        {
                            index = url.LastIndexOf('/');
                            if (response != null && response.StatusCode == HttpStatusCode.NotFound && index > "https://".Length)
                            {
                                url = url.Substring(0, index);
                                continue;
                            }
                            Logger.Warn("failed to connect this site:{0} due to: {1}", url, we.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Debug("rectifying url completed.{0}", e);
                    }
                    return url;
                }
            }
            return url;
        }

        private void InitSPVersion(AuthenticationResult result)
        {
            if ((AveAuthenticationMode.Online & mAveAuthMode) != 0)
            {
                mSPVersion = AveSPServerVersion.ConvertServerVersoin(SiteUrl, "16.0.7507.1202", mAveAuthMode);
            }
            else
            {
                var digest = EnsureFormDigest(result, 2);
                AveSPServerVersion spServerVersion = new AveSPServerVersion(SiteUrl, digest, result, mAveAuthMode);
                mSPVersion = spServerVersion.GetSPServerVersion();
            }
        }

        private IAveRequest InitRequest(AuthenticationResult result)
        {
            switch (this.SPVersion.VersionType)
            {
                //case AveSPServerVersionType.SP2007:
                //    mRequest = new AveWebServiceRequest(SiteUrl, mUserAccountInfo, Credentials, this.SPVersion.Version);
                //    break;
                case AveSPServerVersionType.SP2010:
                    Type requestType2010 = System.Reflection.Assembly.LoadFrom(Path.GetDirectoryName(typeof(AveClientRequest).Assembly.Location) + "\\2010\\AgentCommonCompoundRequest.dll").GetType("AvePoint.ObjectModel.CompoundRequest.AveClientCompoundRequest");
                    mRequest = Activator.CreateInstance(requestType2010, new object[] { SiteUrl, mUserAccountInfo, result.Credential, SPVersion.Version }) as IAveRequest;
                    break;
                case AveSPServerVersionType.SP2013:
                    Type requestType2013 = System.Reflection.Assembly.LoadFrom(Path.GetDirectoryName(typeof(AveClientRequest).Assembly.Location) + "\\2013\\SP2013ClientOMRequest.dll").GetType("AvePoint.ObjectModel.ClientOM.AveClientOM2013Request");
                    mRequest = Activator.CreateInstance(requestType2013, new object[] { SiteUrl, mUserAccountInfo, result.Credential, SPVersion.Version }) as IAveRequest;
                    break;
                case AveSPServerVersionType.SP2016:
                    Type requestTypeSP2016 = System.Reflection.Assembly.LoadFrom(Path.GetDirectoryName(typeof(AveClientRequest).Assembly.Location) + "\\2016\\SP2016ClientOMRequest.dll").GetType("AvePoint.ObjectModel.ClientOM.AveClientOM2016Request");
                    mRequest = Activator.CreateInstance(requestTypeSP2016, new object[] { SiteUrl, mUserAccountInfo, result.Credential, SPVersion.Version }) as IAveRequest;
                    break;
                case AveSPServerVersionType.SP2019:
                case AveSPServerVersionType.SPSE:
                    Type requestTypeSP2019 = System.Reflection.Assembly.LoadFrom(Path.GetDirectoryName(typeof(AveClientRequest).Assembly.Location) + "\\2019\\SP2019ClientOMRequest.dll").GetType("AvePoint.ObjectModel.ClientOM.AveClientOM2019Request");
                    mRequest = Activator.CreateInstance(requestTypeSP2019, new object[] { SiteUrl, mUserAccountInfo, result.Credential, SPVersion.Version }) as IAveRequest;
                    break;
                case AveSPServerVersionType.Office365:
                    Type requestTypeOffice365 = System.Reflection.Assembly.LoadFrom(Path.GetDirectoryName(typeof(AveClientRequest).Assembly.Location) + "\\Office365\\AgentCommonOffice365OMRequest.dll").GetType("AvePoint.ObjectModel.ClientOM.AveClientOMOffice365Request");
                    mRequest = Activator.CreateInstance(requestTypeOffice365, new object[] { SiteUrl, mUserAccountInfo, result.tokenProviders, SPVersion.Version }) as IAveRequest;
                    break;
                case AveSPServerVersionType.None:
                    throw new ArgumentException("Can't get SharePoint version information.");
            }
            mRequest.SetCurrentWebUrl(WebUrl);
            return mRequest;
        }

        /// <summary>
        /// ADO-154183：只有在连接出错时才会retry该方法
        /// </summary>
        /// <param name="retryCount">retry该方法的最大次数</param>
        private string EnsureFormDigest(AuthenticationResult result, int retryCount)
        {
            AveSiteService siteService = new AveSiteService(SiteUrl + "/_vti_bin/Sites.asmx");
            siteService.Timeout = 3 * 60 * 1000;
            try
            {
                ITokenProvider token = null;
                if (result.tokenProviders != null)
                {
                    foreach (var provider in result.tokenProviders)
                    {
                        if (provider.TokenType.Equals(TokenType.Bearer) || token == null)
                        {
                            token = provider;
                        }
                    }
                }
                siteService.TokenProvider = token;
                siteService.Credentials = result.Credential;
                return siteService.GetUpdatedFormDigestInformation(null);
            }
            catch (WebException e)
            {
                Logger.Info("EnsureFormDigest error. Error Message: {0}", e);
                int interval = WrapperConfiguration.BPOS_S.ClientRequestRetryInterval;
                if (retryCount <= 0 || (!AveExceptionHelper.IsConnectionException(e) && !AveExceptionHelper.IsHTTP429Error(e, ref interval)))
                {
                    throw;
                }
                Thread.Sleep(interval);
                Logger.Debug("Retry EnsureFormDigest.");
                AveEventHelper.Retry(delegate ()
                {
                    EnsureFormDigest(result,retryCount--);
                });
            }
            return null;
        }

        //紧内部使用 
        internal IAveRequest GetInnerRequest()
        {
            return mRequest;
        }


        #region IDisposable Members

        public void Dispose()
        {

        }
        #endregion


        #region Public method
        public void AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl)
        {
            this.mRequest.AddSiteAdmin(mUserAccountInfo.UserName, SiteUrl, tenantAdminSiteUrl);
            Logger.Info("set user as site administrator successfully.Site:{0}", SiteUrl);
        }

        public List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames)
        {
            return mRequest.LoadPersonalSiteInfosForUsers(usernames);
        }

        public Dictionary<string, object> GetBrowserSiteInfo()
        {
            return mRequest.GetBrowserSiteInfo();
        }

        public Dictionary<string, object> GetUsers(string url, string groupName, string scope)
        {
            return mRequest.GetUsers(url, groupName, scope);
        }

        public Dictionary<string, object> GetSite()
        {
            return mRequest.GetSite();
        }

        public Dictionary<string, object> GetUser(string userEmail)
        {
            return mRequest.GetUser(userEmail);
        }

        public Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource)
        {
            return mRequest.GetWebTemplates(webServerRelativeUrl, lcid, doIncludeCrossLanguage, webtemplateSource);
        }
        #endregion
    }
}

