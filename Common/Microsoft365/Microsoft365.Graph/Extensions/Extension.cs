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

namespace Microsoft365.Graph.Extensions;
using System;
internal static class Extension
{
    internal static string GetMailboxId(this GraphBetaModels.MailboxFolder folder)
    {
        var parentMailboxUrl = folder.ParentMailboxUrl;
        if (Uri.TryCreate(parentMailboxUrl, UriKind.Absolute, out var uri) &&
             (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return uri.Segments.Last().Trim('/').EnsureIfNotNullOrEmpty();
        }
        throw new ArgumentException("ParentMailboxUrl is null, empty, or not a valid URL.");
    }

    internal static BearerTokenAuthenticationProvider ToAuthenticationProvider(this IATokenProviderBase tokenProvider)
    {
        return new BearerTokenAuthenticationProvider(new AccessTokenProvider(tokenProvider));
    }
    
    internal static BearerTokenAuthenticationProvider ToAuthenticationProviderForSecurityService(this IATokenProviderBase tokenProvider)
    {
        return new BearerTokenAuthenticationProvider(new SecurityServiceAccessTokenProvider(tokenProvider));
    }

    internal static Uri AppendQueryParameter(this Uri uri, string key, string value)
    {
        var uriBuilder = new UriBuilder(uri);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
        query[key] = value;
        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }

    internal static async Task<ODataError?> GetODataErrorAsync(this HttpContent content, bool decompress = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var mediaType = content.Headers.ContentType?.MediaType;
            if (@"application/json".EqualsIgnoreCase(mediaType))
            {
                using var stream = await content.ReadStreamAsync(decompress, cancellationToken);
                //ParseNodeFactoryRegistry.DefaultInstance support application/json, text/plain, application/x-www-form-urlencoded.
                //But I only get the error in json, cannot test with other media type. If you get error in text/plain or application/x-www-form-urlencoded, try test and reuse this code.
                var node = await ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNodeAsync(mediaType!, stream, cancellationToken);
                return node.GetObjectValue(ODataError.CreateFromDiscriminatorValue);
            }
            //add handling for other media type here
            return null;
        }
        catch
        {
            return null;
        }
    }

    internal static bool IsShared(this DriveItem? item)
    {
        return item?.Shared is not null;
    }
    
    internal static string? DownloadUrl(this DriveItem driveItem)
    {
        object? downloadUrlObject = null;
        driveItem.AdditionalData?.TryGetValue("@microsoft.graph.downloadUrl", out downloadUrlObject);
        var downloadUrl = downloadUrlObject?.ToString();
        return downloadUrl;
    }

    internal static bool IsItemNotFound(this Exception ex)
    {
        if (ex is ODataError ode && (ode.ResponseStatusCode == (int)HttpStatusCode.NotFound || "itemNotFound".EqualsIgnoreCase(ode.Error?.Code)) ||
            ex is HttpRequestException httpex && (httpex.StatusCode == HttpStatusCode.NotFound || httpex.Message.Contains("Response status code does not indicate success: 404 (Not Found)")))
        {
            return true;
        }
        if (ex.InnerException != null)
        {
            return IsItemNotFound(ex.InnerException);
        }
        return false;
    }

    internal static void SetIfNotNull<TValue>(this TValue? value, Action<TValue> propertySetter) where TValue : struct
    {
        if (value != null)
        {
            propertySetter(value.Value);
        }
    }

    internal static void SetIfNotNull<TValue>(this TValue? value, Action<TValue> propertySetter) where TValue : class
    {
        if (value != null)
        {
            propertySetter(value);
        }
    }
}