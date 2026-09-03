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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Amazon.AWS4Signature.#.cctor()", MessageId = "aws")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Amazon.AWS4Signature.#ComputeSignature(System.String,System.String,System.String,System.DateTime,System.String,System.String,System.String)", MessageId = "aws")]
namespace AvePoint.Media.Storage.Cloud.Amazon
{
    public class AWS4Signature
    {
        public const string ISO8601BasicDateFormat = "yyyyMMdd";
        public const string ISO8601BasicDateTimeFormat = "yyyyMMddTHHmmssZ";

        public const string Scheme = "AWS4";
        public const string Algorithm = "HMAC-SHA256";
        public const string Terminator = "aws4_request";
        public static readonly byte[] TerminatorBytes = Encoding.UTF8.GetBytes(Terminator);

        public const string Credential = "Credential";
        public const string SignedHeaders = "SignedHeaders";
        public const string Signature = "Signature";

        public const string EmptyBodySha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
        public const string StreamingBodySha256 = "STREAMING-AWS4-HMAC-SHA256-PAYLOAD";

        public const string HostHeader = "host";
        public const string AuthorizationHeader = "Authorization";

        internal const string XAmzDate = "X-Amz-Date";
        internal const string XAmzSignedHeaders = "X-Amz-SignedHeaders";
        internal const string XAmzContentSha256 = "X-Amz-Content-SHA256";
        internal const string XAmzDecodedContentLength = "X-Amz-Decoded-Content-Length";

        static readonly Regex CompressWhitespaceRegex = new Regex("\\s+");

        public static void Sign(HttpWebRequest request, string awsAccessKeyId, string awsSecretAccessKey, string region)
        {
            string signingResult = SignRequest(request, awsAccessKeyId, awsSecretAccessKey, region);
            request.Headers[AuthorizationHeader] = signingResult;
        }

        public static string SignRequest(HttpWebRequest request, string awsAccessKeyId, string awsSecretAccessKey, string region)
        {
            var signedAt = InitializeHeaders(request, request.RequestUri);
            //var region = "us-east-1";//DetermineRegion(clientConfig);
            var service = "s3";//DetermineService(clientConfig);

            var resourcePath = string.Empty;

            resourcePath = request.RequestUri.AbsolutePath;
            resourcePath = HttpUtility.UrlDecode(resourcePath);

            // if UseQueryString is indicated and Parameters are present, canonicalize those (including uri encoding them)
            // otherwise if we spotted parameters in the resource path, canonicalize those instead (which should be encoded
            // already)

            string canonicalQueryParams = GetSortedQueryString(request);

            //NameValueCollection quertCollection = HttpUtility.ParseQueryString(request.RequestUri.Query);
            //if (quertCollection != null && quertCollection.Count > 0)
            //    canonicalQueryParams = CanonicalizeQueryParameters(request.Parameters);
            //else if (resourcePathParamStart != -1)
            //    canonicalQueryParams = CanonicalizeQueryParameters(request.ResourcePath.Substring(resourcePathParamStart + 1), false);

            var bodyHash = SetRequestBodyHash(request);
            var sortedHeaders = SortHeaders(request.Headers);
            var canonicalRequest = CanonicalizeRequest(resourcePath,
                                                       request.Method,
                                                       sortedHeaders,
                                                       canonicalQueryParams,
                                                       bodyHash);
            //if (metrics != null)
            //    metrics.AddProperty(Metric.CanonicalRequest, canonicalRequest);

            return ComputeSignature(awsAccessKeyId,
                                    awsSecretAccessKey,
                                    region,
                                    signedAt,
                                    service,
                                    CanonicalizeHeaderNames(sortedHeaders),
                                    canonicalRequest);
        }

        private static string GetResourcePath(HttpWebRequest request)
        {
            string resourcePath = "";
            string fullPath = request.RequestUri.ToString();

            return resourcePath;
        }

