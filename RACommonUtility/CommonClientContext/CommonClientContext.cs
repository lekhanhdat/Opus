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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Object;
using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.Security;
using AvePoint.Wrapper.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using System.Collections.Generic;
using AvePoint.RA.Common;
using Microsoft365.SharePoint.CSOM.Extension;

namespace AvePoint.RA.RACommonUtility
{
    public class CommonClientContext
    {
        private RALogger logger = RALogger.GetInstance(typeof(CommonClientContext));
        private CookieContainer cookieContainer;
        private RMSPTreeNode siteCollectionNode;
        private RemoteSiteCollection remoteSiteCollectionNode;

        public void clientContext_ExecutingWebRequest(object sender, WebRequestEventArgs e)
        {
            try
            {
                //e.WebRequestExecutor.WebRequest.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
#if !DEBUG
                e.WebRequestExecutor.WebRequest.UserAgent = string.Format("ISV|AvePoint|CloudRecords/{0}", RMGlobalConfiguration.EnvSetting.ProductVersion);
#endif
                if (cookieContainer != null)
                {
                    e.WebRequestExecutor.WebRequest.CookieContainer = cookieContainer;
                }

            }
            catch (Exception ex)
            {
                logger.Warn("add form based auth error {0}", ex.ToString());
            }
        }

        public ClientContext InitClientContext(AveBPOSAccountInfo bposInfo, string siteUrl)
        {
            ClientContext context = null;

            try
            {
                context = RetryUtility.RetryWhen(() =>
                {
                    context = new ClientContext(siteUrl);
                    var tokenProvider = bposInfo.Convert2TokenProvider();
                    context.SetTokenProvider(tokenProvider);
                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
                    Site testSite = context.Site;
                    context.Load(testSite);
                    context.ExecuteQuery();
                    logger.Info("Site URL {0}", context.Site.Url);
                    return context;
                }, ShouldRetryErrorMessage, 3);

            }
            catch (Exception e)
            {
                context?.Dispose();
                context = null;
                logger.Info("retry init o365 {0} {1}", siteUrl, e.ToString());
            }

            return context;
        }
        
        public ClientContext InitClientContext(RMSPTreeNode node, AveBPOSAccountInfo bposInfo = null)
        {
            RMSPTreeNode siteNode = GetSiteCollectionNode(node);
            siteCollectionNode = siteNode;
            ClientContext context = null;

            try
            {
                context = RetryUtility.RetryWhen(() =>
                {
                    context = new ClientContext(siteNode.FullPath);
                    var BPOSInfo = bposInfo == null? CommonPoolUserUtil.GetAveBPOSAccountInfo(siteCollectionNode.BposInfo, siteNode.FullPath) : bposInfo;
                    var tokenProvider = BPOSInfo.Convert2TokenProvider();
                    context.SetTokenProvider(tokenProvider);
                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
                    Site testSite = context.Site;
                    context.Load(testSite);
                    context.ExecuteQuery();
                    logger.Info("Online Site URL {0}", context.Site.Url);
                    return context;
                }, ShouldRetryErrorMessage, 3);

            }
            catch (Exception e)
            {
                context?.Dispose();
                context = null;
                logger.Info("retry init o365 {0} {1}", node.FullPath, e.ToString());
            }

            return context;
        }

