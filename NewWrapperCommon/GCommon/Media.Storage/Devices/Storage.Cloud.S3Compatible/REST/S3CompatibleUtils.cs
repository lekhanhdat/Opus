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


namespace AvePoint.Media.Storage.S3Compatible.REST
{
    #region using directives
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    #endregion

    class S3CompatibleUtils : CommonUtility
    {
        private static StorageLogger logger = new StorageLogger(typeof(S3CompatibleUtils));
        private static Regex partNumberRegex = new Regex("\\?partNumber\\=.*$");
        private static Regex uploadIdRegex = new Regex("\\?uploadId\\=.*$");

        public static string GetReqeustDate(TimeSpan timeSpan)
        {
            string result = null;
            try
            {
                DateTime requestDate = DateTime.UtcNow;
                requestDate = requestDate + timeSpan;
                result = requestDate.ToString("r");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }
            return result;
        }

        /*
         * * Authorization = "AWS" + " " + AWSAccessKeyId + ":" + Signature;
         */

        public static void AddAuthorization(HttpWebRequest request, string awsAccessKeyId, string awsSecretAccessKey)
        {
            string authorization = "AWS " + awsAccessKeyId + ":" + GetSignature(request, awsSecretAccessKey);
            request.Headers.Add(HttpRequestHeader.Authorization, authorization);
        }

        public static string GetSignature(HttpWebRequest request, string secretAccessKeyId)
        {
            string verb = request.Method;
            string contentMD5 = GetHeaderValue(request, HttpRequestHeader.ContentMd5);
            string contentType = GetHeaderValue(request, HttpRequestHeader.ContentType);
            string date = GetHeaderValue(request, HttpRequestHeader.Date);
            string canonicalizedAmzHeaders = GetCanonicalizedAmzHeaders(request);
            string canonicalizedResource = GetCanonicalizedResource(request);
            StringBuilder stringToSign = new StringBuilder();
            stringToSign.Append(verb)
                        .Append("\n")
                        .Append(contentMD5)
                        .Append("\n")
                        .Append(contentType)
                        .Append("\n")
                        .Append(date)
                        .Append("\n")
                        .Append(canonicalizedAmzHeaders)
                        .Append(canonicalizedResource);
            string signature = Encode(secretAccessKeyId, stringToSign.ToString(), false);
            return signature;
        }

        public static string GetHeaderValue(WebRequest request, HttpRequestHeader header)
        {
            string value = request.Headers[header];

            if (value == null)
            {
                return "";
            }
            return value;
        }

        /*
           CanonicalizedResource = [ "/" + Bucket ] +
            <HTTP-Request-URI, from the protocol name up to the query string> +
            [ sub-resource, if present. For example "?acl", "?location", "?logging", or
            "?torrent"];
         */

        public static string GetCanonicalizedResource(HttpWebRequest request)
        {
            StringBuilder canonicalizedResourse = new StringBuilder();
            Uri uri = request.RequestUri;
            string resource = uri.AbsolutePath;
            string subResource = uri.Query;
            string host = uri.Host;
            canonicalizedResourse.Append(resource);
            if (subResource.Contains("?acl"))
            {
                canonicalizedResourse.Append("?acl");
            }
            if (subResource.Contains("?location"))
            {
                canonicalizedResourse.Append("?location");
            }
            if (subResource.Contains("?logging"))
            {
                canonicalizedResourse.Append("?logging");
            }
            if (subResource.Contains("?torrent"))
            {
                canonicalizedResourse.Append("?torrent");
            }
            if (subResource.Contains("?delete"))
            {
                canonicalizedResourse.Append("?delete");
            }

            if (subResource.Contains("?uploads"))
            {
                canonicalizedResourse.Append("?uploads");
            }
            Match partNumberMatch = partNumberRegex.Match(subResource);
            if (partNumberMatch.Success)
            {
                canonicalizedResourse.Append(partNumberMatch.Groups[0].Value);
            }
            else
            {
                Match uploadIdMatch = uploadIdRegex.Match(subResource);
                if (uploadIdMatch.Success)
                {
                    canonicalizedResourse.Append(uploadIdMatch.Groups[0].Value);
                }
            }
            return canonicalizedResourse.ToString();
        }

        /*
         * To construct the CanonicalizedAmzHeaders part of StringToSign, select all HTTP request headers that start with 
           'x-amz-' (using a case-insensitive comparison) and use the following process.
         * 1.Convert each HTTP header name to lower-case. For example, 'X-Amz-Date' becomes 'xamz-date'.
         * 2.Sort the collection of headers lexicographically by header name.
         * 3.Combine header fields with the same name into one "header-name:comma-separated-valuelist"
             pair as prescribed by RFC 2616, section 4.2, without any white-space between values. For
             example, the two metadata headers 'x-amz-meta-username: fred' and 'x-amz-metausername:barney' 
             would be combined into the single header 'x-amz-meta-username:fred,barney'.
         * 4."Unfold" long headers that span multiple lines (as allowed by RFC 2616, section 4.2) by
              replacing the folding white-space (including new-line) by a single space.
         * 5.Trim any white-space around the colon in the header. For example, the header 'x-amz-meta-username: fred,barney'
             would become 'x-amz-meta-username:fred,barney'
         * 6.Finally, append a new-line (U+000A) to each canonicalized header in the resulting list.Construct the                                  CanonicalizedResource element by concatenating all headers in this list into a
             single string.
         */

        public static string GetCanonicalizedAmzHeaders(HttpWebRequest request)
        {
            StringBuilder headerString = new StringBuilder();
            //header按字典顺序排序用
            SortedList<string, string> aws3Headers = new SortedList<string, string>();
            string regex = " {2,}|\t|\r|\n)";
            WebHeaderCollection headers = request.Headers;
            foreach (string hd in headers)
            {
                //header名字转为小写
                string name = hd.ToLower(CultureInfo.InvariantCulture);
                //获取CanonicalizedAmzHeaders感兴趣的header
                if (name.StartsWith(S3CompatibleConstants.S3Compatible_REST_HEADER_PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    //去掉前后空格、多重空格、制表符、换行符
                    string value = headers[hd].Trim();
                    value = value.Replace(regex, " ");
                    //将同名的header合并成：headerName:value1,value2...的形式。
                    if (aws3Headers.ContainsKey(name))
                    {
                        string tempValue = null;
                        aws3Headers.TryGetValue(name, out tempValue);
                        if (tempValue != null)
                        {
                            value = tempValue + "," + value;
                        }
                        aws3Headers.Remove(name);
                    }
                    aws3Headers.Add(name, value);
                }
            }

            foreach (KeyValuePair<string, string> item in aws3Headers)
            {
                headerString.Append(item.Key)
                            .Append(":")
                            .Append(item.Value)
                            .Append("\n");
            }
            return headerString.ToString();
        }

        public static string Encode(string awsSecretAccessKey, string canonicalString, bool urlEncode)
        {
            Encoding ae = new UTF8Encoding();
            HMACSHA1 signature = new HMACSHA1(ae.GetBytes(awsSecretAccessKey));
            string b64 = Convert.ToBase64String(signature.ComputeHash(ae.GetBytes(canonicalString.ToCharArray())));
            if (urlEncode)
            {
                return null;
                //return HttpUtility.UrlEncode(b64);
            }
            else
            {
                return b64;
            }
        }
    }
}
