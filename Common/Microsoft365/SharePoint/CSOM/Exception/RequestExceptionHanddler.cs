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

namespace Microsoft365.SharePoint
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Xml;
    using Microsoft.SharePoint.Client;
    using Microsoft365.Common.Logger;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Microsoft365.Common.HttpUtil;

    public class RequestExceptionHanddler
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(RequestExceptionHanddler));
        private const string MESSAGE_SITELOCKED = "RM_RS_ScanSiteNoAccessError";
        // Set the maximum sleep time to 30 minutes 
        private const int MAX_RETRY_AFTER = 1800 * 1000; 

        /// <summary>
        /// milliseconds default is 3s
        /// </summary>
        public static int RetryInterval { get; set; } = 3000;

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
                if (exception?.InnerException != null)
                {
                    return IsForbiddenWebException(exception.InnerException);
                }
                return needRetry;
            }
            catch (Exception ex)
            {
                logger.Error("handle forbidden exception failed.due to {0}", ex.ToString());
                return false;
            }
        }

        public static bool IsResourceUsageException(Exception exception)
        {
            var needRetry = false;

            if (exception != null)
            {
                if (exception.GetType().FullName.Equals("System.Web.Services.Protocols.SoapException"))
                {
                    XmlNode detail;
                    try
                    {
                        detail = (XmlNode)exception.GetType().GetProperty("Detail", BindingFlags.Public)?.GetValue(exception);
                    }
                    catch
                    {
                        detail = null;
                    }
                    needRetry = detail != null && detail.OuterXml.Contains("0x80131904");
                }
                else if (exception.GetType().FullName.Equals("Microsoft.SharePoint.Client.ServerException", StringComparison.OrdinalIgnoreCase))
                {
                    needRetry = exception.Message != null && exception.Message.Contains("0x80131904");
                }
                else if (exception.GetType().FullName.Equals("Microsoft.SharePoint.Client.ClientRequestException", StringComparison.OrdinalIgnoreCase))
                {
                    needRetry = exception.Message != null && exception.Message.Contains("Unexpected response");
                }
            }

            return needRetry;
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
                logger.Error("server protocol ciolation error,error message:{0}", e.ToString());
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
            if (/** e.Status == System.Net.WebExceptionStatus.NameResolutionFailure
                || **/ e.Status == WebExceptionStatus.SecureChannelFailure
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
                    && (webResponse.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        webResponse.StatusCode == HttpStatusCode.InternalServerError ||
                        webResponse.StatusCode == HttpStatusCode.BadRequest
                    /*|| webResponse.StatusCode == HttpStatusCode.Forbidden*/))
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
            return false;
        }

        //server exception
        public static bool IsEndValueOutOfRangeException(Exception e)
        {
            //SAAS-28834 Ending value的异常没有确定问题原因，也没有errorcode等信息,且重现概率较低,暂时先通过message来判断
            if (string.Equals(e.GetType().FullName,"System.Runtime.Remoting.ServerException") && !string.IsNullOrEmpty(e.Message) && e.Message.Contains("Ending value cannot be less than starting value"))
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
                        interval = Math.Min(tempInterval, MAX_RETRY_AFTER);
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
        public static bool IsToomanyRequestError(Exception e)
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
                else if (response != null && ((int)response.StatusCode == 429 || (int)response.StatusCode == 503))
                {
                    return true;
                }
            }
            else if (e.InnerException != null)
            {
                return IsToomanyRequestError(e.InnerException);
            }
            return false;
        }

        public static bool IsTimedoutException(Exception exception, ref int retryInterval)
        {
            try
            {
                if (exception is WebException)
                {
                    retryInterval = RetryInterval;
                    WebException we = exception as WebException;
                    return we.Status == WebExceptionStatus.Timeout;
                }
                if (exception is System.Threading.Tasks.TaskCanceledException && exception.InnerException is System.TimeoutException)
                {
                    retryInterval = RetryInterval;
                    logger.Info("[System.TimeoutException], need add more time out time. ");
                    return true;
                }
                if (exception.GetType().ToString().Equals("Microsoft.SharePoint.Client.ServerException", StringComparison.OrdinalIgnoreCase))
                {
                    retryInterval = RetryInterval;
                    PropertyInfo prop = exception.GetType().GetProperty("ServerErrorCode", BindingFlags.Public | BindingFlags.Instance);
                    int serverErrorCode = Convert.ToInt32(prop.GetValue(exception, null));
                    logger.Info(string.Format("ServerException.ErrorCode:{0}, HResult:{1}", serverErrorCode, exception.HResult));
                    //Operation timed out. (Exception from HRESULT: 0x80131505)
                    /*The request channel timed out while waiting for a reply after 00:00:29.9843756.
                     * Increase the timeout value passed to the call to Request or increase the SendTimeout value on the Binding.
                     * The time allotted to this operation may have been a portion of a longer timeout*/
                    return exception.HResult == unchecked((int)0x80131505) || serverErrorCode == -2146233083;
                }
                if ((exception is TargetInvocationException) && exception.InnerException != null)
                {
                    return IsTimedoutException(exception.InnerException, ref retryInterval);
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("Error while identifying timed out exception:{0}", ex.ToString()));
                return false;
            }
        }

        public static bool IsUnauthorizedException(Exception e)
        {
            bool result = false;
            var webException = e as WebException;
            if (webException != null)
            {
                var response = webException.Response as HttpWebResponse;
                if (response != null)
                {
                    result = response.StatusCode == HttpStatusCode.Unauthorized;
                }
                else
                {
                    //System.Net.WebException: The remote server returned an error: (401) Unauthorized.
                    result = webException.Message.IndexOf("(401) Unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            return result;
        }

        public static bool IsProjectRetryException(Exception e)
        {
            if (e != null
                && (e.Message.IndexOf("LastError=AdminViewNotAccessibleToUser", StringComparison.OrdinalIgnoreCase) >= 0
                    || e.Message.IndexOf("LastError=GeneralSecurityAccessDenied", StringComparison.OrdinalIgnoreCase) >= 0
                    || e.Message.IndexOf("LastError=GeneralUnhandledException", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            return false;
        }


        public static bool IsNameResolutionFailureException(Exception e)
        {
            if ((e is WebException we && we.Status == WebExceptionStatus.NameResolutionFailure)
                || e.Message.Contains("The remote name could not be resolved"))
            {
                return true;
            }
            else if (e.InnerException != null)
            {
                return IsNameResolutionFailureException(e.InnerException);
            }
            return false;
        }


        #region Site Locked Exception
        public static bool IsSiteLockedException(Exception e, out string message)
        {
            var result = false;
            message = string.Empty;

            var webException = e as WebException;
            if (webException != null)
            {
                const string key = "x-ms-diagnostics";
                var response = webException.Response as HttpWebResponse;
                if (response != null && response.Headers.AllKeys.Contains(key))
                {
                    var diagnosticInfo = Convert.ToString(response.Headers[key]);
                    result = !string.IsNullOrEmpty(diagnosticInfo) && diagnosticInfo.IndexOf("reason=Access to this Web site has been blocked.", StringComparison.OrdinalIgnoreCase) != -1;
                }
            }

            if (result)
            {
                message = MESSAGE_SITELOCKED;
            }

            return result;
        }

        private static bool IsXml(string content)
        {
            if ((!content.StartsWith("<"))||(!content.EndsWith(">")))
            {
                return false;
            }
            return true;
        }

        public static bool Is0x81020071Exception(string webExceptionResponseContent, out string message)
        {
            message = string.Empty;
            var result = false;
            if (!string.IsNullOrEmpty(webExceptionResponseContent))
            {
                try
                {
                    if (!IsXml(webExceptionResponseContent))
                    {
                        return result;
                    }
                    #region response demo
                    //<?xml version="1.0" encoding="utf-8"?>
                    //<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                    // <soap:Body>
                    //  <soap:Fault>
                    //   <faultcode>soap:Server</faultcode>
                    //   <faultstring>Exception of type 'Microsoft.SharePoint.SoapServer.SoapServerException' was thrown.</faultstring>
                    //   <detail>
                    //    <errorstring xmlns="http://schemas.microsoft.com/sharepoint/soap/">Access to this Web site has been blocked.

                    //Please contact the administrator to resolve this problem.</errorstring>
                    //    <errorcode xmlns="http://schemas.microsoft.com/sharepoint/soap/">0x81020071</errorcode>
                    //   </detail>
                    //  </soap:Fault>
                    // </soap:Body>
                    //</soap:Envelope> 
                    #endregion
                    var xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.LoadXml(webExceptionResponseContent);
                    var nsmgr = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                    nsmgr.AddNamespace("sp", "http://schemas.microsoft.com/sharepoint/soap/");
                    var errorCodeNodes = xmlDoc.SelectNodes("/soap:Envelope/soap:Body/soap:Fault/detail/sp:errorcode", nsmgr);

                    result = errorCodeNodes != null && errorCodeNodes.Count == 1 && string.Equals(errorCodeNodes[0].InnerText, "0x81020071", StringComparison.OrdinalIgnoreCase);
                    if (result)
                    {
                        message = MESSAGE_SITELOCKED;
                        var errorStringNodes = errorCodeNodes[0].ParentNode.SelectNodes("//detail/sp:errorstring", nsmgr);
                        if (errorStringNodes != null && errorStringNodes.Count == 1)
                        {
                            var innerText = errorStringNodes[0].InnerText;
                            if (!string.IsNullOrEmpty(innerText))
                            {
                                message = innerText;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Failed format response content, error: {e}");
                }

            }
            return result;
        }
        #endregion

        public static bool IsSessionRevokedException(string webExceptionResponseContent)
        {
            if (!string.IsNullOrEmpty(webExceptionResponseContent))
            {
                try
                {
                    #region response demo
                    /*
                     * {
                     *     "error": {
                     *         "code": "unauthenticated",
                     *         "innerError": {
                     *             "code": "authSessionRevoked"
                     *         },
                     *         "message": "Session has been revoked."
                     *     }
                     * }
                     */
                    #endregion
                    if (webExceptionResponseContent.TrimStart().StartsWith("{") || webExceptionResponseContent.TrimStart().StartsWith("["))
                    {
                        Dictionary<string, object> json = JsonConvert.DeserializeObject<Dictionary<string, object>>(webExceptionResponseContent);
                        if (json.TryGetValue("error", out object error) && error is JObject errorObj)
                        {
                            if (string.Equals(errorObj["innerError"]?["code"]?.ToString(), "authSessionRevoked", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                            if (string.Equals(errorObj["message"]?.ToString(), "Session has been revoked.", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Failed to check whether session revoked exception, error: {e}");
                }

            }
            return false;
        }
        
        private static List<string> SpecialExceptionCodes = new List<string>() { "-2147217327", "-2147217328", "-2147217322", "-2147023080", "-2130209002", "-2130209003" };

        /// <summary>
        /// Check for special exception that no need to retry multiple times
        /// -2147217327 - virus scan exception
        /// -2147217322 - IRM exception
        /// -2130209002/3 - PDF conflict with IRM
        /// -2147023080 - This site has exceeded its maximum file storage limit.
        /// </summary>
        /// <param name="webExceptionResponseContent"></param>
        /// <returns></returns>
        public static bool IsNoNeedRetryExcecption(string webExceptionResponseContent, out string message)
        {
            var errorCode = GetWebExceptionResponseErrorCode(webExceptionResponseContent, out message);
            return !string.IsNullOrEmpty(errorCode) && SpecialExceptionCodes.Contains(errorCode);
        }

        public static string GetWebExceptionResponseErrorCode(string webExceptionResponseContent, out string message)
        {
            string errorCode = string.Empty;
            message = string.Empty;
            if (!string.IsNullOrEmpty(webExceptionResponseContent))
            {
                if (!IsXml(webExceptionResponseContent))
                {
                    return errorCode;
                }
                try
                {
                    #region response demo
                    /*
	                    <m:code>-2147217327, Microsoft.SharePoint.SPException</m:code><m:message xml:lang="en-US">The virus scanner discovered an issue while scanning the file. Please try opening the file directly from the browser, or contact your administrator. Additional information: 'Win32/NewDotNet'</m:message>
                        <m:code>-2147217322, Microsoft.SharePoint.SPException</m:code><m:message xml:lang="en-US">The document you tried to download could not be protected. You may need to contact the library administrator to help resolving. Error code is: 80070057.</m:message>
                        <m:code>-2147023080, Microsoft.SharePoint.SPException</m:code><m:message xml:lang="en-US">This site has exceeded its maximum file storage limit. To free up space, delete files you don't need and empty the recycle bin.</m:message>
                    */
                    #endregion
                    var xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.LoadXml(webExceptionResponseContent);
                    var nsmgr = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                    var errorCodeNodes = xmlDoc.SelectNodes("/m:error/m:code", nsmgr);
                    var errorMessage = xmlDoc.SelectNodes("/m:error/m:message", nsmgr);
                    var errorCodeStr = (errorCodeNodes != null && errorCodeNodes.Count == 1 && errorCodeNodes[0].InnerText != null) ? errorCodeNodes[0].InnerText : string.Empty;
                    errorCode = (!String.IsNullOrEmpty(errorCodeStr) && errorCodeStr.IndexOf(',') > 0) ? errorCodeStr.Substring(0, errorCodeStr.IndexOf(',')) : string.Empty;
                    message = (errorMessage != null && errorMessage.Count == 1 && errorMessage[0].InnerText != null) ? errorMessage[0].InnerText : string.Empty;
                }
                catch (Exception e)
                {
                    logger.Warn($"Failed format response content, error: {e}");
                }
            }
            return errorCode;
        }

        public static void LogException(Exception e)
        {
            if (e is WebException we)
            {
                var logContent = $"WebException: status: {we.Status}";
                if (we.Response is HttpWebResponse response)
                {
                    logContent = $"{logContent}, response header is:{response.Headers}, response status code is: {response.StatusCode}";
                }
                logger.Warn(logContent);
            }
            else if (e.InnerException != null)
            {
                LogException(e.InnerException);
            }
        }

        public static bool CheckIsFileNotFoundException(Exception ex)
        {
            if (ex is System.IO.FileNotFoundException
                || CheckIsFileNotFoundException(ex as ServerException)
                || CheckIsFileNotFoundException(ex as WebException))
            {
                return true;
            }

            return false;
        }

        #region check site exist

        public static bool CheckSiteDeletedByHttpRequest(string siteUrl)
        {
            try
            {
                return GetHttpStatusCode(siteUrl) == HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                logger.Warn($"Got site delete status from http request, site url: {siteUrl}, ex: {ex}");
            }
            return false;
        }

        private static HttpStatusCode GetHttpStatusCode(string siteUrl)
        {
            using (var httpClient = HttpClientFactory.CreateHttpClient("TestConnection"))
            {
                var response = httpClient.SendAsync(new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, siteUrl)).ConfigureAwait(false).GetAwaiter().GetResult();
                logger.Warn($"Got site delete status from http request, site url: {siteUrl}, Response: {response}");
                return response.StatusCode;
            }
        }

        #endregion

        #region check site forbidden
        public static bool CheckSiteForbiddenByHttpRequest(string siteUrl)
        {
            try
            {
                return GetHttpStatusCode(siteUrl) == HttpStatusCode.Forbidden;
            }
            catch (Exception e)
            {
                logger.Warn($"CheckSiteForbiddenByHttpRequest failed, site url: {siteUrl}, ex: {e}");
            }
            return false;
        }

        #endregion
    }
}