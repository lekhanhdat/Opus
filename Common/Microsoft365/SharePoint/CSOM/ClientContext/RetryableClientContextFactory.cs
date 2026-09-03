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
namespace Microsoft365.SharePoint.CSOM
{
    using Microsoft.SharePoint.Client;
    using Microsoft365.Authentication.TokenProvider;
    using Microsoft365.Common.Logger;
    using Microsoft365.SharePoint.CSOM.Extension;
    using System;
    using System.Net;

    public class RetryableClientContextFactory:IDisposable
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(RetryableClientContextFactory));
        protected string UserAgent { get; set; }
        protected IATokenProvider TokenProvider { get; set; }
        protected int RetryCount { get; set; }
        protected int RetryInterval { get; set; }
        public RetryableClientContextFactory(string userAgent, IATokenProvider tokenProvider, int retryCount, int retryInterval)
        {
            TokenProvider = tokenProvider;
            UserAgent = userAgent;
            RetryCount = retryCount;
            RetryInterval = retryInterval;
        }
        public RetryableClientContext GetClientContext(string siteUrl)
        {           
            var clientContext = InitialContext(siteUrl);
            clientContext.ExecutingWebRequest -= SetCredential;
            clientContext.ExecutingWebRequest += SetCredential;
            clientContext.SetRefreshToken((ctx) =>
            {
                clientContext.ExecutingWebRequest -= SetCredential;
                clientContext.ExecutingWebRequest += SetCredential;
            });
            return clientContext;
        }

        public RetryableProjectClientContext GetProjectClientContext(string siteUrl)
        {
            var clientContext = InitialProjectContext(siteUrl);
            clientContext.ExecutingWebRequest -= SetProjectContextCredential;
            clientContext.ExecutingWebRequest += SetProjectContextCredential;
            clientContext.SetRefreshToken((ctx) =>
            {
                clientContext.ExecutingWebRequest -= SetProjectContextCredential;
                clientContext.ExecutingWebRequest += SetProjectContextCredential;
            });
            return clientContext;
        }

        private void SetCredential(object sender, WebRequestEventArgs arg)
        {
            var context = sender as ClientContext;
            var accessToken =
                TokenProvider.GetSharePointToken(context.Url, SPTokenType.Adaptation, SPUserType.Adaptation);
            SetToken(context,arg, accessToken);

        }

        private void SetToken(ClientContext context, WebRequestEventArgs arg, AccessTokenResult accessToken)
        {
            logger.Info($"SetToken:{accessToken?.TokenType},HasValue:{string.IsNullOrEmpty(accessToken?.AccessToken)},ValueLength:{accessToken?.AccessToken?.Length}");
            if (accessToken.IsValid())
            {
                switch (accessToken?.TokenType)
                {
                    case Authentication.TokenType.IDCLR:
                        var digestInfo = SharePointContext.GetFormDigestProvider().GetFormDigestForCookieByRestAPI(context.Url, accessToken.AccessToken);
                        if (digestInfo != null)
                        {
                            logger.Error($"digestInfo:{digestInfo.DigestValue}");
                            arg.WebRequestExecutor.RequestHeaders["X-RequestDigest"] = digestInfo.DigestValue;

                            if (digestInfo.RequestSchemaVersion != null)
                            {
                                context.RequestSchemaVersion = digestInfo.RequestSchemaVersion;
                            }
                        }
                        else
                        {
                            logger.Error($"digestInfo:is null");
                        }
                        arg.WebRequestExecutor.RequestHeaders["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
                        arg.WebRequestExecutor.RequestHeaders[HttpRequestHeader.Cookie] = accessToken.AccessToken;
                        arg.WebRequestExecutor.WebRequest.UserAgent = UserAgent;
                        break;
                    case Authentication.TokenType.Bearer:
                        arg.WebRequestExecutor.WebRequest.UserAgent = UserAgent;
                        arg.WebRequestExecutor.WebRequest.Headers["Authorization"] = $"Bearer {accessToken.AccessToken}";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                logger.Error("token is null.");
                if (accessToken?.Exception != null)
                {
                    throw accessToken.Exception;
                }
                throw new ArgumentNullException("accessToken");
            }
        }

        private void SetProjectContextCredential(object sender, WebRequestEventArgs arg)
        {
            var context = sender as ClientContext;
            var accessToken = TokenProvider.GetSharePointToken(context.Url, SPTokenType.Adaptation, SPUserType.ServiceAccount);
            SetToken(context,arg, accessToken);
        }

        private RetryableProjectClientContext InitialProjectContext(string siteUrl)
        {
            var context = new RetryableProjectClientContext(siteUrl, RetryCount, RetryInterval, true);
            return context;
        }

        private RetryableClientContext InitialContext(string siteUrl)
        {
            var context = new RetryableClientContext(siteUrl,RetryCount,RetryInterval,true);
            return context;
        }

        public void Dispose()
        {
            TokenProvider = null;
        }
    }
}