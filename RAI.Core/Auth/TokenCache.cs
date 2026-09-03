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
using System;
using System.Collections.Generic;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RAI.Core.Auth
{
    /// <summary>
    /// Token cache for storing access tokens with thread-safe operations
    /// </summary>
    public static class TokenCache
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, CachedToken> _cache = new Dictionary<string, CachedToken>();
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(TokenCache));

        /// <summary>
        /// Cached token with expiration time
        /// </summary>
        private class CachedToken
        {
            public string Token { get; set; } = string.Empty;
            public DateTime ExpirationTime { get; set; }
            
            public bool IsExpired => DateTime.UtcNow >= ExpirationTime;
        }

        /// <summary>
        /// Get cached token if available and not expired
        /// </summary>
        /// <param name="cacheKey">Cache key for the token</param>
        /// <returns>Cached token or null if not found or expired</returns>
        public static string? GetCachedToken(string cacheKey)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(cacheKey, out var cachedToken))
                {
                    if (!cachedToken.IsExpired)
                    {
                        _logger.Debug("Using cached token for key: {0}", cacheKey);
                        return cachedToken.Token;
                    }
                    else
                    {
                        // Remove expired token
                        _cache.Remove(cacheKey);
                        _logger.Debug("Cached token expired for key: {0}", cacheKey);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Store token in cache with 50 minutes expiration
        /// </summary>
        /// <param name="cacheKey">Cache key for the token</param>
        /// <param name="token">Access token to cache</param>
        public static void CacheToken(string cacheKey, string token)
        {
            lock (_lock)
            {
                var expirationTime = DateTime.UtcNow.AddMinutes(50); // Cache for 50 minutes
                _cache[cacheKey] = new CachedToken
                {
                    Token = token,
                    ExpirationTime = expirationTime
                };
                _logger.Debug("Cached token for key: {0}, expires at: {1:yyyy-MM-dd HH:mm:ss} UTC", cacheKey, expirationTime);
            }
        }

        /// <summary>
        /// Clear all cached tokens
        /// </summary>
        public static void ClearCache()
        {
            lock (_lock)
            {
                _cache.Clear();
                _logger.Info("Token cache cleared");
            }
        }

        /// <summary>
        /// Get the number of cached tokens
        /// </summary>
        /// <returns>Number of tokens currently in cache</returns>
        public static int GetCacheCount()
        {
            lock (_lock)
            {
                return _cache.Count;
            }
        }

        /// <summary>
        /// Remove expired tokens from cache
        /// </summary>
        /// <returns>Number of expired tokens removed</returns>
        public static int CleanupExpiredTokens()
        {
            lock (_lock)
            {
                var expiredKeys = new List<string>();
                
                foreach (var kvp in _cache)
                {
                    if (kvp.Value.IsExpired)
                    {
                        expiredKeys.Add(kvp.Key);
                    }
                }

                foreach (var key in expiredKeys)
                {
                    _cache.Remove(key);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.Debug("Cleaned up {0} expired tokens from cache", expiredKeys.Count);
                }

                return expiredKeys.Count;
            }
        }

        /// <summary>
        /// Generate cache key for service account credentials
        /// </summary>
        /// <param name="serviceAccountEmail">Service account email</param>
        /// <param name="scopes">Token scopes</param>
        /// <returns>Cache key</returns>
        public static string GenerateCacheKey(string serviceAccountEmail, string[] scopes)
        {
            var scopesString = string.Join(",", scopes ?? Array.Empty<string>());
            return $"sa:{serviceAccountEmail}:scopes:{scopesString}";
        }

        /// <summary>
        /// Generate cache key for Managed Identity credentials
        /// </summary>
        /// <param name="scopes">Token scopes</param>
        /// <returns>Cache key</returns>
        public static string GenerateManagedIdentityCacheKey(string[] scopes)
        {
            var scopesString = string.Join(",", scopes ?? Array.Empty<string>());
            return $"managed_identity:scopes:{scopesString}";
        }

        /// <summary>
        /// Generate cache key for key file authentication
        /// </summary>
        /// <param name="keyFilePath">Path to the service account key file</param>
        /// <param name="scopes">Token scopes</param>
        /// <returns>Cache key</returns>
        public static string GenerateKeyFileCacheKey(string keyFilePath, string[] scopes)
        {
            var scopesString = string.Join(",", scopes ?? Array.Empty<string>());
            return $"keyfile:{keyFilePath}:scopes:{scopesString}";
        }
    }
}
