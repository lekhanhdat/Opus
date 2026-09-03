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

namespace AvePoint.Media.Storage.Egnyte
{
    #region
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Web.Script.Serialization;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Util;
    using System.Web;
    using System.Diagnostics.CodeAnalysis;
    #endregion

    #region CodeReview
    [AveCodeReview(
        "2013/10/16",
        "xiao.zhang@avepoint.com",
        "xiao.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 },
        "ADO-93945",
        true,
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }
        )]
    #endregion

    class EgnyteUtil
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(EgnyteUtil));

        public static EgnyteObject ParseJsonString(String jsonString)
        {
            EgnyteObject egnyteObject = new EgnyteObject();
            JavaScriptSerializer javaScript = new JavaScriptSerializer();
            egnyteObject = javaScript.Deserialize<EgnyteObject>(jsonString);
            return egnyteObject;
        }

        internal static String Encode(String url)
        {
            return HttpUtility.UrlEncode(url).Replace("+", "%20").Replace("%2f", "/").Replace("%5c", "/");
        }

        public static HttpWebRequest GenerateRequest(String Method, String url, String token)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = Method;
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));
            return request;
        }

        public delegate T RetryDelegate<T>();


       [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "msg")]      
        public static T Retry<T>(RetryDelegate<T> del, Int32 maxCount, Int32 retryInterval)
        {
            Int32 counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    return del.Invoke();
                }
                catch (WebException ex)
                {
                    if (counter > maxCount)
                    {
                        logger.Error("Too many retry failed. Retry count:{0}, msg:{1}", counter, ex.Message, ex);
                        throw;
                    }
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            if (response.Headers.ToString().Contains("ERR_403_DEVELOPER_OVER_RATE"))
                            {
                                throw new Exception("Exceeded daily quota.");
                            }
                            if (response.Headers.ToString().Contains("ERR_403_DEVELOPER_OVER_QPS"))
                            {
                                logger.Debug("Exceeded per second throttle.Retry after 1000ms.");
                                Thread.Sleep(1000);
                                continue;
                            }
                            else
                            {
                                Thread.Sleep(retryInterval);
                                continue;
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(ex.Message, ex);
                        }
                        else if (response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            logger.Info("This exception is a connection fail exception:" + ex.Message);
                            if (counter < maxCount)
                            {
                                logger.Info("Retry after " + retryInterval + " ms. Retry count: " + counter);
                                Thread.Sleep(retryInterval);
                                continue;
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else
                        {
                            String body = String.Empty;
                            using (Stream respStream = response.GetResponseStream())
                            {
                                using (StreamReader streamReader = new StreamReader(respStream))
                                {
                                    body = streamReader.ReadToEnd();
                                }
                            }
                            logger.Error("Execute request failed, msg:{0}, response body:{1}:", ex.Message, body, ex);
                            throw;
                        }
                    } 
                    else if (ex.Status == WebExceptionStatus.ConnectionClosed || ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                    {
                        logger.Info("This exception is a connection fail exception:" + ex.Message);
                        if (counter < maxCount)
                        {
                            logger.Info("Retry after " + retryInterval + " ms. Retry count: " + counter);
                            Thread.Sleep(retryInterval);
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        logger.Error("Execute request failed:" + ex.Message, ex);
                        throw;
                    }
                }
            }
        }
    }

    class EgnyteConstants
    {
        public static readonly String META_ID_HEADER = "__EgnyteMetaID__";
        public static readonly String HttpMethod_PUT = "PUT";
        public static readonly String HttpMethod_GET = "GET";
        public static readonly String HttpMethod_DELETE = "DELETE";
        public static readonly String HttpMethod_POST = "POST";
    }
}
