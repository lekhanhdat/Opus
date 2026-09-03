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
namespace AvePoint.Media.Storage.Cloud.Cleversafe
{
    #region using directives
    using AvePoint.Media.Storage.Cloud.Amazon;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    #endregion
    class CleversafeUtils : CommonUtility
    {
        private static StorageLogger logger = new StorageLogger(typeof(CleversafeUtils));
        private static Regex partNumberRegex = new Regex("\\?partNumber\\=.*$");
        private static Regex uploadIdRegex = new Regex("\\?uploadId\\=.*$");

        public static String GetReqeustDate(TimeSpan timeSpan)
        {
            String result = null;
            try
            {
                DateTime requestDate = DateTime.UtcNow;
                requestDate = requestDate + timeSpan;
                result = requestDate.ToString("r");
            }
            catch (Exception e)
            {
                logger.Warn("An error while getting reqeust date. Details : {0}", e);
            }
            return result;
        }

        public static void AddAuthorization(HttpWebRequest request, String awsAccessKeyId, String awsSecretAccessKey)
        {
            logger.Debug("signature version is 2 ");
            String authorization = "AWS " + awsAccessKeyId + ":" + GetSignature(request, awsSecretAccessKey);
            request.Headers.Add(HttpRequestHeader.Authorization, authorization);
        }

        public static String GetSignature(HttpWebRequest request, String secretAccessKeyId)
        {
            String verb = request.Method;
            String contentMD5 = GetHeaderValue(request, HttpRequestHeader.ContentMd5);
            String contentType = GetHeaderValue(request, HttpRequestHeader.ContentType);
            String date = GetHeaderValue(request, HttpRequestHeader.Date);
            String canonicalizedAmzHeaders = GetCanonicalizedAmzHeaders(request);
            String canonicalizedResource = GetCanonicalizedResource(request);
            StringBuilder StringToSign = new StringBuilder();
            StringToSign.Append(verb)
                        .Append("\n")
                        .Append(contentMD5)
                        .Append("\n")
                        .Append(contentType)
                        .Append("\n")
                        .Append(date)
                        .Append("\n")
                        .Append(canonicalizedAmzHeaders)
                        .Append(canonicalizedResource);
            String signature = Encode(secretAccessKeyId, StringToSign.ToString(), false);
            return signature;
        }

        public static String Encode(String awsSecretAccessKey, String canonicalString, Boolean urlEncode)
        {
            Encoding ae = new UTF8Encoding();
            var signature = new HMACSHA1(ae.GetBytes(awsSecretAccessKey));
            String b64 = Convert.ToBase64String(signature.ComputeHash(ae.GetBytes(canonicalString.ToCharArray())));
            if (urlEncode)
            {
                return null;
            }
            else
            {
                return b64;
            }
        }

        public static String GetHeaderValue(WebRequest request, HttpRequestHeader header)
        {
            String value = request.Headers[header];
            if (value == null)
            {
                return "";
            }
            return value;
        }

        public static String GetCanonicalizedAmzHeaders(HttpWebRequest request)
        {
            StringBuilder headerString = new StringBuilder();
            //header按字典顺序排序用
            SortedList<String, String> aws3Headers = new SortedList<String, String>();
            String regex = " {2,}|\t|\r|\n)";
            WebHeaderCollection headers = request.Headers;
            foreach (String hd in headers)
            {
                //header名字转为小写
                String name = hd.ToLower(CultureInfo.InvariantCulture);
                //获取CanonicalizedAmzHeaders感兴趣的header
                if (name.StartsWith(CleversafeConstant.AWS3_REST_HEADER_PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    //去掉前后空格、多重空格、制表符、换行符
                    String value = headers[hd].Trim();
                    value = value.Replace(regex, " ");
                    //将同名的header合并成：headerName:value1,value2...的形式。
                    if (aws3Headers.ContainsKey(name))
                    {
                        String tempValue = null;
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
            foreach (KeyValuePair<String, String> item in aws3Headers)
            {
                headerString.Append(item.Key)
                            .Append(":")
                            .Append(item.Value)
                            .Append("\n");
            }
            return headerString.ToString();
        }

        public static String GetCanonicalizedResource(HttpWebRequest request)
        {
            StringBuilder canonicalizedResourse = new StringBuilder();
            Uri uri = request.RequestUri;
            String resource = uri.AbsolutePath;
            String subResource = uri.Query;
            String host = uri.Host;
            canonicalizedResourse.Append(resource);
            if (subResource.Contains("?facl"))
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
    }
}
