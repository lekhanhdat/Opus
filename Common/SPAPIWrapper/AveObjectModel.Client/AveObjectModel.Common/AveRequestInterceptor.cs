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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
//using Microsoft365.SharePoint;

namespace AvePoint.ObjectModel.Common
{
    public delegate object MethodInvokeBeforeHook(IAveRequest request, MethodInfo targetMethod, object[] args);

    public delegate void MethodInvokeAfterHook(IAveRequest request, MethodInfo targetMethod, object[] args, object userData);

    //only use for performance log
    public class AveRequestInterceptor : DispatchProxy
    {
        private IAveRequest mTarget;
        private static IAveLogger logger = AveLogger.GetInstance(typeof(AveRequestInterceptor));
        private IAveRequest mProxy;
        private static readonly List<string> mSkipedKeys = new List<string> { "ParentWebAllProperties", "AllPropertiesObject", "SchemaXml" };

        private static readonly List<string> mDiscoverMethods = new List<string>
        { "QueryListItemForFB", "QueryItemForFB", "QueryListForIB","QueryListItemForIB","QueryListRootFolder",
        "QueryWebForIB", "QueryWebRootFolder", "QueryRootWeb","QueryWeb","QueryWebListForFB","GetSiteChangedForIB" };

        private string mSiteUrl = string.Empty;
        private AveBPOSAccountInfo mUserAccountInfo = null;
        private string domainName = string.Empty;

        public event MethodInvokeBeforeHook PreHook;

        public event MethodInvokeAfterHook PostHook;

        internal class AveRequstCache
        {
            public DateTime LastModified;
            public AveBPOSAccountInfo UserInfo;
            public object Proxy;
        }

        //主要解决Dictionary  Stack多线程不安全问题。
        private static class RequestCacheLockManager
        {
            private readonly static object syncObjct = new object();
            private static Dictionary<string, Stack<AveRequstCache>> requestCache = new Dictionary<string, Stack<AveRequstCache>>(StringComparer.OrdinalIgnoreCase);

            public static AveRequstCache GetAveRequstCacheFromStack(string siteUrl)
            {
                AveRequstCache aveRequstCache = null;
                lock (syncObjct)
                {
                    Stack<AveRequstCache> aveRequstCacheStack;
                    if (requestCache.TryGetValue(siteUrl, out aveRequstCacheStack))
                    {
                        if (aveRequstCacheStack.Count > 0)
                        {
                            aveRequstCache = aveRequstCacheStack.Pop();
                        }
                    }
                }
                return aveRequstCache;
            }

            public static void AddAveRequstCacheToStack(string siteUrl, AveRequstCache aveRequest)
            {
                lock (syncObjct)
                {
                    Stack<AveRequstCache> aveRequstCacheStack;
                    if (!requestCache.TryGetValue(siteUrl, out aveRequstCacheStack))
                    {
                        aveRequstCacheStack = new Stack<AveRequstCache>();
                        requestCache[siteUrl] = aveRequstCacheStack;
                    }
                    aveRequstCacheStack.Push(aveRequest);
                }
            }
        }
        public AveRequestInterceptor()
            : base()
        { }
        public AveRequestInterceptor(string mSiteUrl, AveBPOSAccountInfo mUserAccountInfo)
            : base()
        {
            InitialRequest(mSiteUrl, mUserAccountInfo);
            this.mTarget = GetAvailableRequest(mSiteUrl, mUserAccountInfo);
        }

