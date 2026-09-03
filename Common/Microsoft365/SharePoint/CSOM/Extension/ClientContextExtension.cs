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
namespace Microsoft365.SharePoint.CSOM.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;
    using Microsoft.SharePoint.Client;
    using Microsoft365.Authentication;
    using Microsoft365.Common.Exception;
    using Microsoft365.Common.Logger;
    using Microsoft365.Common.Utility;
    using Microsoft365.Configuration;
    using Microsoft365.SharePoint;
    [Obsolete]

    public static class ClientContextExtension
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(ClientContextExtension));
        public static void ResetContext<TContext>(this TContext context, ITokenProvider tokenProvider)
           where TContext : ClientContext
        {
            if (tokenProvider != null)
            {
                context.Credentials = tokenProvider;

                context.ExecutingWebRequest -= SetFormDigest;
                context.ExecutingWebRequest -= SetCredential;
                context.ExecutingWebRequest -= RefreshCredential;
                context.ExecutingWebRequest -= RefreshFormDigest; //avoid duplicate

                context.ExecutingWebRequest += RefreshFormDigest;
                context.ExecutingWebRequest += SetCredential;
            }
        }

        private static void RefreshFormDigest(object sender, WebRequestEventArgs e)
        {
            var context = sender as ClientContext;

            if (context != null)
            {
                context.ExecutingWebRequest -= SetCredential;
                context.ExecutingWebRequest -= RefreshFormDigest;
                context.ExecutingWebRequest += RefreshCredential; //重新获取digestInfo的request中需要先获取token
                var digestInfo = SharePointContext.GetFormDigestProvider().GetFormDigest(context.Url, context.Credentials as ITokenProvider);
                context.ExecutingWebRequest -= RefreshCredential;
                context.ExecutingWebRequest += SetCredential;  //上述操作重新获取token之后不用再重新获取
                context.ExecutingWebRequest += SetFormDigest;

                if (digestInfo == null || digestInfo.DigestValue == null)
                {
                    logger.Error("[SetFormDigest]FormDigestInfo is null. Context.Url:{0}", context.Url);
                }
                else
                {
                    e.WebRequestExecutor.RequestHeaders["X-RequestDigest"] = digestInfo.DigestValue;

                    if (digestInfo.RequestSchemaVersion != null)
                    {
                        context.RequestSchemaVersion = digestInfo.RequestSchemaVersion;
                    }
                }
            }
        }

        private static void RefreshCredential(object sender, WebRequestEventArgs e)
        {
            var context = sender as ClientContext;
            var tokenProvider = context.Credentials as ITokenProvider;

            if (tokenProvider != null)
            {
                if (tokenProvider.TokenType == TokenType.Bearer)
                {
                    e.WebRequestExecutor.RequestHeaders[HttpRequestHeader.Authorization] = tokenProvider.GetToken(new Uri(context.Url));
                }
                else if (tokenProvider.TokenType == TokenType.IDCLR)
                {
                    e.WebRequestExecutor.RequestHeaders["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
                    e.WebRequestExecutor.RequestHeaders[HttpRequestHeader.Cookie] = tokenProvider.GetToken(new Uri(context.Url), true);
                }
                else
                {
                    throw new Microsoft365ApiException(Mirosoft365ApiErrorMessage.TokenProviderNotSupportedFormat(tokenProvider.TokenType), Microsoft365ApiErrorCode.TokenProviderNotSupported);
                }
            }
        }

        public static void SetFormDigest<TContext>(this TContext context)
            where TContext : ClientContext
        {
            context.ExecutingWebRequest -= SetFormDigest;//avoid duplicate
            context.ExecutingWebRequest += SetFormDigest;
        }

        public static void SetFormDigest(object sender, WebRequestEventArgs e)
        {
            var context = sender as ClientContext;
            if (context != null)
            {
                context.ExecutingWebRequest -= SetFormDigest;
                var digestInfo = SharePointContext.GetFormDigestProvider().GetFormDigest(context.Url, context.Credentials as ITokenProvider);
                context.ExecutingWebRequest += SetFormDigest;
                if (digestInfo == null || digestInfo.DigestValue == null)
                {
                    try
                    {
                        var tokenProvider = context.Credentials as ITokenProvider;
                        if (tokenProvider != null && tokenProvider.TokenType != TokenType.Bearer)
                        {
                            logger.Error("[SetFormDigest]FormDigestInfo is null. Context.Url:{0}", context.Url);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Error occurred while [SetFormDigest]. Context.Url:{0},{1}", context.Url, ex);
                    }
                    //context.ExecutingWebRequest -= SetFormDigest;
                }
                else
                {
                    e.WebRequestExecutor.RequestHeaders["X-RequestDigest"] = digestInfo.DigestValue;

                    if (digestInfo.RequestSchemaVersion != null)
                    {
                        context.RequestSchemaVersion = digestInfo.RequestSchemaVersion;
                    }
                }

            }
        }

        public static void SetTokenProvider(this ClientContext context, ITokenProvider tokenProvider)
        {
            if (tokenProvider != null)
            {
                context.Credentials = tokenProvider;
                context.ExecutingWebRequest -= SetCredential; //avoid duplicate
                context.ExecutingWebRequest += SetCredential;
            }
        }

        public static void SetCredential(object sender, WebRequestEventArgs e)
        {
            var context = sender as ClientContext;
            var tokenProvider = context.Credentials as ITokenProvider;

            if (tokenProvider != null)
            {
                if (tokenProvider.TokenType == TokenType.Bearer)
                {
                    e.WebRequestExecutor.RequestHeaders[HttpRequestHeader.Authorization] = tokenProvider.GetToken(new Uri(context.Url));
                }
                else if (tokenProvider.TokenType == TokenType.IDCLR)
                {
                    e.WebRequestExecutor.RequestHeaders["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
                    e.WebRequestExecutor.RequestHeaders[HttpRequestHeader.Cookie] = tokenProvider.GetToken(new Uri(context.Url));
                }
                else
                {
                    throw new Microsoft365ApiException(Mirosoft365ApiErrorMessage.TokenProviderNotSupportedFormat(tokenProvider.TokenType), Microsoft365ApiErrorCode.TokenProviderNotSupported);
                }
            }
        }

        private static Func<ClientRuntimeContext, Dictionary<long, ObjectPath>> readContextObjectPaths;
        private static Action<ClientRuntimeContext, Dictionary<long, ObjectPath>> writeContextObjectPaths;

        public static Dictionary<long, ObjectPath> ReadObjectPaths(this ClientRuntimeContext context)
        {
            if (readContextObjectPaths == null)
            {
                var fieldInfo = typeof(ClientRuntimeContext).GetField("m_objectPaths", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

                readContextObjectPaths = TypeInvoker.CreateGetter<ClientRuntimeContext, Dictionary<long, ObjectPath>>(fieldInfo);
            }

            return readContextObjectPaths(context);
        }

        public static void WriteObjectPaths(this ClientRuntimeContext context, Dictionary<long, ObjectPath> value)
        {
            if (writeContextObjectPaths == null)
            {
                var fieldInfo = typeof(ClientRuntimeContext).GetField("m_objectPaths", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

                writeContextObjectPaths = TypeInvoker.CreateSetter<ClientRuntimeContext, Dictionary<long, ObjectPath>>(fieldInfo);
            }

            writeContextObjectPaths(context, value);
        }
    }
}