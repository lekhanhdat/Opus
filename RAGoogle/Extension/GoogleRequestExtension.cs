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

namespace RAGoogle.Extension
{
    using AvePoint.RA.CommonUtil;
    using Google;
    using Google.Apis.Auth.OAuth2.Responses;
    using Google.Apis.Requests;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    public static class GoogleRequestExtension
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const int MaxRetryCount = 10;
        public const int LimitExceedRetryTime = 60;
        //mm
        public static readonly int MaxThrottlingRetryTime = 15 * 60 * 1000;
        private static int NetworkExceptionRetryTime = 120 * 1000;

        public static readonly object _lockObject = new object();
        private static int quotaCount = 0;//api quota occurred count

        public static void SetNetworkIssueRetryTime(int milliseconds)
        {
            NetworkExceptionRetryTime = milliseconds;
            logger.Info("Set sleep time for network issue {0}.", NetworkExceptionRetryTime);
        }

        public static async Task ExecuteWithRetryAsync(Func<Task> func)
        {
            int retryCount = 0;
            while (retryCount < MaxRetryCount)
            {
                try
                {
                    retryCount++;
                    await func();
                    return;
                }
                catch (GoogleApiException gex)
                {
                    if (retryCount == MaxRetryCount)
                    {
                        throw;
                    }

                    if (NeedRetry(gex, retryCount))
                    {
                        logger.Warn(
                            $"Retry count:{retryCount}.Execute request exception:{gex}.");
                        continue;
                    }

                    throw;
                }
                catch (TokenResponseException te)//Google.Apis.Auth.OAuth2.Responses.TokenResponseException: Error:"internal_failure", Description:"", Uri:""
                {
                    if (retryCount == MaxRetryCount)
                    {
                        throw;
                    }
                    if (te.Message.Contains("internal_failure"))
                    {
                        var throttlingTime = GetThrottlingTime(retryCount);
                        logger.Warn($"Google request TokenResponse exception. retry count {retryCount}, Plan to Sleep {throttlingTime}, error:{te}");
                        Thread.Sleep(throttlingTime);
                        continue;
                    }
                    if (te.Message.Contains("unauthorized_client"))
                    {
                        var throttlingTime = GetThrottlingTime(retryCount);
                        logger.Warn($"Unauthorized Client. retry count {retryCount}, Plan to Sleep {throttlingTime}, error:{te}");
                        Thread.Sleep(throttlingTime);
                        continue;
                    }
                    logger.Error($"retry count {retryCount}.ExecuteEx request failed: {te}");
                    throw;
                }
                catch (Exception e)
                {
                    if (retryCount == MaxRetryCount)
                    {
                        throw;
                    }
                    if (IsNetworkException(e))
                    {
                        logger.Warn(
                            $"Network error execute request, retry count:{retryCount}. Detail: {e}.");
                        Thread.Sleep(1 * 60 * 1000);
                        continue;
                    }
                    if (e.InnerException != null)
                    {
                        var ex = e.InnerException as WebException;
                        if (ex is { Status: WebExceptionStatus.ProtocolError })
                        {
                            logger.Warn(
                                $"Retry count:{retryCount}.Request exception:{ex}, error code:{(int)ex.Status}.");
                            Thread.Sleep(15 * 1000);
                            continue;
                        }

                        if (ex != null && (int)ex.Status == 429)
                        {
                            logger.Warn(
                                $"Retry count:{retryCount}.Request exception:{ex}, error code:{(int)ex.Status}.");
                            Thread.Sleep(GoogleRequestExtension.GetThrottlingTime(retryCount));
                            continue;
                        }
                    }
                    if (ShouldRetryAsFallback(e, retryCount))
                        continue;
                    throw;
                }
            }
            throw new CommonException($"ExecuteEx {nameof(func)} failed after retry.");
        }