        private void InitialRequest(string mSiteUrl, AveBPOSAccountInfo mUserAccountInfo)
        {
            this.mSiteUrl = mSiteUrl;
            this.mUserAccountInfo = mUserAccountInfo;

            if (mUserAccountInfo != null && !string.IsNullOrEmpty(mUserAccountInfo.UserName))
            {
                var index = mUserAccountInfo.UserName.IndexOf('@');
                if (index >= 0 && index + 1 < mUserAccountInfo.UserName.Length)
                {
                    domainName = mUserAccountInfo.UserName.Substring(index + 1);
                }
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
            while (true)
            {
                cache = RequestCacheLockManager.GetAveRequstCacheFromStack(siteUrl);
                if (cache == null)
                {
                    requestAvailable = false;
                    break;
                }
                requestAvailable = IsTimeout(TimeSpan.FromMinutes(30), cache.LastModified) ? false : cache.UserInfo != null && cache.UserInfo.Equals(mUserAccountInfo);
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
            if (requestAvailable && cache != null)
            {
                availableRequest = cache.Proxy as IAveRequest;
                this.mProxy = availableRequest;
                this.mSiteUrl = availableRequest.Url;
            }
            else
            {
                AveClientRequest cRequest = new AveClientRequest(siteUrl, userAccountInfo);
                availableRequest = cRequest.InitRequest();
                this.mProxy = CreateProxy(availableRequest);
                this.mSiteUrl = availableRequest.Url;
            }
            return availableRequest;
        }

        private bool IsTimeout(TimeSpan checkInterval, DateTime dateTime)
        {
            return DateTime.UtcNow - dateTime > checkInterval;
        }

        /// <summary>
        /// Dispose available request and put it back to cache
        /// </summary>
        /// <param name="request"></param>
        /// <param name="siteUrl"></param>
        public static void DisposeAvailableRequest(AveRequestParameter requestParameter, string siteUrl)
        {
            if (!requestParameter.IsDisposed)
            {
                requestParameter.AveRequest.Dispose(true);
                AveRequstCache fakeRequest = new AveRequstCache() { LastModified = DateTime.UtcNow, Proxy = requestParameter.AveRequest, UserInfo = requestParameter.UserInfo };
                RequestCacheLockManager.AddAveRequstCacheToStack(siteUrl, fakeRequest);
                requestParameter.Dispose();
            }
        }

        protected object OnPreRequest(MethodInfo targetMethod, object[] args)
        {
            if (PreHook != null)
            {
                return PreHook(mTarget, targetMethod, args);
            }
            return null;
        }

        protected void OnPostRequest(MethodInfo targetMethod, object[] args, object userData)
        {
            if (PostHook != null)
            {
                PostHook(mTarget, targetMethod, args, userData);
            }
        }

        private void PreLog(IAveRequest request, MethodInfo targetMethod, object[] args)
        {
            if (/*logger.IsDebugEnabled &&*/ targetMethod != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("method: {0}, args: [", targetMethod.Name);
                if (args != null && !mDiscoverMethods.Contains(targetMethod.Name))
                {
                    AveJsonSerializer serializer = new AveJsonSerializer(SkipMappingEntry, SkipAveAssembly);
                    string jsonArgs = serializer.SerializeToJson(args);
                    sb.AppendFormat("{0}, ", jsonArgs);
                }
                else
                {
                    sb.Append("Unnecessary");
                }
                sb.Append("]");
                sb.Append("\tMemory used: " + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / (1000.00 * 1024));
                //logger.Debug(sb.ToString());
                logger.Warn(sb.ToString());
            }
        }

        private bool SkipMappingEntry(DictionaryEntry entry)
        {
            return mSkipedKeys.Contains(entry.Key as string, StringComparer.OrdinalIgnoreCase);
        }

        private bool SkipAveAssembly(object o)
        {
            if (o != null)
            {
                Type type = o.GetType();
                if (type == typeof(AveDocumentInfo))
                {
                    return false;
                }
                Assembly currentAssembl = type.Assembly;
                return typeof(AveList).Assembly.Equals(currentAssembl) || typeof(IAveList).Assembly.Equals(currentAssembl);
            }
            return false;
        }

        private void EndLog(IAveRequest request, MethodInfo targetMethod, object[] args,object result)
        {
            if (targetMethod != null && WrapperConfiguration.WrapperConfigurationForBPOS.DetailLog)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("method: {0}, return: ", targetMethod.Name);
                if (result != null)
                {
                    AveJsonSerializer serializer = new AveJsonSerializer(SkipMappingEntry, SkipAveAssembly);
                    string returnValue = serializer.SerializeToJson(result);
                    sb.Append(returnValue);
                }
                logger.Debug(sb.ToString());
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "AveSecurityTriming is a part of keys")]
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            object result;
            object userData = OnPreRequest(targetMethod, args);
            //PreLog(mTarget, msg);
            //EnsureCredentialIsValid(mTarget.Credentials);
            using (AvePerformanceScope performanceScope = new AvePerformanceScope("Wrapper.Client." + targetMethod.Name))
            {
                try
                {
                    result = targetMethod.Invoke(mTarget, args);
                }
                catch (TargetInvocationException te)
                {
                    PreLog(mTarget, targetMethod, args);
                    result = HandleTargetInvocationException(te, targetMethod, args);
                }
                catch (Exception e)
                {
                    logger.Error("method {0} completed failed due to: message: {1}, stack trace: {2}", targetMethod.Name, e.Message, e.StackTrace);
                    //message = new ReturnMessage(new AveRPCException(mcMsg.MethodBase, mcMsg.Args, e, e.Message), mcMsg);
                    throw;
                }
                OnPostRequest(targetMethod,args, userData);
                EndLog(mTarget, targetMethod, args,result);
            }
            return result;
        }

