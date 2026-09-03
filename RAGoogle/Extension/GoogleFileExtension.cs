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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Services;
using AvePoint.Records.Core.Utilities.Extensions;
using RAGoogle.Models;
using RAGoogle.Util;
using File = Google.Apis.Drive.v3.Data.File;

namespace RAGoogle.Extension;

public static class GoogleFileExtension
{
    private static readonly Dictionary<string, string> mimeTypeToExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        { "application/vnd.google-apps.document", "docx" },
        { "application/vnd.google-apps.audio", "mp3" },
        { "application/vnd.google-apps.drive-sdk", "sdk" },
        { "application/vnd.google-apps.spreadsheet", "xlsx" },
        { "application/vnd.google-apps.jam", "pdf" },
        { "application/vnd.google-apps.photo", "jpg" },
        { "application/vnd.google-apps.script", "json" },
        { "application/vnd.google-apps.presentation", "pptx" },
        { "application/vnd.google-apps.video", "mp4" },
        { "application/vnd.google-apps.vid", "mp4" },
        { "application/vnd.google-apps.file", "File" },
        { "application/vnd.google-apps.folder", "Folder"},
        { "application/vnd.google-apps.shortcut", "Shortcut"},
        { "application/vnd.google-apps.drawing", "png" }, //change svg to png
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx" },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx" },
        { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "pptx" },
        { "application/vnd.oasis.opendocument.presentation", "odp" },
        { "application/vnd.oasis.opendocument.text", "odt" },
        { "application/x-vnd.oasis.opendocument.spreadsheet", "ods" },
        { "application/vnd.google-apps.script+json", "json" },
        { "application/rtf", "rft" },
        { "application/pdf", "pdf" },
        { "text/plain", "txt" },
        { "application/zip", "zip" },
        { "application/epub+zip", "epub" },
        { "text/markdown", "md" },
        { "text/csv", "csv" },
        { "text/tab-separated-values", "tsv" },
        { "image/jpeg", "jpg" },
        { "image/png", "png" },
        { "image/svg+xml", "svg" },
        { "video/mp4", "mp4" },
    };

    private static readonly IRALogger logger = RALogger.GetInstance(typeof(GoogleFileExtension));
    public static bool IsFolder(this File file)
    {
        return file.MimeType.Eq(GoogleConstant.GoogleFolder);
    }

    public static bool IsShortcut(this File file)
    {
        return file.MimeType?.Eq(GoogleConstant.GoogleShortcut) ?? false;
    }

    public static bool IsHomeSite(this File file)
    {
        return file.MimeType?.Eq(GoogleConstant.GoogleSitePage) ?? false;
    }

    public static string GetPath(this File file)
    {
        try
        {
            Uri uri = new Uri(file.WebViewLink);
            if (uri != null)
            {
                UriBuilder uriBuilder = new UriBuilder(uri)
                {
                    Query = string.Empty
                };
                return uriBuilder.ToString();
            }
        }
        catch (Exception ex)
        {
            logger.Error($"{file.WebViewLink} get path failed, Exception: {ex}.");
        }
        return file.WebViewLink;
    }

    public static string GetFileExtension(this File file)
    {
        if (mimeTypeToExtension.TryGetValue(file.MimeType, out string? extension))
        {
            return extension;
        }
        return file.FileExtension;
    }

    public static string GetFileExtentionFromMimeType(string mimeType)
    {
        if (mimeTypeToExtension.TryGetValue(mimeType, out var extension))
        {
            return $".{extension}";
        }

        return string.Empty;
    }

    public static bool IsFileSupportVersion(this File file)
    {
        if (!file.MimeType.Contains("vnd.google-apps") || GoogleConstant.GoogleSupportVersion.Contains(file.MimeType))
        {
            return true;
        }
        return false;
    }

    public static GoogleItemData ConvertToDto(this File item, GoogleDriveData gDrive, string parentIds, string parentPath = "", string memberEmail = "")
    {
        return new GoogleItemData()
        {
            Id = item.Id,
            Name = item.Name,
            Path = item.GetPath(),
            Size = item.Size,
            FileExtension = item.GetFileExtension(),
            MimeType = item.MimeType,
            RelativePath = $"{parentPath}/{item.Name}",
            HasAugmentedPermissions = item.HasAugmentedPermissions,
            ParentId = item.Parents.IsNotNullOrEmpty() ? item.Parents[0] : string.Empty,
            ParentIds = parentIds,
            CreatedTime = item.CreatedTimeDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow,
            CreatedBy = item.Owners is { Count: > 0 } ? item.Owners[0].DisplayName : string.Empty,
            ModifiedBy = item.LastModifyingUser?.DisplayName ?? string.Empty,
            ModifiedTime = item.ModifiedTimeDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow,
            Level = item.IsFolder() ? RMNodeLevel.GoogleFolder : RMNodeLevel.GoogleFile,
            DriveName = gDrive.Name,
            DriveId = gDrive.Id,
            TenantId = gDrive.TenantId,
            MemberEmail = memberEmail,
            ModifiedByEmail = item.LastModifyingUser?.EmailAddress ?? string.Empty,
            Description = item.Description,
            WebViewLink = item.WebViewLink
        };
    }

    public static GoogleItemData CheckModifiedByEmail(this GoogleItemData itemData, IDictionary<string, string> permissionIdWithUserEmail, File file)
    {
        if (itemData.ModifiedByEmail.IsNullOrEmpty() && itemData.ModifiedBy.IsNotNullOrEmpty())
        {
            if (permissionIdWithUserEmail.TryGetValue(file.LastModifyingUser.PermissionId, out var userEmail))
            { 
                itemData.ModifiedByEmail = userEmail;
            }
        }

        return itemData;
    }
    public static string GetFileNameWithSuffix(string fileName, string suffix)
    {
        var extentionByName = Path.GetExtension(fileName);

        return $"{Path.GetFileNameWithoutExtension(fileName)}{suffix}{extentionByName}";
    }
}
