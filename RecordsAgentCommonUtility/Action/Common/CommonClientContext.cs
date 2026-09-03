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

//using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
//using AvePoint.GCommon.Contract.Tree.Object;
//using AvePoint.RA.Contract.Object;
//using Microsoft.SharePoint.Client;
//using System;
//using System.Net;
//using System.Security;
//using AvePoint.Wrapper.Common;
//using AvePoint.RA.CommonUtil;
//using AvePoint.Hybrid.Utility;
//using AvePoint.Hybrid.Utility.Configuration;
//using AvePoint.Office365.Api;

//namespace AvePoint.RA.RACommonUtility
//{
//    public class CommonClientContext
//    {
//        private RALogger logger = RALogger.GetInstance(typeof(CommonClientContext));
//        private ICredentials credential;
//        private CookieContainer cookieContainer;
//        private AvePoint.RA.Contract.Global.Object.RMSPTreeNode siteCollectionNode;
//        private AvePoint.RA.Contract.Global.JobMessage.SiteInfo remoteSiteCollectionNode;

//        public void clientContext_ExecutingWebRequest(object sender, WebRequestEventArgs e)
//        {
//            try
//            {
//                //e.WebRequestExecutor.WebRequest.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
//#if !DEBUG
//                e.WebRequestExecutor.WebRequest.UserAgent = string.Format("ISV|AvePoint|CloudRecords/{0}", RMGlobalConfiguration.EnvSetting.ProductVersion);
//#endif
//                if (cookieContainer != null)
//                {
//                    //logger.Info("test client context request cookie container");
//                    e.WebRequestExecutor.WebRequest.CookieContainer = cookieContainer;
//                }

//            }
//            catch (Exception ex)
//            {
//                logger.Warn("add form based auth error {0}", ex.ToString());
//            }
//        }

//        public ClientContext InitClientContext(AveBPOSAccountInfo bposInfo, string siteUrl)
//        {
//            ClientContext context = null;

//            try
//            {
//                context = RetryUtility.RetryWhen(() =>
//                {
//                    context = new ClientContext(siteUrl);
//                    //var tokenProvider = GetTokenProvider(bposInfo);
//                    //context.SetTokenProvider(tokenProvider);

//                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
//                    Site testSite = context.Site;
//                    context.Load(testSite);
//                    context.ExecuteQuery();
//                    logger.Info("Site URL {0}", context.Site.Url);
//                    return context;
//                }, ShouldRetryErrorMessage, 3);

//            }
//            catch (Exception e)
//            {
//                context?.Dispose();
//                context = null;
//                logger.Info("retry init o365 {0} {1}", siteUrl, e.ToString());
//            }

//            return context;
//        }
//        public ClientContext InitClientContext(AvePoint.RA.Contract.Global.Object.RMSPTreeNode node)
//        {
//            AvePoint.RA.Contract.Global.Object.RMSPTreeNode siteNode = GetSiteCollectionNode(node);
//            siteCollectionNode = siteNode;
//            ClientContext context = null;

//            try
//            {
//                context = RetryUtility.RetryWhen(() =>
//                {
//                    context = new ClientContext(siteNode.FullPath);
//                    var BPOSInfo = GetBPOSInfo();
//                    //CommonPoolUserUtil.GetAveBPOSAccountInfo(siteCollectionNode.BposInfo, siteNode.FullPath);
//                    //var tokenProvider = GetTokenProvider(BPOSInfo);
//                    //context.SetTokenProvider(tokenProvider);
//                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
//                    Site testSite = context.Site;
//                    context.Load(testSite);
//                    context.ExecuteQuery();
//                    logger.Info("Online Site URL {0}", context.Site.Url);
//                    return context;
//                }, ShouldRetryErrorMessage, 3);

//            }
//            catch (Exception e)
//            {
//                context?.Dispose();
//                context = null;
//                logger.Info("retry init o365 {0} {1}", node.FullPath, e.ToString());
//            }

//            return context;
//        }

//        //private ITokenProvider GetTokenProvider(AveBPOSAccountInfo info)
//        //{
//        //    return new SPOIDCLRTokenProvider(info.UserName, info.Password);
//        //}

//        private AveBPOSAccountInfo GetBPOSInfo()
//        {

//            AveBPOSAccountInfo aveBPOSAccountInfo = new AveBPOSAccountInfo()
//            {
//                //Domain = gcBposInfo.UserAccountInfo.Domain,
//                UserName = @"ccso\\jychu",
//                Password = "2wsx3edcR"
//            };
//            var account = AgentAccountUtil.Get();
//            //AveBPOSAccountInfo aveBPOSAccountInfo = new AveBPOSAccountInfo()
//            //{
//            //    Domain = account.Domain,
//            //    UserName = account.UserName,
//            //    Password = account.Password
//            //};