        private static string GetSortedQueryString(HttpWebRequest request)
        {
            string queryString = request.RequestUri.Query;
            queryString = HttpUtility.UrlDecode(queryString);

            if (string.IsNullOrEmpty(queryString))
            {
                return "";
            }
            if (queryString.StartsWith("?", StringComparison.OrdinalIgnoreCase))
            {
                queryString = queryString.TrimStart('?');
            }

            string[] queryP = queryString.Split('&');
            SortedDictionary<string, string> sortedKeyValue = new SortedDictionary<string, string>();
            foreach (string queryStr in queryP)
            {
                if (queryStr.Contains("="))
                {
                    string[] strs = queryStr.Split('=');
                    sortedKeyValue.Add(strs[0], strs[1]);
                }
                else
                {
                    sortedKeyValue.Add(queryStr, "");
                }
            }

            string canonicalQueryParams = "";
            foreach (string key in sortedKeyValue.Keys)
            {
                canonicalQueryParams = canonicalQueryParams + UrlEncode(key, false) + "=" + UrlEncode(sortedKeyValue[key], false) + "&";
                //if (sortedKeyValue[key] != null)
                //{
                //    canonicalQueryParams = canonicalQueryParams + UrlEncode(key, false) + "=" + UrlEncode(sortedKeyValue[key], false) + "&";
                //}
                //else 
                //{
                //    canonicalQueryParams = canonicalQueryParams + UrlEncode(key, false) + "&";
                //}

            }
            if (canonicalQueryParams.EndsWith("&", StringComparison.OrdinalIgnoreCase))
                canonicalQueryParams = canonicalQueryParams.TrimEnd('&');

            return canonicalQueryParams;
        }

        public static DateTime InitializeHeaders(HttpWebRequest webRequest, Uri requestEndpoint)
        {
            return InitializeHeaders(webRequest, requestEndpoint, DateTime.UtcNow);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "X-Amz-Date")]
        public static DateTime InitializeHeaders(HttpWebRequest webRequest, Uri requestEndpoint, DateTime requestDateTime)
        {
            // clean up any prior signature in the headers if resigning
            webRequest.Headers.Remove(AuthorizationHeader);

            if (webRequest.Headers[HostHeader] == null)
            {
                var hostHeader = requestEndpoint.Host;
                if (!requestEndpoint.IsDefaultPort)
                    hostHeader += ":" + requestEndpoint.Port;
                System.Reflection.MethodInfo addWithoutValidateHeadersMethod = typeof(WebHeaderCollection).GetMethod("AddWithoutValidate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                addWithoutValidateHeadersMethod.Invoke(webRequest.Headers, new[] { "Host", hostHeader });
            }

            var dt = DateTime.UtcNow;
            webRequest.Headers[XAmzDate] = dt.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);

            return dt;
        }

        /// <summary>
        /// Computes and returns an AWS4 signature for the specified canonicalized request
        /// </summary>
        /// <param name="awsAccessKey"></param>
        /// <param name="awsSecretAccessKey"></param>
        /// <param name="region"></param>
        /// <param name="signedAt"></param>
        /// <param name="service"></param>
        /// <param name="signedHeaders"></param>
        /// <param name="canonicalRequest"></param>
        /// <param name="metrics"></param>
        /// <returns></returns>
        public static string ComputeSignature(string awsAccessKey, string awsSecretAccessKey, string region, DateTime signedAt,
                                                         string service, string signedHeaders, string canonicalRequest)
        {
            var dateStamp = FormatDateTime(signedAt, ISO8601BasicDateFormat);
            var scope = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}/{3}", dateStamp, region, service, Terminator);

            var stringToSign = new StringBuilder();
            stringToSign.AppendFormat(CultureInfo.InvariantCulture, "{0}-{1}\n{2}\n{3}\n",
                                      Scheme, Algorithm, FormatDateTime(signedAt, ISO8601BasicDateTimeFormat), scope);

            var canonicalRequestHashBytes = CryptoUtil.ComputeHash(canonicalRequest);
            stringToSign.Append(ToHex(canonicalRequestHashBytes, true));

            var key = ComposeSigningKey(awsSecretAccessKey, region, dateStamp, service);

