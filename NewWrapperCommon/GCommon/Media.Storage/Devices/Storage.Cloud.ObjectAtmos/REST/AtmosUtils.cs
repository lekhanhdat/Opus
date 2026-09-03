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

namespace AvePoint.Media.Storage.Cloud.ObjectAtmos
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Security.Cryptography;
    using System.Globalization;
    using System.Web;
    using System.Reflection;
    #endregion

    class AtmosUtils
    {
        #region Public Method
        /*
         * signature = Base64(HMACSHA1(HashString))
         * 
         *  HashString = HTTPRequestMethod + '\n' +
         *               ContentType + '\n' +
         *               Range + '\n' +
         *               Date + '\n' +
         *               CanonicalizedResource + '\n' +
         *               CanonicalizedEMCHeaders
         */
        public static string GetSignature(HttpWebRequest request, string secretKey, Dictionary<string, string> headers)
        {
            string verb = request.Method;
            string contentType = GetHeaderValue(headers, "Content-Type");
            string range = GetHeaderValue(headers, "Range");
            string date = GetHeaderValue(headers, "Date");
            string canonicalizedResource = GetCanonicalizedResource(request);
            string canonicalizedEMCHeaders = GetCanonicalizedEMCHeaders(headers);

            StringBuilder hashStr = new StringBuilder();
            hashStr.Append(verb)
                   .Append("\n")
                   .Append(contentType)
                   .Append("\n")
                   .Append(range)
                   .Append("\n")
                   .Append(date)
                   .Append("\n")
                   .Append(canonicalizedResource)
                   .Append("\n")
                   .Append(canonicalizedEMCHeaders);

            HMACSHA1 mac = new HMACSHA1(Convert.FromBase64String(secretKey));
            byte[] hashBytes = Encoding.UTF8.GetBytes(hashStr.ToString());
            mac.TransformFinalBlock(hashBytes, 0, hashBytes.Length);

            string signature = Convert.ToBase64String(mac.Hash);

            return signature;
        }

        public static void AddSignatureHeader(HttpWebRequest request, string secretKey, Dictionary<string, string> headers) 
        {
            string signature = GetSignature(request, secretKey, headers);

            request.Headers.Add(ObjectAtmosConstants.ATMOS_SIGNATURE, signature);
        }

        public static string GetReqeustDate()
        {
            string result = null;

            try
            {
                DateTime requestDate = DateTime.UtcNow;
                StringBuilder df = new StringBuilder();
                df.Append("DDD".ToLower(CultureInfo.InvariantCulture)).Append(", DD ".ToLower(CultureInfo.InvariantCulture)).Append("MMM ").Append("YYYY".ToLower(CultureInfo.InvariantCulture)).Append(" HH:mm:ss ");
                //"ddd, dd MMM yyyy HH:mm:ss "
                result = requestDate.ToString(df.ToString(), CultureInfo.InvariantCulture) + "GMT";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }

            return result;
        }
        #endregion
       
        #region Private Method
        /*
         * 1. Remove any white space before and after the colon and at the end of the metadata value. Multiple
              white spaces embedded within a metadata value are replaced by a single white space. For example:
              Before canonicalization: x-emc-meta: title=Mountain Dew
              After canonicalization: x-emc-meta:title=Mountain Dew
           2. Convert all header names to lowercase.
           3. Sort the headers alphabetically.
           4. For headers with values that span multiple lines, convert them into one line by replacing any
              newline characters and extra embedded white spaces in the value.
           5. Concatenate all headers together, using newlines (\n) separating each header from the next one.
              There should be no terminating newline character at the end of the last header.
         */
        private static string GetCanonicalizedEMCHeaders(Dictionary<string, string> headers)
        {
            /*
             * As we use SortedList, Rule 3 will be implemented automatically.
             */
            SortedList<string, string> emcHeaders = new SortedList<string, string>();

            string key = null;
            string val = null;
            foreach (KeyValuePair<string, string> hd in headers)
            {
                /**
                 * Rule 2
                 */
                key = hd.Key.ToLower(CultureInfo.InvariantCulture);

                if (key.Contains("x-emc"))
                {
                    /**
                     * Rule 4
                     */
                    val = hd.Value.Replace("\n", "");

                    /**
                     * Rule 1
                     */
                    if (key.Equals("x-emc-meta"))
                    {
                        val = val.Trim();
                        val = Regex.Replace(val, " {2,}", " ");
                    }
                    emcHeaders.Add(key, val);
                }
            }

            /**
             * Rule 5
             */
            bool first = true;
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> hd in emcHeaders)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append("\n");
                }

                builder.Append(hd.Key)
                       .Append(":")
                       .Append(hd.Value);
            }

            return builder.ToString();
        }

        private static string GetCanonicalizedResource(HttpWebRequest request)
        {
            Uri uri = request.RequestUri;
            return HttpUtility.UrlDecode(uri.AbsolutePath, new UTF8Encoding()).ToLower(CultureInfo.InvariantCulture);
        }

        private static string GetHeaderValue(Dictionary<string, string> headers, string name)
        {
            string value = "";

            if (headers.ContainsKey(name))
            {
                value = headers[name];
            }

            return value;
        }        
        #endregion
    }
}
