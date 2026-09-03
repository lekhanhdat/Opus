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

public static class GraphIdHelper
{
    public static string EncodeDriveId(Guid siteId, Guid webId, Guid listId)
    {
        var bytes = new byte[16 * 3];
        siteId.ToByteArray().CopyTo(bytes, 0);
        webId.ToByteArray().CopyTo(bytes, 16);
        listId.ToByteArray().CopyTo(bytes, 32);
        return $"b!{Convert.ToBase64String(bytes)}";
    }

    public static (Guid SiteId, Guid WebId, Guid ListId) DecodeDriveId(string graphDriveId)
    {
        ArgumentNullException.ThrowIfNull(graphDriveId);
        if (graphDriveId.Length <= 2)
        {
            throw new InvalidCastException(nameof(graphDriveId));
        }
        var encodedDriveId = graphDriveId[2..].Replace('_', '/').Replace('-', '+');
        var encodedDriveIdBytes = Convert.FromBase64String(encodedDriveId);
        var siteIdBytes = encodedDriveIdBytes.Take(16).ToArray();
        var webIdBytes = encodedDriveIdBytes.Skip(16).Take(16).ToArray();
        var listIdBytes = encodedDriveIdBytes.Skip(32).Take(16).ToArray();
        return (SiteId: new Guid(siteIdBytes), WebId: new Guid(webIdBytes), ListId: new Guid(listIdBytes));
    }

    public static string ToGraphSiteId(string siteUrl, Guid spSiteId, Guid spWebId)
    {
        return ToGraphSiteId(siteUrl, spSiteId.ToString(), spWebId.ToString());
    }

    private static string ToGraphSiteId(string siteUrl, string spSiteId, string spWebId)
    {
        if (Uri.TryCreate(siteUrl, UriKind.Absolute, out var uri))
        {
            return string.Join(',', uri.Host, spSiteId, spWebId);
        }
        throw new ArgumentException($"Invaild site url: {siteUrl}");
    }

    public static string GraphSiteId(this SharepointIds sharepointIds)
    {
        return ToGraphSiteId(siteUrl: sharepointIds.SiteUrlDecoded().EnsureIfNotNullOrEmpty(),
                             spSiteId: sharepointIds.SiteId.EnsureIfNotNullOrEmpty(),
                             spWebId: sharepointIds.WebId.EnsureIfNotNullOrEmpty());
    }
}