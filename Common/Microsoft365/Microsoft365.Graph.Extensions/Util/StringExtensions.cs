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
namespace System;


/// <summary>
/// Provides extension methods for mailbox services.
/// </summary>
public static class StringExtensions
{
    [return: NotNullIfNotNull(nameof(path))]
    public static string? AppendBackslash(this string? path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        return path.EndsWith('\\') ? path : $"{path}\\";
    }

    public static string? UriDecode(this string? uri)
    {
        //Microsoft Graph expects that URLs conform to RFC 3986
        //https://learn.microsoft.com/en-us/graph/onedrive-addressing-driveitems#path-encoding
        if (string.IsNullOrEmpty(uri)) return uri;
        return Uri.UnescapeDataString(uri);
    }

    public static string? ToServerRelativeUrl(this string? fullUrl)
    {
        if (fullUrl != null && !fullUrl.IsNullOrEmpty() &&
            Uri.TryCreate(fullUrl,
                          new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = true },//bypass canonicalization,
                          out var uri))
        {
            return uri.AbsolutePath;
        }
        return null;
    }

}