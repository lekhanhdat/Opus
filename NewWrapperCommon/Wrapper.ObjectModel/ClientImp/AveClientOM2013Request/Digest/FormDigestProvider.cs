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
namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Xml.Linq;
    using Microsoft.SharePoint.Client;
    using GCommon;
    using Wrapper.Common;

    public class FormDigestProvider
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(FormDigestProvider));
        public IKeyValueCache<string, FormDigest> cache = new AccessTimeCache<string, FormDigest>(1000);

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

        public FormDigest GetFormDigest(ClientContext context)
        {
            var key = GenerateKey(context);

            var digest = cache.Get(key);

            if (digest == null || DateTime.UtcNow >= digest.Expiration)
            {
                digest = GetFormDigestInternal(context);
                cache.AddOrUpdate(key, digest);
            }

            return digest;
        }

        private string GenerateKey(ClientContext context)
        {
            if (context.Credentials != null)
            {
                var onlineCredentials = context.Credentials as Microsoft.SharePoint.Client.SharePointOnlineCredentials;

                if (onlineCredentials != null)
                {
                    if (!string.IsNullOrEmpty(onlineCredentials.UserName))
                    {
                        return string.Concat(context.Url, "-", onlineCredentials.UserName);
                    }
                }
                else
                {
                    var networkCredential = context.Credentials as NetworkCredential;

                    if (networkCredential != null)
                    {
                        if (!string.IsNullOrEmpty(networkCredential.UserName))
                        {
                            return string.Concat(context.Url, "-", networkCredential.Domain, "-", networkCredential.UserName);
                        }
                    }
                }
            }
            else if (context.AuthenticationMode == ClientAuthenticationMode.Default)
            {
                return string.Concat(context.Url, "-", Environment.UserDomainName);
            }
            throw new Exception("Doesn't suppport credential type");
        }

        //public FormDigest GetFormDigest(string webFullUrl, ITokenProvider tokenProvider)
        //{
        //    var digest = cache.Get(webFullUrl);

        //    if (digest == null || DateTime.UtcNow >= digest.Expiration)
        //    {
        //        digest = GetFormDigestInternal(webFullUrl, tokenProvider);
        //    }

        //    cache.AddOrUpdate(webFullUrl, digest);

        //    return digest;
        //}

        //private FormDigest GetFormDigestInternal(string webFullUrl, ITokenProvider tokenProvider)
        //{
        //    using (var context = new ClientContextWrapper(webFullUrl))
        //    {
        //        context.SetCredentical(tokenProvider);

        //        return GetFormDigestInternal(context);
        //    }
        //}

        private FormDigest GetFormDigestInternal(ClientContext context)
        {
            return GetDigestDefault(context);
        }
        
        private static FormDigest GetDigestDefault(ClientContext context)
        {
            var digest = new FormDigest();
            var info = context.GetFormDigestDirect();
            digest.DigestValue = info.DigestValue;
            digest.Expiration = info.Expiration;
            digest.RequestSchemaVersion = GetVersion(info);
            return digest;
        }
        
        private static Version GetVersion(FormDigestInfo info)
        {
            return AveAssemblyUtility.GetPropertyValue(info, "RequestSchemaVersion") as Version;
        }
    }
}
