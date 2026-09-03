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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Contract.Object;
using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.Security;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Tenant;
using System.Security.Cryptography.X509Certificates;
using AvePoint.Wrapper.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using Microsoft365.SharePoint.CSOM.Extension;

namespace AvePoint.RA.SharePoint.Common
{
    public class CommonClientContext
    {
        private RALogger logger = RALogger.GetInstance(typeof(CommonClientContext));
        private CookieContainer cookieContainer;
        private RMSPTreeNode siteCollectionNode;
        private RemoteSiteCollection remoteSiteCollectionNode;
        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        /// <summary>
        /// 
        /// </summary>
        public CommonClientContext()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="util"></param>

        public void clientContext_ExecutingWebRequest(object sender, WebRequestEventArgs e)
        {
            try
            {
                //e.WebRequestExecutor.WebRequest.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                //#if !DEBUG
                e.WebRequestExecutor.WebRequest.UserAgent = string.Format("ISV|AvePoint|CloudRecords/{0}", RMGlobalConfiguration.EnvSetting.ProductVersion);
                //#endif
                if (cookieContainer != null)
                {
                    //logger.Info("test client context request cookie container");
                    e.WebRequestExecutor.WebRequest.CookieContainer = cookieContainer;
                }

            }
            catch (Exception ex)
            {
                logger.Warn("add form based auth error {0}", ex.ToString());
            }
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
                    var BPOSInfo = bposInfo == null? PoolUserUtil.GetAveBPOSAccountInfo(siteCollectionNode.BposInfo, siteNode.FullPath) : bposInfo;
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

        public Guid GetSiteId(RMSPSampleTreeNode node)
        {
            ClientContext context = null;
            try
            {

                context = new ClientContext(node.FullPath);
                var BPOSInfo = PoolUserUtil.GetAveBPOSAccountInfo(node.BposInfo, node.FullPath);
                var tokenProvider = BPOSInfo.Convert2TokenProvider();
                context.SetTokenProvider(tokenProvider);
                context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
                Site testSite = context.Site;
                context.Load(testSite, t => t.Id, t => t.Url);
                context.ExecuteQuery();
                return testSite.Id;

            }
            catch (Exception e)
            {
                context?.Dispose();
                context = null;
                logger.Info("init o365 get id{0} {1}", node.FullPath, e.ToString());
                throw;
            }

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
                    var BPOSInfo = PoolUserUtil.GetBPOSInfoAsync(node).Result;
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

        #region
        public static ClientContext GetClientContextWithAccessToken(string targetUrl, string accessToken)
        {
            ClientContext clientContext = new ClientContext(targetUrl);

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

        #endregion

    }
}