//            return aveBPOSAccountInfo;

//        }

//        public ClientContext InitClientContext(AvePoint.RA.Contract.Global.JobMessage.SiteInfo node)
//        {
//            remoteSiteCollectionNode = node;
//            ClientContext context = null;
//            try
//            {
//                context = RetryUtility.RetryWhen(() =>
//                {
//                    var startTime = DateTime.Now;
//                    context = new ClientContext(remoteSiteCollectionNode.SiteUrl);
//                    var BPOSInfo = GetBPOSInfo();
//                    logger.Warn($"3.2.1 time elapsed for InitClientContext(GetBPOSInfo)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
//                    startTime = DateTime.Now;
//                    //var tokenProvider = GetTokenProvider(BPOSInfo);

//                    //context.SetTokenProvider(tokenProvider);

//                    SecureString ss = new SecureString();
//                    foreach (char c in BPOSInfo.Password)
//                    { ss.AppendChar(c); }
//                    credential = new Microsoft.SharePoint.Client.SharePointOnlineCredentials(BPOSInfo.UserName, ss);
//                    context.Credentials = credential;
//                    logger.Warn($"3.2.2 time elapsed for InitClientContext(SetTokenProvider)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
//                    startTime = DateTime.Now;
//                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
//                    Site testSite = context.Site;
//                    context.Load(testSite);
//                    logger.Warn($"3.2.3 time elapsed for InitClientContext(Load)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
//                    startTime = DateTime.Now;
//                    context.ExecuteQuery();
//                    logger.Warn($"3.2.4 time elapsed for InitClientContext(ExecuteQuery)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
//                    logger.Info("Online Site URL {0}", context.Site.Url);
//                    return context;
//                }, ShouldRetryErrorMessage, 3);

//            }
//            catch (Exception e)
//            {
//                context?.Dispose();
//                context = null;
//                logger.Info("retry init o365 {0} {1}", remoteSiteCollectionNode.SiteUrl, e.ToString());
//            }
//            return context;
//        }

//        public ClientContext InitClientContext(string userName, string password, string siteUrl)
//        {
//            ClientContext context = null;
//            try
//            {
//               // WrapperWin32Native.LoadAssemblyForAuthentication();
//                try
//                {
//                    SPOnlineAuthentication onlineAuth = new SPOnlineAuthentication(siteUrl);
//                    cookieContainer = onlineAuth.Login(userName, password);
//                }
//                catch (Exception le)
//                {
//                    logger.Info("get cookie container failed {0}", le.ToString());
//                }
//                context = new ClientContext(siteUrl);
//                SecureString ss = new SecureString();
//                foreach (char c in password)
//                {
//                    ss.AppendChar(c);
//                }
//                credential = new Microsoft.SharePoint.Client.SharePointOnlineCredentials(userName, ss);
//                context.Credentials = credential;
//                //context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
//                Site testSite = context.Site;
//                context.Load(testSite);
//                context.ExecuteQuery();
//                logger.Info("Online Site URL {0}", context.Site.Url);
//            }
//            catch (Exception e)
//            {
//                context.Dispose();
//                logger.Info("retry init o365 {0} {1}", siteUrl, e.ToString());
//            }
//            return context;
//        }

//        private bool ShouldRetryErrorMessage(Exception e)
//        {
//            return e is TimeoutException
//                || e is UnauthorizedAccessException
//                || e is WebException;
//        }
//        public AvePoint.RA.Contract.Global.Object.RMSPTreeNode GetSiteCollectionNode(AvePoint.RA.Contract.Global.Object.RMSPTreeNode node)
//        {
//            while (node.Level != (int)NodeLevel.SiteCollection)
//            {
//                node = node.Parent;
//            }
//            return node;
//        }

//        public static ClientContext GetClientContextWithAccessToken(string targetUrl, string accessToken)
//        {
//            ClientContext clientContext = new ClientContext(targetUrl);

//            clientContext.AuthenticationMode = ClientAuthenticationMode.Anonymous;
//            clientContext.FormDigestHandlingEnabled = false;
//            clientContext.ExecutingWebRequest +=
//                delegate (object oSender, WebRequestEventArgs webRequestEventArgs)
//                {
//                    webRequestEventArgs.WebRequestExecutor.RequestHeaders["Authorization"] =
//                        "Bearer " + accessToken;
//                };

//            return clientContext;
//        }
//    }
//}
