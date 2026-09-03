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
namespace Microsoft365.Common.HttpUtil
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Linq;
    using System.Net.Cache;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Net.Security;
    using Microsoft365.Common.Logger;

    internal struct HttpClientCacheParameter
    { 
        public Int32 Milliseconds { get; set; }
        public int AutomaticDecompression { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public bool PreAuthenticate { get; set; }
        public string Host { get; set; }
        public string ExternalCacheKey { get; set; }
        public string CertificateHash { get; set; }
        public int RetryHashCode { get; set; }
    }

    public static class HttpWebRequestExtension
    {
        public static HttpWebResponse GetResponseByHttpClient(this HttpWebRequest request, Stream contentStream = null, string cacheKey = "", IList<IRetryStrategy> retryStrategies = null)
        {
            return HttpWebRequestFactory.SendHttpRequest(request, contentStream, cacheKey, retryStrategies);
        }
    }

    /// <summary>
    /// Credentials is not support for now.
    /// </summary>
    internal class HttpWebRequestFactory
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(HttpWebRequestFactory));
        internal static List<HttpClientCacheParameter> GetCache()
        {
            return factoryCache.Keys.ToList();
        }

        internal static string GetMessage()
        {
            StringBuilder builder = new StringBuilder();
            var requests = GetCache();
            builder.AppendLine($"HttpClientCacheNumber:{requests.Count}");
            foreach (var request in requests)
            {
                builder.AppendLine($"{request.Host}-{request.ExternalCacheKey}-{request.PreAuthenticate}-{request.Milliseconds}-{request.AllowAutoRedirect}-{request.AutomaticDecompression}");
            }
            return builder.ToString();
        }
        private static ConcurrentDictionary<HttpClientCacheParameter, HttpClient> factoryCache = new ConcurrentDictionary<HttpClientCacheParameter, HttpClient>();

        private static string GetCertificatesHash(X509CertificateCollection certificates)
        {
            if (certificates == null || certificates.Count == 0)
            {
                return string.Empty;
            }
            List<string> certificateList = new List<string>();
            foreach (var certificate in certificates)
            {
                certificateList.Add(certificate.GetCertHashString());
            }
            certificateList.Sort();
            return string.Join("-", certificateList);
        }

        private static int GetRetryHashCode(IList<IRetryStrategy> retryStrategies)
        {
            if (retryStrategies == null || retryStrategies.Count == 0)
            {
                return 0;
            }
            var hash = new HashCode();
            for (int k = 0; k < retryStrategies.Count; k++)
            {
                hash.Add(retryStrategies[k]);
            }
            return hash.ToHashCode();
        }

        /// <summary>
        /// only need basic properties from httpwebrequest, content should be standand alone, and be a stream can seek.Differrent cache key must have a different http client.
        /// </summary>
        /// <param name="httpWebRequest"></param>
        /// <param name="contentStream"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        /// <exception cref="WebException"></exception>
        public static HttpWebResponse SendHttpRequest(HttpWebRequest httpWebRequest,Stream contentStream=null,string cacheKey="",IList<IRetryStrategy> retryStrategies=null)
        {
            var parameter = new HttpClientCacheParameter
            {
                Milliseconds = httpWebRequest.Timeout,
                AllowAutoRedirect = httpWebRequest.AllowAutoRedirect,
                AutomaticDecompression = (int)httpWebRequest.AutomaticDecompression,
                ExternalCacheKey = cacheKey,
                Host = httpWebRequest.Host,
                PreAuthenticate = httpWebRequest.PreAuthenticate,
                CertificateHash= GetCertificatesHash(httpWebRequest.ClientCertificates),
                RetryHashCode= GetRetryHashCode(retryStrategies)
            };

            var request = new HttpRequestMessage(new HttpMethod(httpWebRequest.Method), httpWebRequest.RequestUri);
            if (contentStream != null && contentStream.Length > 0)
            {

                request.Content = new StreamContent(contentStream);
                request.Content.Headers.ContentLength = contentStream.Length;
//#if DEBUG
//                logger.Debug($"[MonitorRequest]Execute Request:set content stream {contentStream.Length}");
//#endif
            }
            else
            {
                var _requestStream = typeof(HttpWebRequest).GetField("_requestStream", flags).GetValue(httpWebRequest);
                if (_requestStream != null)
                {
                    ArraySegment<byte> bytes = (ArraySegment<byte>)_requestStream.GetType().GetMethod("GetBuffer",flags).Invoke(_requestStream,new object[] { });
                    if (bytes.Any())
                    {
//#if DEBUG
//                        logger.Debug($"[MonitorRequest]Execute Request:set _requestStream bytes {bytes.Count}");
//#endif
                        request.Content = new ByteArrayContent(bytes.Array!, bytes.Offset, bytes.Count);
                        request.Content.Headers.ContentLength = bytes.Count;
                    }
                }
            }

            if (httpWebRequest.RequestUri != null)
            {
                request.Headers.Host = httpWebRequest.RequestUri.Host;
            }

            AddCacheControlHeaders(request, httpWebRequest);

            foreach (var headerName in httpWebRequest.Headers.AllKeys)
            {
                // The System.Net.Http APIs require HttpRequestMessage headers to be properly divided between the request headers
                // collection and the request content headers collection for all well-known header names.  And custom headers
                // are only allowed in the request headers collection and not in the request content headers collection.
                if (HttpKnownHeaderNames.IsWellKnownContentHeader(headerName))
                {
                    if (request.Content == null)
                    {
                        // Create empty content so that we can send the entity-body header.
                        request.Content = new ByteArrayContent(Array.Empty<byte>());
//#if DEBUG
//                        logger.Debug($"[MonitorRequest]Execute Request:Create empty content");
//#endif
                    }
                    if (string.Equals(HttpKnownHeaderNames.ContentLength, headerName))
                    {
//#if DEBUG
//                        logger.Debug($"[MonitorRequest]Execute Request:skip content length");
//#endif
                        continue;
                    }
                    request.Content.Headers.TryAddWithoutValidation(headerName, httpWebRequest.Headers[headerName!]);
                }
                else
                {
                    if (string.Equals(HttpKnownHeaderNames.Host, headerName) && request.Headers.GetValues(HttpKnownHeaderNames.Host).Any())
                    {
//#if DEBUG
//                        logger.Debug($"[MonitorRequest]Execute Request:skip Host");
//#endif
                        continue;
                    }
                    request.Headers.TryAddWithoutValidation(headerName, httpWebRequest.Headers[headerName!]);
                }
            }
            request.Headers.TransferEncodingChunked = httpWebRequest.SendChunked;
            if (httpWebRequest.KeepAlive)
            {
                request.Headers.Connection.Add(HttpKnownHeaderNames.KeepAlive);
            }
            else
            {
                request.Headers.ConnectionClose = true;
            }
            request.Version = httpWebRequest.ProtocolVersion;
            bool created = false;
           
            var client = factoryCache.GetOrAdd<(HttpWebRequest Request, IList<IRetryStrategy> Strategies)>(parameter,
                (param,parameters) =>
                { 
                    var clientItem= CreateHttpClientWithSocketHandler(param, parameters.Request, parameters.Strategies);
                    created = true;
                    logger.Info($"[MonitorRequest] Create HttpClient For HttpWebRequest {parameter.Host}-{parameter.ExternalCacheKey}-{parameter.PreAuthenticate}-{parameter.Milliseconds}-{parameter.AllowAutoRedirect}-{parameter.AutomaticDecompression}-{parameter.CertificateHash}");
                    return clientItem;
                }, (httpWebRequest, retryStrategies));
//#if DEBUG
//            logger.Debug($"[MonitorRequest]Execute Request:NewCreated:{created}, {parameter.Host}-{parameter.ExternalCacheKey}-{parameter.PreAuthenticate}-{parameter.Milliseconds}-{parameter.AllowAutoRedirect}-{parameter.AutomaticDecompression}-{parameter.CertificateHash}");
//            logger.Debug($"[MonitorRequest]Execute Request:RequstDetail:{Environment.NewLine}" +
//                $"RequestUri:{request.RequestUri}{Environment.NewLine}" +
//                $"Headers:{request.Headers}{Environment.NewLine}" +
//                $"ContentHeaders:{request.Content?.Headers}{Environment.NewLine}" +
//                $"ContentLength:{request.Content?.Headers?.ContentLength}{Environment.NewLine}" +
//                $"SecurityProtocol:{ServicePointManager.SecurityProtocol}");

//#endif

           var response = client.Send(request, httpWebRequest.AllowReadStreamBuffering ? HttpCompletionOption.ResponseContentRead : HttpCompletionOption.ResponseHeadersRead);
            HttpWebResponse httpWebResponse = CreateResponse(response, httpWebRequest.RequestUri, httpWebRequest.CookieContainer);
            int maxSuccessStatusCode = httpWebRequest.AllowAutoRedirect ? 299 : 399;
            if ((int)httpWebResponse.StatusCode > maxSuccessStatusCode || (int)httpWebResponse.StatusCode < 200)
            {
                throw new WebException(
                    $@"The remote server returned an error: ({(int)httpWebResponse.StatusCode}) {httpWebResponse.StatusDescription}.",
                    null,
                    WebExceptionStatus.ProtocolError,
                    httpWebResponse);
            }
            return httpWebResponse;
        }

        private static HttpClient CreateHttpClientWithSocketHandler(HttpClientCacheParameter parameter, HttpWebRequest httpWebRequest,IList<IRetryStrategy> retryStrategies)
        {
            var sockethandler = new SocketsHttpHandler() { MaxConnectionsPerServer = 64 };
            //var client = new HttpClient(sockethandler);
            var client = HttpClientFactory.CreateHttpClient(sockethandler, true, retryStrategies, null);
            sockethandler.AllowAutoRedirect = httpWebRequest.AllowAutoRedirect;
            sockethandler.MaxAutomaticRedirections = 50;
            sockethandler.MaxResponseHeadersLength = 64;
            sockethandler.PreAuthenticate = httpWebRequest.PreAuthenticate;
            client.Timeout = TimeSpan.FromMilliseconds(httpWebRequest.Timeout);
            sockethandler.UseCookies = false;
            sockethandler.UseProxy = false;
            sockethandler.SslOptions = new SslClientAuthenticationOptions
            {
                //https://learn.microsoft.com/zh-cn/dotnet/core/compatibility/networking/7.0/allowrenegotiation-default
                AllowRenegotiation = true,
                EnabledSslProtocols = SslProtocols.None | SslProtocols.Tls12 | SslProtocols.Tls13,
                //RemoteCertificateValidationCallback = delegate { return true; },
                CertificateRevocationCheckMode = ServicePointManager.CheckCertificateRevocationList ? X509RevocationMode.Online : X509RevocationMode.NoCheck
            };
            if (httpWebRequest.ClientCertificates != null && httpWebRequest.ClientCertificates.Count > 0)
            {
                sockethandler.SslOptions.ClientCertificates = httpWebRequest.ClientCertificates;
            }
            sockethandler.ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.NoDelay = true;

                    using (cancellationToken.UnsafeRegister(s => ((Socket)s!).Dispose(), socket))
                    {
                        socket.Connect(context.DnsEndPoint);
                    }

                    // Throw in case cancellation caused the socket to be disposed after the Connect completed
                    cancellationToken.ThrowIfCancellationRequested();

                    if (httpWebRequest.ReadWriteTimeout > 0) // default is 5 minutes, so this is generally going to be true
                    {
                        socket.SendTimeout = socket.ReceiveTimeout = httpWebRequest.ReadWriteTimeout;
                    }
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }

                return await System.Threading.Tasks.Task.FromResult(new NetworkStream(socket, ownsSocket: true));
            };
            return client;
        }

        private static BindingFlags flags = BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.SetProperty
                  | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;
        private static HttpWebResponse CreateResponse(HttpResponseMessage httpResponseMessage, Uri responseUri, CookieContainer? cookieContainer)
        {
            var cts = typeof(HttpWebResponse).GetConstructors(flags);
            var fields = typeof(HttpWebResponse).GetFields(flags);
            System.Text.StringBuilder bs = new System.Text.StringBuilder();
            foreach (var ctor in cts)
            {
                bs.AppendLine($"Constructor: {ctor.ToString()}");
               
            }
            bs.AppendLine("");
            foreach (var f in fields)
            {
                bs.AppendLine($"Field: {f.Name}");
            }
            try
            {
                //public static object? CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type, BindingFlags bindingAttr, Binder? binder, object?[]? args, CultureInfo? culture);
                return (HttpWebResponse)Activator.CreateInstance(typeof(HttpWebResponse), flags, null,new object[] { httpResponseMessage, responseUri, cookieContainer },CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                Console.WriteLine(bs.ToString());
                throw;
            }
        }


        private static void AddCacheControlHeaders(HttpRequestMessage request, HttpWebRequest originalRequest)
        {
            RequestCachePolicy? policy = GetApplicableCachePolicy(originalRequest);

            if (policy != null && policy.Level != RequestCacheLevel.BypassCache)
            {
                CacheControlHeaderValue? cacheControl = null;
                HttpHeaderValueCollection<NameValueHeaderValue> pragmaHeaders = request.Headers.Pragma;

                if (policy is HttpRequestCachePolicy httpRequestCachePolicy)
                {
                    switch (httpRequestCachePolicy.Level)
                    {
                        case HttpRequestCacheLevel.NoCacheNoStore:
                            cacheControl = new CacheControlHeaderValue
                            {
                                NoCache = true,
                                NoStore = true
                            };
                            pragmaHeaders.Add(new NameValueHeaderValue("no-cache"));
                            break;
                        case HttpRequestCacheLevel.Reload:
                            cacheControl = new CacheControlHeaderValue
                            {
                                NoCache = true
                            };
                            pragmaHeaders.Add(new NameValueHeaderValue("no-cache"));
                            break;
                        case HttpRequestCacheLevel.CacheOnly:
                            throw new WebException("The request was aborted: The request cache-only policy does not allow a network request and the response is not found in cache.", WebExceptionStatus.CacheEntryNotFound);
                        case HttpRequestCacheLevel.CacheOrNextCacheOnly:
                            cacheControl = new CacheControlHeaderValue
                            {
                                OnlyIfCached = true
                            };
                            break;
                        case HttpRequestCacheLevel.Default:
                            cacheControl = new CacheControlHeaderValue();

                            if (httpRequestCachePolicy.MinFresh > TimeSpan.Zero)
                            {
                                cacheControl.MinFresh = httpRequestCachePolicy.MinFresh;
                            }

                            if (httpRequestCachePolicy.MaxAge != TimeSpan.MaxValue)
                            {
                                cacheControl.MaxAge = httpRequestCachePolicy.MaxAge;
                            }

                            if (httpRequestCachePolicy.MaxStale > TimeSpan.Zero)
                            {
                                cacheControl.MaxStale = true;
                                cacheControl.MaxStaleLimit = httpRequestCachePolicy.MaxStale;
                            }

                            break;
                        case HttpRequestCacheLevel.Refresh:
                            cacheControl = new CacheControlHeaderValue
                            {
                                MaxAge = TimeSpan.Zero
                            };
                            pragmaHeaders.Add(new NameValueHeaderValue("no-cache"));
                            break;
                    }
                }
                else
                {
                    switch (policy.Level)
                    {
                        case RequestCacheLevel.NoCacheNoStore:
                            cacheControl = new CacheControlHeaderValue
                            {
                                NoCache = true,
                                NoStore = true
                            };
                            pragmaHeaders.Add(new NameValueHeaderValue("no-cache"));
                            break;
                        case RequestCacheLevel.Reload:
                            cacheControl = new CacheControlHeaderValue
                            {
                                NoCache = true
                            };
                            pragmaHeaders.Add(new NameValueHeaderValue("no-cache"));
                            break;
                        case RequestCacheLevel.CacheOnly:
                            throw new WebException("The request was aborted: The request cache-only policy does not allow a network request and the response is not found in cache.", WebExceptionStatus.CacheEntryNotFound);
                    }
                }

                if (cacheControl != null)
                {
                    request.Headers.CacheControl = cacheControl;
                }
            }
        }

        private static RequestCachePolicy? GetApplicableCachePolicy(HttpWebRequest originalRequest)
        {
            if (originalRequest.CachePolicy != null)
            {
                return originalRequest.CachePolicy;
            }
            //else if (_isDefaultCachePolicySet && DefaultCachePolicy != null)
            //{
            //    return DefaultCachePolicy;
            //}
            else
            {
                return WebRequest.DefaultCachePolicy;
            }
        }
    }
}