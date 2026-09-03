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

namespace ExchangeUtility
{
    using AvePoint.GCommon;
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Collections;
    using System.Net;
    using System.Xml;
    using System.Linq;
    using AvePoint.RA.CommonUtil;
    using System.Threading.Tasks;

    internal abstract class DecoratableRetryController : IRetryable
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(DecoratableRetryController));
        private readonly IRetryable innerRetryable;

        protected DecoratableRetryController(IRetryable retryable) => innerRetryable = retryable;

        public virtual async Task<T> Retry<T>(Func<Task<T>> tryBlockAction, RequestInfo requestInfo)
        {
            if (innerRetryable == null)
            {
                return tryBlockAction == null ? throw new ArgumentNullException(nameof(tryBlockAction)) : await tryBlockAction();
            }

            return await innerRetryable.Retry(tryBlockAction, requestInfo);
        }

        public void SetExchangeService(ExchangeService exchangeService)
        {
        }

        public IRetryable Clone(ExchangeService exchangeService)
        {
            return innerRetryable;
        }

        //public IRetryable Clone() => innerRetryable;
    }

    internal class ExceptionWrapper : DecoratableRetryController
    {
        private readonly FormattedMessageException.Context context;

        public ExceptionWrapper(FormattedMessageException.Context context, IRetryable retryable)
            : base(retryable)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.CheckArguments();
            this.context = context;
        }

        public override async Task<T> Retry<T>(Func<Task<T>> execute, RequestInfo requestInfo)
        {
            try
            {
                return await base.Retry(execute, requestInfo);
            }
            catch (ServiceResponseException srspEx)
            {
                srspEx.ThrowIfWellknownError(this.context);
                throw;
            }
        }
    }

    internal class Retryable : DecoratableRetryController
    {
        private readonly int maxRetryTime = 1;
        private readonly int maxSepcificRetryTime = 5;
        private static readonly RequestInfo DEFAULT_REQUEST_INFO = new RequestInfo() { WebServiceMethod = "Unknown" };

        public Retryable(IRetryable retryable = null)
            : base(retryable)
        { }

        public Retryable(int maxRetryTime, IRetryable retryable = null)
            : this(retryable)
        {
            maxSepcificRetryTime = maxRetryTime;
        }

        //public override async Task<T> Retry<T>(Func<Task<T>> tryBlockAction, RequestInfo requestInfo)
        //{
        //    //Remove retry logic for nestle EWS API to avoid long running job.
        //    try
        //    {
        //        requestInfo ??= DEFAULT_REQUEST_INFO;
        //        LogEWSRequest(requestInfo);
        //        var obj = await base.Retry(tryBlockAction, requestInfo);
        //        LogEWSErrorResponse(obj as IEnumerable);
        //        return obj;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn($"Try block throw a exception.method: {requestInfo.WebServiceMethod}, error: {ex}.");
        //        throw;
        //    }
        //}

        public override async Task<T> Retry<T>(Func<Task<T>> tryBlockAction, RequestInfo requestInfo)
        {
            var maxSepcificRetryTime = this.maxSepcificRetryTime;
            var retryTime = 0;
            var specificRetryTime = 0;
            requestInfo ??= DEFAULT_REQUEST_INFO;

            do
            {
                try
                {
                    LogEWSRequest(requestInfo);
                    var obj = await base.Retry(tryBlockAction, requestInfo);
                    LogEWSErrorResponse(obj as IEnumerable);
                    return obj;
                }
                catch (ServiceResponseException srspEx)
                {
                    logger.Warn("Try block throw a ServiceResponseException, retry time: {0}, method: {1}, error code: {2}, error: {3}", specificRetryTime, requestInfo.WebServiceMethod, srspEx.ErrorCode, srspEx);
                    logger.Warn("Error details: {0}", srspEx.ErrorDetails().ToString());
                    LogEWSErrorResponse(srspEx);
                    if (!string.IsNullOrEmpty(srspEx.ErrorDetails().ToString()) && srspEx.ErrorDetails().ToString().Contains("ErrorQuotaExceededOnDelete")) throw;
                    if (srspEx.Message.Contains("The mailbox operation failed")) throw;
                    if (srspEx.ErrorCode == ServiceError.ErrorCannotDeleteObject) throw;
                    if (specificRetryTime >= maxSepcificRetryTime) throw;
                    if (srspEx.ErrorCode == ServiceError.ErrorMailboxConfiguration)
                        System.Threading.Thread.Sleep(10 * 1000);
                    else if (!srspEx.WaitForNextRequest()) throw;
                }
                catch (EWSRetryException ewsRetryEx)
                {
                    logger.Warn("Try block throw a EWSRetryException, retry time: {0}, method: {1}, error: {2}", specificRetryTime, requestInfo.WebServiceMethod, ewsRetryEx);
                    LogEWSErrorResponse(ewsRetryEx);
                    if (specificRetryTime >= maxSepcificRetryTime) throw;
                    if (!ewsRetryEx.WaitForNextRequest()) throw;
                }
                catch (ServiceRequestException sreqEx)
                {
                    logger.Warn("Try block throw a ServiceRequestException, retry time: {0}, method: {1}, error: {2}", specificRetryTime, requestInfo.WebServiceMethod, sreqEx);
                    LogEWSErrorResponse(sreqEx);
                    if (sreqEx.Message.Contains("The request failed. The operation has timed out")) throw;
                    if (specificRetryTime >= maxSepcificRetryTime) throw;
                    if (sreqEx.Message.Trim().TrimEnd('.').Equals("The request failed", StringComparison.OrdinalIgnoreCase))
                        System.Threading.Thread.Sleep(60 * 1000);
                    else if (sreqEx.Message.Contains("Server Unavailable") || sreqEx.Message.Contains("Service Unavailable")
                        || sreqEx.Message.Contains("The remote name could not be resolved: 'outlook.office365.com'")
                        || sreqEx.Message.Contains("The response received from the service didn't contain valid XML")
                        || (sreqEx.Message.Contains("The request failed") && !sreqEx.Message.Contains("Timeout")))
                    {
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else
                    {
                        if (!sreqEx.WaitForNextRequest()) throw;
                    }
                }
                catch (Exception ex) when (ex is XmlException or ArgumentNullException)
                {
                    logger.Warn("Try block throw a XmlException, retry time: {0}, method: {1}, error: {2}", specificRetryTime, requestInfo.WebServiceMethod, ex);
                    if (specificRetryTime >= maxSepcificRetryTime) throw;
                    if (!ex.WaitForNextRequest()) throw;
                }
                catch (Exception ex) when (ex.Message.Contains("ErrorConnectionFailedTransientError"))
                {
                    logger.Warn("Try block throw a ErrorConnectionFailedTransientError, retry time: {0}, method: {1}, error: {2}", specificRetryTime, requestInfo.WebServiceMethod, ex);
                    if (specificRetryTime >= maxSepcificRetryTime) throw;
                    System.Threading.Thread.Sleep(150000);
                }
                catch (Exception ex) when (ex.IsConnectonForciblyClosedExceptioin())
                {
                    logger.Warn("Try block throw a ConnectonForciblyClosedExceptioin, retry time: {0}, method: {1}, error: {2}", specificRetryTime, requestInfo.WebServiceMethod, ex);
                    if (specificRetryTime >= maxSepcificRetryTime) throw;
                    System.Threading.Thread.Sleep(10 * 1000);
                }
                catch (Exception ex) when (ex.Message.Contains("Requested value 'Guest' was not found"))
                {
                    maxSepcificRetryTime = 10;
                    logger.Warn("Try block throw a guest was not found exception, retry time: {0}, method: {1}, error: {2}", specificRetryTime, requestInfo.WebServiceMethod, ex);
                    if (specificRetryTime >= maxSepcificRetryTime) throw;
                    System.Threading.Thread.Sleep(20 * 1000);
                }
                catch (Exception ex) when (retryTime < maxRetryTime)
                {
                    logger.Warn("Try block throw a exception, retry time: {0}, method: {1}, error: {2}", retryTime, requestInfo.WebServiceMethod, ex);
                    System.Threading.Thread.Sleep(10 * 1000);
                    retryTime++;
                }

                specificRetryTime++;
            } while (true);
        }

        private static void LogEWSRequest(RequestInfo requestInfo)
        {
            try
            {
                EWSMonitor.Instance.IncreaseRequestNumber(requestInfo.WebServiceMethod);
            }
            catch (Exception e)
            {
                logger.Error($"The log of EWS Request's error message : {e.Message}");
            }
        }

        private static void LogEWSErrorResponse(IEnumerable collection)
        {
            if (collection == null) return;
            var firstErrorItem = collection.Cast<ServiceResponse>().
                Where(rsp => rsp != null).
                FirstOrDefault(rsp => rsp.Result != ServiceResult.Success);
            if (firstErrorItem == null) return;
            LogEWSErrorResponse(firstErrorItem.ErrorCode.ToString());
        }

        private static void LogEWSErrorResponse(string error)
        {
            try
            {
                EWSMonitor.Instance.IncreaseErrorResponseNumber(error);
            }
            catch (Exception e)
            {
                logger.Error($"The log of EWS Error Request's error message : {e.Message}");
            }
        }

        private static void LogEWSErrorResponse(Exception ex)
        {
            if (ex == null) return;
            var srspEx = ex as ServiceResponseException;
            if (srspEx != null)
            {
                LogEWSErrorResponse(srspEx.ErrorCode.ToString());
                return;
            }

            var inner = GetDeepestInnerException(ex);
            LogEWSErrorResponse(inner.GetType().Name);
        }

        private static Exception GetDeepestInnerException(Exception ex)
        {
            Exception parent = ex;
            while (parent.InnerException != null)
            {
                parent = parent.InnerException;
            }
            return parent;
        }
    }

    internal class AADTokenRefresher : DecoratableRetryController
    {
        public AADTokenRefresher(AppTokenAuthObject authObj, ExchangeService service, IRetryable innerRetryable = null)
            : base(innerRetryable)
        {
            AuthObj = authObj ?? throw new ArgumentNullException(nameof(authObj));
            ExchangeServices = service ?? throw new ArgumentNullException(nameof(service));
        }

        internal AppTokenAuthObject AuthObj { get; private set; }

        internal ExchangeService ExchangeServices { get; private set; }

        public override async Task<T> Retry<T>(Func<Task<T>> tryBlockAction, RequestInfo requestInfo)
        {
            try
            {
                return await base.Retry(tryBlockAction, requestInfo);
            }
            catch (ServiceRequestException srqesEx) when (IsHttp401Exception(srqesEx))
            {
                logger.Warn("Http 401 error occurred, refresh access token and try again. Error: {0}", srqesEx);
                AuthObj.BindToExchangeService(ExchangeServices);
                return await base.Retry(tryBlockAction, requestInfo);
            }
        }

        private bool IsHttp401Exception(ServiceRequestException srqesEx)
        {
            if (srqesEx.InnerException is WebException webEx)
            {
                if (webEx.Response is HttpWebResponse httpWebResponse)
                {
                    return httpWebResponse.StatusCode == HttpStatusCode.Unauthorized;
                }
            }

            if (srqesEx.InnerException is EwsHttpClientException ewsEx)
            {
                return ewsEx.Response.StatusCode == HttpStatusCode.Unauthorized;
            }

            return false;
        }
    }
}