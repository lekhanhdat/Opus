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
using AvePoint.GCommon;
using Microsoft365.Authentication;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft365.SharePoint.Extension;

namespace AvePoint.ObjectModel.ClientOM
{
    class FileRPCProcessor
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(FileRPCProcessor));
        public static void AddFileByRPC(ClientContext context,ITokenProvider tokenProvider,string parentWebFullUrl,string parentWebServerRelativeUrl,string fileServerRelativeUrl, Stream bodyStream, bool isOverwrite)
        {
            string url = parentWebFullUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
            string fileListRelativeUrl = "";
            if (fileServerRelativeUrl.StartsWith(parentWebServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileListRelativeUrl = fileServerRelativeUrl.Substring(parentWebServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            else
            {
                fileListRelativeUrl = fileServerRelativeUrl;
            }
            string order = "method=put+document%3a"
                + "&service%5fname=" + System.Web.HttpUtility.UrlEncode(parentWebServerRelativeUrl)
                + "&document=%5bdocument%5fname%3d" + System.Web.HttpUtility.UrlEncode(fileListRelativeUrl) + "%3bmeta%5finfo%3d%5bvti%5fmodifiedby%3bSW%7cSHAREPOINT%5c%5csystem%3bvti%5fauthor%3bSW%7cSHAREPOINT%5c%5csystem%5d%5d"
                + "&put%5foption=edit" + (isOverwrite ? ",overwrite" : "") + "&comment=" + "&keep%5fchecked%5fout=false\n";
            var streamHeader = Encoding.UTF8.GetBytes(order);
            using (WebResponse response = StartWebRequest(context, tokenProvider, url, streamHeader, bodyStream))
            {
                string responseString = GetResponseString(response.GetResponseStream());
                CheckForInternalErrorMessage(responseString);
                CheckForSuccessMessage(responseString);
            }

        }
        private static WebResponse StartWebRequest(ClientContext context, ITokenProvider tokenProvider, string url, byte[] streamHeader, Stream content)
        {
            ReliableHttpWebRequest request = ReliableHttpWebRequest.CreateRequest(url);
            //GetFormDigest();
            request.RefreshDigestInfo(context.Url, tokenProvider);
            request.Timeout = 600000;
            request.ReadWriteTimeout = 1800000;
            request.Method = "POST";
            request.Headers["MINME_Version"] = "1.0";
            request.UserAgent = "MSFrontPage/15.0";
            request.Accept = "auth/sicily";
            request.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "T";
            request.PreAuthenticate = true;
            request.Headers["Accept-encoding"] = "gzip, deflate";
            request.ContentLength = streamHeader.Length + content.Length;
            request.AllowWriteStreamBuffering = false;
            request.SetTokenProvider(url, tokenProvider, false);
            request.ContentType = "application/x-vermeer-urlencoded";
            request.Headers.Add("X-Vermeer-Content-Type", "application/x-vermeer-urlencoded");
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(streamHeader, 0, streamHeader.Length);
                content.CopyTo(reqStream);
                reqStream.Flush();
            }
            return request.GetResponse();
        }
        private static void CheckForSuccessMessage(string response)
        {
            string message = GetReturnValue(response, "message");
            if (null == message || !message.StartsWith("successfully"))
            {
                throw new WebException("Failed to perform operation. Message:" + message);
            }
            mLogger.Info("Finished upload a large file by RPC");
        }
        private static void CheckForInternalErrorMessage(string response)
        {
            string message = DecodeString(GetReturnValue(response, "msg"));
            if (!string.IsNullOrEmpty(message))
            {
                throw new WebException(message);
            }
        }
        private static string DecodeString(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex("&#([0-9]{1,3});&#([0-9]{1,3});");
                foreach (System.Text.RegularExpressions.Match match in rg.Matches(source))
                {
                    byte[] bytes = new[] { byte.Parse(match.Groups[1].Value), byte.Parse(match.Groups[2].Value) };
                    source = source.Replace(match.Value, Encoding.UTF8.GetString(bytes));
                }
                source = System.Web.HttpUtility.HtmlDecode(source);
            }
            return source;
        }
        private static string GetReturnValue(string response, string key)
        {
            key = key.TrimEnd('=') + "=";
            int startPos = response.IndexOf(key);
            if (-1 == startPos)
            {
                return
                null;
            }
            startPos += key.Length;
            int endPos = response.IndexOf("\n", startPos);
            return response.Substring(startPos, endPos - startPos);
        }
        private static string GetResponseString(Stream responseStream)
        {
            StreamReader sr = new StreamReader(responseStream, Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }
}
