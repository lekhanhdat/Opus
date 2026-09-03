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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RAI.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace AvePoint.RA.Common.AI
{
    /// <summary>
    /// AI Client Common Utilities
    /// Provides common functionality that can be shared across different AI Provider implementations
    /// </summary>
    public static class AiClientUtils
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(AiClientUtils));

        /// <summary>
        /// Execute an async operation with thread-safe initialization pattern
        /// </summary>
        /// <param name="isInitialized">Flag indicating if already initialized</param>
        /// <param name="semaphore">Semaphore for thread safety</param>
        /// <param name="initializeAction">Initialization action to execute</param>
        /// <param name="clientTypeName">Client type name for logging</param>
        /// <returns>Task representing the initialization operation</returns>
        public static async Task ExecuteInitializationAsync(
            Func<bool> isInitialized,
            SemaphoreSlim semaphore,
            Func<Task> initializeAction,
            string clientTypeName)
        {
            if (isInitialized())
                return;

            await semaphore.WaitAsync();
            try
            {
                if (isInitialized())
                    return;

                _logger.Info("Initializing {0}", clientTypeName);

                await initializeAction();

                _logger.Info("{0} initialized successfully", clientTypeName);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to initialize {0}: {1}", clientTypeName, ex, ex.Message);
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Create development HTTP client with proxy configuration for debugging
        /// Supports both development proxy and fallback to default client
        /// </summary>
        /// <param name="clientTypeName">Client type name for logging</param>
        /// <param name="timeoutMinutes">HTTP client timeout in minutes (default: 5)</param>
        /// <returns>HTTP client configured for development proxy or default client</returns>
        public static HttpClient CreateDevelopmentHttpClient(string clientTypeName, int timeoutMinutes = 5)
        {
#if DEBUG
            try
            {
                // Use CustomHttpClientFactory approach for proxy configuration
                var httpClient = CreateHttpClientWithProxyFromConfig();
                if (httpClient != null)
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(timeoutMinutes);
                    _logger.Info("Created HttpClient using proxy configuration for {0}", clientTypeName);
                    return httpClient;
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("Error creating HttpClient with proxy configuration for {0}: {1}. Falling back to default.", clientTypeName, ex.Message);
            }
#endif
            // Fallback to default HttpClient
            return new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(timeoutMinutes)
            };
        }

        /// <summary>
        /// Create HTTP client with proxy configuration from Proxy.json file
        /// </summary>
        /// <returns>HttpClient with proxy configuration or null if proxy config not found</returns>
        public static HttpClient? CreateHttpClientWithProxyFromConfig()
        {
#if DEBUG
            try
            {
                string developmentJson = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "Proxy.json");
                if (!File.Exists(developmentJson))
                {
                    return null; // No proxy config found
                }

                LocalProxy? proxyConfig = null;
                using (StreamReader stream = new StreamReader(developmentJson))
                {
                    string proxyJson = stream.ReadToEnd();
                    if (!string.IsNullOrEmpty(proxyJson))
                    {
                        proxyConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<LocalProxy>(proxyJson);
                    }
                }

                if (proxyConfig == null || string.IsNullOrEmpty(proxyConfig.Host))
                {
                    return null; // Invalid proxy config
                }

                var proxy = new System.Net.WebProxy(proxyConfig.Host, true)
                {
                    Credentials = new System.Net.NetworkCredential(proxyConfig.Account, proxyConfig.Password)
                };

                var handler = new HttpClientHandler()
                {
                    UseProxy = true,
                    Proxy = proxy,
                    CheckCertificateRevocationList = false,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                    UseCookies = false,
                    PreAuthenticate = false,
                    AutomaticDecompression = System.Net.DecompressionMethods.None
                };

                return new HttpClient(handler);
            }
            catch
            {
                return null;
            }
#else
            return null;
#endif
        }

        /// <summary>
        /// Validate configuration value and throw descriptive exception if missing
        /// </summary>
        /// <param name="configValue">Configuration value to validate</param>
        /// <param name="configKey">Configuration key name for error message</param>
        /// <param name="clientTypeName">Client type name for error context</param>
        /// <exception cref="InvalidOperationException">Thrown when configuration is missing</exception>
        public static void ValidateRequiredConfiguration(string? configValue, string configKey, string clientTypeName)
        {
            if (string.IsNullOrWhiteSpace(configValue))
            {
                throw new InvalidOperationException($"{clientTypeName}: Configuration '{configKey}' is required but not found or empty");
            }
        }

        /// <summary>
        /// Get configuration value with default fallback
        /// </summary>
        /// <param name="configKey">Configuration key to retrieve</param>
        /// <param name="defaultValue">Default value if configuration is not found</param>
        /// <returns>Configuration value or default</returns>
        public static string GetConfigurationWithDefault(RMAppSettingKey configKey, string defaultValue)
        {
            var value = RMGlobalConfiguration.AppConfig[configKey];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        /// <summary>
        /// Create AI provider with common validation and error handling
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <param name="providerCode">Provider code</param>
        /// <param name="aiType">AI provider type</param>
        /// <param name="configAction">Action to configure provider-specific settings</param>
        /// <returns>Configured AI provider</returns>
        public static AiProvider CreateAiProvider(
            string providerName,
            string providerCode,
            AiProviderType aiType,
            Action<AiProvider> configAction)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));

            if (string.IsNullOrWhiteSpace(providerCode))
                throw new ArgumentException("Provider code cannot be null or empty", nameof(providerCode));

            var aiProvider = new AiProvider
            {
                Name = providerName,
                Code = providerCode,
                AiType = aiType
            };

            // Apply provider-specific configuration
            configAction?.Invoke(aiProvider);

            return aiProvider;
        }

        /// <summary>
        /// Safe dispose with logging and exception handling
        /// </summary>
        /// <param name="disposable">Object to dispose</param>
        /// <param name="objectName">Object name for logging</param>
        /// <param name="clientTypeName">Client type name for logging context</param>
        public static void SafeDispose(IDisposable? disposable, string objectName, string clientTypeName)
        {
            if (disposable == null) return;

            try
            {
                disposable.Dispose();
                _logger.Debug("{0} {1} disposed successfully", clientTypeName, objectName);
            }
            catch (Exception ex)
            {
                _logger.Warn("Error disposing {0} {1}: {2}", clientTypeName, objectName, ex.Message);
            }
        }

        /// <summary>
        /// Check if an exception indicates an authentication error
        /// Common patterns across different AI providers
        /// </summary>
        /// <param name="exception">Exception to check</param>
        /// <returns>True if the exception indicates authentication failure</returns>
        public static bool IsAuthenticationError(Exception exception)
        {
            // Check for HTTP 401 errors
            if (exception is HttpRequestException httpEx)
            {
                var message = httpEx.Message?.ToLowerInvariant() ?? "";
                if (message.Contains("401") || message.Contains("unauthorized"))
                {
                    return true;
                }
            }

            // Check for WebException with 401 status
            if (exception is WebException webEx && webEx.Response is HttpWebResponse httpResponse)
            {
                return httpResponse.StatusCode == HttpStatusCode.Unauthorized;
            }

            // Check inner exceptions
            if (exception.InnerException != null)
            {
                return IsAuthenticationError(exception.InnerException);
            }

            // Check exception message for common authentication failure indicators
            var exceptionMessage = exception.Message?.ToLowerInvariant() ?? "";
            return exceptionMessage.Contains("unauthorized") ||
                   exceptionMessage.Contains("401") ||
                   exceptionMessage.Contains("invalid_token") ||
                   exceptionMessage.Contains("token expired") ||
                   exceptionMessage.Contains("authentication failed") ||
                   exceptionMessage.Contains("access denied");
        }

        /// <summary>
        /// Execute operation with operation name logging and error context
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">Operation to execute</param>
        /// <param name="operationName">Operation name for logging</param>
        /// <param name="clientTypeName">Client type name for logging context</param>
        /// <returns>Result of the operation</returns>
        public static async Task<T> ExecuteOperationWithLoggingAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            string clientTypeName)
        {
            try
            {
                _logger.Debug("Executing {0} for {1}", operationName, clientTypeName);
                var result = await operation();
                _logger.Debug("{0} completed successfully for {1}", operationName, clientTypeName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to execute {0} for {1}: {2}", operationName, clientTypeName, ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Execute operation with operation name logging (void return)
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="operationName">Operation name for logging</param>
        /// <param name="clientTypeName">Client type name for logging context</param>
        /// <returns>Task representing the operation</returns>
        public static async Task ExecuteOperationWithLoggingAsync(
            Func<Task> operation,
            string operationName,
            string clientTypeName)
        {
            await ExecuteOperationWithLoggingAsync(async () =>
            {
                await operation();
                return 0; // Return dummy value for generic version
            }, operationName, clientTypeName);
        }
    }
}
