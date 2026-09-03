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
using System.Linq;
using AvePoint.Wrapper.Common;
using System.Net.Sockets;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public delegate void RequestFailedEventHandler(ReconnectableHttpWebRequest request, Exception ex);
    public class ReconnectableHttpWebRequest : WebRequest, IDisposable
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(ReconnectableHttpWebRequest));
        private Uri mUrl;
        protected HttpWebRequest mRequest;
        private AveCoordinatedStream mRequestStream;
        private HttpWebRequest mAsyncRequest;
        private string userAgent;
        public event RequestFailedEventHandler RequestFailed;

        public static ReconnectableHttpWebRequest CreateRequest(string url)
        {
            return new ReconnectableHttpWebRequest(HttpWebRequest.Create(url) as HttpWebRequest);
        }

        public static ReconnectableHttpWebRequest CreateRequest(Uri uri)
        {
            return new ReconnectableHttpWebRequest(HttpWebRequest.Create(uri) as HttpWebRequest);
        }

        public static ReconnectableHttpWebRequest CreateRequest(HttpWebRequest request)
        {
            return new ReconnectableHttpWebRequest(request);
        }

        public ReconnectableHttpWebRequest(Uri url)
        {
            mUrl = url;
            mRequest = WebRequest.Create(mUrl) as HttpWebRequest;
            InitRequest();
        }

        public ReconnectableHttpWebRequest(HttpWebRequest request)
        {
            mUrl = request.RequestUri;
            mRequest = request;
            InitRequest();
        }

        private void InitRequest()
        {
            this.CookieContainer = mRequest.CookieContainer;
            this.ReadWriteTimeout = mRequest.ReadWriteTimeout;
            this.ContentLength = mRequest.ContentLength;
            this.ContentType = mRequest.ContentType;
            this.Method = mRequest.Method;
            this.PreAuthenticate = mRequest.PreAuthenticate;
            this.Timeout = mRequest.Timeout;
            this.UseDefaultCredentials = mRequest.UseDefaultCredentials;
            this.UserAgent = mRequest.UserAgent;
            this.CachePolicy = mRequest.CachePolicy;
            this.ConnectionGroupName = mRequest.ConnectionGroupName;
            this.Credentials = mRequest.Credentials;
            this.Headers = mRequest.Headers;
            this.Proxy = mRequest.Proxy;
            this.KeepAlive = mRequest.KeepAlive;
            this.AllowAutoRedirect = mRequest.AllowAutoRedirect;
            this.AllowWriteStreamBuffering = mRequest.AllowWriteStreamBuffering;
            this.Accept = mRequest.Accept;
            this.AutomaticDecompression = mRequest.AutomaticDecompression;
            RetrieveHost();
            RetrieveClientCertificates();
        }

        public override System.IO.Stream GetRequestStream()
        {
            if (mRequestStream == null)
            {
                mRequestStream = new AveCoordinatedStream(true);
            }
            return mRequestStream;
        }

        public override WebResponse GetResponse()
        {
            return RetryGetResponse(WrapperConfiguration.BPOS_S.RetryCount);
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
            WebResponse webResponse = null;
            try
            {
                webResponse = mAsyncRequest.EndGetResponse(asyncResult);
                var httpWebResponse = webResponse as HttpWebResponse;
                HealthScoreUtility.Process(httpWebResponse);
            }
            catch (Exception e)
            {
                OnResponseFailed(e);
                int retryCount = WrapperConfiguration.BPOS_S.RetryCount - 1;
                if (ShouldRetry(e, ref retryCount))
                {
                    webResponse = RetryGetResponse(retryCount);
                }
                else
                {
                    throw;
                }
            }
            if (asyncResult != null)
            {
                asyncResult.AsyncWaitHandle.Close();
            }
            if (mRequestStream != null)
            {
                mRequestStream.ExplictlyClose();
                mRequestStream = null;
            }
            return webResponse;
        }

        private WebResponse RetryGetResponse(int retryCount)
        {
            WebResponse webResponse = null;
            Exception exception = null;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    webResponse = SendRequest();
                    var httpWebResponse = webResponse as HttpWebResponse;
                    exception = null;
                    HealthScoreUtility.Process(httpWebResponse);
                    break;
                }
                catch (Exception e)
                {
                    exception = e;
                    OnResponseFailed(e);
                    if (ShouldRetry(e, ref retryCount))
                    {
                        mLogger.Warn("failed to send request:{0} due to: {1}, retry: {2}", mUrl, e, i);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            if (mRequestStream != null)
            {
                mRequestStream.ExplictlyClose();
                mRequestStream = null;
            }
            if (exception != null)
            {
                throw exception;
            }
            return webResponse;
        }

        private bool CheckSPHealth(WebResponse response)
        {
            if (response == null)
            {
                return true;
            }
            int score = 0;
            int.TryParse(response.Headers["X-SharePointHealthScore"], out score);
            return score < 7;
        }

        private WebResponse SendRequest()
        {
            WebRequest request = CreateRequest();
            if (mRequestStream != null && !string.Equals(this.Method, "Get", StringComparison.OrdinalIgnoreCase))
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    mRequestStream.Position = 0;
                    AveIOHelper.Copy(mRequestStream, requestStream);
                }
            }
            return request.GetResponse();
        }

        private WebRequest CreateRequest()
        {
            HealthScoreUtility.StartRequest(mUrl);

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
            request.AllowWriteStreamBuffering = this.AllowWriteStreamBuffering;
            request.Accept = this.Accept;
            request.AutomaticDecompression = this.AutomaticDecompression;
            AssignHost();
            AssignClientCertificates();

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

        private void RemoveRestrictedHeaders(WebHeaderCollection webheaders)
        {
            if (webheaders != null)
            {
                webheaders.Remove(HttpRequestHeader.UserAgent);
                webheaders.Remove(HttpRequestHeader.ContentType);
                webheaders.Remove(HttpRequestHeader.KeepAlive);
            }
        }

        private bool ShouldRetry(Exception e, ref int retryCount)
        {
            int retryInterval = 3000;//WrapperConfiguration.BPOS_S.RetryInterval;
            if (IsHTTP429Error(e, ref retryInterval))
            {
                retryCount = int.MaxValue; //throttled 会自动计算时间来跳出循环
                return true;
            }
            else if (IsConnectonForciblyClosedExceptioin(e) || IsUnstableNetworkException(e as WebException))
            {
                retryCount = WrapperConfiguration.BPOS_S.RetryCount - 1;
                System.Threading.Thread.Sleep(retryInterval);
                return true;
            }
            else if (IsRetryableServerException(e))
            {
                retryCount = WrapperConfiguration.BPOS_S.RetryCount - 1;
                System.Threading.Thread.Sleep(retryInterval);
                return true;
            }
            else if (IsServerProtocolViolationError(e, ref retryInterval))
            {
                retryCount = WrapperConfiguration.BPOS_S.RetryCount - 1;
                mLogger.Info("start to sleep {0} milliseconds for server protocol violation error.", retryInterval);
                System.Threading.Thread.Sleep(retryInterval);
                return true;
            }
            return false;
        }

        //we assume socketexception or ioexception caused by connection forcilby closed
        private bool IsConnectonForciblyClosedExceptioin(Exception te)
        {
            if (te.InnerException is SocketException || te.InnerException is IOException)
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsConnectonForciblyClosedExceptioin(te.InnerException);
            }
            return false;
        }

        private bool IsUnstableNetworkException(WebException e)
        {
            if (e == null)
            {
                return false;
            }
            ///If the name resolution failure, no need to retry.
            if (/** e.Status == System.Net.WebExceptionStatus.NameResolutionFailure
                || **/ e.Status == WebExceptionStatus.SecureChannelFailure
                || e.Status == WebExceptionStatus.ConnectFailure
                || e.Status == WebExceptionStatus.KeepAliveFailure
                || e.Status == WebExceptionStatus.ConnectionClosed
                || e.Status == WebExceptionStatus.PipelineFailure
                || e.Status == WebExceptionStatus.SendFailure
                || e.Status == WebExceptionStatus.UnknownError
                || e.Status == WebExceptionStatus.Pending)
            {
                return true;
            }
            if (e.Response != null)
            {
                HttpWebResponse webResponse = e.Response as HttpWebResponse;
                if (webResponse != null
                    && (webResponse.StatusCode == HttpStatusCode.ServiceUnavailable
                    || webResponse.StatusCode == HttpStatusCode.InternalServerError))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsServerProtocolViolationError(Exception e, ref int retryInterval)
        {
            if ((e is WebException) && (e as WebException).Status == WebExceptionStatus.ServerProtocolViolation)
            {
                mLogger.Error("server protocol violation error,error message:{0}", e);
                retryInterval = retryInterval * 2;
                return true;
            }
            return false;
        }

        private bool IsRetryableServerException(Exception e)
        {
            if (e != null && !string.IsNullOrEmpty(e.Message) && e.Message.Contains("0x80131904"))
            {
                return true;
            }
            return false;
        }

        //HTTP 429 ERROR, Too Many Request.
        //Check is request failed due to server unavailable - http status code 503
        private bool IsHTTP429Error(Exception e, ref int interval)
        {
            if (e is WebException)
            {
                var webException = e as WebException;
                HttpWebResponse response = webException.Response as HttpWebResponse;
                if (response != null && ((int)response.StatusCode == 429 || (int)response.StatusCode == 503))
                {
                    HealthScoreUtility.Process(response);
                    return true;
                }
                if (response == null)
                {
                    if (webException.Message != null && webException.Message.Contains("The remote server returned an error: (429)"))
                    {
                        HealthScoreUtility.Process(mUrl.Authority, (HttpStatusCode)429);
                        return true;
                    }
                }
            }
            else if (e.InnerException != null)
            {
                return IsHTTP429Error(e.InnerException, ref interval);
            }
            return false;
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

        public DecompressionMethods AutomaticDecompression { get; set; }

        public bool AllowWriteStreamBuffering { get; set; }

        public string UserAgent
        {
            get
            {
                if (!string.IsNullOrEmpty(WrapperConfiguration.UserAgentTag))
                {
                    return WrapperConfiguration.UserAgentTag;
                }
                return userAgent;
            }
            set
            {
                userAgent = value;
            }
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

        public void Dispose()
        {
            mRequestStream.Dispose();
        }


        private void OnResponseFailed(Exception ex)
        {
            var requestFailed = RequestFailed;
            if (requestFailed != null)
            {
                requestFailed(this, ex);
            }
        }
        virtual protected void RetrieveHost() { }
        virtual protected void AssignHost() { }
        virtual protected void RetrieveClientCertificates() { }
        virtual protected void AssignClientCertificates() { }
    }
    class O365TenantHealthScore
    {
        private static readonly AveLogger mLogger = AveLogger.GetInstance(typeof(O365TenantHealthScore));
        private object healthScoreLockObj = new object();
        private object throttledLockObj = new object();

        private bool isThrottled;

        private DateTime firstThrottledTime;

        private int sleepTime;

        private int? interactiveTagTimes;

        public string Domain { get; private set; }
        public int HealthScore { get; private set; }



        public O365TenantHealthScore(string domain)
        {
            Domain = domain;
            sleepTime = WrapperConfiguration.BPOS_S.HealthScoreSleepTime;
            firstThrottledTime = DateTime.MinValue;
        }

        /// <summary>
        /// 每次Response处理下，如果发现HealthScore很高，就需要等待一会儿。
        /// 并且记录状态
        /// </summary>
        /// <param name="healthScore"></param>
        /// <param name="statusCode"></param>
        public void Process(int healthScore, int sleepTimeFromHeader, HttpStatusCode statusCode)
        {
            if (healthScore != -1)
            {
                lock (healthScoreLockObj)
                {
                    HealthScore = healthScore;
                    //TotalResponse++;
                }
            }
            if (sleepTimeFromHeader > 0)
            {
                sleepTime = sleepTimeFromHeader * 1000;
            }
            Process(statusCode);
        }

        public void Process(HttpStatusCode statusCode)
        {
            lock (throttledLockObj)
            {
                if (statusCode == (HttpStatusCode)429 || statusCode == (HttpStatusCode)503)
                {
                    isThrottled = true;
                    if (firstThrottledTime == DateTime.MinValue)
                    {
                        firstThrottledTime = DateTime.UtcNow;
                    }
                }
                else
                {
                    firstThrottledTime = DateTime.MinValue;
                    isThrottled = false;
                }
            }
        }

        public void StartRequest()
        {
            int lastHealthScore;

            lock (healthScoreLockObj)
            {
                lastHealthScore = HealthScore;
                //TotalRequest++;
            }

            lock (throttledLockObj)
            {
                if (interactiveTagTimes.HasValue)
                {
                    if (++interactiveTagTimes >= 300)
                    {
                        interactiveTagTimes = null;
                        WrapperConfiguration.RemoveInterActiveTag();
                    }
                }
            }

            //一旦出现429,就需要提高level,否则retry时间太短还是会导致online资源占用太多。
            if (isThrottled)
            {
                lock (throttledLockObj)
                {
                    if (firstThrottledTime.AddMilliseconds(WrapperConfiguration.BPOS_S.HealthScoreThrottledTimeout) < DateTime.UtcNow)
                    {
                        if (interactiveTagTimes.HasValue)
                        {
                            isThrottled = false;
                            throw new AveWrapperException(string.Format("The throttled action is time out. Domain:{0}", this.Domain));
                        }
                        else
                        {
                            WrapperConfiguration.AddInterActiveTag();
                            interactiveTagTimes = 0;
                        }
                    }
                }
                if (isThrottled)
                {
                    mLogger.Warn("start to sleep {0} for url:{1} because the throttle is enabled", sleepTime, Domain);
                    System.Threading.Thread.Sleep(sleepTime);
                }
            }
            else if (WrapperConfiguration.BPOS_S.EnableHealthScoreMonitor && lastHealthScore > WrapperConfiguration.BPOS_S.HealthScoreWarningValue)
            {
                mLogger.Warn("start to sleep {0} for url:{1} because the health score is {2}", sleepTime, Domain, lastHealthScore);
                System.Threading.Thread.Sleep(sleepTime);
            }
        }

    }
    public static class HealthScoreUtility
    {
        private static readonly AveLogger mLogger = AveLogger.GetInstance(typeof(HealthScoreUtility));
        private static O365TenantHealthScore lastScore;
        private static readonly Dictionary<string, O365TenantHealthScore> healthScores = new Dictionary<string, O365TenantHealthScore>(StringComparer.OrdinalIgnoreCase);

        public static int LastHealthScore
        {
            get
            {
                return lastScore != null ? lastScore.HealthScore : 0;
            }
        }

        public static int GetLastHealthScoreByDomain(string domain)
        {
            O365TenantHealthScore score;
            string fakeDomainName = domain;
            //if (!string.IsNullOrEmpty(WrapperRuntime.CurrentContext.UserLoginName))
            //{
            //    fakeDomainName = string.Format("{0}---{1}", WrapperRuntime.CurrentContext.UserLoginName, domain);
            //}
            lock (healthScores)
            {
                if (healthScores.TryGetValue(fakeDomainName, out score))
                {
                    return score.HealthScore;
                }
            }
            return 0;
        }

        internal static void Process(HttpWebResponse response)
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
                        if (!int.TryParse(healthScoreAsString, out healthScoreValue))
                        {
                            healthScoreValue = -1;
                        }
                        var sleepTime = -1;
                        if (response.Headers.AllKeys.Contains("Retry-After"))
                        {
                            sleepTime = Convert.ToInt32(response.Headers["Retry-After"]);
                        }
                        var healthScore = GetHealthScore(domain);
                        healthScore.Process(healthScoreValue, sleepTime, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("process response failed:{0}", ex);
            }
        }
        internal static void Process(string domain, HttpStatusCode statusCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(domain))
                {
                    var healthScore = GetHealthScore(domain);
                    healthScore.Process(statusCode);
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("process domain:{0} with status code:{1} failed:{2}", domain, statusCode, ex);
            }
        }


        static O365TenantHealthScore GetHealthScore(string domain)
        {
            O365TenantHealthScore score;
            string fakeDomainName = domain;
            //if (!string.IsNullOrEmpty(WrapperRuntime.CurrentContext.UserLoginName))
            //{
            //    fakeDomainName = string.Format("{0}---{1}", WrapperRuntime.CurrentContext.UserLoginName, domain);
            //}
            if (lastScore != null && fakeDomainName.Equals(lastScore.Domain, StringComparison.OrdinalIgnoreCase))
            {
                score = lastScore;
            }
            else
            {
                lock (healthScores)
                {
                    if (!healthScores.TryGetValue(fakeDomainName, out score))
                    {
                        score = new O365TenantHealthScore(fakeDomainName);
                        healthScores[fakeDomainName] = score;
                    }
                }
                lastScore = score;
            }

            return score;
        }

        internal static void StartRequest(Uri url)
        {
            GetHealthScore(url.Authority).StartRequest();
        }
    }
}
