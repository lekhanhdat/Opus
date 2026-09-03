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
namespace Microsoft365.SharePoint.FormDigest
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using Microsoft365.Authentication;
    using Microsoft365.Common.Cache;
    using Microsoft365.Common.Exception;
    using Microsoft365.Common.HttpUtil;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;
    using Microsoft365.SharePoint.Extension;

    class FormDigestProvider : IFormDigestProvider
    {

        public IKeyValueCache<string, FormDigest> cache = new KeyValueCache<string, FormDigest>(Microsoft365Configuration.AuthenticationConfiguration.TokenSetting.MaxCacheInstance, Microsoft365Configuration.AuthenticationConfiguration.TokenSetting.CacheInstanceLifeCycleEdge, Microsoft365Configuration.AuthenticationConfiguration.TokenSetting.CacheInstanceLifeCycleSecondTime);

        public int Capacity
        {
            get
            {
                return cache.Capacity;
            }

            set
            {
                cache.Capacity = value;
            }
        }

        public void Clear()
        {
            cache.Clear();
        }

        public FormDigest GetFormDigest(string url, ITokenProvider tokenProvider, bool refresh = false)
        {
            var key = GenerateKey(url, tokenProvider);

            var digest = cache.Get(key);

            if (digest == null || refresh || DateTime.UtcNow >= digest.Expiration)
            {
                digest = GetFormDigestInternal(url, tokenProvider);
                cache.AddOrUpdate(key, digest);
            }

            return digest;
        }

        private string GenerateKey(string url,ITokenProvider tokenProvider)
        {
            if (tokenProvider != null&&(!string.IsNullOrEmpty(tokenProvider.Identifier)))
            {
                return string.Concat(url, "-", tokenProvider.TokenType, "-", tokenProvider.Identifier);
            }

            throw new Microsoft365ApiException(Mirosoft365ApiErrorMessage.TokenProviderNotSupportedFormat(tokenProvider), Microsoft365ApiErrorCode.TokenProviderNotSupported);
        }

        private FormDigest GetFormDigestInternal(string url, ITokenProvider tokenProvider)
        {
            if (tokenProvider != null && tokenProvider.TokenType == TokenType.Bearer)
            {
                return new FormDigest() { Expiration = DateTime.UtcNow.AddDays(1), DigestValue = null };
            }

            return GetFormDigestByRestAPI(url,tokenProvider);
        }

        private static FormDigest GetFormDigestByRestAPI(string url,ITokenProvider tokenProvider)
        {
            //Validate input
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            //Create REST Request
            Uri uri = new Uri(url + "/_api/contextinfo");
            var request = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(uri);
            request.SetToken(url, tokenProvider, false);
            request.Method = "POST";
            request.ContentLength = 0;

            //Retrieve Response

            HttpWebResponse restResponse = request.WebRequest.GetResponseByHttpClient(null, "Authentication", RestClientFactory.DefaultStrategies);

            if (restResponse != null)
            {
                using (restResponse)
                {
                    using (var responseStream = restResponse.GetResponseStream())
                    {
                        return ParseFormDigest(responseStream);
                    }
                }
            }

            return null;
        }

        public FormDigest GetFormDigestForCookieByRestAPI(string url, string cookie)
        {
            //Validate input
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            //Create REST Request
            Uri uri = new Uri(url + "/_api/contextinfo");
            var request = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(uri);
            request.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";

            request.Headers[HttpRequestHeader.Cookie] = cookie;

            request.Method = "POST";
            request.ContentLength = 0;

            //Retrieve Response

            HttpWebResponse restResponse = request.WebRequest.GetResponseByHttpClient(null, "Authentication", RestClientFactory.DefaultStrategies);

            if (restResponse != null)
            {
                using (restResponse)
                {
                    using (var responseStream = restResponse.GetResponseStream())
                    {
                        return ParseFormDigest(responseStream);
                    }
                }
            }

            return null;
        }

        private static FormDigest ParseFormDigest(Stream responseStream)
        {
            XDocument atomDoc = XDocument.Load(responseStream);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            var digestValueElement = atomDoc.Descendants((d + "FormDigestValue")).FirstOrDefault();
            var timeoutElement = atomDoc.Descendants((d + "FormDigestTimeoutSeconds")).FirstOrDefault();
            if (digestValueElement == null || timeoutElement == null)
            {
                return null;
            }
            string digestValue = digestValueElement.Value;
            int digestTimeout = int.Parse(timeoutElement.Value, CultureInfo.InvariantCulture);
            Version mVersion = null;
            var versionElementParent = atomDoc.Descendants((d + "SupportedSchemaVersions")).FirstOrDefault();
            if (versionElementParent == null)
            {
                mVersion = new Version("15.0.0.0");
            }
            var versionElements = versionElementParent?.Descendants(d + "element").ToList();
            if (versionElements == null || versionElements?.Count == 0)
            {
                mVersion = new Version("15.0.0.0");
            }
            if (versionElements != null)
            {
                foreach (var versionElement in versionElements)
                {
                    var c = new Version(versionElement?.Value);
                    if (c > new Version("15.0.0.0"))
                    {
                        break;
                    }
                    mVersion = c;
                }
            }
            return new FormDigest
            {
                DigestValue = digestValue,
                Expiration = DateTime.UtcNow.AddSeconds((double)digestTimeout * 0.75),
                RequestSchemaVersion = mVersion
            };
        }
    }
}