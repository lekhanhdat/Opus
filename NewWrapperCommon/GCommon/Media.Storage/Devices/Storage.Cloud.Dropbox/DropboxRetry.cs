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
namespace AvePoint.Media.Storage.Cloud.Dropbox
{
    #region using
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    #endregion

    internal class DropboxRetry
    {
        private Boolean isRetry;
        private Int32 maxRetryCount;
        private Int32 retryInterval;
        private StorageLogger logger = new StorageLogger(typeof(DropboxRetry));

        public DropboxRetry(Boolean isRetry, Int32 maxRetryCount, Int32 retryInternal)
        {
            this.isRetry = isRetry;
            this.maxRetryCount = maxRetryCount;
            this.retryInterval = retryInternal;
        }

        public T Retry<T>(RetryDelegate<T> del)
        {
            var counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    return del.Invoke();
                }
                catch (WebException ex)
                {
                    if (counter > this.maxRetryCount)
                    {
                        logger.Error("Too many retry failed. Retry count:{0}, msg:{1}", counter, ex);
                        throw;
                    }
                    if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                    {
                        var resp = ex.Response as HttpWebResponse;
                        if (resp.StatusCode == HttpStatusCode.InternalServerError || resp.StatusCode == HttpStatusCode.RequestTimeout || resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            if (this.isRetry || counter >= this.maxRetryCount)
                            {
                                throw new DeviceNotAvailableException(String.Format("This exception is a connection fail exception:" + ex.Message));
                            }
                            else
                            {
                                logger.Info("Retry after " + this.retryInterval + " ms. Retry count: " + counter);
                                Thread.Sleep(this.retryInterval);
                                continue;
                            }
                        }
                        else
                        {
                            var body = String.Empty;
                            var errorSummary = String.Empty;
                            using (var respStream = resp.GetResponseStream())
                            {
                                using (var sr = new StreamReader(respStream))
                                {
                                    body = sr.ReadToEnd();
                                }
                                var regexId = new Regex(DropboxConstants.ErrorSummary);
                                var mc = regexId.Matches(body);
                                foreach (var m in mc)
                                {
                                    var temp = m.ToString().Split(':');
                                    errorSummary = temp[1].Trim(new Char[] { '\\', '\"' }).Substring(2);
                                }
                            }
                            logger.Error("Execute request failed, msg:{0}, response body:{1}:", ex, body);
                            if (errorSummary.Contains("path/not_found") || errorSummary.Contains("path_lookup/not_found"))
                                throw new PathNotFoundException(ex.Message, ex);
                            throw;
                        }
                    }
                    else if (ex.Status == WebExceptionStatus.ConnectionClosed || ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                    {
                        if (this.isRetry || counter >= this.maxRetryCount)
                        {
                            throw new DeviceNotAvailableException(String.Format("This exception is a connection fail exception:" + ex.Message));
                        }
                        else
                        {
                            logger.Info("Retry after " + this.retryInterval + " ms. Retry count: " + counter);
                            Thread.Sleep(this.retryInterval);
                            continue;
                        }
                    }
                    else
                    {
                        logger.Error("Execute request failed: {0}", ex);
                        throw;
                    }
                }
                catch (RetryableException re)//TODO
                {
                    if (counter > this.maxRetryCount)
                    {
                        logger.Error("Too many retry failed. Retry count:{0}, msg:{1}", counter, re);
                        throw;
                    }
                    logger.Info("Retry after at once. Retry count: " + counter);
                    continue;
                }
            }
        }
    }
}