        //when there is a socket exception, we will try again, cause when there are 30 jobs running concurrently, connection abort exeption will be throwed randomly
        private object Retry(MethodInfo targetMethod, object[] args, int times, int interval)
        {
            object result = null;
            for (int i = 0; i < times; i++)
            {
                try
                {
                    logger.Info("retry {0} {1} time", targetMethod.Name, i + 1);
                    if (interval > 0)
                    {
                        Thread.Sleep(interval);
                    }
                    result=targetMethod.Invoke(mTarget, args);
                    break;
                }
                catch (TargetInvocationException te)
                {
                    logger.Error("retry method {0} completed failed due to: message: {1}", targetMethod.Name, te.ToString());
                    if (i == times - 1)
                    {
                        //message = new ReturnMessage(new AveRPCException(mcMsg.MethodBase, mcMsg.Args, te.InnerException, te.InnerException.Message), mcMsg);
                        if (te.InnerException is AveInternalException)
                        {
                           throw te.InnerException;
                        }
                        else
                        {
                            throw new AveInternalException(te.InnerException.Message, te.InnerException);
                        }
                    }
                }
            }
            return result;
        }

        private object HandleTargetInvocationException(TargetInvocationException te, MethodInfo targetMethod, object[] args)
        {
            string innerErrorMessage = string.Empty;
            var innerEx = te.InnerException;
            if (innerEx is SPListExistException || innerEx is AveWrapperI18NException || innerEx is AveChangeTokenExpireException)
            {
                throw innerEx;
            }
            else if (innerEx.GetType().FullName.Equals("System.Web.Services.Protocols.SoapException"))
            {
                try
                {
                    XmlNode detail = (XmlNode)innerEx.GetType().GetProperty("Detail", BindingFlags.Public)?.GetValue(innerEx);
                    innerErrorMessage = detail?.InnerText;
                }
                catch
                {
                    innerErrorMessage = innerEx.ToString();
                }
            }
            else
            {
                innerErrorMessage = string.IsNullOrEmpty(innerEx.Message) ? te.Message : innerEx.Message;
            }

            if (RequestExceptionHanddler.IsResourceUsageException(innerEx))
            {
                logger.Warn("using health score to retry method {0} completed failed due to: message: {1}, stack trace: {2}",
                   targetMethod.Name, innerErrorMessage, string.IsNullOrEmpty(innerEx.StackTrace) ? te.StackTrace : innerEx.StackTrace);

                HealthScoreUtility.Process(new Uri(mSiteUrl), 0, (HttpStatusCode)429);

                return Retry(targetMethod,args, WrapperConfiguration.WrapperConfigurationForBPOS.RetryCount, WrapperConfiguration.WrapperConfigurationForBPOS.RetryInterval);
            }

            var internalException = innerEx as AveInternalException;
            if (internalException != null && internalException.NeedRetry)
            {
                return Retry(targetMethod, args, WrapperConfiguration.WrapperConfigurationForBPOS.RetryCount, WrapperConfiguration.WrapperConfigurationForBPOS.RetryInterval * 2);
            }

            //if (RequestExceptionHanddler.IsForbiddenWebException(te))
            //{
            //    logger.Warn("method {0} completed failed due to forbidden exception, start to retry get message.", mcMsg.MethodName);
            //    return Retry(mcMsg, 2, WrapperConfiguration.BPOS_S.RetryInterval);
            //}

