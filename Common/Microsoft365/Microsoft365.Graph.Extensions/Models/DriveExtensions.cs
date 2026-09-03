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


/// <summary>
/// Provides extension methods for Graph Beta models.
/// </summary>
public static partial class ModelExtensions
{

    public static bool IsFolder(this DriveItem? item)
    {
        return item?.Folder is not null || item?.Package is not null;
    }

    public static bool IsFile(this DriveItem? item)
    {
        return item?.File is not null;
    }

    public static bool IsDeleted(this DriveItem? item)
    {
        return item?.Deleted is not null;
    }

    public static bool IsOneNoteFile(this DriveItem? driveItem)
    {
        if (driveItem is null) return false;
        return System.IO.Path.GetExtension(driveItem.Name).EqualsIgnoreCase(".one");
    }


    public static string SharingUrlEncoded(this string sharingUrl)
    {
        return "u!" + Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(sharingUrl);
    }
    public static string? WebDavUrlDecoded(this DriveItem item)
    {
        return item.WebDavUrl.UriDecode();
    }

}