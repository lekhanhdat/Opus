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
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RAI.Core.Auth
{
    /// <summary>
    /// Helper class for handling authentication errors and implementing retry logic with token cache clearing
    /// </summary>
    public static class AuthenticationRetryHelper
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(AuthenticationRetryHelper));

        /// <summary>
        /// Determines if an exception indicates an authentication (401) error
        /// </summary>
        /// <param name="exception">The exception to check</param>
        /// <returns>True if the exception indicates a 401 authentication error</returns>
        public static bool IsAuthenticationError(Exception exception)
        {
            if (exception == null)
                return false;

            // Check HttpRequestException with 401 status code
            if (exception is HttpRequestException httpEx)
            {
                var message = httpEx.Message?.ToLowerInvariant();
                if (message != null && (message.Contains("401") || message.Contains("unauthorized")))
                {
                    return true;
                }
            }

            // Check WebException with 401 status code  
            if (exception is WebException webEx && webEx.Response is HttpWebResponse httpResponse)
            {
                return httpResponse.StatusCode == HttpStatusCode.Unauthorized;
            }

            // Check for InvalidOperationException that wraps authentication errors
            if (exception is InvalidOperationException && exception.InnerException != null)
            {
                return IsAuthenticationError(exception.InnerException);
            }

            // Check exception message for authentication-related errors
            var exceptionMessage = exception.Message?.ToLowerInvariant();
            if (exceptionMessage != null)
            {
                return exceptionMessage.Contains("401") ||
                       exceptionMessage.Contains("unauthorized") ||
                       exceptionMessage.Contains("authentication failed") ||
                       exceptionMessage.Contains("invalid credentials") ||
                       exceptionMessage.Contains("token has expired") ||
                       exceptionMessage.Contains("authentication error");
            }

            // Recursively check inner exceptions
            if (exception.InnerException != null)
            {
                return IsAuthenticationError(exception.InnerException);
            }

            return false;
        }

        /// <summary>
        /// Executes an operation with retry logic for authentication errors
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">Name of the operation for logging purposes</param>
        /// <param name="maxRetries">Maximum number of retries (default: 1)</param>
        /// <returns>Result of the operation</returns>
        public static async Task<T> ExecuteWithAuthRetryAsync<T>(
            Func<Task<T>> operation, 
            string operationName, 
            int maxRetries = 1)
        {
            var attempts = 0;
            
            while (attempts <= maxRetries)
            {
                try
                {
                    if (attempts > 0)
                    {
                        _logger.Debug("Retrying {0} (attempt {1}/{2})", operationName, attempts + 1, maxRetries + 1);
                    }
                    
                    return await operation();
                }
                catch (Exception ex) when (IsAuthenticationError(ex) && attempts < maxRetries)
                {
                    attempts++;
                    _logger.Warn("Authentication error detected during {0} (attempt {1}): {2}", 
                        operationName, attempts, ex.Message);
                    
                    _logger.Info("Clearing token cache due to authentication error");
                    try
                    {
                        TokenCache.ClearCache();
                        _logger.Info("Token cache cleared successfully");
                    }
                    catch (Exception cacheEx)
                    {
                        _logger.Error("Failed to clear token cache: {0}", cacheEx.Message);
                    }
                    
                    // Add a small delay before retry to avoid immediate retry
                    await Task.Delay(1000);
                }
                catch (Exception)
                {
                    // Non-authentication errors or max retries exceeded
                    throw;
                }
            }
            
            // This should never be reached due to the throw in the catch block above
            throw new InvalidOperationException($"Unexpected state in retry logic for {operationName}");
        }
    }
}