            //int retryInterval = -1;
            //if (RequestExceptionHanddler.IsConnectonForciblyClosedExceptioin(te)
            //    || RequestExceptionHanddler.IsTimedoutException(te, ref retryInterval)
            //    || RequestExceptionHanddler.IsRetryableWebException(te, ref retryInterval)
            //   // || RequestExceptionHanddler.IsToomanyRequestError(te)
            //    || RequestExceptionHanddler.IsEndValueOutOfRangeException(te))
            //{
            //    logger.Warn("method {0} completed failed,need retry due to: message: {1}, stack trace: {2}", mcMsg.MethodName, innerErrorMessage, te.InnerException == null ? te.StackTrace : te.InnerException.StackTrace);
            //    return Retry(mcMsg, 2, retryInterval);
            //}
            //else
            {
                logger.Error("method {0} completed failed due to: message: {1}, stack trace: {2}",
                    targetMethod.Name, innerErrorMessage, string.IsNullOrEmpty(innerEx.StackTrace) ? te.StackTrace : innerEx.StackTrace);
                //if (mDiscoverMethods.Contains(mcMsg.MethodName) || innerErrorMessage.Contains("System.OutOfMemoryException"))
                //{
                //    logger.Error("Discover exception, need throw");
                //    throw;
                //}
                //AveRPCException rpcException = new AveRPCException(mcMsg.MethodBase, mcMsg.Args, te.InnerException, innerErrorMessage);

                switch (innerEx.GetType().FullName)
                {
                    case "Microsoft.SharePoint.Client.ServerException":
                        PropertyInfo prop = innerEx.GetType().GetProperty("ServerErrorCode", BindingFlags.Public | BindingFlags.Instance);
                        //rpcException.Details.Add("ServerErrorCode", prop.GetValue(te.InnerException, null).ToString());
                        //rpcException.Details.Add("ExceptionType", "Microsoft.SharePoint.Client.ServerException");
                        int serverErrorCode = Convert.ToInt32(prop.GetValue(innerEx, null));
                        if (serverErrorCode == AveSPErrorCode.TP_E_OVERQUOTA ||
                            serverErrorCode == AveSPErrorCode.V_OVER_QUOTA ||
                            serverErrorCode == AveSPErrorCode.ERROR_NOT_ENOUGH_QUOTA)
                        {
                            throw new AveExceedStorageLimitException(WrapperReportResourceKey.Wrapper_ExceedStorageLimit.ToString(), AvePoint.Wrapper.Resource.WrapperRestoreReportResource.Wrapper_ExceedStorageLimit, innerEx);
                        }
                        if (serverErrorCode == AveSPErrorCode.TP_E_USER_DOESNOT_EXIST)
                        {
                            throw new AveWrapperUserNotFoundOrNotUniqueException(innerErrorMessage, innerEx);
                        }
                        break;
                    case "Microsoft.SharePoint.Client.ClientRequestException":
                        if (!string.IsNullOrEmpty(innerEx.Message) &&
                            (innerEx.Message.IndexOf("Access to this Web site has been blocked.", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            innerEx.Message.IndexOf("RM_RS_ScanSiteNoAccessError", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            var lockEx = new AveSkipLockSiteException(innerEx.Message, innerEx);
                            lockEx.SiteState = SiteState.NoAccess;
                            throw lockEx;
                        }
                        break;
                    default:
                        break;
                }

                if (innerEx is AveInternalException)
                {
                    throw innerEx;
                }
                else
                {
                    throw new AveInternalException(innerErrorMessage, innerEx);
                }
            }
        }

        private static IAveRequest CreateProxy(IAveRequest target)
        {
            var proxy = Create<IAveRequest, AveRequestInterceptor>()
                as AveRequestInterceptor;
            proxy.InitialRequest(target.Url, target.BposInfo);
            proxy.mTarget = target;

            return proxy as IAveRequest;
        }


        public IAveRequest Proxy
        {
            get
            {
                if (mProxy != null)
                {
                    return mProxy;
                }
                return CreateProxy(mTarget);
            }
        }
    }
}