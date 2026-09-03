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
using AvePoint.RA.Contract.Exceptions;

namespace RASalesforce.APIs;

public abstract class SalesforceAPIService
{
    private readonly static IRALogger logger = RALogger.GetInstance(typeof(SalesforceAPIService));
    private SemaphoreSlim ThrottledLockObj = new(1, 1);
    private SalesforceAPIProvider provider;
    public SalesforceAPIService(SalesforceAPIProvider provider)
    {
        this.provider = provider;
    }

    internal abstract void InitClientSetting(SFToken token);

    protected async Task<T> InvokerAsync<T>(Func<Task<T>> action, int retryTimes, int sleepTime = 0)
    {
        return await RetryWhenAsync(async () =>
        {
            try
            {
                T t = await action();
                return t;
            }
            catch (Exception ex)
            {
                await HandleApiException(ex);
                throw;
            }
        }, retryTimes, sleepTime);
    }

    protected async Task<SFToken> GetTokenAsync()
    {
        var token = await provider.RefreshToken();
        if (token.NeedRefresh)
        {
            provider.RefreshSetting(token);
        }
        return token;
    }

    private async Task<T> RetryWhenAsync<T>(Func<Task<T>> action, int retryTimes, int sleepTime = 2000)
    {
        for (int i = 0; i < retryTimes; i++)
        {
            while (SalesforceAPIHelper.Instance.IsPaused)
            {
                if (!SalesforceAPIHelper.Instance.IsPaused)
                {
                    break;
                }
            }
            try
            {
                SalesforceAPIHelper.Instance.IncRequest();
                return await action();
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString());
                if (e.Message.Contains("INVALID_TYPE_FOR_OPERATION"))
                {
                    break;
                }

                if (i == retryTimes - 1 )
                {
                    throw;
                }
                else
                {
                    Thread.Sleep(sleepTime);
                    continue;
                }
            }
        }
        return default;
    }

    private async Task HandleApiException(Exception ex)
    {
        if (ex.Message.Contains("REQUEST_LIMIT_EXCEEDED: TotalRequests Limit exceeded."))
        {
            // 这里有两种情况，一种是真的被限流了，一种是被限流后的后续请求，只有真的被限流了才将IsThrottled更新为true
            await ThrottledLockObj.WaitAsync();
            try
            {
                SalesforceAPIHelper.Instance.Pause();
                logger.Warn("Throttled, wait for 24 hours.");
                await Task.Delay(1000 * 60 * 60 * 24);
            }
            finally
            {
                ThrottledLockObj.Release();
            }
        }
        else if (ex.Message.Contains("invalid_session_id") ||
                 ex.Message.Contains("session expires or a user logs out") ||
                 ex.Message.Contains("(500) Internal Server"))
        {
            logger.Warn("Session timeout.");
            var token = await provider.RefreshToken(true);
            provider.RefreshSetting(token);
        }
        else if (ex.Message.Contains("invalid_search") || ex.Message.Contains("INVALID_TYPE_FOR_OPERATION"))
        {
            throw ex;
        }
    }
}