            var signature = CryptoUtil.ComputeHash(key, stringToSign.ToString());
            return GetAuthorizationHeader(awsAccessKey, signedHeaders, scope, key, signature);
        }

        /// <summary>
        /// Formats the supplied date and time for use in AWS4 signing, where various formats are used.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="formatString">The required format</param>
        /// <returns>The UTC date/time in the requested format</returns>
        public static string FormatDateTime(DateTime dt, string formatString)
        {
            return dt.ToString(formatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Compute and return the multi-stage signing key for the request.
        /// </summary>
        /// <param name="awsSecretAccessKey">The clear-text AWS secret key, if not held in secureKey</param>
        /// <param name="region">The region in which the service request will be processed</param>
        /// <param name="date">Date of the request, in yyyyMMdd format</param>
        /// <param name="service">The name of the service being called by the request</param>
        /// <returns>Computed signing key</returns>
        public static byte[] ComposeSigningKey(string awsSecretAccessKey, string region, string date, string service)
        {
            char[] ksecret = null;

            try
            {
                ksecret = (Scheme + awsSecretAccessKey).ToCharArray();

                var hashDate = CryptoUtil.ComputeHash(Encoding.UTF8.GetBytes(ksecret), Encoding.UTF8.GetBytes(date));
                var hashRegion = CryptoUtil.ComputeHash(hashDate, Encoding.UTF8.GetBytes(region));
                var hashService = CryptoUtil.ComputeHash(hashRegion, Encoding.UTF8.GetBytes(service));
                return CryptoUtil.ComputeHash(hashService, TerminatorBytes);
            }
            finally
            {
                // clean up all secrets, regardless of how initially seeded (for simplicity)
                if (ksecret != null)
                    Array.Clear(ksecret, 0, ksecret.Length);
            }
        }

        /// <summary>
        /// If the caller has already set the x-amz-content-sha256 header with a pre-computed
        /// content hash, or it is present as ContentStreamHash on the request instance, return
        /// the value to be used in request canonicalization. 
        /// If not set as a header or in the request, attempt to compute a hash based on
        /// inspection of the style of the request content.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// The computed hash, whether already set in headers or computed here. Null
        /// if we were not able to compute a hash.
        /// </returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "X-Amz-Content-SHA")]
        public static string SetRequestBodyHash(HttpWebRequest request)
        {
            string computedContentHash = request.Headers[XAmzContentSha256];
            if (computedContentHash != null)
                return computedContentHash;

            if (request.SendChunked)
            {
                //computedContentHash = StreamingBodySha256;
                //if (request.Headers["Content-Length"] != null)
                //{
                //    // substitute the originally declared content length with the true size of
                //    // the data we'll upload, which is inflated with chunk metadata
                //    request.Headers[XAmzDecodedContentLength] = request.Headers["Content-Length"];
                //    var originalContentLength = long.Parse(request.Headers["Content-Length"], CultureInfo.InvariantCulture);
                //    request.Headers["Content-Length"]
                //        = ChunkedUploadWrapperStream.ComputeChunkedContentLength(originalContentLength).ToString(CultureInfo.InvariantCulture);
                //}
                //request.Headers["Content-Encoding"] = "aws-chunked";
            }
            else
            {
                if (request.Method.Equals("put") && request.GetRequestStream() != null)
                    computedContentHash = ComputeContentStreamHash(request.GetRequestStream());
                else
                {
                    //byte[] payloadHashBytes;
                    //if (request.Content != null)
                    //    payloadHashBytes = CryptoUtilFactory.CryptoInstance.ComputeSHA256Hash(request.Content);
                    //else
                    //{
                    //    var payload = request.UseQueryString ? "" : GetRequestPayload(request);
                    //    payloadHashBytes = CryptoUtilFactory.CryptoInstance.ComputeSHA256Hash(Encoding.UTF8.GetBytes(payload));
                    //}
                    SHA256 signature = SHA256.Create();
                    byte[] payloadHashBytes = signature.ComputeHash(Encoding.UTF8.GetBytes(""));
                    computedContentHash = ToHex(payloadHashBytes, true);
                }
            }

            if (computedContentHash != null)
                request.Headers.Add(XAmzContentSha256, computedContentHash);

            return computedContentHash;
        }

        //internal static string DetermineRegion(ClientConfig clientConfig)
        //{
        //    if (!string.IsNullOrEmpty(clientConfig.AuthenticationRegion))
        //        return clientConfig.AuthenticationRegion.ToLower(CultureInfo.InvariantCulture);

        //    if (!string.IsNullOrEmpty(clientConfig.ServiceURL))
        //    {
        //        var parsedRegion = AWSSDKUtils.DetermineRegion(clientConfig.ServiceURL);
        //        if (!string.IsNullOrEmpty(parsedRegion))
        //            return parsedRegion.ToLower(CultureInfo.InvariantCulture);
        //    }

        //    return clientConfig.RegionEndpoint != null
        //        ? clientConfig.RegionEndpoint.SystemName
        //        : string.Empty;
        //}

        //internal static string DetermineService(ClientConfig clientConfig)
        //{
        //    return !string.IsNullOrEmpty(clientConfig.AuthenticationServiceName)
        //        ? clientConfig.AuthenticationServiceName.ToLower(CultureInfo.InvariantCulture)
        //        : AWSSDKUtils.DetermineService(clientConfig.DetermineServiceURL()).ToLower(CultureInfo.InvariantCulture);
        //}

        /// <summary>
        /// Computes and returns the canonical request
        /// </summary>
        /// <param name="resourcePath">the path of the resource being operated on</param>
        /// <param name="httpMethod">The http method used for the request</param>
        /// <param name="sortedHeaders">The full request headers, sorted into canonical order</param>
        /// <param name="canonicalQueryString">The query parameters for the request</param>
        /// <param name="precomputedBodyHash">
        /// The hash of the binary request body if present. If not supplied, the routine
        /// will look for the hash as a header on the request.
        /// </param>
        /// <returns>Canonicalised request as a string</returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "X-Amz-Content-SHA")]
        protected static string CanonicalizeRequest(string resourcePath, string httpMethod,
                                                    IDictionary<string, string> sortedHeaders,
                                                    string canonicalQueryString,
                                                    string precomputedBodyHash)
        {
            StringBuilder canonicalRequest = new StringBuilder();
            canonicalRequest.AppendFormat("{0}\n", httpMethod);
            canonicalRequest.AppendFormat("{0}\n", CanonicalizeResourcePath(resourcePath));
            canonicalRequest.AppendFormat("{0}\n", canonicalQueryString);

            canonicalRequest.AppendFormat("{0}\n", CanonicalizeHeaders(sortedHeaders));
            canonicalRequest.AppendFormat("{0}\n", CanonicalizeHeaderNames(sortedHeaders));

            if (precomputedBodyHash != null)
            {
                canonicalRequest.Append(precomputedBodyHash);
            }
            else
            {
                if (sortedHeaders[XAmzContentSha256] != null)
                    canonicalRequest.Append(sortedHeaders[XAmzContentSha256]);
            }

            return canonicalRequest.ToString();
        }

        /// <summary>
        /// Returns the canonicalized resource path for the service endpoint
        /// </summary>
        /// <param name="resourcePath">Resource path for the request</param>
        /// <returns>Canonicalized resource path for the endpoint</returns>
        protected static string CanonicalizeResourcePath(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath) || resourcePath.Equals("/"))
                return "/";
            var canonicalizedPath = resourcePath.StartsWith("/", StringComparison.Ordinal) ? resourcePath : "/" + resourcePath;
            return UrlEncode(canonicalizedPath, true);
        }

        /// <summary>
        /// Reorders the headers for the request for canonicalization.
        /// </summary>
        /// <param name="requestHeaders">The set of proposed headers for the request</param>
        /// <returns>List of headers that must be included in the signature</returns>
        /// <remarks>For AWS4 signing, all headers are considered viable for inclusion</remarks>
        protected static IDictionary<string, string> SortHeaders(NameValueCollection requestHeaders)
        {
            var canonicalizedHeaders = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string headerKey in requestHeaders.AllKeys)
            {
                canonicalizedHeaders.Add(headerKey, requestHeaders[headerKey]);
            }
            return canonicalizedHeaders;
        }

        /// <summary>
        /// Computes the canonical headers with values for the request. Only headers included in the signature
        /// are included in the canonicalization process.
        /// </summary>
        /// <param name="sortedHeaders">All request headers, sorted into canonical order</param>
        /// <returns>Canonicalized string of headers, with the header names in lower case.</returns>
        protected static string CanonicalizeHeaders(IDictionary<string, string> sortedHeaders)
        {
            if (sortedHeaders == null || sortedHeaders.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in sortedHeaders)
            {
                builder.Append(entry.Key.ToLower(CultureInfo.InvariantCulture));
                builder.Append(":");
                builder.Append(CompressSpaces(entry.Value));
                builder.Append("\n");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns the set of headers included in the signature as a flattened, ;-delimited string
        /// </summary>
        /// <param name="sortedHeaders">The headers included in the signature</param>
        /// <returns>Formatted string of header names</returns>
        protected static string CanonicalizeHeaderNames(IDictionary<string, string> sortedHeaders)
        {
            var builder = new StringBuilder();
            foreach (KeyValuePair<string, string> header in sortedHeaders)
            {
                if (builder.Length > 0)
                    builder.Append(";");
                builder.Append(header.Key.ToLower(CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Computes and returns the canonicalized query string, if query parameters have been supplied.
        /// Parameters with no value will be canonicalized as 'param='. The expectation is that parameters
        /// have not already been url encoded prior to canonicalization.
        /// </summary>
        /// <param name="queryString">The set of parameters being passed on the uri</param>
        /// <param name="uriEncodeParameters">
        /// Parameters must be uri encoded into the canonical request and by default the signer expects
        /// that the supplied collection contains non-encoded data. Set this to false if the encoding was
        /// done prior to signer entry.
        /// </param>
        /// <returns>The uri encoded query string parameters in canonical ordering</returns>
        protected static string CanonicalizeQueryParameters(string queryString, bool uriEncodeParameters = true)
        {
            if (string.IsNullOrEmpty(queryString))
                return string.Empty;

            var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var queryParamsStart = queryString.IndexOf('?');
            var qs = queryString.Substring(++queryParamsStart);
            int subStringPos = 0;
            int index = qs.IndexOfAny(new char[] { '&', ';' }, 0);
            if (index == -1 && subStringPos < qs.Length)
                index = qs.Length;
            while (index != -1)
            {
                string token = qs.Substring(subStringPos, index - subStringPos);

                // If the next character is a space then this isn't the end of query string value
                // Content Disposition is an example of this.
                if (!(index + 1 < qs.Length && qs[index + 1] == ' '))
                {
                    int equalPos = token.IndexOf('=');
                    if (equalPos == -1)
                        queryParams.Add(token, null);
                    else
                        queryParams.Add(token.Substring(0, equalPos), token.Substring(equalPos + 1));

                    subStringPos = index + 1;
                }

                if (qs.Length <= index + 1)
                    break;

                index = qs.IndexOfAny(new char[] { '&', ';' }, index + 1);
                if (index == -1 && subStringPos < qs.Length)
                    index = qs.Length;
            }

            return CanonicalizeQueryParameters(queryParams, uriEncodeParameters);
        }

        /// <summary>
        /// Computes and returns the canonicalized query string, if query parameters have been supplied.
        /// Parameters with no value will be canonicalized as 'param='. The expectation is that parameters
        /// have not already been url encoded prior to canonicalization.
        /// </summary>
        /// <param name="parameters">The set of parameters to be encoded in the query string</param>
        /// <param name="uriEncodeParameters">
        /// Parameters must be uri encoded into the canonical request and by default the signer expects
        /// that the supplied collection contains non-encoded data. Set this to false if the encoding was
        /// done prior to signer entry.
        /// </param>
        /// <returns>The uri encoded query string parameters in canonical ordering</returns>
        protected static string CanonicalizeQueryParameters(IDictionary<string, string> parameters,
                                                            bool uriEncodeParameters = true)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;

            var canonicalQueryString = new StringBuilder();
            var queryParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            foreach (var p in queryParams)
            {
                if (canonicalQueryString.Length > 0)
                    canonicalQueryString.Append("&");
                if (uriEncodeParameters)
                {
                    if (string.IsNullOrEmpty(p.Value))
                        canonicalQueryString.AppendFormat("{0}=", UrlEncode(p.Key, false));
                    else
                        canonicalQueryString.AppendFormat("{0}={1}", UrlEncode(p.Key, false), UrlEncode(p.Value, false));
                }
                else
                {
                    if (string.IsNullOrEmpty(p.Value))
                        canonicalQueryString.AppendFormat("{0}=", p.Key);
                    else
                        canonicalQueryString.AppendFormat("{0}={1}", p.Key, p.Value);
                }
            }

            return canonicalQueryString.ToString();
        }

        static string CompressSpaces(string data)
        {
            if (data == null || !data.Contains(" "))
                return data;

            var compressed = CompressWhitespaceRegex.Replace(data, " ");
            return compressed;
        }

        /// <summary>
        /// Returns the request parameters in the form of a query string.
        /// </summary>
        /// <param name="request">The request instance</param>
        /// <returns>Request parameters in query string format</returns>
        //static string GetRequestPayload(IRequest request)
        //{
        //    if (request.Content == null)
        //        return AWSSDKUtils.GetParametersAsString(request.Parameters);
        //    else
        //    {
        //        var encoding = Encoding.GetEncoding(DEFAULT_ENCODING);
        //        return encoding.GetString(request.Content, 0, request.Content.Length);
        //    }
        //}

        public const string validUrlCharacters1 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
        public const string validUrlCharacters2 = "/";//  ":'()!*[]";
        static string UrlEncode2(string data, bool path)
        {
            //int rfcNumber = 3986;
            StringBuilder encoded = new StringBuilder(data.Length * 2);

            string unreservedChars = String.Concat(validUrlCharacters1, (path ? validUrlCharacters2 : ""));

            foreach (char symbol in System.Text.Encoding.UTF8.GetBytes(data))
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    encoded.Append(symbol);
                }
                else
                {
                    encoded.Append("%").Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int)symbol));
                }
            }

            return encoded.ToString();
        }

        public static string UrlEncode(string input, bool ignoreSlash)
        {
            StringBuilder result = new StringBuilder();
            foreach (char ch in System.Text.Encoding.UTF8.GetBytes(input))
            {
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '-' || ch == '~' || ch == '.')
                {
                    result.Append(ch);
                }
                else if (ch == '/')
                {
                    result.Append(ignoreSlash ? ch + "" : "%2F");
                }
                else
                {
                    result.Append("%" + ((byte)ch).ToString("X2", CultureInfo.InvariantCulture));
                }
            }
            return result.ToString();
        }

        internal static string ToHex(byte[] data, bool lowercase)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString(lowercase ? "x2" : "X2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        public static string GetAuthorizationHeader(string awsAccessKeyId, string signedHeaders, string scope, byte[] signingKey, byte[] signature)
        {
            StringBuilder authorizationHeader = new StringBuilder();
            authorizationHeader.AppendFormat("{0}-{1} ", Scheme, Algorithm);
            authorizationHeader.AppendFormat("{0}={1}/{2}, ", Credential, awsAccessKeyId, scope);
            authorizationHeader.AppendFormat("{0}={1}, ", SignedHeaders, signedHeaders);
            authorizationHeader.AppendFormat("{0}={1}", Signature, ToHex(signature, true));

            return authorizationHeader.ToString();
        }


        public static string ComputeContentStreamHash(Stream contentStream)
        {
            var position = contentStream.Position;
            HMACSHA256 signature = new HMACSHA256();
            byte[] payloadHashBytes = signature.ComputeHash(contentStream);
            string contentStreamHash = ToHex(payloadHashBytes, true);
            contentStream.Seek(position, SeekOrigin.Begin);

            return contentStreamHash;
        }
    }
}
