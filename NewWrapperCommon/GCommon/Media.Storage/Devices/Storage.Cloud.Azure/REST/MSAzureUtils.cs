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


namespace AvePoint.Media.Storage.Cloud.Azure
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web;
    using System.IO;
    using System.Xml;
    using System.Diagnostics;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.GCommon.Utility.Cryptography;

    #endregion

    class MSAzureUtils
    {

        static string[] sArray;
        public static string[] SArray
        {
            get
            {
                if (sArray == null)
                {
                    sArray = new string[256];
                    for (int i = 0; i < sArray.Length; i++)
                    {
                        if (i < 16)
                        {
                            sArray[i] = Convert.ToString((char)(97 + i));
                        }
                        else
                        {
                            sArray[i] = sArray[i / 16] + (char)(97 + i % 16);
                        }
                    };
                }
                return sArray;
            }
        }

        public static string MetadataKeyEncode(string s)
        {
            StringBuilder result = new StringBuilder();
            byte[] bs = Encoding.Unicode.GetBytes(s);

            foreach (var b in bs)
            {
                result.Append(SArray[b]);
                result.Append("_");
            }
            return result.ToString();
        }

        public static string MetadataKeyDecode(string s)
        {
            string[] ss = s.Split(new char[] { '_' });
            byte[] bs = new byte[ss.Length - 1];
            for (int i = 0; i < bs.Length; i++)
            {
                int n = 0;
                for (int j = 0; i < SArray.Length; j++)
                {
                    if (SArray[j].Equals(ss[i]))
                    {
                        n = j;
                        break;
                    }
                }
                bs[i] = Convert.ToByte(n);
            }
            return Encoding.Unicode.GetString(bs);
        }

        public static string AuthorizationHeader(HttpWebRequest request, string storageAccount, string storageKey)
        {
            string MessageSignature;

            string method = request.Method;

            MessageSignature = String.Format("{0}\n\n\n{1}\n{5}\n\n\n\n{2}\n\n\n\n{3}{4}",
                method,
                (method == "GET" || method == "HEAD") ? String.Empty : request.ContentLength.ToString(),
                "",
                GetCanonicalizedHeaders(request),
                GetCanonicalizedResource(request.RequestUri, storageAccount),
                ""
                );
            //byte[] SignatureBytes = System.Text.Encoding.UTF8.GetBytes(MessageSignature);
            //System.Security.Cryptography.HMACSHA256 SHA256 = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(storageKey));
            //String AuthorizationHeader = "SharedKey " + storageAccount + ":" + Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));
            String AuthorizationHeader = "SharedKey " + storageAccount + ":" + GetHMACSHA256String(MessageSignature, storageKey);
            return AuthorizationHeader;
        }

        private static string GetHMACSHA256String(string signature, string storageKey)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.HMASHA256, Convert.FromBase64String(storageKey));
            byte[] value = hash.ComputeHash(Encoding.UTF8.GetBytes(signature));
            return Convert.ToBase64String(value);
        }

        public static string GetCanonicalizedResource(Uri address, string accountName)
        {
            StringBuilder str = new StringBuilder();
            StringBuilder builder = new StringBuilder("/");
            builder.Append(accountName);
            builder.Append(address.AbsolutePath);
            str.Append(builder.ToString());
            NameValueCollection values2 = new NameValueCollection();

            NameValueCollection values = HttpUtility.ParseQueryString(address.Query);
            foreach (string str2 in values.Keys)
            {
                ArrayList list = new ArrayList(values.GetValues(str2));
                list.Sort();
                StringBuilder builder2 = new StringBuilder();
                foreach (object obj2 in list)
                {
                    if (builder2.Length > 0)
                    {
                        builder2.Append(",");
                    }
                    builder2.Append(obj2.ToString());
                }
                values2.Add((str2 == null) ? str2 : str2.ToLowerInvariant(), builder2.ToString());
            }
            ArrayList list2 = new ArrayList(values2.AllKeys);
            list2.Sort();
            foreach (string str3 in list2)
            {
                StringBuilder builder3 = new StringBuilder(string.Empty);
                builder3.Append(str3);
                builder3.Append(":");
                builder3.Append(values2[str3]);
                str.Append("\n");
                str.Append(builder3.ToString());
            }
            return str.ToString();
        }

        public static string GetCanonicalizedHeaders(HttpWebRequest request)
        {
            ArrayList headerNameList = new ArrayList();
            StringBuilder sb = new StringBuilder();
            foreach (string headerName in request.Headers.Keys)
            {
                if (headerName.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
                {
                    headerNameList.Add(headerName.ToLowerInvariant());
                }
            }
            headerNameList.Sort();
            foreach (string headerName in headerNameList)
            {
                StringBuilder builder = new StringBuilder(headerName);
                string separator = ":";
                foreach (string headerValue in GetHeaderValues(request.Headers, headerName))
                {
                    string trimmedValue = headerValue.Replace("\r\n", String.Empty);
                    builder.Append(separator);
                    builder.Append(trimmedValue);
                    separator = ",";
                }
                sb.Append(builder.ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public static ArrayList GetHeaderValues(NameValueCollection headers, string headerName)
        {
            ArrayList list = new ArrayList();
            string[] values = headers.GetValues(headerName);
            if (values != null)
            {
                foreach (string str in values)
                {
                    list.Add(str.TrimStart(null));
                }
            }
            return list;
        }

        public static void signRequest(HttpWebRequest request, MSAzureRequest config)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string message = canonicalizeHttpRequest(request, config.UserName);
            string computedBase64Signature = ComputeMacSha(message, config);
            request.Headers.Add("Authorization", string.Format(CultureInfo.InvariantCulture,
                "{0} {1}:{2}", MSAzureConstants.SharedKeyAuthSchemeName, config.UserName, computedBase64Signature));
        }

        public static void signRequest(HttpWebRequest request, CloudOpenParameter openParam)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            string headerStr;
            if (request.Method.Equals("HEAD", StringComparison.CurrentCultureIgnoreCase))
            {
                headerStr = AuthorizationHeader(request, openParam.UserName, openParam.Password);
            }
            else
            {
                string message = canonicalizeHttpRequest(request, openParam.UserName);
                string computedBase64Signature = ComputeMacSha(message, openParam);
                headerStr = string.Format(CultureInfo.InvariantCulture,
                    "{0} {1}:{2}", MSAzureConstants.SharedKeyAuthSchemeName, openParam.UserName, computedBase64Signature);
            }
            request.Headers.Add("Authorization", headerStr);
        }

        internal static string GetSignForSAS(string filePath, string storageKey, DateTime currentTime)
        {
            string str = null;
            string sharedAccessStartTime = null;
            string sharedAccessExpiryTime = null;
            str = "r";
            //sharedAccessStartTime = currentTime.ToString("s") + "Z";
            sharedAccessExpiryTime = currentTime.AddDays(7).ToString("s") + "Z";
            string message = string.Format(CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n{4}\n{5}", new object[] { str, sharedAccessStartTime, sharedAccessExpiryTime, filePath, null, "2014-02-14" });

            string cacheControl = null;
            string contentDisposition = null;
            string contentEncoding = null;
            string contentLanguage = null;
            string contentType = null;
            message = message + string.Format(CultureInfo.InvariantCulture, "\n{0}\n{1}\n{2}\n{3}\n{4}", new object[] { cacheControl, contentDisposition, contentEncoding, contentLanguage, contentType });

            return GetHMACSHA256String(message, storageKey);
        }

        public static string canonicalizeHttpRequest(HttpWebRequest request, String accountName)
        {
            StringBuilder canonicalizedString = new StringBuilder();
            string verb = request.Method;
            string contentEncoding = getHeaderValue(request, MSAzureConstants.ContentEncoding);
            string contentLanguage = getHeaderValue(request, MSAzureConstants.ContentLanguage);
            string contentLength = request.ContentLength.ToString();
            //https://docs.microsoft.com/en-us/rest/api/storageservices/authorize-with-shared-key
            var msApiVersion = request.Headers["x-ms-version"].ToString();
            if (request.ContentLength == -1 || (request.ContentLength == 0 && !msApiVersion.Equals("2014-02-14")))
            {
                contentLength = string.Empty;
            }
            string contentMD5 = getHeaderValue(request, MSAzureConstants.ContentMD5);
            string contentType = getHeaderValue(request, MSAzureConstants.ContentType);
            string date = string.Empty;
            string ifModifiedSince = string.Empty;
            string ifMatch = string.Empty;
            string ifNoneMatch = string.Empty;
            string ifUnmodifiedSince = string.Empty;
            string Range = getHeaderValue(request, MSAzureConstants.Range);

            canonicalizedString.Append(verb).Append("\n");
            canonicalizedString.Append(contentEncoding).Append("\n");
            canonicalizedString.Append(contentLanguage).Append("\n");
            canonicalizedString.Append(contentLength).Append("\n");
            canonicalizedString.Append(contentMD5).Append("\n");
            canonicalizedString.Append(contentType).Append("\n");
            canonicalizedString.Append(date).Append("\n");
            canonicalizedString.Append(ifModifiedSince).Append("\n");
            canonicalizedString.Append(ifMatch).Append("\n");
            canonicalizedString.Append(ifNoneMatch).Append("\n");
            canonicalizedString.Append(ifUnmodifiedSince).Append("\n");
            canonicalizedString.Append(Range).Append("\n");
            canonicalizedString.Append(getCanonicalizedHeaders(request.Headers));
            canonicalizedString.Append("/").Append(accountName).Append(getCanonicalizedResource(request.Address));
            return canonicalizedString.ToString();
        }

        private static string ComputeMacSha(string canonicalizedString, MSAzureRequest config)
        {
            //byte[] dataToMAC = Encoding.UTF8.GetBytes(canonicalizedString);
            //byte[] key = Convert.FromBase64String(config.Password);
            //using (HMACSHA256 hmacsha1 = new HMACSHA256(key))
            //{
            //    return Convert.ToBase64String(hmacsha1.ComputeHash(dataToMAC));
            //}
            return GetHMACSHA256String(canonicalizedString, config.Password);
        }

        private static string ComputeMacSha(string canonicalizedString, CloudOpenParameter openParam)
        {
            //byte[] dataToMAC = Encoding.UTF8.GetBytes(canonicalizedString);
            //byte[] key = Convert.FromBase64String(openParam.Password);
            //using (HMACSHA256 hmacsha1 = new HMACSHA256(key))
            //{
            //    return Convert.ToBase64String(hmacsha1.ComputeHash(dataToMAC));
            //}
            return GetHMACSHA256String(canonicalizedString, openParam.Password);
        }

        private static string getHeaderValue(HttpWebRequest request, string headerName)
        {
            ArrayList values = getHeaderValues(request.Headers, headerName);
            if (values.Count > 0)
            {
                return (string)values[0];
            }
            return string.Empty;
        }

        private static ArrayList getHeaderValues(NameValueCollection headers, string headerName)
        {
            ArrayList arrayOfValues = new ArrayList();
            string[] values = headers.GetValues(headerName);
            if (values != null)
            {
                foreach (string value in values)
                {
                    // canonization formula requires the string to be left trimmed.
                    arrayOfValues.Add(value.TrimStart());
                }
            }

            return arrayOfValues;
        }

        private static string getCanonicalizedHeaders(NameValueCollection headers)
        {
            ArrayList httpStorageHeaderNameArray = new ArrayList();
            foreach (string key in headers.Keys)
            {
                if (key.ToLowerInvariant().StartsWith("x-ms", StringComparison.Ordinal))
                {
                    httpStorageHeaderNameArray.Add(key.ToLowerInvariant());
                }
            }

            httpStorageHeaderNameArray.Sort();
            StringBuilder builder = new StringBuilder();

            // Now go through each header's values in the sorted order and append them to the canonicalized string.
            foreach (string key in httpStorageHeaderNameArray)
            {
                StringBuilder canonicalizedElement = new StringBuilder(key);
                string delimiter = ":";
                ArrayList values = getHeaderValues(headers, key);

                // Go through values, unfold them, and then append them to the canonicalized element string.
                foreach (string value in values)
                {
                    // Unfolding is simply removal of CRLF.
                    string unfoldedValue = value.Replace("\r\n", string.Empty);

                    // Append it to the canonicalized element string.
                    canonicalizedElement.Append(delimiter);
                    canonicalizedElement.Append(unfoldedValue);
                    delimiter = ",";
                }
                builder.Append(canonicalizedElement).Append("\n");
            }
            return builder.ToString();
        }

        private static string getCanonicalizedResource(Uri address)
        {
            StringBuilder canonicalizedResource = new StringBuilder();
            canonicalizedResource.Append(address.AbsolutePath).Append("\n");

            NameValueCollection queryVariables = HttpUtility.ParseQueryString(address.Query);
            string[] keys = new string[queryVariables.Count];
            int index = 0;
            foreach (string key in queryVariables)
            {
                keys[index++] = key;
            }
            Array.Sort(keys);
            foreach (string key in keys)
            {
                string value = queryVariables[key];
                canonicalizedResource.Append(key).Append(":").Append(value).Append("\n");
            }
            return canonicalizedResource.ToString().Substring(0, canonicalizedResource.Length - 1);
        }

        public static string convertDateTimeToHttpString(DateTime dateTime)
        {
            // On the wire everything should be represented in UTC. This assert will catch invalid callers who
            // are violating this rule.
            Debug.Assert(dateTime == DateTime.MaxValue || dateTime == DateTime.MinValue || dateTime.Kind == DateTimeKind.Utc);

            // 'R' means rfc1123 date which is what our server uses for all dates...
            // It will be in the following format:
            // Sun, 28 Jan 2008 12:11:37 GMT
            return dateTime.ToString("r");//, CultureInfo.InvariantCulture);
            //return XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.RoundtripKind);
        }

        public static Stream GetSegmentStream(Stream stream, long len)
        {
            Stream result = new MemoryStream();

            int size = 0x10000;//64K
            byte[] buffer = new byte[size];
            long remain = len;

            int read = 0;
            while (remain > 0)
            {
                read = stream.Read(buffer, 0, size);
                if (read == 0)
                {
                    break;
                }
                result.Write(buffer, 0, read);
                remain -= read;
            }
            result.Position = 0;

            return result;
        }

        public static string GenerateBlockId(int index, int size)
        {
            int sizeLen = size.ToString().Length;
            int indexLen = index.ToString().Length;

            StringBuilder blockId = new StringBuilder("BlockId");
            for (int i = 0; i < (sizeLen - indexLen); i++)
            {
                blockId.Append("0");
            }
            blockId.Append(index);

            byte[] bytes = Encoding.UTF8.GetBytes(HttpUtility.UrlEncode(blockId.ToString()));
            return Convert.ToBase64String(bytes);
        }

        public static Stream BuildBlockListXml(List<string> blockIds)
        {
            MemoryStream stream = new MemoryStream();

            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.WriteStartDocument();
            writer.WriteStartElement(MSAzureConstants.BlockList);
            foreach (string id in blockIds)
            {
                writer.WriteElementString("Latest", id);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

    }
}
