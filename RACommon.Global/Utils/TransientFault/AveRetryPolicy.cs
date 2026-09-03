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
using AvePoint.RA.CommonUtil;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.TransientFault
{

    /// <summary>
    /// 统一重试策略，当执行操作可能遇到临时性故障或异常需要重试时使用
    /// </summary>
    public class AveRetryPolicy
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static AveRetryPolicy noRetry = new AveRetryPolicy(new AveTransientErrorIgnoreStrategy(), AveRetryStrategy.NoRetry);
        private static AveRetryPolicy defaultFixed = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), AveRetryStrategy.DefaultFixed);
        private static AveRetryPolicy defaultProgressive = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), AveRetryStrategy.DefaultProgressive);
        private static AveRetryPolicy defaultExponential = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), AveRetryStrategy.DefaultExponential);
        public event EventHandler<AveRetryingEventArgs> Retrying;
        /// <summary>
        /// 不重试策略
        /// </summary>
        public static AveRetryPolicy NoRetry
        {
            get
            {
                return AveRetryPolicy.noRetry;
            }
        }
        /// <summary>
        /// 默认等值时间间隔重试策略
        /// </summary>
        public static AveRetryPolicy DefaultFixed
        {
            get
            {
                return AveRetryPolicy.defaultFixed;
            }
        }
        /// <summary>
        /// 默认线性递增时间间隔重试策略
        /// </summary>
        public static AveRetryPolicy DefaultProgressive
        {
            get
            {
                return AveRetryPolicy.defaultProgressive;
            }
        }
        /// <summary>
        /// 默认指数级递增时间间隔重试策略
        /// </summary>
        public static AveRetryPolicy DefaultExponential
        {
            get
            {
                return AveRetryPolicy.defaultExponential;
            }
        }
        public AveRetryStrategy AveRetryStrategy
        {
            get;
            private set;
        }
        public ITransientErrorDetectionStrategy ErrorDetectionStrategy
        {
            get;
            private set;
        }
        public AveRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, AveRetryStrategy retryStrategy)
        {
            Guard.ArgumentNotNull(errorDetectionStrategy, "errorDetectionStrategy");
            Guard.ArgumentNotNull(retryStrategy, "retryPolicy");
            this.ErrorDetectionStrategy = errorDetectionStrategy;
            if (errorDetectionStrategy == null)
            {
                throw new InvalidOperationException("The error detection strategy type must implement the ITransientErrorDetectionStrategy interface.");
            }
            this.AveRetryStrategy = retryStrategy;
        }
        public AveRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount)
            : this(errorDetectionStrategy, new FixedIntervalRetryStrategy(retryCount))
        {
        }
        public AveRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan retryInterval)
            : this(errorDetectionStrategy, new FixedIntervalRetryStrategy(retryCount, retryInterval))
        {
        }
        public AveRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
            : this(errorDetectionStrategy, new ExponentialBackoffRetryStrategy(retryCount, minBackoff, maxBackoff, deltaBackoff))
        {
        }
        public AveRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : this(errorDetectionStrategy, new IncrementalRetryStrategy(retryCount, initialInterval, increment))
        {
        }
        /// <summary>
        /// 同步方式执行某无返回值操作，配以相应重试策略
        /// </summary>
        public virtual void ExecuteAction(Action action)
        {
            Guard.ArgumentNotNull(action, "action");
            this.ExecuteAction<object>(delegate
            {
                action();
                return null;
            });
        }
        /// <summary>
        /// 同步方式执行某无返回值、带一个参数的操作，配以相应重试策略
        /// </summary>
        public virtual void ExecuteAction<T>(Action<T> action, T param)
        {
            Guard.ArgumentNotNull(action, "action");
            this.ExecuteAction<object>(delegate
            {
                action(param);
                return null;
            });
        }

        /// <summary>
        /// 同步方式执行某有返回值操作，配以相应重试策略
        /// </summary>
        public virtual TResult ExecuteAction<TResult>(Func<TResult> func)
        {
            Guard.ArgumentNotNull(func, "func");
            int num = 0;
            TimeSpan zero = TimeSpan.Zero;
            ShouldRetry shouldRetry = this.AveRetryStrategy.GetShouldRetry();
            TResult result;
            while (true)
            {
                Exception ex = null;
                try
                {
                    result = func();
                    break;
                }
                catch (AveRetryLimitExceededException ex2)
                {
                    if (ex2.InnerException != null)
                    {
                        throw ex2.InnerException;
                    }
                    result = default(TResult);
                    break;
                }
                catch (Exception ex3)
                {
                    ex = ex3;
                    if (!this.ErrorDetectionStrategy.IsTransient(ex) || !shouldRetry(num++, ex, out zero))
                    {
                        logger.Error("Retry policy terminated. Current retry count {0}, Error {1}", num, ex.ToString());
                        throw;
                    }
                }
                if (zero.TotalMilliseconds < 0.0)
                {
                    zero = TimeSpan.Zero;
                }
                this.OnRetrying(num, ex, zero);
                if (num > 1 || !this.AveRetryStrategy.FastFirstRetry)
                {
                    Task.Delay(zero).Wait();
                }
            }
            return result;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int retryTimes, TimeSpan retryInterval)
        {
            var currentRetryTimes = 0;
            while (true)
            {
                try
                {
                    return await func();
                }
                catch (Exception e)
                {
                    if (ErrorDetectionStrategy.IsTransient(e) && currentRetryTimes < retryTimes)
                    {
                        currentRetryTimes++;
                        Thread.Sleep(retryInterval);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            throw new Exception("Retry failed");
        }

        protected virtual void OnRetrying(int retryCount, Exception lastError, TimeSpan delay)
        {
            if (this.Retrying != null)
            {
                this.Retrying(this, new AveRetryingEventArgs(retryCount, delay, lastError));
            }
        }
    }

    /// <summary>
    /// 统一重试策略，当执行操作可能遇到临时性故障或异常需要重试时使用
    /// </summary>
    public class AveRetryPolicy<T> : AveRetryPolicy where T : class, ITransientErrorDetectionStrategy, new()
    {
        public AveRetryPolicy(AveRetryStrategy retryStrategy)
            : base(!(typeof(T).GetType().IsValueType) ? Activator.CreateInstance<T>() : default(T), retryStrategy)
        {
        }
        public AveRetryPolicy(int retryCount)
            : base(!(typeof(T).GetType().IsValueType) ? Activator.CreateInstance<T>() : default(T), retryCount)
        {
        }
        public AveRetryPolicy(int retryCount, TimeSpan retryInterval)
            : base(!(typeof(T).GetType().IsValueType) ? Activator.CreateInstance<T>() : default(T), retryCount, retryInterval)
        {
        }
        public AveRetryPolicy(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
            : base(!(typeof(T).GetType().IsValueType) ? Activator.CreateInstance<T>() : default(T), retryCount, minBackoff, maxBackoff, deltaBackoff)
        {
        }
        public AveRetryPolicy(int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : base(!(typeof(T).GetType().IsValueType) ? Activator.CreateInstance<T>() : default(T), retryCount, initialInterval, increment)
        {
        }
    }
}