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
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Http;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RAI.Core.Auth
{
    /// <summary>
    /// Google Cloud authentication helper for Service Account
    /// </summary>
    public static class GoogleCloudAuth
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(GoogleCloudAuth));
        /// <summary>
        /// Get access token using Managed Identity (Application Default Credentials)
        /// This method supports Google Cloud environments with managed identity
        /// </summary>
        /// <param name="scopes">Required scopes for the token</param>
        /// <returns>Access token</returns>
        public static async Task<string> GetAccessTokenUsingManagedIdentityAsync(string[]? scopes = null)
        {
            var effectiveScopes = scopes ?? GetVertexAIScopes();

            return await ExecuteWithRetryAsync(async () =>
            {
                try
                {
                    _logger.Debug("Getting access token using Managed Identity (Application Default Credentials)");

                    var credential = await GoogleCredential.GetApplicationDefaultAsync();
                    var scoped = credential.CreateScoped(effectiveScopes);
                    var token = await scoped.UnderlyingCredential.GetAccessTokenForRequestAsync();

                    _logger.Info("Successfully obtained access token using Managed Identity");
                    return token;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to get access token using Managed Identity: {ex.Message}", ex);
                }
            }, "Getting access token using Managed Identity");
        }

        /// <summary>
        /// Get access token using ServiceAccountCredential directly (recommended approach)
        /// This method follows the pattern used in VertexAIEmbeddingProvider
        /// </summary>
        /// <param name="serviceAccountEmail">Service account email</param>
        /// <param name="privateKey">Private key in PEM format</param>
        /// <param name="scopes">Required scopes for the token</param>
        /// <param name="httpClientFactory">Optional HTTP client factory for proxy configuration</param>
        /// <returns>Access token</returns>
        public static async Task<string> GetAccessTokenUsingCredentialAsync(
            string serviceAccountEmail,
            string privateKey,
            string[]? scopes,
            Google.Apis.Http.IHttpClientFactory? httpClientFactory = null)
        {
            if (string.IsNullOrWhiteSpace(serviceAccountEmail))
                throw new ArgumentException("Service account email cannot be null or empty", nameof(serviceAccountEmail));
            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ArgumentException("Private key cannot be null or empty", nameof(privateKey));

            var effectiveScopes = scopes ?? GetVertexAIScopes();

            return await ExecuteWithRetryAsync(async () =>
            {
                try
                {
                    var tokenServerUrl = "https://oauth2.googleapis.com/token";
                    var initializer = new ServiceAccountCredential.Initializer(serviceAccountEmail, tokenServerUrl)
                        .FromPrivateKey(privateKey);

                    initializer.Scopes = effectiveScopes;

                    // Configure HTTP client factory if provided (for proxy support)
                    if (httpClientFactory is not null)
                    {
                        initializer.HttpClientFactory = httpClientFactory;
                    }

                    var serviceCredential = new ServiceAccountCredential(initializer);
                    var accessToken = await serviceCredential.GetAccessTokenForRequestAsync();

                    _logger.Info("Successfully obtained access token using ServiceAccountCredential");
                    return accessToken;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to get access token using ServiceAccountCredential: {ex.Message}", ex);
                }
            }, $"Getting access token for service account: {serviceAccountEmail}");
        }

        /// <summary>
        /// Get default scopes for Vertex AI
        /// </summary>
        /// <returns>Array of default scopes</returns>
        public static string[] GetDefaultScopes()
        {
            return new[]
            {
                "https://www.googleapis.com/auth/cloud-platform",
                "https://www.googleapis.com/auth/cloud-platform.read-only",
                "https://www.googleapis.com/auth/devstorage.full_control",
                "https://www.googleapis.com/auth/devstorage.read_only",
                "https://www.googleapis.com/auth/devstorage.read_write"
            };
        }

        /// <summary>
        /// Get Vertex AI specific scopes
        /// </summary>
        /// <returns>Array of Vertex AI scopes</returns>
        public static string[] GetVertexAIScopes()
        {
            return new[]
            {
                "https://www.googleapis.com/auth/cloud-platform"
            };
        }

        /// <summary>
        /// Get access token for Google Vertex AI with automatic authentication method selection and token caching
        /// </summary>
        /// <param name="serviceAccountEmail">Service account email (optional, if provided will use Service Account authentication)</param>
        /// <param name="privateKey">Private key (required if serviceAccountEmail is provided)</param>
        /// <param name="scopes">Required scopes for the token</param>
        /// <param name="httpClientFactory">Optional HTTP client factory for proxy configuration</param>
        /// <returns>Access token</returns>
        public static async Task<string> GetVertexAIAccessTokenAsync(
            string? serviceAccountEmail = null,
            string? privateKey = null,
            string[]? scopes = null,
            Google.Apis.Http.IHttpClientFactory? httpClientFactory = null)
        {
            var effectiveScopes = scopes ?? GetVertexAIScopes();
#if DEBUG
            if (httpClientFactory == null)
            {
                httpClientFactory = new CustomHttpClientFactory();
            }
#endif
            // Generate cache key based on authentication method
            string cacheKey;
            if (!string.IsNullOrWhiteSpace(serviceAccountEmail))
            {
                cacheKey = TokenCache.GenerateCacheKey(serviceAccountEmail, effectiveScopes);
            }
            else
            {
                cacheKey = TokenCache.GenerateManagedIdentityCacheKey(effectiveScopes);
            }

            // Check for cached token
            var cachedToken = TokenCache.GetCachedToken(cacheKey);
            if (!string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken;
            }

            // Get fresh token
            string freshToken;
            if (!string.IsNullOrWhiteSpace(serviceAccountEmail))
            {
                if (string.IsNullOrWhiteSpace(privateKey))
                {
                    throw new ArgumentException("Private key cannot be null or empty when service account email is provided", nameof(privateKey));
                }

                _logger.Debug("Using Service Account authentication for Vertex AI token...");
                freshToken = await GetAccessTokenUsingCredentialAsync(serviceAccountEmail, privateKey, effectiveScopes, httpClientFactory);
            }
            else
            {
                // Use Managed Identity when no Service Account credentials are provided
                _logger.Debug("Using Managed Identity authentication for Vertex AI token...");
                freshToken = await GetAccessTokenUsingManagedIdentityAsync(effectiveScopes);
            }

            // Cache the fresh token
            TokenCache.CacheToken(cacheKey, freshToken);

            return freshToken;
        }

        /// <summary>
        /// Execute an operation with retry logic for network failures
        /// </summary>
        private static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            string operationDescription)
        {
            var attempts = 0;
            var maxAttempts = NetworkConfig.RetryAttempts;

            while (attempts < maxAttempts)
            {
                attempts++;

                try
                {
                    _logger.Debug("Attempting {0} (attempt {1}/{2})", operationDescription, attempts, maxAttempts);
                    return await operation();
                }
                catch (Exception ex) when (IsNetworkRelatedError(ex) && attempts < maxAttempts)
                {
                    _logger.Warn("Network error on attempt {0}: {1}", attempts, ex.Message);
                    _logger.Debug("Retrying in {0}ms...", NetworkConfig.RetryDelayMs * attempts);

                    await Task.Delay(NetworkConfig.RetryDelayMs * attempts); // Exponential backoff
                }
                catch (Exception ex)
                {
                    _logger.Error("Non-retryable error: {0}", ex.Message);
                    throw;
                }
            }

            throw new InvalidOperationException($"Failed to {operationDescription} after {maxAttempts} attempts");
        }

        /// <summary>
        /// Check if an exception is related to network connectivity
        /// </summary>
        private static bool IsNetworkRelatedError(Exception ex)
        {
            if (ex is System.Net.Http.HttpRequestException ||
                ex is System.Net.WebException ||
                ex is SocketException ||
                ex is TimeoutException)
            {
                return true;
            }

            // Check inner exceptions
            if (ex.InnerException != null)
            {
                return IsNetworkRelatedError(ex.InnerException);
            }

            // Check for specific error messages
            var message = ex.Message.ToLowerInvariant();
            return message.Contains("connection") ||
                   message.Contains("timeout") ||
                   message.Contains("network") ||
                   message.Contains("unreachable") ||
                   message.Contains("dns") ||
                   message.Contains("refused");
        }

    }
}
