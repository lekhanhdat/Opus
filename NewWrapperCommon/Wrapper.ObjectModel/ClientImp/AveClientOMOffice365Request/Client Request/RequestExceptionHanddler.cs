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
namespace AvePoint.ObjectModel.O365
{
    using System;
    using System.Net;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using AvePoint.GCommon;
    using System.Reflection;

    public class RequestExceptionHanddler
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(RequestExceptionHanddler));

        public static bool IsForbiddenWebException(Exception exception)
        {
            var needRetry = false;
            try
            {
                if (exception != null && exception is WebException)
                {
                    var response = (exception as WebException).Response as HttpWebResponse;
                    if (response != null && (int)response.StatusCode == 403)
                    {
                        needRetry = true;
                    }
                }
                if (exception.InnerException != null)
                {
                    return IsForbiddenWebException(exception.InnerException);
                }
                return needRetry;
            }
            catch (Exception ex)
            {
                mLogger.Error("handle forbidden exception failed.due to {0}", ex.ToString());
                return false;
            }
        }

        public static bool IsUnauthorizedWebException(Exception exception)
        {
            var needRetry = false;
            try
            {
                if (exception != null && exception is WebException)
                {
                    var response = (exception as WebException).Response as HttpWebResponse;
                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        needRetry = true;
                    }
                }
                if (exception.InnerException != null)
                {
                    return IsUnauthorizedWebException(exception.InnerException);
                }
                return needRetry;
            }
            catch (Exception ex)
            {
                mLogger.Error("handle Unauthorized exception failed.due to {0}", ex.ToString());
                return false;
            }
        }
        

        public static bool IsServerException(Exception e)
        {
            return e != null
                && e.GetType().FullName.Equals("Microsoft.SharePoint.Client.ServerException", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsClientRequestException(Exception e)
        {
            return e != null
                && e.GetType().FullName.Equals("Microsoft.SharePoint.Client.ClientRequestException", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUnexpectedResponseException(Exception e)
        {
            if (IsClientRequestException(e))
            {
                return e.Message != null && e.Message.Contains("Unexpected response");
            }
            return false;
        }

        public static bool IsRequestChannelTimeoutException(Exception e)
        {
            if (IsServerException(e))
            {
                return e.Message != null && e.Message.Contains("The request channel timed out attempting to send after");
            }
            else if (e.InnerException != null)
            {
                return IsRequestChannelTimeoutException(e.InnerException);
            }
            return false;
        }

        //we assume socketexception or ioexception caused by connection forcilby closed
        public static bool IsConnectonForciblyClosedExceptioin(Exception te)
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

        public static bool IsServerProtocolViolationError(Exception e, ref int retryInterval)
        {
            if ((e is WebException) && (e as WebException).Status == WebExceptionStatus.ServerProtocolViolation)
            {
                mLogger.Error("server protocol ciolation error,error message:{0}", e.ToString());
                return true;
            }
            return false;
        }

        /// <summary>
        /// net work error
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsUnstableNetworkException(WebException e)
        {
            if (e == null)
            {
                return false;
            }
            ///If the name resolution failure, no need to retry.
            if (/*e.Status == WebExceptionStatus.NameResolutionFailure
                || */e.Status == WebExceptionStatus.SecureChannelFailure
                || e.Status == WebExceptionStatus.ConnectFailure
                || e.Status == WebExceptionStatus.KeepAliveFailure
                || e.Status == WebExceptionStatus.ConnectionClosed
                || e.Status == WebExceptionStatus.PipelineFailure
                || e.Status == WebExceptionStatus.SendFailure
                || e.Status == WebExceptionStatus.UnknownError
                || e.Status == WebExceptionStatus.Pending
                || e.Status == WebExceptionStatus.Timeout)
            {
                return true;
            }
            if (e.Response != null)
            {
                HttpWebResponse webResponse = e.Response as HttpWebResponse;
                if (webResponse != null
                    && (webResponse.StatusCode == HttpStatusCode.ServiceUnavailable
                        || webResponse.StatusCode == HttpStatusCode.InternalServerError
                        /*|| webResponse.StatusCode == HttpStatusCode.Forbidden */))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// don't know detail about this method, copy from old code
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool Is0x80131904Exception(Exception e)
        {
            if (e != null && !string.IsNullOrEmpty(e.Message) && e.Message.Contains("0x80131904"))
            {
                return true;
            }
            else if (e.InnerException != null)
            {
                return Is0x80131904Exception(e.InnerException);
            }
            return false;
        }

        //server exception
        public static bool IsEndValueOutOfRangeException(Exception e)
        {
            //SAAS-28834 Ending value的异常没有确定问题原因，也没有errorcode等信息,且重现概率较低,暂时先通过message来判断
            if (e is System.Runtime.Remoting.ServerException && !string.IsNullOrEmpty(e.Message) && e.Message.Contains("Ending value cannot be less than starting value"))
            {
                return true;
            }
            else if (e.InnerException != null)
            {
                IsEndValueOutOfRangeException(e.InnerException);
            }
            return false;
        }

        public static bool IsRetryableWebException(Exception e, ref int interval)
        {
            if (e is WebException)
            {
                HttpWebResponse response = (e as WebException).Response as HttpWebResponse;
                if (response != null && response.Headers != null && response.Headers.AllKeys.Contains("Retry-After"))
                {
                    var tempInterval = Convert.ToInt32(response.Headers["Retry-After"]) * 1000;
                    if (tempInterval > interval)
                    {
                        interval = tempInterval;
                    }
                    return true;
                }
            }
            else if (e.InnerException != null)
            {
                return IsRetryableWebException(e.InnerException, ref interval);
            }
            return false;
        }

        /// <summary>
        /// HTTP 429 ERROR, Too Many Request.
        /// Check is request failed due to server unavailable - http status code 503
        /// Response has header Retry-After should wait and retry
        /// </summary>
        /// <param name="e"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static bool IsToomanyRequestError(Exception e,ref int retryInterval)
        {
            if (e is WebException)
            {
                var webException = e as WebException;
                HttpWebResponse response = webException.Response as HttpWebResponse;
                #region comment
                //部分环境中不能返回完整的status code，例如:
                // / Info 338 0 R
                // / Root 340 0 R
                // / Prev 116
                // / ID[< e551056254896c20f0e895cad721a10b >< 615483acb503db07c4459955aac44a81 >]
                // >>
                //startxref
                //246851
                //%% EOF
                //HTTP / 1.1 429: 
                //Content - Type: text / plain; charset = utf - 8
                //Retry - After: 120
                //Server: Microsoft - IIS / 8.5
                //SPRequestGuid: 4905dd9d - 50cf - 3000 - bc3a - 55a8e05f5676
                //request - id: 4905dd9d - 50cf - 3000 - bc3a - 55a8e05f5676
                //Strict - Transport - Security: max - age = 31536000
                //X - FRAME - OPTIONS: SAMEORIGIN
                //    SPRequestDuration: 38
                //SPIisLatency: 2
                //X - Powered - By: ASP.NET
                //    MicrosoftSharePointTeamServices: 16.0.0.6223
                //X - Content - Type - Options: nosniff
                //X - MS - InvokeApp: 1; RequireReadOnly
                //    P3P: CP = "ALL IND DSP COR ADM CONo CUR CUSo IVAo IVDo PSA PSD TAI TELo OUR SAMo CNT COM INT NAV ONL PHY PRE PUR UNI"
                //Date: Sat, 11 Mar 2017 6:16:01 GMT
                //Content - Length: 21
                #endregion
                if (response == null)
                {
                    if (webException.Message != null && webException.Message.Contains("The remote server returned an error: (429)"))
                    {
                        return true;
                    }
                }
                else if (response != null)
                {
                    switch ((int)response.StatusCode)
                    {
                        case 429:
                        case 503:
                            //according to original logic, sleep 2min
                            retryInterval = 120 * 1000;
                            return true;
                    }                  
                }
            }
            else if (e.InnerException != null)
            {
                return IsToomanyRequestError(e.InnerException,ref retryInterval);
            }
            return false;
        }
        
        public static bool IsMetadataServiceServerException(Exception exception)
        {
            const string mmsTimeout = "The request channel timed out while waiting for a reply after";
            const string mmsUnavaliable1 = "There was no endpoint listening at";
            const string mmsUnavaliable2 = "MetadataWebService.svc";
            try
            {
                if (exception.GetType().ToString().Equals("Microsoft.SharePoint.Client.ServerException", StringComparison.OrdinalIgnoreCase))
                {
                    if (exception.Message.IndexOf(mmsTimeout,StringComparison.OrdinalIgnoreCase)>=0
                        || (exception.Message.IndexOf(mmsUnavaliable1, StringComparison.OrdinalIgnoreCase) >= 0 
                                && exception.Message.IndexOf(mmsUnavaliable2, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return true;
                    }
                }
                if ((exception is TargetInvocationException) && exception.InnerException != null)
                {
                    return IsMetadataServiceServerException(exception.InnerException);
                }
                return false;
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Error while identifying timed out exception:{0}", ex.ToString()));
                return false;
            }
        }
    }

}
