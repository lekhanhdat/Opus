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
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveTaskRetryHelper));
        protected int mRetryInterval = 3000; //WrapperConfiguration.BPOS_S.RetryInterval;
        protected readonly int MAX_RETRY_ATTEMPT_COUNT;
        protected readonly TimeSpan MAX_RETRY_ATTEMPT_Time;
        protected readonly bool mRetryNotMaterWhatException = false;
        private Dictionary<string, List<string>> excepDetails;
        private List<int> errorCodes;
        private bool retryByAttemptCount = true;

        public AveTaskRetryHelper(int maxRetryAttemptCount)
        {
            MAX_RETRY_ATTEMPT_COUNT = maxRetryAttemptCount > 1 ? maxRetryAttemptCount : 1;
            excepDetails = new Dictionary<string, List<string>>();
        }

        public AveTaskRetryHelper(TimeSpan maxRetryAttemptTime)
        {
            MAX_RETRY_ATTEMPT_Time = maxRetryAttemptTime == new TimeSpan(0, 0, 0) ? new TimeSpan(0, 10, 0) : maxRetryAttemptTime;
            mRetryInterval = 2 * 60 * 1000;
            excepDetails = new Dictionary<string, List<string>>();
            retryByAttemptCount = false;
        }

        public AveTaskRetryHelper(int maxRetryAttemptCount, bool retryNotMaterWhatException, int retryInterval) : this(maxRetryAttemptCount, retryNotMaterWhatException)
        {
            mRetryInterval = retryInterval > 0 ? retryInterval : mRetryInterval;
        }

        public AveTaskRetryHelper(int maxRetryAttemptCount, bool retryNotMaterWhatException)
        {
            MAX_RETRY_ATTEMPT_COUNT = maxRetryAttemptCount > 1 ? maxRetryAttemptCount : 1;
            mRetryNotMaterWhatException = retryNotMaterWhatException;
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

        public void AddRetryExceptionDetail(params int[] serverErrorCodes)
        {
            if (mRetryNotMaterWhatException)
            {
                return;
            }
            if (errorCodes == null)
            {
                errorCodes = new List<int>();
            }
            foreach (var errorCode in serverErrorCodes)
            {
                if (!errorCodes.Contains(errorCode))
                {
                    errorCodes.Add(errorCode);
                }
            }
        }
        public void AddRetryExceptionDetail(params Exception[] exceps)
        {
            if (mRetryNotMaterWhatException)
            {
                return;
            }
            foreach (Exception excep in exceps)
            {
                string excepType = excep.GetType().Name;
                string excepMessage = excep.Message.Contains("Exception of type") ? string.Empty : excep.Message;
                AddRetryExceptionDetail(excepType, excepMessage);
            }
        }

        public void AddRetryExceptionDetail(string excepType, string excepMessage)
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
                        logger.Warn("Retry {0} from an exception ( {1} time(s) retried ) , error message : {2}", taskToRetry.Method.Name, attemptCount + 1, e.Message);
                        System.Threading.Thread.Sleep(WrapperConfiguration.WrapperConfigurationForBPOS.RetryInterval);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}", e.ToString());
                    throw;
                }
            }
        }

        public virtual void ExecuteWithRetryMechanismV2(Action taskToRetry)
        {
            int attemptCount = 0;
            DateTime retryStartTime = DateTime.Now;
            while (true)
            {
                try
                {
                    taskToRetry();
                    break;
                }
                catch (Exception e)
                {
                    if (!CheckNeedContinue(attemptCount++, retryStartTime))
                    {
                        logger.Error("Failed to retry from an exception ( {0} time(s) retried ), error message : {1}", attemptCount - 1, e.ToString());
                        throw;
                    }
                    if (NeedRetry(e))
                    {
                        logger.Warn("Retry from an exception ( {0} time(s) retried ) , error message : {1}", attemptCount - 1, e.Message);
                        System.Threading.Thread.Sleep(mRetryInterval);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}", e.ToString());
                    throw;
                }
            }
        }

        // retry with linear backoff strategy, the sleep time will increase linearly as the attempt count increases.
        public virtual void ExecuteWithRetryMechanismV3(Action taskToRetry)
        {
            int attemptCount = 0;
            while (attemptCount < MAX_RETRY_ATTEMPT_COUNT)
            {
                try
                {
                    taskToRetry();
                    return;
                }
                catch (Exception e)
                {
                    attemptCount++;
                    if (attemptCount == MAX_RETRY_ATTEMPT_COUNT)
                    {
                        logger.Error($"Failed to retry from an exception ( {MAX_RETRY_ATTEMPT_COUNT} time(s) retried ), error message : {e}");
                        throw;
                    }
                    if (NeedRetry(e))
                    {
                        int sleepTime = mRetryInterval * attemptCount;
                        logger.Warn($"Retry {taskToRetry.Method.Name}... from an exception. Attempt {attemptCount}... Sleep {sleepTime}ms. Ex: {e}");
                        System.Threading.Thread.Sleep(sleepTime);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}", e.ToString());
                    throw;
                }
            }
        }

        private bool CheckNeedContinue(int attemptCount, DateTime retryStartTime)
        {
            if (retryByAttemptCount)
            {
                return attemptCount < MAX_RETRY_ATTEMPT_COUNT;
            }
            else
            {
                return DateTime.Now - retryStartTime < MAX_RETRY_ATTEMPT_Time;
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
                        System.Threading.Thread.Sleep(WrapperConfiguration.WrapperConfigurationForBPOS.RetryInterval);
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
            if (mRetryNotMaterWhatException)
            {
                return true;
            }
            return NeedRetryByExceptionMessage(e) || CheckCommonConnectionExceptions(e); //底层ReliableHttpWebRequest中有检测 retry逻辑，所以此处就不再再次检查。
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
                        System.Threading.Thread.Sleep(WrapperConfiguration.WrapperConfigurationForBPOS.RetryInterval);
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
            int retryInterval = WrapperConfiguration.WrapperConfigurationForBPOS.RetryInterval;
            if (RequestExceptionHanddler.IsConnectonForciblyClosedExceptioin(e) || RequestExceptionHanddler.IsUnstableNetworkException(e as WebException))
            {
                System.Threading.Thread.Sleep(retryInterval);
                return true;
            }
            else if (RequestExceptionHanddler.IsToomanyRequestError(e,ref retryInterval)
                && RequestExceptionHanddler.IsRetryableWebException(e, ref retryInterval))
            {
                System.Threading.Thread.Sleep(retryInterval);
                return true;
            }
            return false;
        }
    }
}
