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
using AvePoint.Opus.RelatedRecords.Contract;
using Microsoft.SharePoint;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AvePoint.Opus.RelatedRecords.Utilities
{
    internal static class OpusApiTokenService
    {
        private const int TokenRefreshBufferMinutes = 2;
        private static readonly object _syncRoot = new object();
        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler());

        private static OpusAPIInfo _apiInfo;
        private static string _accessToken;
        private static DateTime _tokenExpiresAtUtc = DateTime.MinValue;

        public static void SaveOpusApiInfo(OpusAPIInfo apiInfo)
        {
            if (apiInfo == null)
            {
                throw new ArgumentNullException(nameof(apiInfo));
            }

            if (string.IsNullOrWhiteSpace(apiInfo.OpusIdentityUrl)
                || string.IsNullOrWhiteSpace(apiInfo.OpusWebApiUrl)
                || string.IsNullOrWhiteSpace(apiInfo.ClientId)
                || string.IsNullOrWhiteSpace(apiInfo.ThumbPrint))
            {
                throw new ArgumentException("OpusAPIInfo is invalid. opusIdentityUrl, opusWebApiUrl, clientId and thumbPrint are required.");
            }

            var normalized = new OpusAPIInfo
            {
                OpusIdentityUrl = NormalizeUrl(apiInfo.OpusIdentityUrl),
                OpusWebApiUrl = NormalizeUrl(apiInfo.OpusWebApiUrl),
                ClientId = apiInfo.ClientId.Trim(),
                ThumbPrint = NormalizeThumbprint(apiInfo.ThumbPrint),
                TenantId = (apiInfo.TenantId ?? string.Empty).Trim()
            };

            lock (_syncRoot)
            {
                bool changed = _apiInfo == null
                    || !string.Equals(_apiInfo.OpusIdentityUrl, normalized.OpusIdentityUrl, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(_apiInfo.OpusWebApiUrl, normalized.OpusWebApiUrl, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(_apiInfo.ClientId, normalized.ClientId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(_apiInfo.ThumbPrint, normalized.ThumbPrint, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(_apiInfo.TenantId, normalized.TenantId, StringComparison.OrdinalIgnoreCase);

                _apiInfo = normalized;
                if (changed)
                {
                    _accessToken = null;
                    _tokenExpiresAtUtc = DateTime.MinValue;
                }
            }
        }

        public static void SaveOpusApiInfoAndValidateToken(OpusAPIInfo apiInfo)
        {
            if (apiInfo == null)
            {
                throw new ArgumentNullException(nameof(apiInfo));
            }

            var normalized = new OpusAPIInfo
            {
                OpusIdentityUrl = NormalizeUrl(apiInfo.OpusIdentityUrl),
                OpusWebApiUrl = NormalizeUrl(apiInfo.OpusWebApiUrl),
                ClientId = apiInfo.ClientId.Trim(),
                ThumbPrint = NormalizeThumbprint(apiInfo.ThumbPrint),
                TenantId = (apiInfo.TenantId ?? string.Empty).Trim()
            };

            OpusAPIInfo previousApiInfo;
            string previousAccessToken;
            DateTime previousTokenExpiresAtUtc;

            lock (_syncRoot)
            {
                previousApiInfo = _apiInfo;
                previousAccessToken = _accessToken;
                previousTokenExpiresAtUtc = _tokenExpiresAtUtc;
            }

            try
            {
                GetAccessToken(normalized);

                lock (_syncRoot)
                {
                    _apiInfo = normalized;
                }
            }
            catch
            {
                lock (_syncRoot)
                {
                    _apiInfo = previousApiInfo;
                    _accessToken = previousAccessToken;
                    _tokenExpiresAtUtc = previousTokenExpiresAtUtc;
                }

                throw;
            }
        }

        public static string GetAccessToken()
        {
            return GetAccessToken(null);
        }

        private static string GetAccessToken(OpusAPIInfo overrideApiInfo)
        {
            OpusAPIInfo current;
            lock (_syncRoot)
            {
                current = overrideApiInfo ?? _apiInfo;
                if (current == null)
                {
                    throw new InvalidOperationException("OpusAPIInfo has not been initialized.");
                }

                // For override validation flow, do not reuse a token cached for potentially different _apiInfo.
                if (overrideApiInfo == null && IsTokenValid(_accessToken, _tokenExpiresAtUtc))
                {
                    return _accessToken;
                }
            }

            var tokenEndpoint = CombineUrl(current.OpusIdentityUrl, "/connect/token");
            var scope = "records.readwrite.all";
            string clientAssertion = null;
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                clientAssertion = CreateClientAssertion(current, tokenEndpoint);
            });

            using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = current.ClientId,
                ["scope"] = scope,
                ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                ["client_assertion"] = clientAssertion
            }))
            using (var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint))
            {
                request.Content = content;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = _httpClient.SendAsync(request).GetAwaiter().GetResult())
                {
                    var payload = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"Token endpoint returned {(int)response.StatusCode} {response.ReasonPhrase}: {payload}");
                    }

                    var json = JObject.Parse(payload);
                    var token = json.Value<string>("access_token");
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        throw new InvalidOperationException($"access_token not found in token response: {payload}");
                    }

                    var expiresIn = json.Value<int?>("expires_in") ?? 0;
                    var expiresAt = ResolveTokenExpiryUtc(token, expiresIn);

                    lock (_syncRoot)
                    {
                        _accessToken = token;
                        _tokenExpiresAtUtc = expiresAt;
                        return _accessToken;
                    }
                }
            }
        }

        public static DateTime GetTokenExpiresAtUtc()
        {
            lock (_syncRoot)
            {
                return _tokenExpiresAtUtc;
            }
        }

        public static string CallExternalApi(string method, string relativePath, string requestBody = null)
        {
            var current = GetCurrentApiInfo();
            var requestUrl = CombineUrl(current.OpusWebApiUrl, relativePath);
            var token = GetAccessToken();

            using (var request = new HttpRequestMessage(new HttpMethod((method ?? "GET").Trim().ToUpperInvariant()), requestUrl))
            {
                request.Headers.TryAddWithoutValidation("Token-Source", "IdentityServer");
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");

                if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Delete || request.Method == HttpMethod.Put)
                {
                    request.Content = new StringContent(requestBody ?? string.Empty, Encoding.UTF8, "application/json");
                }

                using (var response = _httpClient.SendAsync(request).GetAwaiter().GetResult())
                {
                    var payload = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new WebException($"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {payload}");
                    }

                    return payload;
                }
            }
        }

        private static OpusAPIInfo GetCurrentApiInfo()
        {
            lock (_syncRoot)
            {
                if (_apiInfo == null)
                {
                    throw new InvalidOperationException("OpusAPIInfo has not been initialized.");
                }

                return _apiInfo;
            }
        }

        private static bool IsTokenValid(string token, DateTime expiresAtUtc)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            return DateTime.UtcNow.AddMinutes(TokenRefreshBufferMinutes) < expiresAtUtc;
        }

        private static DateTime ResolveTokenExpiryUtc(string accessToken, int expiresInSeconds)
        {
            if (expiresInSeconds > 0)
            {
                return DateTime.UtcNow.AddSeconds(expiresInSeconds);
            }

            var exp = ReadJwtExp(accessToken);
            if (exp > 0)
            {
                return DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            }

            return DateTime.UtcNow.AddHours(1);
        }

        private static long ReadJwtExp(string jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
            {
                return 0;
            }

            var parts = jwt.Split('.');
            if (parts.Length < 2)
            {
                return 0;
            }

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            var payload = JObject.Parse(payloadJson);
            return payload.Value<long?>("exp") ?? 0;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            return Convert.FromBase64String(base64);
        }

        private static string CreateClientAssertion(OpusAPIInfo apiInfo, string audience)
        {
            var certificate = FindCertificateByThumbprint(apiInfo.ThumbPrint);
            if (certificate == null)
            {
                throw new InvalidOperationException($"Certificate with thumbprint '{apiInfo.ThumbPrint}' was not found in CurrentUser or LocalMachine store.");
            }

            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException($"Certificate '{certificate.Thumbprint}' does not have a private key.");
            }

            var now = DateTimeOffset.UtcNow;
            var header = new JObject
            {
                ["alg"] = "RS256",
                ["typ"] = "JWT",
                ["x5t"] = Base64UrlEncode(certificate.GetCertHash())
            };

            var payload = new JObject
            {
                ["iss"] = apiInfo.ClientId,
                ["sub"] = apiInfo.ClientId,
                ["aud"] = audience,
                ["jti"] = Guid.NewGuid().ToString("N"),
                ["iat"] = now.ToUnixTimeSeconds(),
                ["nbf"] = now.ToUnixTimeSeconds(),
                ["exp"] = now.AddMinutes(30).ToUnixTimeSeconds()
            };

            if (!string.IsNullOrWhiteSpace(apiInfo.TenantId))
            {
                payload["tid"] = apiInfo.TenantId;
            }

            var unsignedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(header.ToString(Newtonsoft.Json.Formatting.None)))
                              + "."
                              + Base64UrlEncode(Encoding.UTF8.GetBytes(payload.ToString(Newtonsoft.Json.Formatting.None)));

            var bytesToSign = Encoding.UTF8.GetBytes(unsignedToken);
            using (var rsa = certificate.GetRSAPrivateKey())
            {
                if (rsa == null)
                {
                    throw new InvalidOperationException($"Certificate '{certificate.Thumbprint}' private key is not RSA.");
                }

                var signature = rsa.SignData(bytesToSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return unsignedToken + "." + Base64UrlEncode(signature);
            }
        }

        private static X509Certificate2 FindCertificateByThumbprint(string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(thumbprint))
            {
                throw new ArgumentException("Certificate thumbprint is required.", nameof(thumbprint));
            }

            var normalizedThumbprint = NormalizeThumbprint(thumbprint);
            return FindCertificate(StoreLocation.CurrentUser, normalizedThumbprint)
                ?? FindCertificate(StoreLocation.LocalMachine, normalizedThumbprint);
        }

        private static X509Certificate2 FindCertificate(StoreLocation location, string thumbprint)
        {
            using (var store = new X509Store(StoreName.My, location))
            {
                store.Open(OpenFlags.ReadOnly);
                var matches = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                return matches.Count > 0 ? matches[0] : null;
            }
        }

        private static string NormalizeThumbprint(string thumbprint)
        {
            return thumbprint.Replace(" ", string.Empty).ToUpperInvariant();
        }

        private static string NormalizeUrl(string url)
        {
            return (url ?? string.Empty).Trim().TrimEnd('/');
        }

        private static string CombineUrl(string baseUrl, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("Base URL is not configured.");
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return NormalizeUrl(baseUrl);
            }

            return new Uri(new Uri(NormalizeUrl(baseUrl) + "/"), relativePath.TrimStart('/')).ToString();
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