        public Dictionary<string, DateTime> GetSiteModifiedDateCache(RemoteSiteCollection remoteSite, bool isOneDrive = false)
        {
            Dictionary<string, DateTime> siteModifiedDateCache = new Dictionary<string, DateTime>();
            Microsoft.Online.SharePoint.TenantAdministration.Tenant tenant = null;
            Microsoft.SharePoint.Client.ClientContext currentContext = null;
            try
            {              
                using (var performance0 = new PerformanceScope("CommonClientContext.GetTenant"))
                {                 
                    var bposInfo = CommonPoolUserUtil.GetBPOSInfo(remoteSite);
                    //var factory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, Wrapper.Common.AveContextKind.ClientObjectModel);

                    AvePoint.RA.RACommonUtility.CommonClientContext clientContext = new AvePoint.RA.RACommonUtility.CommonClientContext();
                    currentContext = clientContext.InitClientContext(bposInfo, remoteSite.AdminUrl);
                    //SPOSitePropertiesEnumerableFilter filter = new SPOSitePropertiesEnumerableFilter();
                    tenant = new Microsoft.Online.SharePoint.TenantAdministration.Tenant(currentContext);
                    currentContext.Load(tenant);
                    currentContext.ExecuteQuery();
                }
                Microsoft.Online.SharePoint.TenantAdministration.SPOSitePropertiesEnumerable siteProperties = null;
                Microsoft.Online.SharePoint.TenantAdministration.SPOSitePropertiesEnumerableFilter sspFilter = new Microsoft.Online.SharePoint.TenantAdministration.SPOSitePropertiesEnumerableFilter()
                {
                    // get personal sites 
                    //IncludePersonalSite = PersonalSiteFilter.Include, // needed to for personal sites 
                    //IncludeDetail = true,
                    //Template = "SPSPERS"

                    // get classic team sites 
                    //IncludeDetail = true, 
                    //Template = "STS"

                    // get modern sites 
                    //IncludeDetail = true, 
                    //Template = "GROUP" 

                    // get communication sites 
                    //IncludeDetail = true, 
                    //Template = "SITEPAGEPUBLISHING" 
                };
                if (isOneDrive)
                {
                    sspFilter.IncludePersonalSite = Microsoft.Online.SharePoint.TenantAdministration.PersonalSiteFilter.Include;
                    sspFilter.Template = "SPSPERS";
                }
                else
                {
                    sspFilter.IncludePersonalSite = Microsoft.Online.SharePoint.TenantAdministration.PersonalSiteFilter.Exclude;
                }

                using (var performance0 = new PerformanceScope("CommonClientContext.GetSiteProperties"))
                { 
                    string nextIndex = null;
                    do
                    {
                        sspFilter.StartIndex = nextIndex;
                        siteProperties = tenant.GetSitePropertiesFromSharePointByFilters(sspFilter);
                        currentContext.Load(siteProperties);
                        currentContext.ExecuteQuery();
                        nextIndex = siteProperties.NextStartIndexFromSharePoint;
                        using (var performance2 = new PerformanceScope("CommonClientContext.AddSiteProperties"))
                        {
                            foreach (var p in siteProperties)
                            {
                                siteModifiedDateCache.Add(p.Url.ToLower(), p.LastContentModifiedDate);
                            }
                        }
                    }
                    while (nextIndex != null);
                }

            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while getting site modified date cache,tenant id:{remoteSite?.TenantId} error:{e.ToString()}");
            }
            return siteModifiedDateCache;
        }
        public ClientContext InitClientContext(RMSPTreeNode node)
        {
            RMSPTreeNode siteNode = GetSiteCollectionNode(node);
            siteCollectionNode = siteNode;
            ClientContext context = null;
  
            try
            {
                context = RetryUtility.RetryWhen(() =>
                {
                    context = new ClientContext(siteNode.FullPath);
                    var BPOSInfo = CommonPoolUserUtil.GetAveBPOSAccountInfo(siteCollectionNode.BposInfo, siteNode.FullPath);
                    var tokenProvider = BPOSInfo.Convert2TokenProvider();
                    context.SetTokenProvider(tokenProvider);
                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
                    Site testSite = context.Site;
                    context.Load(testSite);
                    context.ExecuteQuery();
                    logger.Info("Online Site URL {0}", context.Site.Url);
                    return context;
                }, ShouldRetryErrorMessage, 3);
               
            }
            catch (Exception e)
            {
                context?.Dispose();
				context = null;
                logger.Info("retry init o365 {0} {1}", node.FullPath, e.ToString());
            }
            
            return context;
        }

        public ClientContext InitClientContext(RemoteSiteCollection node)
        {
            remoteSiteCollectionNode = node;
            ClientContext context = null;
            try
            {
                context = RetryUtility.RetryWhen(() => 
                {
                    var startTime = DateTime.Now;
                    context = new ClientContext(remoteSiteCollectionNode.url);
                    var BPOSInfo = CommonPoolUserUtil.GetBPOSInfo(node);
                    logger.Warn($"3.2.1 time elapsed for InitClientContext(GetBPOSInfo)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
                    startTime = DateTime.Now;
                    var tokenProvider = BPOSInfo.Convert2TokenProvider();

                    context.SetTokenProvider(tokenProvider);
                    logger.Warn($"3.2.2 time elapsed for InitClientContext(SetTokenProvider)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
                    startTime = DateTime.Now;
                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
                    Site testSite = context.Site;
                    context.Load(testSite);
                    logger.Warn($"3.2.3 time elapsed for InitClientContext(Load)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
                    startTime = DateTime.Now;
                    context.ExecuteQuery();
                    logger.Warn($"3.2.4 time elapsed for InitClientContext(ExecuteQuery)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
                    logger.Info("Online Site URL {0}", context.Site.Url);
                    return context;
                }, ShouldRetryErrorMessage, 3);
                
            }
            catch (Exception e)
            {
                context?.Dispose();
				context = null;
                logger.Info("retry init o365 {0} {1}", remoteSiteCollectionNode.url, e.ToString());
            }
            return context;
        }

        private bool ShouldRetryErrorMessage(Exception e)
        {
            return e is TimeoutException
                || e is UnauthorizedAccessException
                || e is WebException;
        }
        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        
        public static ClientContext GetClientContextWithAccessToken(string targetUrl, string accessToken)
        {
            ClientContext clientContext = new ClientContext(targetUrl);
            //clientContext.RequestTimeout = 180000000; //Should validate the default request time.
            //clientContext.AuthenticationMode = ClientAuthenticationMode.Anonymous;
            //clientContext.FormDigestHandlingEnabled = false;
            clientContext.ExecutingWebRequest +=
                delegate (object oSender, WebRequestEventArgs webRequestEventArgs)
                {
                    webRequestEventArgs.WebRequestExecutor.RequestHeaders["Authorization"] =
                        "Bearer " + accessToken;
                };

            return clientContext;
        }
    }
}
