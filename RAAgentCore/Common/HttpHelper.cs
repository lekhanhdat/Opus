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
using AvePoint.RA.CommonUtil;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AvePoint.RA.FileSystem.Core
{
    public class HttpHelper
    {
        private static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region properties
        private readonly Encoding ENCODING = Encoding.UTF8;
        #endregion

        #region constructor
        public HttpHelper()
        {
            //token
        }
        #endregion

        #region public methods

        /// <summary>
        /// Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string HTTPJsonPost(string url, string msg)
        {
            string result = string.Empty;
            int retryCount = 0;
            while (true)
            {
                try
                {
                    result = CommonHttpRequest(msg, url, "POST");
                    break;
                }
                catch (WebException we)
                {
                    if (we.Response != null)
                    {
                        HttpWebResponse response = (HttpWebResponse)we.Response;
                        logger.Info($"Error code: { response.StatusCode} Retry count: {retryCount}");
                        retryCount++;
                        if (retryCount >= 3)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        logger.Error("respose is null.");
                        throw;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Error occurred while sending post request. Error: " + e.ToString());
                    throw;
                }
            }
            return result;
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string HTTPJsonGet(string url)
        {
            string result = string.Empty;
            int retryCount = 0;
            while (true)
            {
                try
                {
                    HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                    request.ContentType = "application/json";
                    request.Method = "GET";
                    HttpWebResponse resp = request.GetResponse() as HttpWebResponse;
                    System.IO.StreamReader reader = new System.IO.StreamReader(resp.GetResponseStream(), this.ENCODING);
                    result = reader.ReadToEnd();
                    break;
                }
                catch (WebException we)
                {
                    if (we.Response != null)
                    {
                        HttpWebResponse response = (HttpWebResponse)we.Response;
                        logger.Info($"Error code: { response.StatusCode} Retry count: {retryCount}");
                        retryCount++;
                        if (retryCount >= 3)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        logger.Error("respose is null.");
                        throw;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Error occurred while sending get request. Error: " + e.ToString());
                    throw;
                }
            }
            return result;
        }

        /// <summary>
        /// Put
        /// </summary>
        /// <param name="data"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string HTTPJsonDelete(string url, string data)
        {
            return CommonHttpRequest(data, url, "DELETE");
        }

        /// <summary>
        /// Put
        /// </summary>
        /// <param name="data"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string HTTPJsonPut(string url, string data)
        {
            return CommonHttpRequest(data, url, "PUT");
        }


        #endregion



        #region private

        public string CommonHttpRequest(string data, string uri, string type)
        {
            string serviceUrl = uri;
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(serviceUrl);          
            myRequest.Method = type;
            myRequest.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            myRequest.Headers.Add("Authorization", "Token " + "");
            myRequest.ContentType = "application/json";
            myRequest.Accept = "application/json";
            //myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.Timeout = 500000;
            myRequest.ReadWriteTimeout = 500000;
            //string sRealIp = GetHostAddress();
            //if (!string.IsNullOrEmpty(sRealIp))
            //{
            //    myRequest.Headers.Add("ClientIp", sRealIp);
            //}
            if (data != null)
            {
                byte[] buf = this.ENCODING.GetBytes(data);
                myRequest.ContentLength = buf.LongLength;
                using (Stream reqstream = myRequest.GetRequestStream())
                {
                    reqstream.Write(buf, 0, (int)buf.Length);
                }
            }
            HttpWebResponse resp = myRequest.GetResponse() as HttpWebResponse;
            System.IO.StreamReader reader = new System.IO.StreamReader(resp.GetResponseStream(), this.ENCODING);
            string ReturnXml = reader.ReadToEnd();
            reader.Close();
            resp.Close();
            return ReturnXml;
        }
        #endregion


        //public static string GetHostAddress()
        //{
        //    try
        //    {
        //        string userHostAddress = HttpContext.Current.Request.UserHostAddress;

        //        if (string.IsNullOrEmpty(userHostAddress))
        //        {
        //            userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        //        }


        //        if (!string.IsNullOrEmpty(userHostAddress) && IsIP(userHostAddress))
        //        {
        //            return userHostAddress;
        //        }
        //        return "127.0.0.1";
        //    }
        //    catch
        //    {
        //        return "127.0.0.1";
        //    }

        //}

        public static bool IsIP(string ip)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        public static long ConvertDataTimeLong(DateTime dt)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan toNow = dt.Subtract(dtStart);
            long timeStamp = toNow.Ticks;
            timeStamp = long.Parse(timeStamp.ToString().Substring(0, timeStamp.ToString().Length - 4));
            return timeStamp;
        }

        public static DateTime ConvertLongDateTime(long d)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(d + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }

        private string ConvertToJsonString<T>(T model)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, model);

            byte[] dataBytes = new byte[stream.Length];

            stream.Position = 0;

            stream.Read(dataBytes, 0, (int)stream.Length);

            string dataString = Encoding.UTF8.GetString(dataBytes);
            return dataString;
        }
    }

    public static class WebClientHelper
    {
        public static string Post(string url, string jsonData)
        {
            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            client.Encoding = System.Text.Encoding.UTF8;
            byte[] data = Encoding.UTF8.GetBytes(jsonData);
            byte[] responseData = client.UploadData(new Uri(url), "POST", data);
            string response = Encoding.UTF8.GetString(responseData);
            return response;
        }

        public static void PostAsync(string url, string jsonData, Action<string> onComplete, Action<Exception> onError)
        {
            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            client.Encoding = System.Text.Encoding.UTF8;
            byte[] data = Encoding.UTF8.GetBytes(jsonData);

            client.UploadDataCompleted += (s, e) =>
            {
                if (e.Error == null && e.Result != null)
                {
                    string response = Encoding.UTF8.GetString(e.Result);
                    onComplete(response);
                }
                else
                {
                    onError(e.Error);
                }
            };

            client.UploadDataAsync(new Uri(url), "POST", data);
        }
    }
}
