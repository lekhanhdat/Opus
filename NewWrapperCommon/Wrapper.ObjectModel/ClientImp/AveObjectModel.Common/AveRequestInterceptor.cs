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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.ObjectModel.WebService;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.ObjectModel.CompoundRequest;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Services;
using System.Diagnostics;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Web.Services.Protocols;
using System.Threading;

namespace AvePoint.ObjectModel.Common
{
    public delegate object MethodInvokeBeforeHook(IAveRequest request, IMessage message);

    public delegate void MethodInvokeAfterHook(IAveRequest request, IMessage message, object userData);

    //only use for performance log
    class AveRequestInterceptor : RealProxy
    {
        private IAveRequest mTarget;
        private Type mTargetType;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveRequestInterceptor));
        private object mProxy;

        private string mSiteUrl = string.Empty;
        private AveBPOSAccountInfo mUserAccountInfo = null;
        private string mSPVersion = string.Empty;

        internal AveAuthenticationMode AuthMode;

        public event MethodInvokeBeforeHook PreHook;
        public event MethodInvokeAfterHook PostHook;

        internal class AveRequstCache
        {
            public DateTime LastModified;
            public object Proxy;
            public string SPVersion;
            public AveAuthenticationMode AuthMode;
        }
        private static Dictionary<string, Dictionary<string, Stack<AveRequstCache>>> RequestCache = new Dictionary<string, Dictionary<string, Stack<AveRequstCache>>>(StringComparer.OrdinalIgnoreCase);
        private static object synObj = new object();

        public AveRequestInterceptor(string mSiteUrl, AveBPOSAccountInfo mUserAccountInfo)
            : base(typeof(IAveRequest))
        {
            this.mSiteUrl = mSiteUrl;
            this.mUserAccountInfo = mUserAccountInfo;
            this.mTarget = GetAvailableRequest(mSiteUrl, mUserAccountInfo);
            this.mTargetType = mTarget.GetType();
        }

        private static bool ContainsUrlForMutiThread(string url, string userName)
        {
            lock (RequestCache)
            {
                if(RequestCache.ContainsKey(url))
                {
                    return RequestCache[url].ContainsKey(userName);
                }
                return false;
            }
        }

        /// <summary>
        /// Get an available request from cache. (Threads sync safe)
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="userAccountInfo"></param>
        /// <returns>Get an available request</returns>
        private IAveRequest GetAvailableRequest(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            IAveRequest availableRequest = null;
            bool requestAvailable = true;
            AveRequstCache cache = null;
            if (AveRequestInterceptor.RequestCache == null || (!string.IsNullOrEmpty(userAccountInfo.GetAccountName()) && !AveRequestInterceptor.ContainsUrlForMutiThread(siteUrl, userAccountInfo.GetAccountName())))
            {
                requestAvailable = false;
            }
            else
            {
                Stack<AveRequstCache> caches;
                lock (RequestCache)
                {
                    caches = AveRequestInterceptor.RequestCache[siteUrl][userAccountInfo.GetAccountName()];
                }
                while (caches.Count > 0)
                {
                    lock (synObj)
                    {
                        cache = caches.Pop();
                    }
                    if ((DateTime.UtcNow - cache.LastModified).Minutes >= 30)
                    {
                        requestAvailable = false;
                    }
                    else
                    {
                        //check cookie, will move to another function later
                        //AveClientOMRequest clientRequest = cache.CacheRequest as AveClientOMRequest;
                        IAveRequest reqProxy = cache.Proxy as IAveRequest;
                        if (reqProxy != null)
                        {
                            System.Net.CookieContainer netCookies = reqProxy.Credentials as System.Net.CookieContainer;
                            if (netCookies != null && netCookies.GetCookies(new Uri(siteUrl))[0].Expired)
                            {
                                requestAvailable = false;
                            }
                        }
                    }
                    if (requestAvailable)
                    {
                        break;
                    }
                    else
                    {// here we should dispose this request in cache;                     
                        IDisposable disobj = cache.Proxy as IDisposable;
                        if (disobj != null)
                        {
                            disobj.Dispose();
                        }
                    }
                }
            }
            if (requestAvailable && cache != null)
            {
                availableRequest = cache.Proxy as IAveRequest;
                //cache.LastModified = DateTime.UtcNow;
                this.mProxy = availableRequest;
                mSPVersion = cache.SPVersion;
                this.AuthMode = cache.AuthMode;
            }
            else
            {
                AveClientRequest cRequest = new AveClientRequest(siteUrl, userAccountInfo, AuthenticationModeOptionDefaultValue.DefaultValue);
                availableRequest = cRequest.GetInnerRequest();
                this.AuthMode = cRequest.AveAuthMode;
                this.mProxy = base.GetTransparentProxy();
                this.mSPVersion = cRequest.SPVersion != null ? cRequest.SPVersion.Version : string.Empty;

            }
            return availableRequest;
        }
        /// <summary>
        /// Dispose available request and put it back to cache
        /// </summary>
        /// <param name="request"></param>
        /// <param name="siteUrl"></param>
        public static void DisposeAvailableRequest(AveRequestParameter requestParameter, string siteUrl, string userName)
        {
            if (!requestParameter.IsDisposed)
            {
                Stack<AveRequstCache> targetStack;
                lock (RequestCache)
                {
                    if (!ContainsUrlForMutiThread(siteUrl, userName))
                    {
                        var temp = new Dictionary<string, Stack<AveRequstCache>>();
                        temp.Add(userName, new Stack<AveRequstCache>());
                        AveRequestInterceptor.RequestCache[siteUrl] = temp;
                    }
                    targetStack = AveRequestInterceptor.RequestCache[siteUrl][userName];
                }
                requestParameter.AveRequest.Dispose(true);
                AveRequstCache fakeRequest = new AveRequstCache()
                {
                    LastModified = DateTime.UtcNow,
                    Proxy = requestParameter.AveRequest,
                    SPVersion = requestParameter.SPVersion,
                    AuthMode = requestParameter.AuthMode
                };
                lock (synObj)
                {
                    targetStack.Push(fakeRequest);
                }
                requestParameter.Dispose();
            }
        }

        protected object OnPreRequest(IMessage msg)
        {
            if (PreHook != null)
            {
                return PreHook(mTarget, msg);
            }
            return null;
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        protected void OnPostRequest(IMessage msg, object userData)
        {
            if (PostHook != null)
            {
                PostHook(mTarget, msg, userData);
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        private void PreLog(IAveRequest request, IMessage message)
        {
            IMethodCallMessage callMsg = message as IMethodCallMessage;
            if (mLogger.IsDebugEnabled && callMsg != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("method: {0}, args: [", callMsg.MethodName);
                if (callMsg.Args != null)
                {
                    AveJsonSerializer serializer = new AveJsonSerializer();
                    string jsonArgs = serializer.SerializeToJson(callMsg.Args);
                    sb.AppendFormat("{0}, ", jsonArgs.GCommonLogBase64());
                }
                sb.Append("]");
                sb.Append("\tMemory used: " + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / (1000.00 * 1024));
                mLogger.Debug(sb.ToString());
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        private void EndLog(IAveRequest request, IMessage message)
        {
            IMethodCallMessage callMsg = message as IMethodCallMessage;
            if (mLogger.IsDebugEnabled && callMsg != null)
            {
                mLogger.Debug("method {0} completed successfully", callMsg.MethodName);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "AveSecurityTriming is a part of keys")]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        public override IMessage Invoke(IMessage msg)
        {
            IMessage message = null;
            IMethodCallMessage mcMsg = msg as IMethodCallMessage;

            object userData = OnPreRequest(msg);
            PreLog(mTarget, msg);
            using (AvePerformanceScope performanceScope = new AvePerformanceScope("Wrapper.Client." + mcMsg.MethodName))
            {
                try
                {
                    object[] args = mcMsg.Args;
                    object ret = AveAssemblyUtility.InvokeMethod(mTarget, mcMsg.MethodName, mcMsg.MethodSignature as Type[], args);
                    message = new ReturnMessage(ret, args, args.Length, mcMsg.LogicalCallContext, mcMsg);
                }
                catch (TargetInvocationException te)
                {
                    string innerErrorMessage = string.Empty;
                    if (te.InnerException is SoapException)
                    {
                        SoapException se = AveExceptionHelper.GetCertainInnerException<SoapException>(te.InnerException);
                        innerErrorMessage = string.IsNullOrEmpty(se.Detail.InnerText) ? se.Message : se.Detail.InnerText;
                    }
                    else
                    {
                        innerErrorMessage = te.InnerException == null ? te.Message : te.InnerException.Message;
                    }

                    if (innerErrorMessage.Contains("Access denied. You do not have permission to perform this action or access this resource.")
                        || innerErrorMessage.Contains("0x80070005")//Access is denied.
                        || innerErrorMessage.Contains("AveSecurityTriming")
                        || innerErrorMessage.Contains("You don't have Add and Customize Pages permissions required to perform this action.")
                        || (te.InnerException != null && te.InnerException is AveSecurityTrimingException))
                    {
                        mLogger.Warn("Method {0} completed failed, due to: message: {1}, stack trace: {2}.", mcMsg.MethodName, innerErrorMessage, te.InnerException == null ? te.StackTrace : te.InnerException.StackTrace);
                        throw new AveSecurityTrimingException(innerErrorMessage, te.InnerException);
                    }
                    mLogger.Error("Method {0} completed failed, due to: message: {1}, stack trace: {2}.", mcMsg.MethodName, innerErrorMessage, te.InnerException == null ? te.StackTrace : te.InnerException.StackTrace);
                    int retryInterval = -1;
                    if (AveExceptionHelper.IsConnectionException(te) || AveExceptionHelper.IsHTTP429Error(te, ref retryInterval))
                    {
                        return Retry(mcMsg, 2, retryInterval);
                    }
                    else if (IsNotFoundException(te))
                    {
                        message = new ReturnMessage(te.InnerException, mcMsg);
                    }
                    else
                    {
                        message = new ReturnMessage(new AveRPCException(mcMsg.MethodBase, mcMsg.Args, te.InnerException, innerErrorMessage), mcMsg);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Method {0} completed failed, due to: message: {1}, stack trace: {2}.", mcMsg.MethodName, e.Message, e.StackTrace);
                    message = new ReturnMessage(new AveRPCException(mcMsg.MethodBase, mcMsg.Args, e, e.Message), mcMsg);
                }
                OnPostRequest(msg, userData);
                EndLog(mTarget, msg);
            }
            return message;
        }

        private bool IsSessionTimeout()
        {
            if (!string.IsNullOrEmpty(mSiteUrl))
            {
                CookieContainer cookieContainer = mTarget.Credentials as CookieContainer;
                if (cookieContainer != null)
                {
                    CookieCollection cookieCollection = cookieContainer.GetCookies(new Uri(mSiteUrl));
                    foreach (Cookie cookie in cookieCollection)
                    {
                        if (cookie != null && cookie.Expired)
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        private bool IsNotFoundException(Exception e)
        {
            if (e is AveSiteNotFoundException)
            {
                return true;
            }
            WebException webException = e as WebException;
            if (webException != null)
            {
                HttpWebResponse clientResponse = webException.Response as HttpWebResponse;
                if (clientResponse == null)
                {
                    return false;
                }
                else if (clientResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return true;
                }
            }
            if (e.InnerException != null)
            {
                return IsNotFoundException(e.InnerException);
            }
            return false;
        }

        //when there is a socket exception, we will try again, cause when there are 30 jobs running concurrently, connection abort exeption will be throwed randomly
        private IMessage Retry(IMethodCallMessage mcMsg, int times, int interval)
        {
            IMessage message = null;
            for (int i = 0; i < times; i++)
            {
                try
                {
                    if (interval > 0)
                    {
                        Thread.Sleep(interval);
                    }
                    PreLog(mTarget, mcMsg);
                    object ret = AveAssemblyUtility.InvokeMethod(mTarget, mcMsg.MethodName, mcMsg.MethodSignature as Type[], mcMsg.Args);
                    message = new ReturnMessage(ret, null, 0, null, mcMsg);
                    break;
                }
                catch (TargetInvocationException te)
                {
                    mLogger.Error("Retry method {0} completed failed, due to: message: {1}.", mcMsg.MethodName, te.ToString());
                    if (!AveExceptionHelper.IsConnectonForciblyClosedExceptioin(te) || (i == times - 1))
                    {
                        message = new ReturnMessage(new AveRPCException(mcMsg.MethodBase, mcMsg.Args, te.InnerException, te.InnerException.Message), mcMsg);
                        break;
                    }
                }
            }
            return message;
        }

        public IAveRequest Proxy
        {
            get
            {
                if (mProxy != null)
                {
                    return mProxy as IAveRequest;
                }
                var testproxy = base.GetTransparentProxy();
                return base.GetTransparentProxy() as IAveRequest;
            }
        }

        public string SPVersion
        {
            get
            {
                return mSPVersion;
            }
        }
    }
}