        private static bool ShouldRetryAsFallback(Exception e, int retryCount)
        {
            if (e is HttpRequestException httpEx &&
                (httpEx.Message.Contains("429") || httpEx.Message.Contains("413")))
            {
                logger.Warn(
                    $"Retry count:{retryCount}. Http fallback retry:{httpEx}");
                Thread.Sleep(GoogleRequestExtension.GetThrottlingTime(retryCount));
                return true;
            }

            if (IsCanceledOrPartial(e))
            {
                logger.Warn(
                    $"Retry count:{retryCount}. Partial/canceled fallback:{e}");
                Thread.Sleep(60 * 1000);
                return true;
            }

            return false;
        }

        private static bool IsCanceledOrPartial(Exception e)
        {
            return e.Message.Contains("A task was canceled") ||
                   e.Message.Contains("Transferred a partial file");
        }

        public static async Task<TResponse> ExecuteExAsync<TResponse>(this ClientServiceRequest<TResponse> request, bool throwExceptionForUnauthorized = false)
        {
            int retryCount = 0;
            while (retryCount < MaxRetryCount)
            {
                try
                {
                    retryCount++;
                    var response = await request.ExecuteAsync();
                    return response;
                }
                catch (GoogleApiException gex)
                {
                    if (retryCount == MaxRetryCount)
                    {
                        throw;
                    }
                    if (gex.Error != null)
                    {
                        logger.Warn($"Google request [{request.MethodName}] execute exception. retry count {retryCount}, error:{gex.Error.Message}");
                    }
                    if (NeedRetry(gex, retryCount))
                    {
                        continue;
                    }
                    logger.Error($"retry count {retryCount}.ExecuteEx request [{request.MethodName}] failed: {gex}");
                    throw;
                }
                catch (TokenResponseException te)//Google.Apis.Auth.OAuth2.Responses.TokenResponseException: Error:"internal_failure", Description:"", Uri:""
                {
                    if (retryCount == MaxRetryCount)
                    {
                        throw;
                    }
                    if (te.Message.Contains("internal_failure"))
                    {
                        var throttlingTime = GetThrottlingTime(retryCount);
                        logger.Warn($"Google request TokenResponse exception. retry count {retryCount}, Plan to Sleep {throttlingTime}, error:{te}");
                        Thread.Sleep(throttlingTime);
                        continue;
                    }
                    if (te.Message.Contains("unauthorized_client") && !throwExceptionForUnauthorized)
                    {
                        var throttlingTime = GetThrottlingTime(retryCount);
                        logger.Warn($"Unauthorized Client. retry count {retryCount}, Plan to Sleep {throttlingTime}, error:{te}");
                        Thread.Sleep(throttlingTime);
                        continue;
                    }
                    logger.Error($"retry count {retryCount}.ExecuteEx request failed: {te}");
                    throw;
                }
                catch (Exception e)
                {
                    if (retryCount == MaxRetryCount)
                    {
                        throw;
                    }
                    else if (ExceptionNeedRetry(e))
                    {
                        continue;
                    }
                    logger.Error($"retry count {retryCount}.ExecuteEx request [{request.MethodName}] failed: {e}");
                    throw;
                }
            }
            throw new CommonException($"ExecuteEx {nameof(request)} failed after retry.");
        }

