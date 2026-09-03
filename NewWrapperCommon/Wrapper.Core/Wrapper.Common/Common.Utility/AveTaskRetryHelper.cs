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


namespace AvePoint.Wrapper.Common
{
    using AvePoint.GCommon;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Linq;

    public class AveTaskRetryHelper
    {
        protected readonly int MAX_RETRY_ATTEMPT_COUNT;
        private Dictionary<string, List<string>> excepDetails;
        private List<int> errorCodes;
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveTaskRetryHelper));
        public bool ShouldRetryCommonConnectionExceptions { get; set; }

        public AveTaskRetryHelper(int maxRetryAttemptCount)
        {
            MAX_RETRY_ATTEMPT_COUNT = maxRetryAttemptCount > 1 ? maxRetryAttemptCount : 1;
            excepDetails = new Dictionary<string, List<string>>();
            ShouldRetryCommonConnectionExceptions = false;
        }

        public AveTaskRetryHelper(int maxRetryAttemptCount, params Exception[] exceps)
            : this(maxRetryAttemptCount)
        {
            AddRetryExceptionDetail(exceps);
        }

        public AveTaskRetryHelper(int maxRetryAttemptCount, params KeyValuePair<string, string>[] exceps)
            : this(maxRetryAttemptCount)
        {
            foreach (KeyValuePair<string, string> excep in exceps)
            {
                AddRetryExceptionDetail(excep.Key, excep.Value);
            }
        }

        public AveTaskRetryHelper(int maxRetryAttemptCount, params int[] serverErrorCodes)
            : this(maxRetryAttemptCount)
        {
            errorCodes = new List<int>();
            foreach (var errorCode in serverErrorCodes)
            {
                errorCodes.Add(errorCode);
            }
        }

        public void AddRetryExceptionDetail(params Exception[] exceps)
        {
            foreach (Exception excep in exceps)
            {
                string excepType = excep.GetType().Name;
                string excepMessage = excep.Message.Contains("Exception of type") ? string.Empty : excep.Message;
                AddRetryExceptionDetail(excepType, excepMessage);
            }
        }

        private void AddRetryExceptionDetail(string excepType, string excepMessage)
        {
            if (!excepDetails.ContainsKey(excepType))
            {
                List<string> errorMsgs = new List<string>();
                errorMsgs.Add(excepMessage);
                excepDetails.Add(excepType, errorMsgs);
            }
            else
            {
                if (!excepDetails[excepType].Contains(excepMessage))
                {
                    excepDetails[excepType].Add(excepMessage);
                }
            }
        }

        public virtual void ExecuteWithRetryMechanism(Action taskToRetry)
        {
            int attemptCount = -1;
            while (attemptCount++ < MAX_RETRY_ATTEMPT_COUNT)
            {
                try
                {
                    taskToRetry();
                    break;
                }
                catch (Exception e)
                {
                    if (attemptCount == MAX_RETRY_ATTEMPT_COUNT)
                    {
                        logger.Error("Failed to retry from an exception ( {0} time(s) retried ), error message : {1}", MAX_RETRY_ATTEMPT_COUNT, e.ToString());
                        throw;
                    }
                    if (NeedRetry(e))
                    {
                        logger.Warn("Retry from an exception ( {0} time(s) retried ) , error message : {1}", attemptCount + 1, e.Message);
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}", e.ToString());
                    throw;
                }
            }
        }

        public virtual bool ExecuteWithRetryMechanism(Func<bool, bool> taskToRetry)
        {
            int attemptCount = -1;
            bool needReloadFile = false;
            while (attemptCount++ < MAX_RETRY_ATTEMPT_COUNT)
            {
                try
                {
                    return taskToRetry(needReloadFile);
                }
                catch (Exception e)
                {
                    if (attemptCount == MAX_RETRY_ATTEMPT_COUNT)
                    {
                        logger.Error("Failed to retry from an exception ( {0} time(s) retried ), error message : {1}", MAX_RETRY_ATTEMPT_COUNT, e.ToString());
                        throw;
                    }
                    if (NeedRetry(e))
                    {
                        needReloadFile = true;
                        logger.Warn("Retry from an exception ( {0} time(s) retried ) , error message : {1}", attemptCount + 1, e.Message);
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}", e.ToString());
                    throw;
                }
            }
            return false;
        }

        protected bool NeedRetry(Exception e)
        {
            return NeedRetryByExceptionMessage(e);// || CheckCommonConnectionExceptions(e); 底层ReliableHttpWebRequest中有检测 retry逻辑，所以此处就不再再次检查。
        }

        protected bool NeedRetryByExceptionMessage(Exception e)
        {
            bool shouldRetry = false;
            string excepType = e.GetType().Name;
            if (excepDetails.ContainsKey(excepType))
            {
                foreach (string errorMsg in excepDetails[excepType])
                {
                    if (errorMsg == string.Empty || e.Message.Contains(errorMsg))
                    {
                        System.Threading.Thread.Sleep(5000);
                        shouldRetry = true;
                        break;
                    }
                }
            }
            return shouldRetry;
        }

        protected bool NeedRetryByServerErrorCode(int errorCode)
        {
            if (errorCodes != null && errorCodes.Count > 0 &&
                errorCodes.Contains(errorCode))
            {
                return true;
            }
            return false;
        }

        private bool CheckCommonConnectionExceptions(Exception e)
        {
            int retryInterval = 5000;
            if (IsConnectonForciblyClosedExceptioin(e) || IsUnstableNetworkException(e as WebException))
            {
                System.Threading.Thread.Sleep(retryInterval);
                return true;
            }
            else if (IsHTTP429Error(e, ref retryInterval))
            {
                System.Threading.Thread.Sleep(retryInterval);
                return true;
            }
            return false;
        }

        //we assume socketexception or ioexception caused by connection forcilby closed
        private bool IsConnectonForciblyClosedExceptioin(Exception te)
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

        private bool IsUnstableNetworkException(WebException e)
        {
            if (e == null)
            {
                return false;
            }
            if (e.Status == System.Net.WebExceptionStatus.NameResolutionFailure
                || e.Status == WebExceptionStatus.SecureChannelFailure
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
                    || webResponse.StatusCode == HttpStatusCode.Forbidden))
                {
                    return true;
                }
            }
            return false;
        }

        //HTTP 429 ERROR, Too Many Request.
        private bool IsHTTP429Error(Exception e, ref int interval)
        {
            if (e is WebException)
            {
                HttpWebResponse response = (e as WebException).Response as HttpWebResponse;
                if (response != null && (int)response.StatusCode == 429)
                {
                    interval = response.Headers != null && response.Headers.AllKeys.Contains("Retry-After") ? Convert.ToInt32(response.Headers["Retry-After"]) * 1000 : interval;
                    response.Close();
                    return true;
                }
            }
            else if (e.InnerException != null)
            {
                return IsHTTP429Error(e.InnerException, ref interval);
            }
            return false;
        }        
    }
}
