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
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using AvePoint.GCommon;
using Microsoft365.Authentication;
using Microsoft365.SharePoint.Extension;
using Microsoft365.Common.Http;
using Microsoft365.Common.RequestMonitor;
using Microsoft365.SharePoint;
using Microsoft365.Common.HttpUtil;
using Duende.IdentityModel.Client;
using AvePoint.Wrapper.Common.Common.Utility;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using AvePoint.RA.Contract.Global.Exceptions;

namespace AvePoint.Wrapper.Common
{
    public static class ReliableHttpWebRequestExtension
    {
        private static readonly AveLogger mLogger = AveLogger.GetInstance(typeof(ReliableHttpWebRequestExtension));
        public static Stream GetResponsStreamEx(this ReliableHttpWebRequest request, string streamCacheFileNamePrefix)
        {
            bool needDisposeResponse = true;
            var response = request.GetResponse() as HttpWebResponse;
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream content = null;
                    try
                    {
                        if (response.ContentLength > AveSPDataStreamReader.USE_SPDATA_STREAM_READER_LIMIT)
                        {
                            Stream stream = response.GetResponseStream();
                            content = new AveSPDataStreamReader(stream, response.ContentLength, response);
                            needDisposeResponse = false;
                        }
                        else
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                content = new AveCoordinatedStream(streamCacheFileNamePrefix);
                                AveIOHelper.Copy(stream, content);
                                content.Position = 0;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        content?.Dispose();
                        throw;
                    }
                    return content;
                }
                else
                {
                    string responseString = null;
                    try
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (MemoryStream tempStream = new MemoryStream())
                            {
                                AveIOHelper.Copy(responseStream, tempStream);
                                responseString = Encoding.UTF8.GetString(tempStream.GetBuffer());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Output response failed.due to :{0}", ex.ToString());
                    }
                    mLogger.Warn("GetResponsStream failed.Header:{0}, Status: {1}", response.Headers.ToString(), response.StatusCode);
                    string message = string.Format("Invalid response status {0} for GetResponsStream.StatusDescription:{1}", response.StatusCode, response.StatusDescription);
                    throw new WebException(message, WebExceptionStatus.ReceiveFailure);
                }
            }
            finally
            {
                if(needDisposeResponse)
                {
                    response?.Dispose();
                }
            }
        }
    }

    public delegate void RequestFailedEventHandler(ReliableHttpWebRequest request, Exception ex);

    public delegate void ChangeTokenProviderEventHandler(WebRequest request);

    public delegate (Guid tenantId, string defaultAppId) GetTenantIdAndDefaultAppIdHandler();

    [Serializable]
    public class ReliableHttpWebRequest : WebRequest
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Uri mUrl;
        public HttpWebRequest Request { get; private set; }
        private static int RETRY_COUNT = WrapperConfiguration.WrapperConfigurationForBPOS.RetryCount * 2 + 1;
        private AveCoordinatedStream mRequestStream;
        private HttpWebRequest mAsyncRequest;
        [field: NonSerialized]
        public event RequestFailedEventHandler RequestFailed;
        [field: NonSerialized]
        public event ChangeTokenProviderEventHandler ChangeTokenProviderEvent;
        [field: NonSerialized]
        public event GetTenantIdAndDefaultAppIdHandler GetTenantIdAndDefaultAppIdEvent;
        private ITokenProvider mRefreshTokenProvider;
        private string mWebUrl;
        private bool alreadyGetTenantIdAndDefaultAppId;
        public bool NoRetry { get; set; } = false;

        public Guid? TenantId { get; set; }
        public string CurrentAppId { get; set; }
        public string LastAppId { get; set; }
        public string DefaultAppId { get; set; }

        public static ReliableHttpWebRequest CreateRequest(string url, Action<WebRequest> changeTokenFun = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
        {
            var uri = new Uri(url);
            return CreateRequest(uri, changeTokenFun, getTenantIdAndDefaultAppIdFunc);
        }

        public static ReliableHttpWebRequest CreateRequest(Uri uri, Action<WebRequest> changeTokenFun = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
        {
            //HealthScoreUtility.StartRequest(uri);
            OutputUserAgent();
            var request = new ReliableHttpWebRequest(HttpWebRequest.Create(uri) as HttpWebRequest, changeTokenFun, getTenantIdAndDefaultAppIdFunc) 
            { 
                UserAgent = WrapperConfiguration.WrapperConfigurationForBPOS.DefaultUserAgent 
            };
            if (changeTokenFun != null)
            {
                request.ChangeTokenProviderEvent += new ChangeTokenProviderEventHandler(changeTokenFun);
            }
            if (getTenantIdAndDefaultAppIdFunc != null)
            {
                request.GetTenantIdAndDefaultAppIdEvent += new GetTenantIdAndDefaultAppIdHandler(getTenantIdAndDefaultAppIdFunc);
            }
            return request;

        }

        private static void OutputUserAgent()
        {
            //logger.Debug("Create Request With User Agent {0}", WrapperConfiguration.BPOS_S.DefaultUserAgent);
        }

        public static ReliableHttpWebRequest CreateRequest(HttpWebRequest request, Action<WebRequest> changeTokenFun = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
        {
            //HealthScoreUtility.StartRequest(request.RequestUri);
            request.UserAgent = WrapperConfiguration.WrapperConfigurationForBPOS.DefaultUserAgent;
            OutputUserAgent();
            var res = new ReliableHttpWebRequest(request, changeTokenFun, getTenantIdAndDefaultAppIdFunc);
            if (changeTokenFun != null)
            {
                res.ChangeTokenProviderEvent += new ChangeTokenProviderEventHandler(changeTokenFun);
            }
            if (getTenantIdAndDefaultAppIdFunc != null)
            {
                res.GetTenantIdAndDefaultAppIdEvent += new GetTenantIdAndDefaultAppIdHandler(getTenantIdAndDefaultAppIdFunc);
            }
            return res;
        }

        public ReliableHttpWebRequest(Uri url, Action<WebRequest> changeTokenFun = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
        {
            mUrl = url;
            Request = WebRequest.Create(mUrl) as HttpWebRequest;
            if (changeTokenFun != null)
            {
                this.ChangeTokenProviderEvent += new ChangeTokenProviderEventHandler(changeTokenFun);
            }
            if (getTenantIdAndDefaultAppIdFunc != null)
            {
                this.GetTenantIdAndDefaultAppIdEvent += new GetTenantIdAndDefaultAppIdHandler(getTenantIdAndDefaultAppIdFunc);
            }
            InitRequest();
        }

        public ReliableHttpWebRequest(HttpWebRequest request, Action<WebRequest> changeTokenFun = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
        {
            mUrl = request.RequestUri;
            Request = request;
            if (changeTokenFun != null)
            {
                this.ChangeTokenProviderEvent += new ChangeTokenProviderEventHandler(changeTokenFun);
            }
            if (getTenantIdAndDefaultAppIdFunc != null)
            {
                this.GetTenantIdAndDefaultAppIdEvent += new GetTenantIdAndDefaultAppIdHandler(getTenantIdAndDefaultAppIdFunc);
            }
            InitRequest();
        }

        private void InitRequest()
        {
            this.CookieContainer = Request.CookieContainer;
            this.ReadWriteTimeout = Request.ReadWriteTimeout;
            this.ContentLength = Request.ContentLength;
            this.ContentType = Request.ContentType;
            this.Method = Request.Method;
            this.PreAuthenticate = Request.PreAuthenticate;
            this.Timeout = Request.Timeout;
            this.UseDefaultCredentials = Request.UseDefaultCredentials;
            this.UserAgent = Request.UserAgent;
            this.CachePolicy = Request.CachePolicy;
            this.ConnectionGroupName = Request.ConnectionGroupName;
            this.Credentials = Request.Credentials;            
            this.Headers = Request.Headers;
            this.Proxy = Request.Proxy;
            this.KeepAlive = Request.KeepAlive;
            this.AllowAutoRedirect = Request.AllowAutoRedirect;
            this.Accept = Request.Accept;
            this.ClientCertificates = Request.ClientCertificates;
            this.Host = Request.Host;
            this.AutomaticDecompression = Request.AutomaticDecompression;
            this.AllowWriteStreamBuffering = Request.AllowWriteStreamBuffering;
            this.SendChunked = Request.SendChunked;
        }

        public override System.IO.Stream GetRequestStream()
        {
            if (mRequestStream == null)
            {
                mRequestStream = new AveCoordinatedStream("RHWR",0, true);
            }
            return mRequestStream;
        }

        public override WebResponse GetResponse()
        {
            return RetryGetResponse(RETRY_COUNT, WrapperConfiguration.WrapperConfigurationForBPOS.RetryInterval);            
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            mAsyncRequest = CreateRequest() as HttpWebRequest;
            return mAsyncRequest.BeginGetRequestStream(callback, state);
        }

        public override Stream EndGetRequestStream(System.IAsyncResult asyncResult)
        {
            mAsyncRequest.EndGetRequestStream(asyncResult).Dispose();
            mAsyncRequest.Abort();
            asyncResult.AsyncWaitHandle.Close();
            return GetRequestStream();
        }

        public override System.IAsyncResult BeginGetResponse(System.AsyncCallback callback, object state)
        {
            mAsyncRequest = CreateRequest() as HttpWebRequest;
            if (mRequestStream != null)
            {
                using (Stream requestStream = mAsyncRequest.GetRequestStream())
                {
                    mRequestStream.Position = 0;
                    AveIOHelper.Copy(mRequestStream, requestStream);
                }
            }
            return mAsyncRequest.BeginGetResponse(callback, state);
        }

        public override WebResponse EndGetResponse(System.IAsyncResult asyncResult)
        {
            var retryInterval = WrapperConfiguration.WrapperConfigurationForBPOS.RetryInterval;

            WebResponse webResponse = null;             
            try
            {
                webResponse = mAsyncRequest.EndGetResponse(asyncResult);
                var httpWebResponse = webResponse as HttpWebResponse;
                ValidateResponse(httpWebResponse);
                HealthScoreUtility.Process(httpWebResponse);
            }
            catch (Exception e)
            {
                OnResponseFailed(e);
                OutputResponse(e);
                if (IsTooManyRequest(e, mUrl, ref retryInterval) || ShouldRetry(e, ref retryInterval))
                {
                    ReleaseResource(e);

                    webResponse = RetryGetResponse(RETRY_COUNT - 1, retryInterval);
                }
                else
                {
                    throw;
                }                    
            }
            finally 
            {
                if (asyncResult != null)
                {
                    asyncResult.AsyncWaitHandle.Close();
                }
                if (mRequestStream != null)
                {
                    mRequestStream.ExplictlyClose();
                    mRequestStream = null;
                }
            }
            return webResponse;
        }


        private void OnResponseFailed(Exception ex)
        {
            var requestFailed = RequestFailed;
            if(requestFailed != null)
            {
                requestFailed(this, ex);
            }
        }

        private void ReleaseResource(Exception exception)
        {
            var webException = exception as WebException;

            if (webException != null && webException.Response != null)
            {
                webException.Response.Close();
                webException.Response.Dispose();
            }
        }

        private void ValidateResponse(HttpWebResponse response)
        {
            if (response != null && response.StatusCode != HttpStatusCode.OK && (!string.IsNullOrEmpty(response.GetResponseHeader("Retry-After"))))
            {
                throw new WebException("The remote server returned an error.", null, WebExceptionStatus.UnknownError, response);
            }
        }

        private void InitTenantIdAndDefaultAppId()
        {
            try
            {
                if (!alreadyGetTenantIdAndDefaultAppId)
                {
                    if(GetTenantIdAndDefaultAppIdEvent != null)
                    {
                        var res = GetTenantIdAndDefaultAppIdEvent();
                        this.TenantId = res.tenantId;
                        this.CurrentAppId = res.defaultAppId;
                        this.DefaultAppId = res.defaultAppId;
                        this.LastAppId = res.defaultAppId;
                    }
                }
                if (!string.IsNullOrWhiteSpace(this.CurrentAppId)) 
                {
                    AveAppProfileUtility.SetCurrentAppProfileId(new Guid(this.TenantId.ToString()), this.CurrentAppId);
                }
            }
            catch(Exception ex)
            {
                logger.Warn(@$"fail get tenantId and default app id,ex:{ex}");
            }
            finally
            {
                alreadyGetTenantIdAndDefaultAppId = true;
            }
        }

        [field: NonSerialized]
        public static event Action CheckJobNeedStopEvent = () => { };

        [field: NonSerialized]
        public static event Action<string, DateTime> BeforeEachRequestEvent = (arg1, arg2) => { };

        [field: NonSerialized]
        public static event Action<string, DateTime> AfterRequestSuccessEvent = (arg1, arg2) => { };

        private WebResponse RetryGetResponse(int retryCount, int retryInterval)
        {
            WebResponse webResponse = null;
            try
            {
                InitTenantIdAndDefaultAppId();
                int i = 0;
                DateTime throttledTime = DateTime.Now;
                bool isRetry = false;
                while (true)
                {
                    DateTime requestStartTime = DateTime.UtcNow;
                    try
                    {
                        BeforeEachRequestEvent(CurrentAppId, requestStartTime);
                        //logger.Info("Execute request {0} times.Request Url:{1}",i+1,mUrl);
                        webResponse = SendRequestByHttpClient(isRetry);
                        var httpWebResponse = webResponse as HttpWebResponse;
                        ValidateResponse(httpWebResponse);
                        HealthScoreUtility.Process(httpWebResponse);
                        Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.OK);
                        AfterRequestSuccessEvent(CurrentAppId, requestStartTime);
                        break;
                    }
                    catch(JobStopException)
                    {
                        logger.Warn("Reqeust job stop, will cancel request");
                        throw;
                    }
                    catch (Exception e)
                    {
                        AddExceptionPerformanceLog(e, retryInterval);
                        OnResponseFailed(e);
                        OutputResponse(e);
                        i++;
                        if (i > 1)
                        {
                            CheckJobNeedStopEvent();
                        }
                        if (NoRetry)
                        {
                            //sleep for throttling,otherwise,throw
                            if (IsTooManyRequest(e, mUrl, ref retryInterval))
                            {
                                Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Throttled);
                            }
                            else
                            {
                                Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Failed);
                            }
                            throw;
                        }
                        if (IsTooManyRequest(e, mUrl, ref retryInterval))
                        {
                            Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Throttled);
                            DateTime errorTime = DateTime.UtcNow;
                            
                            if (mUrl.ToString().StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                            {
                                logger.Info("Skip the retry for the http url because this is just for authentication, not for SharePoint site.");
                                throw;
                            }

                            if (i >= 15)
                            {
                                throw;
                            }
                            else if (i >= 5)
                            {
                                if (ChangeTokenProviderEvent != null)
                                {
                                    logger.Info("Too many request error, need to change user");
                                    ChangeTokenProviderEvent(this);
                                }
                                else
                                {
                                    logger.Info("ChangeTokenProviderEvent is null");
                                }
                            }

                            logger.Warn("retry index:{0} failed to send request due to too many request exception: {1}", i, e.ToString());
                            ReleaseResource(e);
                        }
                        else if (NoRetry)
                        {
                            throw;
                        }
                        else if (RequestExceptionHanddler.IsTimedoutException(e, ref retryInterval))
                        {
                            Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Failed);
                            if (i < 2)
                            {
                                this.Timeout += 20 * 60 * 1000;
                                this.ReadWriteTimeout += 20 * 60 * 1000;
                                logger.Warn("Request is time out.Will increase timeout,and retry the request.");
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else if (IsSessionRevokedException(e))
                        {
                            Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Failed);
                            if (i < retryCount)
                            {
                                logger.Warn($"Session revoked exception occurred, force refresh token and retry the request. retry index: {i}");
                                Thread.Sleep(retryInterval);
                                ForceRefreshToken();
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else if (IsLabelAppliedException(e, out string labelMessage))
                        {
                            throw new AveLabelAppliedException(labelMessage);
                        }
                        else if (ShouldRetry(e, ref retryInterval))
                        {
                            Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Failed);
                            if (i >= retryCount)
                            {
                                throw;
                            }
                            else
                            {
                                logger.Warn("retry index:{0} failed to send request due to: {1}", i, e.ToString());
                                ReleaseResource(e);
                            }
                        }
                        else if (RequestExceptionHanddler.IsSiteLockedException(e, out string message))
                        {
                            var siteLockEx = new AveSkipLockSiteException(message);
                            siteLockEx.SiteState = SiteState.NoAccess;
                            throw siteLockEx;
                        }
                        else
                        {
                            Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Failed);
                            throw;
                        }
                    }
                    isRetry = true;
                }
                if(TenantId != null)
                {
                    AveAppProfileUtility.ClearBlockStatus(new Guid(TenantId.ToString()));
                }
            }
            finally
            {
                if (mRequestStream != null)
                {
                    mRequestStream.ExplictlyClose();
                    mRequestStream = null;
                }
            }
            return webResponse;
        }

        private static void AddExceptionPerformanceLog(Exception e, int retryInterval)
        {
            var scopeName = e.Message ?? "";
            if (e is WebException && (scopeName.Contains("(429)") || scopeName.Contains("(503)")))
            {
                // already added performance log in IsTooManyRequest method for more accurate retry interval, so skip here to avoid duplicate log
                return;
            }

            if (scopeName.Contains("An error occurred while sending the request") && e.InnerException != null)
            {
                // usually will be the connection forcely closed exception from Socket
                scopeName = e.InnerException.Message ?? "";
            }

            AveRequestStatisticMonitor.Record($"ReliableHttpWebRequest.RetryGetResponse.Exception.{scopeName}", retryInterval);
        }

        private WebResponse SendRequestByHttpClient(bool refreshToken = false)
        {
            var request = CreateRequest(refreshToken) as HttpWebRequest;
            if (mRequestStream != null && mRequestStream.Position != 0) { mRequestStream.Position = 0; }
//#if DEBUG
//            logger.Info($"Header: {request.Headers}");
//            logger.Info($"Content: {mRequestStream?.Length}");
//            logger.Info($"Uri: {request.RequestUri}");
//#endif
            return request.GetResponseByHttpClient(mRequestStream, "CSOM");
        }

        public void SetRefreshTokenProvider(string webUrl, ITokenProvider tokenProvider)
        {
            mRefreshTokenProvider = tokenProvider;
            mWebUrl = webUrl;
        }

        private void RefreshToken()
        {
            if ((!string.IsNullOrEmpty(mWebUrl)) && mRefreshTokenProvider != null)
            {
                logger.Info($"Refresh token for retry request.{mUrl}");
                this.SetTokenProvider(mWebUrl, mRefreshTokenProvider,false);
            }
        }

        private void ForceRefreshToken()
        {
            if (!string.IsNullOrEmpty(mWebUrl) && mRefreshTokenProvider != null)
            {
                logger.Info($"Force refresh token for retry request.{mUrl}");

                if (mRefreshTokenProvider.TokenType == TokenType.IDCLR)
                {
                    this.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";

                    this.Headers[HttpRequestHeader.Cookie] = mRefreshTokenProvider.GetToken(new Uri(mWebUrl), true);
                }
                else
                {
                    this.Headers[HttpRequestHeader.Authorization] = mRefreshTokenProvider.GetToken(new Uri(mWebUrl), true);
                }
            }
        }

        private WebRequest CreateRequest(bool refreshToken=false)
        {
            HealthScoreUtility.StartRequest(mUrl);
            if (refreshToken)
            {
                RefreshToken();
            }
            HttpWebRequest request = WebRequest.Create(mUrl) as HttpWebRequest;
            if (this.Headers != null)
            {
                foreach (string key in this.Headers)
                {
                    if (!WebHeaderCollection.IsRestricted(key))
                    {
                        request.Headers.Set(key, this.Headers[key]);
                    }
                }                
            }
            request.CookieContainer = this.CookieContainer;
            request.ReadWriteTimeout = this.ReadWriteTimeout;            
            request.ContentType = this.ContentType;
            request.Method = this.Method;
            request.PreAuthenticate = this.PreAuthenticate;
            request.Timeout = this.Timeout;
            request.UseDefaultCredentials = this.UseDefaultCredentials;
            request.UserAgent = this.UserAgent;
            request.KeepAlive = this.KeepAlive;
            request.AllowAutoRedirect = this.AllowAutoRedirect;
            request.Accept = this.Accept;

            request.AllowWriteStreamBuffering = this.AllowWriteStreamBuffering;
            request.SendChunked = this.SendChunked;

            if (string.Compare(request.Host, this.Host, false) != 0)
            {
                request.Host = this.Host;
            }

            request.ClientCertificates = this.ClientCertificates;
            request.AutomaticDecompression = this.AutomaticDecompression;

            if (this.ContentLength >= 0)
            {
                request.ContentLength = this.ContentLength;
            }
            if (this.CachePolicy != null)
            {
                request.CachePolicy = this.CachePolicy;
            }
            if (!string.IsNullOrEmpty(this.ConnectionGroupName))
            {
                request.ConnectionGroupName = this.ConnectionGroupName;
            }
            if (this.Credentials != null)
            {
                request.Credentials = this.Credentials;
            }            
            if (this.Proxy != null)
            {
                request.Proxy = this.Proxy;                
            }

            //request.Proxy = new WebProxy("127.0.0.1", 8888);
            return request;
        }

        public static bool ShouldRetry(Exception e, ref int retryInterval)
        {

            if (RequestExceptionHanddler.IsUnstableNetworkException(e as WebException))
            {
                logger.Info("start to sleep {0} milliseconds for connection issue.", retryInterval);
                //retryInterval = retryInterval * 2; //提高connection的retry逻辑,主要是出现太多429之后,就会出现该问题。
                Thread.Sleep(retryInterval);
                return true;
            }
            else if (RequestExceptionHanddler.Is0x80131904Exception(e))
            {
                logger.Info("start to sleep {0} milliseconds for server exception.", retryInterval);
                Thread.Sleep(retryInterval);
                return true;
            }
            else if (RequestExceptionHanddler.IsServerProtocolViolationError(e, ref retryInterval))
            {
                retryInterval = retryInterval * 2;
                logger.Info("start to sleep {0} milliseconds for server protocol violation error.", retryInterval);
                Thread.Sleep(retryInterval);
                return true;
            }
            else if (RequestExceptionHanddler.IsConnectonForciblyClosedExceptioin(e))
            {
                logger.Info("start to sleep {0} milliseconds for connection forcibly closed exception.", retryInterval);
                Thread.Sleep(retryInterval);
                return true;
            }
            else if (RequestExceptionHanddler.IsNameResolutionFailureException(e))
            {
                logger.Info("start to sleep {0} milliseconds for name resolution failure exception.", retryInterval);
                Thread.Sleep(retryInterval);
                return true;
            }
            return false;
        }

        public bool IsTooManyRequest(Exception e, Uri requestUri, ref int retryInterval)
        {
            if (RequestExceptionHanddler.IsRetryableWebException(e, ref retryInterval)
                || RequestExceptionHanddler.IsToomanyRequestError(e))
            {
                // add performance log here to get the accurate retry interval for too many request exception
                AveRequestStatisticMonitor.Record($"ReliableHttpWebRequest.RetryGetResponse.Exception.IsTooManyRequest.{e.Message}", retryInterval);
                WrapperConfiguration.WrapperConfigurationForBPOS.HealthScoreSleepTime = retryInterval;
                //WrapperConfiguration.BPOS_S.HealthScoreThrottledTimeout = retryInterval + 5000; //在Retry-After时间之后再延后5s
                WrapperConfiguration.WrapperConfigurationForBPOS.HealthScoreThrottledTimeout = retryInterval + 5000;
                HealthScoreUtility.Process(requestUri, (HttpStatusCode)429, CurrentAppId);
                return true;
            }
            return false;
        }

        public bool IsSessionRevokedException(Exception e)
        {
            if (e is WebException we)
            {
                HttpWebResponse response = we.Response as HttpWebResponse;
                if (response == null)
                {
                    return false;
                }
                if (RequestExceptionHanddler.IsSessionRevokedException(GetResponseString(response)))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsLabelAppliedException(Exception e, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (e is WebException we)
            {
                HttpWebResponse response = we.Response as HttpWebResponse;
                if (response == null)
                {
                    return false;
                }
                string responseString = GetResponseString(response);
                if (RequestExceptionHanddler.IsLabelAppliedExcecption(responseString, out errorMessage))
                {
                    return true;
                }
            }
            return false;
        }

        private void OutputResponse(Exception e)
        {

            if (e is WebException)
            {
                logger.Warn("Access url:{0} with method:{1}, timeout:{2}, readwritetimeout:{3}, status:{4}, Exception:{5}", mUrl, Method, Timeout, ReadWriteTimeout, (e as WebException).Status, e);
                HttpWebResponse response = (e as WebException).Response as HttpWebResponse;
                if (response == null)
                {
                    return;
                }
                HealthScoreUtility.Process(response);

                //if (WrapperConfiguration.DevelopMode || response.StatusCode != HttpStatusCode.Unauthorized)
                {
                    string responseString = GetResponseString(response);

                    logger.Warn(" response status code is: {0}", response.StatusCode);

                    if (RequestExceptionHanddler.Is0x81020071Exception(responseString, out string errorMessage))
                    {
                        var lockEx = new AveSkipLockSiteException(errorMessage, e);
                        lockEx.SiteState = SiteState.NoAccess;
                        throw lockEx;
                    }
                    if (RequestExceptionHanddler.IsNoNeedRetryExcecption(responseString, out string errMsg))
                    {
                        logger.Warn("Don't need retry for Specific Exception, such as Virus Scan Exception, IRM related exception.");
                        if (string.IsNullOrEmpty(errMsg))
                        {
                            throw e;
                        }
                        else
                        {
                            throw new Exception(errMsg, e);
                        }
                    }
                }
            }
            else
            {
                logger.Warn("Access url:{0} with method:{1}, timeout:{2}, readwritetimeout:{3} Exception:{4}", mUrl, Method, Timeout, ReadWriteTimeout, e);
            }
        }

        private string GetResponseString(HttpWebResponse response)
        {
            string responseString = null;
            try
            {
                Stream responseStream = response.GetResponseStream();
                using (MemoryStream tempStream = new MemoryStream())
                {
                    int size = 1024 * 10;
                    byte[] buffer = new byte[size];
                    int len = 0;
                    while ((len = responseStream.Read(buffer, 0, size)) != 0)
                    {
                        tempStream.Write(buffer, 0, len);
                    }
                    tempStream.Position = 0;
                    byte[] bytes = new byte[tempStream.Length];
                    tempStream.ReadEx(bytes, 0, bytes.Length);
                    responseString = Encoding.UTF8.GetString(bytes);
                }
                //responseStream.Position = 0;
            }
            catch (Exception ex)
            {
                logger.Error("Output response failed.due to :{0}", ex.ToString());
            }
            return responseString;
        }

        public CookieContainer CookieContainer
        {
            get;
            set;
        }

        public int ReadWriteTimeout
        {
            get;
            set;
        }

        public override System.Net.Cache.RequestCachePolicy CachePolicy
        {
            get;
            set;
        }
        public override string ConnectionGroupName
        {
            get;
            set;            
        }
        public override long ContentLength
        {
            get;
            set;            
        }
        public override string ContentType
        {
            get;            
            set;            
        }
        public override ICredentials Credentials
        {
            get;
            set;            
        }
        public override WebHeaderCollection Headers
        {
            get;            
            set;            
        }
        public override string Method
        {
            get;
            set;            
        }
        public override bool PreAuthenticate
        {
            get;
            set;            
        }
        public bool KeepAlive
        {
            get;
            set;
        }
        public override IWebProxy Proxy
        {
            get;
            set;            
        }
        public override System.Uri RequestUri
        {
            get
            {
                return this.mUrl;
            }
        }
        public override int Timeout
        {
            get;
            set;            
        }
        public override bool UseDefaultCredentials
        {
            get;
            set;            
        }

        public string UserAgent
        {
            get;
            set;            
        }

        public bool AllowAutoRedirect
        {
            get;
            set;
        }

        public string Accept
        {
            get;
            set;
        }

        public X509CertificateCollection ClientCertificates { get; set; }
        public string Host { get; set; }
        public DecompressionMethods AutomaticDecompression { get; set; }

        public bool AllowWriteStreamBuffering { get; set; }
        public bool SendChunked { get; set; }
    }

    public class O365TenantHealthScore
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(O365TenantHealthScore));
        private readonly object healthScoreLockObj = new object();
        private readonly object throttledLockObj = new object();
        [field: NonSerialized]
        public static event Action<string, long, string> BeforeThrottlingSleepEvent = (arg1, arg2, arg3) => { };
        [field: NonSerialized]
        public static event Action AfterThrottlingSleepEvent = () => { };
        public string Domain { get; private set; }
        public string Url { get; private set; }
        public int HealthScore { get; private set; }
        public DateTime LastUpdateTime { get; set; }
        public bool IsThrottled { get; private set; }

        public string AppIdOf429 { get; private set; }
        public DateTime LastThrottledTime { get; private set; }

        public O365TenantHealthScore(string domain, string url)
        {
            Domain = domain;
            LastUpdateTime = DateTime.MinValue;
            logger.Info("Init O365TenantHealthScore for host {0}", domain);
            Url = url;
        }

        /// <summary>
        /// 记录状态
        /// </summary>
        /// <param name="healthScore"></param>
        /// <param name="statusCode"></param>
        public void Process(int healthScore, HttpStatusCode statusCode)
        {
            lock (healthScoreLockObj)
            {
                Record();
            }
            Process(statusCode, null);
        }

        private void Record()
        {
            if (LastUpdateTime == DateTime.MinValue
                       || LastUpdateTime.AddMinutes(5) <= DateTime.Now)
            {
                LastUpdateTime = DateTime.Now;
                logger.Info("[Tenant Health Report][Domain:{0}][health Score:{1}][IsThrottled:{2}]", Domain, HealthScore, IsThrottled);
            }
        }

        public void Process(HttpStatusCode statusCode, string currentAppId)
        {
            if (statusCode == (HttpStatusCode)429 || statusCode == (HttpStatusCode)503)
            {
                lock (throttledLockObj)
                {
                    IsThrottled = true;
                    AppIdOf429 = currentAppId;
                    LastThrottledTime = DateTime.UtcNow;
                }
            }
        }


        public void StartRequest()
        {
            int lastHealthScore;

            lock (healthScoreLockObj)
            {
                lastHealthScore = HealthScore;
                Record();
                //TotalRequest++;
            }
            

            //一旦出现429,就需要提高level,否则retry时间太短还是会导致online资源占用太多。
            if (IsThrottled)
            {
                lock (throttledLockObj)
                {
                    logger.Warn($"WrapperConfiguration.WrapperConfigurationForBPOS.HealthScoreThrottledTimeout sleep time is:{WrapperConfiguration.WrapperConfigurationForBPOS.HealthScoreThrottledTimeout}");
                    if (LastThrottledTime.AddMilliseconds(WrapperConfiguration.WrapperConfigurationForBPOS.HealthScoreThrottledTimeout) < DateTime.UtcNow)
                    {
                        IsThrottled = false;
                        AppIdOf429 = string.Empty;
                    }
                }
                
                if (IsThrottled)
                {
                    var sleepTime = WrapperConfiguration.WrapperConfigurationForBPOS.HealthScoreSleepTime;
                    logger.Warn("start to sleep {0} for url:{1} because the throttle is enabled", sleepTime, Domain);
                    Microsoft365RequestMonitorService.Instance.AddThrottlingBlockedTimeRange(DateTime.UtcNow, sleepTime);
                    BeforeThrottlingSleepEvent($@"Accure 429 when reqeust url:{Url}, domain:{Domain}, sleepMS:{sleepTime}, appId:{AppIdOf429}", sleepTime * TimeSpan.TicksPerMillisecond, AppIdOf429);
                    Thread.Sleep(sleepTime);
                    AfterThrottlingSleepEvent();
                }
            }
        }
    }

    public static class HealthScoreUtility
    {
        private static readonly IAveLogger logger = AveLogger.GetInstance(typeof(HealthScoreUtility));
        private static O365TenantHealthScore lastScore;
        private static readonly Dictionary<string, O365TenantHealthScore> healthScores = new Dictionary<string, O365TenantHealthScore>(StringComparer.OrdinalIgnoreCase);

        public static void Process(HttpWebResponse response)
        {
            try
            {
                if (response != null)
                {
                    var domain = response.ResponseUri.Authority;
                    if (!string.IsNullOrEmpty(domain))
                    {
                        var healthScoreAsString = response.GetResponseHeader("X-SharePointHealthScore");
                        var healthScoreValue = 0;
                        int.TryParse(healthScoreAsString, out healthScoreValue);
                        var healthScore = GetHealthScore(domain, response.ResponseUri.AbsolutePath);
                        healthScore.Process(healthScoreValue, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("process response failed:{0}", ex);
            }
        }
        public static void Process(Uri uri, HttpStatusCode statusCode, string currentAppId = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(uri?.Authority))
                {
                    var healthScore = GetHealthScore(uri.Authority, uri.AbsolutePath);
                    healthScore.Process(statusCode, currentAppId);
                }
            }
            catch (Exception ex)
            {
                logger.Error("process domain:{0} with status code:{1} failed:{2}", uri?.Authority, statusCode, ex);
            }
        }

        public static void Process(Uri uri, int healthScoreValue, HttpStatusCode statusCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(uri?.Authority))
                {
                    var healthScore = GetHealthScore(uri?.Authority, uri.AbsolutePath);
                    healthScore.Process(healthScoreValue, statusCode);
                }
            }
            catch (Exception ex)
            {
                logger.Error("process domain:{0} with status code:{1} and health score:{2} failed:{3}", uri?.Authority, statusCode, healthScoreValue, ex);
            }
        }

        static O365TenantHealthScore GetHealthScore(string domain, string url = "")
        {
            O365TenantHealthScore score;

            if (lastScore != null && domain.Equals(lastScore.Domain, StringComparison.OrdinalIgnoreCase))
            {
                score = lastScore;
            }
            else
            {
                lock (healthScores)
                {
                    if (!healthScores.TryGetValue(domain, out score))
                    {
                        score = new O365TenantHealthScore(domain, url);
                        healthScores[domain] = score;
                    }
                }
                lastScore = score;
            }

            return score;
        }

        internal static void StartRequest(Uri url)
        {
            GetHealthScore(url.Authority, url.AbsolutePath).StartRequest();
        }
    }
}