        public static bool NeedRetry(GoogleApiException ex, int retryCount)
        {
            try
            {
                if (ex != null && ex.Error != null)
                {
                    if (ex.Error.Code == 403)
                    {
                        if (ex.Error.Errors != null)
                        {
                            foreach (var singleErrorReason in ex.Error.Errors.Select(x => x.Reason))
                            {
                                if (GoogleApiSingleErrorReason.LimitExceeded.Contains(singleErrorReason))
                                {
                                    var throttlingTime = GetThrottlingTime(retryCount);
                                    logger.Warn($"Rate limit exceeded.Retry count:{retryCount}.Sleep {throttlingTime}.Detail:{ex}.");
                                    SumQuotaCount();
                                    Thread.Sleep(throttlingTime);
                                    return true;
                                }
                            }
                        }
                    }

                    else if (ex.Error.Code == 429)
                    {
                        var throttlingTime = Get429ThrottlingTime(ex);
                        if (throttlingTime == 0)
                        {
                            throttlingTime = GetThrottlingTime(retryCount);
                        }
                        logger.Warn($"429 Too Many Request. Rate limit exceeded.Retry count:{retryCount}.Sleep {throttlingTime}.Detail:{ex}.");
                        SumQuotaCount();
                        Thread.Sleep(throttlingTime);
                        return true;
                    }
                    else if (ex.Error.Code == 500)
                    {
                        logger.Warn($"Server error occurred.Retry count:{retryCount}. sleep 60s.Detail:{ex}.");
                        Thread.Sleep(1 * 60 * 1000);
                        return true;
                    }
                    else if (ex.Error.Code == 503)
                    {
                        logger.Warn($"Service Unavailable.Retry count:{retryCount}. sleep 60s.Detail:{ex}.");
                        Thread.Sleep(1 * 60 * 1000);
                        return true;
                    }
                    else if (ex.Message.Contains("A task was canceled") || ex.Message.Contains("Transferred a partial file"))
                    {
                        logger.Warn($"Api inner error.Retry count:{retryCount}. sleep 60s.Detail: {ex}.");
                        Thread.Sleep(1 * 60 * 1000);
                        return true;
                    }
                    else if (ex.Message.Contains("Precondition check failed"))
                    {
                        var throttlingTime = GetThrottlingTime(retryCount);
                        logger.Warn($"400 Precondition check failed.Retry count:{retryCount}. sleep {throttlingTime}.Detail: {ex}.");
                        Thread.Sleep(throttlingTime);
                        return true;
                    }
                    else if (ex.Message.Contains("unauthorized_client"))
                    {
                        var throttlingTime = GetThrottlingTime(retryCount);
                        logger.Warn($"Unauthorized client.Retry count:{retryCount}. sleep {throttlingTime}.Detail: {ex}.");
                        Thread.Sleep(throttlingTime);
                        return true;
                    }
                    else if (ex.Error.Code == 502)
                    {
                        var throttlingTime = GetThrottlingTime(retryCount);
                        logger.Warn($"502 Network error occurred before reaching the server.Retry count:{retryCount}. sleep {throttlingTime}.Detail:{ex}.");
                        Thread.Sleep(throttlingTime);
                        return true;
                    }
                }
                foreach (var htmlMessage in GoogleApiSingleErrorReason.HtmlMessages)
                {
                    if (ex != null && ex.Message.Contains(htmlMessage))
                    {
                        logger.Warn($"Find specified html error. need retry.sleep 60s.Retry count:{retryCount}");
                        Thread.Sleep(1 * 60 * 1000);
                        return true;
                    }
                }
            }
            catch (Exception retryException)
            {
                logger.Error($"Error when check retry.Retry count:{retryCount} Detail:{retryException.Message}.");
            }
            return false;
        }

        public static bool IsGoogleDocTooLarge(Exception exception)
        {
            if (exception is GoogleApiException ex)
            {
                if (ex.Error != null && ex.Error.Code.Equals(403) && ex.Error.Errors != null)
                {
                    foreach (var singleError in ex.Error.Errors)
                    {
                        if (GoogleApiSingleErrorReason.GoogleDocExceedSize.Equals(singleError.Reason))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }



        public static int GetErrorCode(Exception exception)
        {
            if (exception is GoogleApiException googleEx)
            {
                if (googleEx != null && googleEx.Error != null)
                {
                    return googleEx.Error.Code;
                }
            }
            return -1;
        }

        public static bool IsGoogleApiException(Exception exception)
        {
            if (exception is GoogleApiException)
            {
                return true;
            }
            return false;
        }

        //calculate throttling time between min & max value
        public static int GetThrottlingTime(int retryTime)
        {
            if (retryTime > 0)
            {
                var tempThrottlingRetryTime = retryTime * retryTime * LimitExceedRetryTime * 1000;
                if (tempThrottlingRetryTime > MaxThrottlingRetryTime)
                {
                    return MaxThrottlingRetryTime + RandomNumberGenerator.GetInt32(0, 1000);
                }
                else
                {
                    return tempThrottlingRetryTime + RandomNumberGenerator.GetInt32(0, 1000);
                }
            }
            return LimitExceedRetryTime * 1000 + RandomNumberGenerator.GetInt32(0, 1000);
        }

        private static int Get429ThrottlingTime(GoogleApiException e429)
        {
            int throttlingTime = 0;
            try
            {
                // User-rate limit exceeded.  Retry after 2024-07-25T01:18:53.022Z
                var message = e429.Error.Errors[0].Message;
                var suggestTimeString = message.Split(' ').Last();
                var suggestTime = DateTime.Parse(suggestTimeString);
                throttlingTime = (int)(suggestTime - DateTime.Now).TotalMilliseconds + RandomNumberGenerator.GetInt32(1, 60) * 1000;
            }
            catch (Exception e)
            {
                logger.Error($"Get 429 exception throttling time error: {e}");
            }
            return throttlingTime;
        }

        public static ErrorCode GetErrorCodeForException(Exception ex)
        {
            try
            {
                if (ex is GoogleApiException gException)
                {
                    return GetGoogleApiExceptionErrorCode(gException);
                }
                else if (ex.InnerException != null && ex.InnerException is GoogleApiException gInnerException)
                {
                    return GetGoogleApiExceptionErrorCode(gInnerException);
                }
                else if (ex is MediaException || ex.InnerException != null && ex.InnerException is MediaException)
                {
                    if (ex.Message.Contains("Access Denied", StringComparison.OrdinalIgnoreCase) || ex.InnerException != null && ex.InnerException.Message.Contains("Access Denied", StringComparison.OrdinalIgnoreCase))
                    {
                        return ErrorCodeFactory.GSuiteMediaPermission;
                    }
                    else
                    {
                        return ErrorCodeFactory.GSuiteMediaCommon;
                    }
                }
                else if (ex.Message.Contains("The specified blob does not exist") || ex.InnerException != null && ex.InnerException.Message.Contains("The specified blob does not exist"))
                {
                    return ErrorCodeFactory.GSuiteIndexDBDownload;
                }
                else if (ex.Message.Contains("No such object") || ex.InnerException != null && ex.InnerException.Message.Contains("No such object"))
                {
                    return ErrorCodeFactory.GSuiteIndexDBDownload;
                }
                else if (ex.Message.Contains("HttpStatusCode is NotFound") || ex.InnerException != null && ex.InnerException.Message.Contains("HttpStatusCode is NotFound"))
                {
                    return ErrorCodeFactory.GSuiteIndexDBDownload;
                }
                else if (ex.Message.Contains("The specified key does not exist") || ex.InnerException != null && ex.InnerException.Message.Contains("The specified key does not exist"))
                {
                    return ErrorCodeFactory.GSuiteIndexDBDownload;
                }
                else if (ex.Message.ToLower().Contains("object file") && ex.Message.ToLower().Contains("not found") || ex.InnerException != null && ex.InnerException.Message.ToLower().Contains("object file") && ex.InnerException.Message.ToLower().Contains("not found"))
                {
                    return ErrorCodeFactory.GSuiteIndexDBDownload;
                }
                else if (ex.Message.Contains("Unable to read data from the transport connection") || ex.InnerException != null && ex.InnerException.Message.Contains("Unable to read data from the transport connection"))
                {
                    return ErrorCodeFactory.DownloadFileError;
                }
                else if (ex is FormatException || ex.InnerException != null && ex.InnerException is FormatException)
                {
                    if (ex.Message.Contains("No valid combination of account information found.") || ex.InnerException != null && ex.InnerException.Message.Contains("No valid combination of account information found."))
                    {
                        return ErrorCodeFactory.InvalidAccountInfo;
                    }
                }
                else if (ex.Message.Contains("File ResponseBody is null") || ex.InnerException != null && ex.InnerException.Message.Contains("File ResponseBody is null"))
                {
                    return ErrorCodeFactory.UploadFileError;
                }
            }
            catch (Exception thisException)
            {
                logger.Error("Error when getting error code for exception. Detail :{0}.", thisException.ToString());
            }
            return null;
        }

        public static ErrorCode GetGoogleApiExceptionErrorCode(GoogleApiException ex)
        {
            try
            {
                if (ex != null && ex.Error != null)
                {
                    if (ex.Error.Code == 400)
                    {
                        if (ex.Error.Errors != null)
                        {
                            foreach (var singleErrorReason in ex.Error.Errors.Select(x => x.Reason))
                            {
                                if (GoogleApiSingleErrorReason.FailedPrecondition.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteFailedPrecondition;
                                }
                                if (GoogleApiSingleErrorReason.FieldValueExceedsLimit.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteFieldValueExceedsLimit;
                                }
                                if (GoogleApiSingleErrorReason.InvalidArgument.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteInvalidArgument;
                                }
                            }
                        }
                    }
                    else if (ex.Error.Code == 403)
                    {
                        if (ex.Error.Errors != null)
                        {
                            foreach (var singleErrorReason in ex.Error.Errors.Select(x => x.Reason))
                            {
                                if (GoogleApiSingleErrorReason.DailyLimitExceeded.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteDailyLimitExceeded;
                                }
                                else if (GoogleApiSingleErrorReason.UserRateLimitExceeded.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteRateLimitExceeded;
                                }
                                else if (GoogleApiSingleErrorReason.RateLimitExceeded.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteRateLimitExceeded;
                                }
                                else if (GoogleApiSingleErrorReason.CalendarLimitExceeded.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteRateLimitExceeded;
                                }
                                else if (GoogleApiSingleErrorReason.InsufficientFilePermissions.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteFileAccessDenied;
                                }
                                else if (GoogleApiSingleErrorReason.FileCannotDownload.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteFileAccessDenied;
                                }
                                else if (GoogleApiSingleErrorReason.FileCannotExport.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteFileAccessDenied;
                                }
                                else if (GoogleApiSingleErrorReason.CannotDownloadAbusiveFile.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteUnsafeFile;
                                }
                                else if (GoogleApiSingleErrorReason.CannotChangeOwnPrimarySubscription.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteChangeSubscription;
                                }
                            }
                        }
                        if (ex.Message.Contains("This file has been identified as malware or spam and cannot be downloaded"))
                        {
                            return ErrorCodeFactory.GSuiteUnsafeFile;
                        }
                        if (ex.Message.Contains("Only files with binary content can be downloaded. Use Export with Docs Editors files"))
                        {
                            return ErrorCodeFactory.GSuiteThirdPartyFile;
                        }
                        if (ex.Message.Contains("This file cannot be downloaded by the user") || ex.Message.Contains("This file cannot be exported by the user"))
                        {
                            return ErrorCodeFactory.GSuiteFileAccessDenied;
                        }
                    }
                    else if (ex.Error.Code == 429)
                    {
                        if (ex.Error.Errors != null)
                        {
                            foreach (var singleErrorReason in ex.Error.Errors.Select(x => x.Reason))
                            {
                                if (GoogleApiSingleErrorReason.RateLimitExceeded.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteRateLimitExceeded;
                                }
                            }
                        }
                    }
                    else if (ex.Error.Code == 500)
                    {
                        if (ex.Error.Errors != null)
                        {
                            foreach (var singleErrorReason in ex.Error.Errors.Select(x => x.Reason))
                            {
                                if (GoogleApiSingleErrorReason.InternalError.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteServerErrorOccurred;
                                }
                            }
                        }
                    }
                    else if (ex.Error.Code == 503)
                    {
                        if (ex.Error.Errors != null)
                        {
                            foreach (var singleErrorReason in ex.Error.Errors.Select(x => x.Reason))
                            {
                                if (GoogleApiSingleErrorReason.BackendError.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteServiceUnavailable;
                                }
                            }
                        }
                    }
                    else if (ex.Message.Contains("A task was canceled"))
                    {
                        return ErrorCodeFactory.GSuiteTaskError;
                    }
                    else if (ex.Message.Contains("Transferred a partial file"))
                    {
                        return ErrorCodeFactory.GSuiteTransferredError;
                    }
                    else if (ex.Error.Code == 404)
                    {
                        if (ex.Error.Errors != null)
                        {
                            foreach (var singleErrorReason in ex.Error.Errors.Select(x => x.Reason))
                            {
                                if (GoogleApiSingleErrorReason.NotFound.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuite404NotFound;
                                }
                            }
                        }
                    }
                    else if (ex.Error.Code == 409)
                    {
                        if (ex.Error.Errors != null)
                        {
                            foreach (var singleErrorReason in ex.Error.Errors.Select(x => x.Reason))
                            {
                                if (GoogleApiSingleErrorReason.AlreadyExists.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteAlreadyExists;
                                }
                                if (GoogleApiSingleErrorReason.Aborted.Equals(singleErrorReason))
                                {
                                    return ErrorCodeFactory.GSuiteAborted;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception thisException)
            {
                logger.Error("Error when getting Google APIS error code.Detail :{0}.", thisException.ToString());
            }
            return null;
        }

        public static bool IsNotFound404(Exception exception)
        {
            if (exception is GoogleApiException)
            {
                var ex = exception as GoogleApiException;
                if (ex != null && ex.Error != null && ex.Error.Code.Equals(404) && ex.Error.Errors != null)
                {
                    foreach (var singleError in ex.Error.Errors)
                    {
                        if (GoogleApiSingleErrorReason.NotFound.Equals(singleError.Reason))
                        {
                            logger.Info("Not found error");
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool ExceptionNeedRetry(Exception ex)
        {
            try
            {
                if (ex is TaskCanceledException)
                {
                    logger.Warn($"TaskCanceledException occurred: {ex}.Sleep {NetworkExceptionRetryTime}s");
                    var throttlingTime = NetworkExceptionRetryTime;
                    Thread.Sleep(throttlingTime);
                    return true;
                }
                if (IsNetworkException(ex))
                {
                    logger.Warn($"Network related error occurred: {ex}.Sleep {NetworkExceptionRetryTime}s");
                    var throttlingTime = NetworkExceptionRetryTime;
                    Thread.Sleep(throttlingTime);
                    return true;
                }
            }
            catch (Exception newException)
            {
                logger.Warn("Error when checking retry of exception. Detail:{0}.", newException.ToString());
            }
            return false;
        }
        public static bool IsNetworkException(Exception ex)
        {
            if (ex == null) return false;
            if (IsUnstableNetworkException(ex)) return true;
            if (IsConnectionClosedError(ex)) return true;
            return IsNetworkException(ex.InnerException);
        }
        private static bool IsUnstableNetworkException(Exception ex)
        {
            var webEx = ex as WebException;
            if (webEx != null)
            {
                logger.Warn("WebExceptionStatus: {0}", webEx.Status);
                switch (webEx.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                    case WebExceptionStatus.SecureChannelFailure:
                    case WebExceptionStatus.ConnectFailure:
                    case WebExceptionStatus.KeepAliveFailure:
                    case WebExceptionStatus.PipelineFailure:
                    case WebExceptionStatus.SendFailure:
                    case WebExceptionStatus.UnknownError:
                    case WebExceptionStatus.Pending:
                    case WebExceptionStatus.ProtocolError:
                        return true;
                }
            }
            return false;
        }

        private static bool IsConnectionClosedError(Exception ex)
        {
            #region Exception message& stack
            //Microsoft.Exchange.WebServices.Data.ServiceRequestException: The request failed. The underlying connection was closed: A connection that was expected to be kept alive was closed by the server. 
            //---> System.Net.WebException: The underlying connection was closed: A connection that was expected to be kept alive was closed by the server. 
            //---> System.IO.IOException: Unable to read data from the transport connection: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. 
            //---> System.Net.Sockets.SocketException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond
            //at System.Net.Sockets.Socket.EndReceive(IAsyncResult asyncResult)
            //at System.Net.Sockets.NetworkStream.EndRead(IAsyncResult asyncResult)
            //--- End of inner exception stack trace ---
            //at System.Net.Security._SslStream.EndRead(IAsyncResult asyncResult)
            //at System.Net.TlsStream.EndRead(IAsyncResult asyncResult)
            //at System.Net.PooledStream.EndRead(IAsyncResult asyncResult)
            //at System.Net.Connection.ReadCallback(IAsyncResult asyncResult)
            //--- End of inner exception stack trace ---
            //at System.Net.HttpWebRequest.GetResponse()
            //at Microsoft.Exchange.WebServices.Data.EwsHttpWebRequest.Microsoft.Exchange.WebServices.Data.IEwsHttpWebRequest.GetResponse()
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.GetEwsHttpWebResponse(IEwsHttpWebRequest request)
            //--- End of inner exception stack trace ---
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.GetEwsHttpWebResponse(IEwsHttpWebRequest request)
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.ValidateAndEmitRequest(IEwsHttpWebRequest& request)
            //at Microsoft.Exchange.WebServices.Data.SimpleServiceRequestBase.InternalExecute(Boolean retry)
            //at Microsoft.Exchange.WebServices.Data.MultiResponseServiceRequest`1.InternalExecuteWithoutRetry()
            //at ExchangeUtility.AADTokenRefresher.Retry(Func`1 tryBlockAction)
            //at ExchangeUtility.Retryable.Retry(Func`1 tryBlockAction, Int32 maxRetryTime)
            #endregion
            if (ex is IOException ||
                ex is System.Net.Sockets.SocketException)
            {
                return true;
            }
            return false;
        }

        private static void SumQuotaCount()
        {
            lock (_lockObject)
            {
                quotaCount++;
            }
        }


        public static class GoogleApiSingleErrorReason
        {
            /// <summary>
            /// https://cloud.google.com/storage/docs/json_api/v1/status-codes
            /// https://developers.google.com/drive/api/v3/handle-errors
            /// </summary>
            /// 
            //400
            public const string Invalid = "invalid";
            public const string FailedPrecondition = "failedPrecondition";
            public const string FieldValueExceedsLimit = "fieldValueExceedsLimit";
            public const string InvalidArgument = "invalidArgument";
            //403
            public const string UserRateLimitExceeded = "userRateLimitExceeded";
            public const string RateLimitExceeded = "rateLimitExceeded";
            public const string DailyLimitExceeded = "dailyLimitExceeded"; //+429
            public const string CalendarLimitExceeded = "quotaExceeded";
            public const string InsufficientFilePermissions = "insufficientFilePermissions";
            public const string CannotDownloadAbusiveFile = "cannotDownloadAbusiveFile";
            public const string GoogleDocExceedSize = "exportSizeLimitExceeded";
            public const string FileCannotDownload = "cannotDownloadFile";
            public const string FileCannotExport = "cannotExportFile";
            public const string CannotChangeOwnPrimarySubscription = "cannotChangeOwnPrimarySubscription";
            //500
            public const string BackendError = "backendError"; //+503
            public const string InternalError = "internalError";
            //404
            public const string NotFound = "notFound";
            //409
            public const string AlreadyExists = "alreadyExists";
            public const string Aborted = "aborted";
            public static List<string> LimitExceeded
            {
                get
                {
                    return _limitExceeded;
                }
                set
                {
                    _limitExceeded = value;
                }
            }
            public static List<string> ServerError
            {
                get
                {
                    return _serverError;
                }
                set
                {
                    _serverError = value;
                }
            }
            public static List<string> HtmlMessages
            {
                get
                {
                    return _htmlMessages;
                }
                set
                {
                    _htmlMessages = value;
                }
            }
            private static List<string> _limitExceeded = new List<string>()
            {
                UserRateLimitExceeded,
                RateLimitExceeded,
                DailyLimitExceeded,
                CalendarLimitExceeded
            };

            private static List<string> _serverError = new List<string>()
            {
                BackendError,
                InternalError
            };

            private static List<string> _htmlMessages = new List<string>()
            {
                "The server encountered a temporary error and could not complete your request."
            };
        }
    }
}
